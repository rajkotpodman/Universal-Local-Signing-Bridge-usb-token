using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Bridge.Providers;

public record SmartCardReaderState(
    string ReaderName,
    bool CardPresent,
    string? AtrHex,
    string? KnownCardModel,
    string Status
);

public static class PcscNative
{
    public const uint SCARD_SCOPE_USER = 0;
    public const uint SCARD_SCOPE_SYSTEM = 2;
    public const uint SCARD_STATE_UNAWARE = 0x00000000;
    public const uint SCARD_STATE_PRESENT = 0x00000020;
    public const uint SCARD_STATE_EMPTY = 0x00000010;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct SCARD_READERSTATE
    {
        public string szReader;
        public IntPtr pvUserData;
        public uint dwCurrentState;
        public uint dwEventState;
        public uint cbAtr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 36)]
        public byte[] rgbAtr;
    }

    [DllImport("winscard.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int SCardEstablishContext(uint dwScope, IntPtr pvReserved1, IntPtr pvReserved2, out IntPtr phContext);

    [DllImport("winscard.dll", SetLastError = true)]
    public static extern int SCardReleaseContext(IntPtr hContext);

    [DllImport("winscard.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int SCardListReaders(IntPtr hContext, string? mszGroups, byte[]? mszReaders, ref int pcchReaders);

    [DllImport("winscard.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int SCardGetStatusChange(IntPtr hContext, uint dwTimeout, [In, Out] SCARD_READERSTATE[] rgReaderStates, int cReaders);
}

public class PcscHardwareMonitor
{
    private static readonly Dictionary<string, string> KnownAtrDatabase = new(StringComparer.OrdinalIgnoreCase)
    {
        // Aladdin / SafeNet
        ["3BF2180002C10A31FE58C80975"] = "Aladdin eToken PRO (Java Card)",
        ["3BD5180081313A7D8073C021C0"] = "SafeNet eToken 5110 (FIPS 140-2)",
        ["3BD5180081313A7D8073C021D0"] = "SafeNet eToken 5300 (Touch/Presence)",
        ["3BF81800008131FE450073C8401300900092"] = "Gemalto IDPrime MD 830",
        ["3B7F96000080318065B0850300EF120FFE829000"] = "Gemalto IDPrime 940",

        // Feitian
        ["3B6D000080318065B0831100C883009000"] = "Feitian ePass2003 DSC Token",
        ["3B04A2131091"] = "Feitian ePass1000 Auto",

        // Watchdata
        ["3B690000005744010203049000"] = "Watchdata ProxKey / WD PRO",

        // Longmai
        ["3B9F958131FE451F03893101020064051EB2"] = "Longmai mToken CryptoID",

        // StarKey / SafeSign
        ["3B759400006202020101"] = "StarKey 400 (SafeSign CardOS)",

        // Yubico
        ["3BFD1300008131FE158073C021C057597562694B657940"] = "YubiKey 5 Series (PIV Smart Card)"
    };

    public static IReadOnlyList<SmartCardReaderState> ScanReaders()
    {
        var results = new List<SmartCardReaderState>();
        if (!OperatingSystem.IsWindows()) return results;

        int rc = PcscNative.SCardEstablishContext(PcscNative.SCARD_SCOPE_USER, IntPtr.Zero, IntPtr.Zero, out var hContext);
        if (rc != 0 || hContext == IntPtr.Zero)
        {
            rc = PcscNative.SCardEstablishContext(PcscNative.SCARD_SCOPE_SYSTEM, IntPtr.Zero, IntPtr.Zero, out hContext);
            if (rc != 0 || hContext == IntPtr.Zero) return results;
        }

        try
        {
            int pcchReaders = 0;
            rc = PcscNative.SCardListReaders(hContext, null, null, ref pcchReaders);
            if (rc != 0 || pcchReaders <= 1) return results;

            byte[] readersBuffer = new byte[pcchReaders];
            rc = PcscNative.SCardListReaders(hContext, null, readersBuffer, ref pcchReaders);
            if (rc != 0) return results;

            string raw = Encoding.ASCII.GetString(readersBuffer, 0, pcchReaders);
            string[] readerNames = raw.Split('\0', StringSplitOptions.RemoveEmptyEntries);

            if (readerNames.Length == 0) return results;

            var states = new PcscNative.SCARD_READERSTATE[readerNames.Length];
            for (int i = 0; i < readerNames.Length; i++)
            {
                states[i] = new PcscNative.SCARD_READERSTATE
                {
                    szReader = readerNames[i],
                    dwCurrentState = PcscNative.SCARD_STATE_UNAWARE,
                    rgbAtr = new byte[36]
                };
            }

            PcscNative.SCardGetStatusChange(hContext, 100, states, states.Length);

            for (int i = 0; i < states.Length; i++)
            {
                var s = states[i];
                bool present = (s.dwEventState & PcscNative.SCARD_STATE_PRESENT) != 0;
                string? atrHex = null;
                string? knownModel = null;

                if (present && s.cbAtr > 0)
                {
                    var atrBytes = new byte[s.cbAtr];
                    Array.Copy(s.rgbAtr, atrBytes, s.cbAtr);
                    atrHex = Convert.ToHexString(atrBytes);

                    // Identify from ATR database
                    foreach (var (pattern, model) in KnownAtrDatabase)
                    {
                        if (atrHex.StartsWith(pattern, StringComparison.OrdinalIgnoreCase) ||
                            pattern.StartsWith(atrHex, StringComparison.OrdinalIgnoreCase))
                        {
                            knownModel = model;
                            break;
                        }
                    }

                    if (knownModel == null && atrHex.Contains("73C021", StringComparison.OrdinalIgnoreCase))
                    {
                        knownModel = "Generic Smart Card Token (ISO 7816)";
                    }
                }

                results.Add(new SmartCardReaderState(
                    ReaderName: s.szReader,
                    CardPresent: present,
                    AtrHex: atrHex,
                    KnownCardModel: knownModel ?? (present ? "Active Security Card" : null),
                    Status: present ? "CARD_INSERTED" : "READER_EMPTY"
                ));
            }
        }
        catch
        {
            // Soft fallback
        }
        finally
        {
            PcscNative.SCardReleaseContext(hContext);
        }

        return results;
    }
}
