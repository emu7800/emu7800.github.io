using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace EMU7800.WP.View
{
    public partial class TitlePage
    {
        public TitlePage()
        {
            InitializeComponent();

            var currentApp = (App)Application.Current;
#if PROFILE
            const string note = "(PROFILE BUILD)";
#elif DEBUG
            const string note = "(DEBUG BUILD)";
#else
            var note = (currentApp.BogoMIPS < currentApp.BestResultsBogoMIPS)
                ? string.Format("(Best Results at {0})", currentApp.BestResultsBogoMIPS)
                : string.Empty;
#endif
            BogoMipsTextRun.Text = string.Format("Device BogoMIPS: {0} {1}", currentApp.BogoMIPS, note);

            var version = GetVersion();
            if (version != null)
            {
                VersionTextRun.Text = version;
            }
        }

        void PlayAtariButton_Tap(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("/View/GameProgramSelectPage.xaml", UriKind.Relative);
            try
            {
                NavigationService.Navigate(uri);
            }
            catch (InvalidOperationException)
            {
                // navigation already in progress.
            }
        }

        void AboutButton_Tap(object sender, GestureEventArgs e)
        {
            var uri = new Uri("/View/AboutPage.xaml", UriKind.Relative);
            try
            {
                NavigationService.Navigate(uri);
            }
            catch (InvalidOperationException)
            {
                // navigation already in progress.
            }
        }

        static string GetVersion()
        {
            var xdoc = XDocument.Load("WMAppManifest.xml");
            var version = xdoc.Descendants("App")
                .Attributes("Version")
                    .Select(e => e.Value)
                        .FirstOrDefault();

            if (version == null)
                return null;

            var versionSplit = version.Split('.');
            return string.Format("Version {0}.{1} WP7", versionSplit[0], versionSplit[1]);
        }
    }
}