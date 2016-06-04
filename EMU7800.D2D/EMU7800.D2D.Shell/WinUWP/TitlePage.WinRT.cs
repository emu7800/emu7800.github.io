// © Mike Murphy

namespace EMU7800.D2D.Shell
{
    public partial class TitlePage
    {
        static string GetVersionInfo()
        {
            var version = Windows.ApplicationModel.Package.Current.Id.Version;
            var buildConfig = GetBuildConfiguration();
            var versionInfo = $"Version {version.Major}.{version.Minor} (Core 1.4) {buildConfig}";
            return versionInfo;
        }
    }
}
