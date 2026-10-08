using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bridge.Core.Models;

namespace Bridge.Providers;

public record WorldProviderDescriptor(
    string Id,
    string Manufacturer,
    string BrandName,
    string ModelSupport,
    string CspName,
    string MinidriverName,
    IReadOnlyList<string> KnownPkcs11Dlls,
    string Description,
    bool IsAladdinFamily = false
);

public record WorldProviderStatus(
    string Id,
    string Manufacturer,
    string BrandName,
    string ModelSupport,
    string CspName,
    bool CspInstalled,
    string MinidriverName,
    string? Pkcs11Path,
    bool Pkcs11Available,
    string Description,
    bool IsAladdinFamily,
    string Readiness, // "READY", "INSTALLED", "DRIVER_REQUIRED"
    string StatusDetails
);

public record AttachedTokenInfo(
    string DeviceName,
    string Manufacturer,
    string HardwareId,
    string Status,
    string Class,
    bool IsAladdinToken,
    string ProviderId,
    string Details
);

public static class WorldProviderCatalog
{
    public static readonly IReadOnlyList<WorldProviderDescriptor> AllProviders = new List<WorldProviderDescriptor>
    {
        // 1. Aladdin Knowledge Systems / SafeNet / Thales
        new(
            Id: "aladdin-etoken",
            Manufacturer: "Aladdin Knowledge Systems / SafeNet / Thales",
            BrandName: "Aladdin eToken",
            ModelSupport: "eToken PRO, eToken 5110, eToken 5300, eToken 4100, CardOS",
            CspName: "eToken Base Cryptographic Provider",
            MinidriverName: "eToken Minidriver",
            KnownPkcs11Dlls: new[] {
                "eTPKCS11.dll",
                "etpkcs11.dll",
                "etpkcs11_64.dll",
                @"C:\Windows\System32\eTPKCS11.dll",
                @"C:\Windows\SysWOW64\eTPKCS11.dll",
                @"C:\Program Files\SafeNet\Authentication\SAC\x64\eTPKCS11.dll",
                @"C:\Program Files (x86)\SafeNet\Authentication\SAC\eTPKCS11.dll"
            },
            Description: "Industry-standard Aladdin Knowledge Systems & SafeNet Authentication Client (SAC) hardware token family.",
            IsAladdinFamily: true
        ),

        // 2. Gemalto (Thales DIS)
        new(
            Id: "gemalto-idprime",
            Manufacturer: "Gemalto / Axalto / Thales",
            BrandName: "Gemalto IDPrime & .NET",
            ModelSupport: "IDPrime MD 830, IDPrime 940, Cryptoflex .NET, SafeNet SAC",
            CspName: "Microsoft Base Smart Card Crypto Provider",
            MinidriverName: "axaltocm.dll / basecsp.dll",
            KnownPkcs11Dlls: new[] {
                "gclib.dll",
                "IDPrimePKCS11.dll",
                "IDPrimePKCS1164.dll",
                @"C:\Program Files\Gemalto\IDGo 800 PKCS#11\IDPrimePKCS1164.dll",
                @"E:\security key\Gemalto_extracted\Windows-KB909520-v1.000-x64-ENU_extracted\axaltocm.dll"
            },
            Description: "Gemalto IDPrime smart card minidriver and PKCS#11 authentication client.",
            IsAladdinFamily: true
        ),

        // 3. Feitian / EnterSafe
        new(
            Id: "feitian-epass",
            Manufacturer: "Feitian Technologies / EnterSafe",
            BrandName: "ePass2003 / ePass1000 / ePass Auto",
            ModelSupport: "ePass2003, ePass1000 Auto, FTCOS/PK-01C",
            CspName: "EnterSafe ePass2003 CSP v1.0",
            MinidriverName: "ep2003_minidriver.dll",
            KnownPkcs11Dlls: new[] {
                "eps2003csp11.dll",
                "eps2003csp11_v2.dll",
                "ep1kentry.dll",
                @"C:\Windows\System32\eps2003csp11.dll",
                @"C:\Windows\SysWOW64\eps2003csp11.dll"
            },
            Description: "Widely deployed government and corporate DSC token supporting 2048/4096-bit RSA hardware signing."
        ),

        // 4. Watchdata ProxKey
        new(
            Id: "watchdata-proxkey",
            Manufacturer: "Watchdata Technologies",
            BrandName: "ProxKey / WD PRO",
            ModelSupport: "Watchdata ProxKey, TimeCOS, USB Key",
            CspName: "Watchdata CSP",
            MinidriverName: "WDMiniDriver.dll",
            KnownPkcs11Dlls: new[] {
                "WDPKCS.dll",
                "wdpkcs.dll",
                @"C:\Windows\System32\Watchdata\ProxKey\pkcs11.dll",
                @"C:\Windows\SysWOW64\Watchdata\ProxKey\pkcs11.dll"
            },
            Description: "Watchdata ProxKey token middleware commonly used across national e-filing portals."
        ),

        // 5. Longmai / mToken
        new(
            Id: "longmai-mtoken",
            Manufacturer: "Century Longmai Technology",
            BrandName: "mToken CryptoID",
            ModelSupport: "mToken CryptoID, GM3000, FIPS 140-3 Token",
            CspName: "mToken CryptoID CSP",
            MinidriverName: "mTokenMiniDrv.dll",
            KnownPkcs11Dlls: new[] {
                "CryptoIDA_pkcs11.dll",
                "cryptoida_pkcs11.dll",
                "mtokind.dll",
                @"E:\security key\mToken CryptoID Driver\CryptoID_Setup_extracted\app\CryptoIDA_pkcs11.dll",
                @"E:\security key\mToken CryptoID Driver (FIPS 140-3)\mToken_CryptoID\mToken_CryptoID_extracted\app\cryptoida_pkcs11.dll"
            },
            Description: "High-security Longmai mToken supporting FIPS 140-2 / 140-3 hardware container cryptography."
        ),

        // 6. StarKey / AET SafeSign
        new(
            Id: "starkey-safesign",
            Manufacturer: "AET Europe / StarKey",
            BrandName: "StarKey 400 / SafeSign",
            ModelSupport: "StarKey 400, SafeSign Identity Client, CardOS",
            CspName: "SafeSign CSP",
            MinidriverName: "aetmdrv.dll",
            KnownPkcs11Dlls: new[] {
                "aetpkss1.dll",
                "aetpkss164.dll",
                @"C:\Windows\System32\aetpkss1.dll",
                @"C:\Program Files\AET Europe B.V\SafeSign\Active Client\aetpkss1.dll",
                @"E:\security key\StarKey 400 (For All Windows) 64_bit\SafeSign_64bit_extracted\PackageContents\ExtractedFiles\SourceDir\aetpkss1.dll"
            },
            Description: "AET Europe SafeSign Identity Client powering StarKey 400 hardware security tokens."
        ),

        // 7. Yubico YubiKey
        new(
            Id: "yubico-yubikey",
            Manufacturer: "Yubico",
            BrandName: "YubiKey 5 / 4 / FIPS",
            ModelSupport: "YubiKey 5 Series, YubiKey FIPS, PIV Applet",
            CspName: "Microsoft Base Smart Card Crypto Provider",
            MinidriverName: "Yubico64.dll / Yubico.dll",
            KnownPkcs11Dlls: new[] {
                "ykcs11.dll",
                "libykcs11.dll",
                @"C:\Program Files\Yubico\Yubico PIV Tool\bin\ykcs11.dll"
            },
            Description: "Hardware authentication and PIV smart card signing container."
        ),

        // 8. OpenSC Multi-Vendor
        new(
            Id: "opensc-universal",
            Manufacturer: "OpenSC Project",
            BrandName: "OpenSC Universal PKI",
            ModelSupport: "Generic PKCS#15, ISO 7816-4, OpenPGP, PIV, Nitrokey",
            CspName: "OpenSC CSP",
            MinidriverName: "opensc-minidriver.dll",
            KnownPkcs11Dlls: new[] {
                "opensc-pkcs11.dll",
                "onepin-opensc-pkcs11.dll",
                @"C:\Program Files\OpenSC Project\OpenSC\pkcs11\opensc-pkcs11.dll"
            },
            Description: "Universal open-source smart-card provider supporting over 50+ smart card profiles."
        )
    };

