using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Core.Interfaces;
using Bridge.Core.Models;
using Microsoft.Data.Sqlite;

namespace Bridge.Crypto;

public class SqliteAuditRepository : IAuditRepository
{
    private readonly string _connectionString;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public SqliteAuditRepository(string dbPath = "audit.db")
    {
        _connectionString = $"Data Source={dbPath}";
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS audit_logs (
                id TEXT PRIMARY KEY,
                timestamp TEXT NOT NULL,
                session_id_hash TEXT,
                certificate_id TEXT,
                provider TEXT NOT NULL,
                operation TEXT NOT NULL,
                success INTEGER NOT NULL,
                error_code TEXT,
                duration_ms REAL NOT NULL,
                client_ip TEXT,
                origin TEXT,
                payload_digest_sha256 TEXT,
                details TEXT
            );
            CREATE INDEX IF NOT EXISTS idx_audit_timestamp ON audit_logs(timestamp DESC);
        ";
        cmd.ExecuteNonQuery();
    }

    public async Task RecordAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO audit_logs (
                    id, timestamp, session_id_hash, certificate_id, provider,
                    operation, success, error_code, duration_ms, client_ip,
                    origin, payload_digest_sha256, details
                ) VALUES (
                    @id, @timestamp, @session_id_hash, @certificate_id, @provider,
                    @operation, @success, @error_code, @duration_ms, @client_ip,
                    @origin, @payload_digest_sha256, @details
                );
            ";

            cmd.Parameters.AddWithValue("@id", entry.Id);
            cmd.Parameters.AddWithValue("@timestamp", entry.Timestamp.ToString("O"));
            cmd.Parameters.AddWithValue("@session_id_hash", (object?)entry.SessionIdHash ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@certificate_id", (object?)entry.CertificateId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@provider", entry.Provider);
            cmd.Parameters.AddWithValue("@operation", entry.Operation);
            cmd.Parameters.AddWithValue("@success", entry.Success ? 1 : 0);
            cmd.Parameters.AddWithValue("@error_code", (object?)entry.ErrorCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@duration_ms", entry.DurationMs);
            cmd.Parameters.AddWithValue("@client_ip", (object?)entry.ClientIp ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@origin", (object?)entry.Origin ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@payload_digest_sha256", (object?)entry.PayloadDigestSha256 ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@details", (object?)entry.Details ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IReadOnlyList<AuditEntry>> GetRecentAsync(int limit = 50, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var list = new List<AuditEntry>();
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, timestamp, session_id_hash, certificate_id, provider,
                       operation, success, error_code, duration_ms, client_ip,
                       origin, payload_digest_sha256, details
                FROM audit_logs
                ORDER BY timestamp DESC
                LIMIT @limit;
            ";
            cmd.Parameters.AddWithValue("@limit", Math.Clamp(limit, 1, 200));

            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                list.Add(new AuditEntry(
                    Id: reader.GetString(0),
                    Timestamp: DateTimeOffset.Parse(reader.GetString(1)),
                    SessionIdHash: reader.IsDBNull(2) ? null : reader.GetString(2),
                    CertificateId: reader.IsDBNull(3) ? null : reader.GetString(3),
                    Provider: reader.GetString(4),
                    Operation: reader.GetString(5),
                    Success: reader.GetInt32(6) == 1,
                    ErrorCode: reader.IsDBNull(7) ? null : reader.GetString(7),
                    DurationMs: reader.GetDouble(8),
                    ClientIp: reader.IsDBNull(9) ? null : reader.GetString(9),
                    Origin: reader.IsDBNull(10) ? null : reader.GetString(10),
                    PayloadDigestSha256: reader.IsDBNull(11) ? null : reader.GetString(11),
                    Details: reader.IsDBNull(12) ? null : reader.GetString(12)
                ));
            }

            return list;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<long> GetTotalOperationsCountAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync(cancellationToken);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM audit_logs;";
            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return result != null ? Convert.ToInt64(result) : 0;
        }
        finally
        {
            _lock.Release();
        }
    }
}
