using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Core.Interfaces;
using Bridge.Core.Models;
using Microsoft.Data.Sqlite;

namespace Bridge.Crypto;

public class SqliteSessionStore : ISessionStore
{
    private readonly string _connectionString;
    private readonly int _sessionLifetimeSeconds;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public SqliteSessionStore(string dbPath = "sessions.db", int sessionLifetimeSeconds = 3600)
    {
        _connectionString = $"Data Source={dbPath}";
        _sessionLifetimeSeconds = sessionLifetimeSeconds;
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS sessions (
                session_id TEXT PRIMARY KEY,
                created_at TEXT NOT NULL,
                expires_at TEXT NOT NULL,
                allowed_origin TEXT NOT NULL,
                client_app_id TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS idx_sessions_expires ON sessions(expires_at);
        ";
        cmd.ExecuteNonQuery();
    }

    public async Task<Session> CreateSessionAsync(string clientAppId, string allowedOrigin, CancellationToken cancellationToken = default)
    {
        var sessionId = Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddSeconds(_sessionLifetimeSeconds);

        var session = new Session(sessionId, now, expiresAt, allowedOrigin.TrimEnd('/'), clientAppId);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO sessions (session_id, created_at, expires_at, allowed_origin, client_app_id)
                VALUES (@session_id, @created_at, @expires_at, @allowed_origin, @client_app_id);
            ";
            cmd.Parameters.AddWithValue("@session_id", session.SessionId);
            cmd.Parameters.AddWithValue("@created_at", session.CreatedAt.ToString("O"));
            cmd.Parameters.AddWithValue("@expires_at", session.ExpiresAt.ToString("O"));
            cmd.Parameters.AddWithValue("@allowed_origin", session.AllowedOrigin);
            cmd.Parameters.AddWithValue("@client_app_id", session.ClientAppId);

            await cmd.ExecuteNonQueryAsync(cancellationToken);
            return session;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Session?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT session_id, created_at, expires_at, allowed_origin, client_app_id
                FROM sessions
                WHERE session_id = @session_id;
            ";
            cmd.Parameters.AddWithValue("@session_id", sessionId);

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return new Session(
                    SessionId: reader.GetString(0),
                    CreatedAt: DateTimeOffset.Parse(reader.GetString(1)),
                    ExpiresAt: DateTimeOffset.Parse(reader.GetString(2)),
                    AllowedOrigin: reader.GetString(3),
                    ClientAppId: reader.GetString(4)
                );
            }

            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> ValidateSessionAsync(string sessionId, string origin, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionAsync(sessionId, cancellationToken);
        if (session == null || session.IsExpired)
        {
            return false;
        }

        // If origin was specified in session, ensure matching
        if (!string.IsNullOrWhiteSpace(session.AllowedOrigin) && !string.IsNullOrWhiteSpace(origin))
        {
            var cleanSessionOrigin = session.AllowedOrigin.Trim().TrimEnd('/');
            var cleanReqOrigin = origin.Trim().TrimEnd('/');
            return string.Equals(cleanSessionOrigin, cleanReqOrigin, StringComparison.OrdinalIgnoreCase);
        }

        return true;
    }

    public async Task<int> GetActiveSessionCountAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM sessions WHERE expires_at > @now;";
            cmd.Parameters.AddWithValue("@now", DateTimeOffset.UtcNow.ToString("O"));

            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return result != null ? Convert.ToInt32(result) : 0;
        }
        finally
        {
            _lock.Release();
        }
    }
}
