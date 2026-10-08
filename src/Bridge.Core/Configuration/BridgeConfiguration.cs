namespace Bridge.Core.Configuration;

public class BridgeConfiguration
{
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 8080;
    public string[] AllowedOrigins { get; set; } = new[]
    {
        "http://localhost",
        "http://127.0.0.1",
        "http://localhost:5173",
        "http://127.0.0.1:5173",
        "http://localhost:8080",
        "http://127.0.0.1:8080"
    };
    public string ProviderMode { get; set; } = "MOCK"; // MOCK, WINDOWS, PKCS11
    public bool MockMode { get; set; } = true;
    public string LogLevel { get; set; } = "Information";
    public int SessionLifetimeSeconds { get; set; } = 3600;
    public int RateLimitPerMinute { get; set; } = 60;
    public long MaxRequestSizeBytes { get; set; } = 5 * 1024 * 1024; // 5 MB
    public bool EnableTls { get; set; } = false;
    public string Pkcs11LibraryPath { get; set; } = "";
}