    public static bool IsCspInstalled(string cspName)
    {
        if (string.IsNullOrWhiteSpace(cspName)) return false;
        if (!OperatingSystem.IsWindows()) return false;

        try
        {
            using var base64 = Microsoft.Win32.RegistryKey.OpenBaseKey(
                Microsoft.Win32.RegistryHive.LocalMachine,
                Microsoft.Win32.RegistryView.Registry64);
            using var sub64 = base64.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography\Defaults\Provider\" + cspName);
            if (sub64 != null) return true;

            using var base32 = Microsoft.Win32.RegistryKey.OpenBaseKey(
                Microsoft.Win32.RegistryHive.LocalMachine,
                Microsoft.Win32.RegistryView.Registry32);
            using var sub32 = base32.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography\Defaults\Provider\" + cspName);
            return sub32 != null;
        }
        catch
        {
            return false;
        }
    }

    public static string? FindInstalledPkcs11Library(WorldProviderDescriptor descriptor)
    {
        foreach (var dll in descriptor.KnownPkcs11Dlls)
        {
            if (File.Exists(dll)) return dll;

            var sys32 = Path.Combine(Environment.SystemDirectory, Path.GetFileName(dll));
            if (File.Exists(sys32)) return sys32;

            var wow64 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SysWOW64", Path.GetFileName(dll));
            if (File.Exists(wow64)) return wow64;
        }

        return null;
    }

