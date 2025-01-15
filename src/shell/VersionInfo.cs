using System;

namespace EMU7800;

public static class VersionInfo
{
    public static string AssemblyVersion => GetData(nameof(AssemblyVersion));
    public static string Author => GetData(nameof(Author));
    public static string Description => GetData(nameof(Description));

    static string GetData(string name) => AppContext.GetData(name) is string data ? data : string.Empty;
}
