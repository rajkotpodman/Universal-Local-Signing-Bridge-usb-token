using System.Net;

namespace Bridge.Security;

public static class LocalhostEnforcer
{
    public static bool IsLoopbackAddress(IPAddress? address)
    {
        if (address == null) return false;

        if (IPAddress.IsLoopback(address))
            return true;

        // IPv6 mapped IPv4 loopback check
        if (address.IsIPv4MappedToIPv6 && IPAddress.IsLoopback(address.MapToIPv4()))
            return true;

        return false;
    }
}
