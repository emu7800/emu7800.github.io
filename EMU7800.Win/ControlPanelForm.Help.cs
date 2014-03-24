using System.Windows.Forms;

namespace EMU7800.Win
{
    partial class ControlPanelForm
    {
        #region Event Handlers

        void LinklabelReadMeClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            webbrowserHelp.Navigate(ReadMeUri);
        }

        void LinklabelGameHelpLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linklabelGameHelp.Enabled = false;
            webbrowserHelp.Navigate(CurrGameProgram.HelpUri);
        }

        void WebbrowserHelpDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            linklabelGameHelp.Enabled = true;
        }

        #endregion
    }
}
