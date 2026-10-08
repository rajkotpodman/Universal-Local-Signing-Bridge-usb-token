using System;
using System.Collections.Concurrent;

namespace Bridge.Security;

public class RateLimiter
{
    private readonly int _limitPerMinute;
    private readonly ConcurrentDictionary<string, (DateTime WindowStart, int Count)> _clients = new();

    public RateLimiter(int limitPerMinute = 60)
    {
        _limitPerMinute = limitPerMinute;
    }

    public bool IsAllowed(string clientKey)
    {
        var now = DateTime.UtcNow;

        var result = _clients.AddOrUpdate(
            clientKey,
            _ => (now, 1),
            (_, existing) =>
            {
                if (now - existing.WindowStart > TimeSpan.FromMinutes(1))
                {
                    return (now, 1);
                }

                return (existing.WindowStart, existing.Count + 1);
            }
        );

        return result.Count <= _limitPerMinute;
    }

    public void Reset()
    {
        _clients.Clear();
    }
}
