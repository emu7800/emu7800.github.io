using System;
using System.Runtime.InteropServices;

namespace EMU7800.Shell;

public static class VersionInfo
{
    public static string EMU7800 => nameof(EMU7800);
    public static string ExecutableName => EMU7800 + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)? ".exe" : string.Empty);

    public static string AssemblyVersion => GetData(nameof(AssemblyVersion));
    public static string Author => GetData(nameof(Author));
    public static string Description => GetData(nameof(Description));

    static string GetData(string name) => AppContext.GetData(name) is string data ? data : string.Empty;
}