    public static WorldProviderStatus GetStatus(WorldProviderDescriptor desc)
    {
        var cspInstalled = IsCspInstalled(desc.CspName);
        var pkcs11Path = FindInstalledPkcs11Library(desc);
        var p11Available = !string.IsNullOrEmpty(pkcs11Path);

        string readiness;
        string details;

        if (desc.IsAladdinFamily && (cspInstalled || p11Available))
        {
            readiness = "READY";
            details = cspInstalled
                ? "Aladdin eToken Base Cryptographic Provider is active in Windows Crypto subsystem. Token is ready for hardware signing."
                : $"Aladdin PKCS#11 module found at {pkcs11Path}.";
        }
        else if (cspInstalled && p11Available)
        {
            readiness = "READY";
            details = $"Full support: Windows CSP registered and PKCS#11 module present at {pkcs11Path}.";
        }
        else if (cspInstalled)
        {
            readiness = "READY";
            details = $"Windows CSP '{desc.CspName}' is registered and active in local cryptography store.";
        }
        else if (p11Available)
        {
            readiness = "READY";
            details = $"PKCS#11 driver module located at {pkcs11Path}.";
        }
        else
        {
            readiness = "DRIVER_REQUIRED";
            details = $"Vendor driver/middleware for {desc.BrandName} not detected. Connect token or install vendor package.";
        }

        return new WorldProviderStatus(
            Id: desc.Id,
            Manufacturer: desc.Manufacturer,
            BrandName: desc.BrandName,
            ModelSupport: desc.ModelSupport,
            CspName: desc.CspName,
            CspInstalled: cspInstalled,
            MinidriverName: desc.MinidriverName,
            Pkcs11Path: pkcs11Path,
            Pkcs11Available: p11Available,
            Description: desc.Description,
            IsAladdinFamily: desc.IsAladdinFamily,
            Readiness: readiness,
            StatusDetails: details
        );
    }

    public static IReadOnlyList<WorldProviderStatus> GetAllStatuses()
    {
        return AllProviders.Select(GetStatus).ToList();
    }

