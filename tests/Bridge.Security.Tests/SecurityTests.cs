using System.Net;
using Bridge.Security;
using Xunit;

namespace Bridge.Security.Tests;

public class SecurityTests
{
    [Theory]
    [InlineData("http://localhost", true)]
    [InlineData("http://127.0.0.1", true)]
    [InlineData("http://localhost:5173", true)]
    [InlineData("http://127.0.0.1:8080", true)]
    [InlineData("http://evil-attacker.com", false)]
    [InlineData("https://phishing.org", false)]
    [InlineData("http://192.168.1.100", false)]
    public void OriginValidator_ValidatesProperly(string origin, bool expected)
    {
        var validator = new OriginValidator(new[]
        {
            "http://localhost",
            "http://127.0.0.1",
            "http://localhost:5173",
            "http://127.0.0.1:8080"
        });

        var result = validator.IsAllowed(origin);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void LocalhostEnforcer_AcceptsLoopbackAndRejectsRemote()
    {
        Assert.True(LocalhostEnforcer.IsLoopbackAddress(IPAddress.Loopback));
        Assert.True(LocalhostEnforcer.IsLoopbackAddress(IPAddress.IPv6Loopback));
        Assert.True(LocalhostEnforcer.IsLoopbackAddress(IPAddress.Parse("127.0.0.1")));

        Assert.False(LocalhostEnforcer.IsLoopbackAddress(IPAddress.Parse("192.168.1.5")));
        Assert.False(LocalhostEnforcer.IsLoopbackAddress(IPAddress.Parse("8.8.8.8")));
        Assert.False(LocalhostEnforcer.IsLoopbackAddress(null));
    }

    [Theory]
    [InlineData("SHA256", true)]
    [InlineData("SHA-256", true)]
    [InlineData("SHA384", true)]
    [InlineData("SHA-512", true)]
    [InlineData("MD5", false)]
    [InlineData("SHA1", false)]
    [InlineData("UNKNOWN_ALGO", false)]
    public void AlgorithmValidator_EnforcesStrongAlgorithms(string algo, bool allowed)
    {
        var result = AlgorithmValidator.IsAllowed(algo);
        Assert.Equal(allowed, result);
    }

    [Theory]
    [InlineData("MD5", true)]
    [InlineData("SHA1", true)]
    [InlineData("SHA256", false)]
    public void AlgorithmValidator_DetectsWeakAlgorithms(string algo, bool isWeak)
    {
        Assert.Equal(isWeak, AlgorithmValidator.IsExplicitlyWeak(algo));
    }

    [Fact]
    public void RateLimiter_BlocksExcessRequests()
    {
        var limiter = new RateLimiter(limitPerMinute: 3);
        var key = "test-client";

        Assert.True(limiter.IsAllowed(key));
        Assert.True(limiter.IsAllowed(key));
        Assert.True(limiter.IsAllowed(key));
        Assert.False(limiter.IsAllowed(key)); // 4th request rejected
    }
}
