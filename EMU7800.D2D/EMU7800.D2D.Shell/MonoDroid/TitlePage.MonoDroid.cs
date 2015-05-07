// © Mike Murphy

using EMU7800.MonoDroid;

namespace EMU7800.D2D.Shell
{
    public partial class TitlePage
    {
        static string GetVersionInfo()
        {
            var pi = MainActivity.App.ApplicationContext.PackageManager.GetPackageInfo(MainActivity.App.ApplicationContext.PackageName, 0);
            var versionInfo = string.Format("Version {0} (Core 1.4) {1}", pi.VersionName, GetBuildConfiguration());
            return versionInfo;
        }
    }
}