    public static IReadOnlyList<AttachedTokenInfo> GetAttachedTokens()
    {
        var list = new List<AttachedTokenInfo>();
        if (!OperatingSystem.IsWindows()) return list;

        try
        {
            using var baseKey = Microsoft.Win32.RegistryKey.OpenBaseKey(
                Microsoft.Win32.RegistryHive.LocalMachine,
                Microsoft.Win32.RegistryView.Registry64);
            using var usbKey = baseKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\USB");

            if (usbKey != null)
            {
                foreach (var vidSubName in usbKey.GetSubKeyNames())
                {
                    bool isAladdin = vidSubName.Contains("VID_0529", StringComparison.OrdinalIgnoreCase);
                    bool isFeitian = vidSubName.Contains("VID_096E", StringComparison.OrdinalIgnoreCase);
                    bool isWatchdata = vidSubName.Contains("VID_2003", StringComparison.OrdinalIgnoreCase) || vidSubName.Contains("VID_04B9", StringComparison.OrdinalIgnoreCase);
                    bool isGemalto = vidSubName.Contains("VID_08E6", StringComparison.OrdinalIgnoreCase);
                    bool isYubico = vidSubName.Contains("VID_1050", StringComparison.OrdinalIgnoreCase);

                    if (isAladdin || isFeitian || isWatchdata || isGemalto || isYubico)
                    {
                        using var vidKey = usbKey.OpenSubKey(vidSubName);
                        if (vidKey == null) continue;

                        foreach (var instName in vidKey.GetSubKeyNames())
                        {
                            using var instKey = vidKey.OpenSubKey(instName);
                            if (instKey == null) continue;

                            var service = instKey.GetValue("Service")?.ToString() ?? "";
                            var desc = instKey.GetValue("DeviceDesc")?.ToString() ?? "";
                            var mfg = instKey.GetValue("Mfg")?.ToString() ?? "";
                            var loc = instKey.GetValue("LocationInformation")?.ToString() ?? "";
                            var hwId = (instKey.GetValue("HardwareID") as string[])?.FirstOrDefault() ?? $"USB\\{vidSubName}";

                            if (desc.Contains(";")) desc = desc.Split(';').Last();
                            if (mfg.Contains(";")) mfg = mfg.Split(';').Last();

                            // An active device has active service assigned
                            if (isAladdin && service.Equals("AKSUP", StringComparison.OrdinalIgnoreCase))
                            {
                                var cleanName = (string.IsNullOrWhiteSpace(desc) || desc.Equals("USB Token", StringComparison.OrdinalIgnoreCase))
                                    ? "Aladdin eToken Pro (USB Token)"
                                    : desc;

                                list.Add(new AttachedTokenInfo(
                                    DeviceName: cleanName,
                                    Manufacturer: string.IsNullOrWhiteSpace(mfg) ? "Aladdin Knowledge Systems Ltd." : mfg,
                                    HardwareId: hwId,
                                    Status: "ATTACHED_AND_ACTIVE",
                                    Class: "USB Security Token",
                                    IsAladdinToken: true,
                                    ProviderId: "aladdin-etoken",
                                    Details: $"Connected at {loc}. Active Driver: {service} (Aladdin USB Protocol Running)."
                                ));
                            }
                            else if (isFeitian && !string.IsNullOrEmpty(loc))
                            {
                                list.Add(new AttachedTokenInfo(
                                    DeviceName: string.IsNullOrWhiteSpace(desc) ? "Feitian ePass Token" : desc,
                                    Manufacturer: string.IsNullOrWhiteSpace(mfg) ? "Feitian Technologies" : mfg,
                                    HardwareId: hwId,
                                    Status: "ATTACHED_AND_ACTIVE",
                                    Class: "USB Security Token",
                                    IsAladdinToken: false,
                                    ProviderId: "feitian-epass",
                                    Details: $"Connected at {loc}."
                                ));
                            }
                            else if (isWatchdata && !string.IsNullOrEmpty(loc))
                            {
                                list.Add(new AttachedTokenInfo(
                                    DeviceName: string.IsNullOrWhiteSpace(desc) ? "Watchdata ProxKey Token" : desc,
                                    Manufacturer: string.IsNullOrWhiteSpace(mfg) ? "Watchdata Technologies" : mfg,
                                    HardwareId: hwId,
                                    Status: "ATTACHED_AND_ACTIVE",
                                    Class: "USB Security Token",
                                    IsAladdinToken: false,
                                    ProviderId: "watchdata-proxkey",
                                    Details: $"Connected at {loc}."
                                ));
                            }
                            else if (isGemalto && !string.IsNullOrEmpty(loc))
                            {
                                list.Add(new AttachedTokenInfo(
                                    DeviceName: string.IsNullOrWhiteSpace(desc) ? "Gemalto IDPrime Token" : desc,
                                    Manufacturer: string.IsNullOrWhiteSpace(mfg) ? "Gemalto / Thales" : mfg,
                                    HardwareId: hwId,
                                    Status: "ATTACHED_AND_ACTIVE",
                                    Class: "USB Security Token",
                                    IsAladdinToken: true,
                                    ProviderId: "gemalto-idprime",
                                    Details: $"Connected at {loc}."
                                ));
                            }
                            else if (isYubico && !string.IsNullOrEmpty(loc))
                            {
                                list.Add(new AttachedTokenInfo(
                                    DeviceName: string.IsNullOrWhiteSpace(desc) ? "YubiKey Security Key" : desc,
                                    Manufacturer: string.IsNullOrWhiteSpace(mfg) ? "Yubico Inc." : mfg,
                                    HardwareId: hwId,
                                    Status: "ATTACHED_AND_ACTIVE",
                                    Class: "USB Security Token",
                                    IsAladdinToken: false,
                                    ProviderId: "yubico-yubikey",
                                    Details: $"Connected at {loc}."
                                ));
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // Graceful fallback
        }

        return list;
    }
}
