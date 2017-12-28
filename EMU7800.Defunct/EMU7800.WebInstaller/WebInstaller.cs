using System;
using System.Deployment.Application;
using System.Windows.Forms;

namespace EMU7800.WebInstaller
{
    public partial class WebInstaller : Form
    {
        const string ClickOnceUrl = @"http://emu7800.sourceforge.net/EMU7800.Win.application";
        InPlaceHostingManager _iphm;

        public WebInstaller()
        {
            InitializeComponent();
        }

        void buttonInstall_Click(object sender, EventArgs e)
        {
            InstallApplication();
        }

        void InstallApplication()
        {
            buttonInstall.Enabled = false;
            buttonInstall.Text = "Installing";

            try
            {
                _iphm = new InPlaceHostingManager(new Uri(ClickOnceUrl), false);
            }
            catch (PlatformNotSupportedException)
            {
                const string message = "Unable to install the application, requires Windows XP or higher.";
                ShowMessageBoxError(message);
                return;
            }

            _iphm.GetManifestCompleted += _iphm_GetManifestCompleted;
            _iphm.GetManifestAsync();
        }

        void _iphm_GetManifestCompleted(object sender, GetManifestCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ShowMessageBoxError("Unable to install the application, could not download manifest.");
                return;
            }

            // Verify this application can be installed.
            try
            {
                // the true parameter allows InPlaceHostingManager to grant the permissions requested in the application manifest.
                _iphm.AssertApplicationRequirements(true);
            }
            catch (Exception ex)
            {
                ShowMessageBoxError("Unable to install the application, an error occurred while verifying: " + ex.Message);
                return;
            }

            progressbarDownloadProgress.Visible = true;

            _iphm.DownloadProgressChanged += _iphm_DownloadProgressChanged;
            _iphm.DownloadApplicationCompleted += _iphm_DownloadApplicationCompleted;
            _iphm.DownloadApplicationAsync();
        }

        void _iphm_DownloadApplicationCompleted(object sender, DownloadApplicationCompletedEventArgs e)
        {
            progressbarDownloadProgress.Visible = false;

            if (e.Error != null)
            {
                ShowMessageBoxError(e.Error.Message);
                return;
            }

            MessageBox.Show("EMU7800 installation complete!", "Installation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            buttonInstall.Text = "Installed";
        }

        void _iphm_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressbarDownloadProgress.Value = e.ProgressPercentage;
        }

        void ShowMessageBoxError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            buttonInstall.Enabled = true;
            buttonInstall.Text = "Install";
        }
    }
}
