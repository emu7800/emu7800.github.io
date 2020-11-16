// © Mike Murphy

using System.Reflection;

namespace EMU7800.D2D.Shell
{
    public partial class TitlePage
    {
        string GetVersionInfo()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var versionInfo = $"Version {version.Major}.{version.Minor} (Core 1.4) {GetBuildConfiguration()}";
            return versionInfo;
        }
    }
}
