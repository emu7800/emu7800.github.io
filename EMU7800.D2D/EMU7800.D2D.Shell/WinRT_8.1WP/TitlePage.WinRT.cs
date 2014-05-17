// © Mike Murphy

namespace EMU7800.D2D.Shell
{
    public partial class TitlePage
    {
        string GetVersionInfo()
        {
            var version = Windows.ApplicationModel.Package.Current.Id.Version;
            var versionInfo = string.Format("Version {0}.{1} (Core 1.4) {2}", version.Major, version.Minor, GetBuildConfiguration());
            return versionInfo;
        }
    }
}
