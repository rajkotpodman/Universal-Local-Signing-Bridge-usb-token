using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Core.Models;

namespace Bridge.Core.Interfaces;

public interface IAuditRepository
{
    Task RecordAsync(AuditEntry entry, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditEntry>> GetRecentAsync(int limit = 50, CancellationToken cancellationToken = default);

    Task<long> GetTotalOperationsCountAsync(CancellationToken cancellationToken = default);
}

public interface ISessionStore
{
    Task<Session> CreateSessionAsync(string clientAppId, string allowedOrigin, CancellationToken cancellationToken = default);

    Task<Session?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default);

    Task<bool> ValidateSessionAsync(string sessionId, string origin, CancellationToken cancellationToken = default);

    Task<int> GetActiveSessionCountAsync(CancellationToken cancellationToken = default);
}
