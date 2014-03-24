using System;
using System.IO;
using System.Windows.Forms;
using EMU7800.Core;

namespace EMU7800.Win
{
    partial class ControlPanelForm
    {
        #region Event Handlers

        void ButtonLoadMachineStateClick(object sender, EventArgs e)
        {
            var openfiledialogFileSelect = new OpenFileDialog
            {
                Title = "Select EMU Machine State",
                Filter = "Machine States (*.emu)|*.emu",
                FilterIndex = 1,
                InitialDirectory = _globalSettings.OutputDirectory
            };

            if (openfiledialogFileSelect.ShowDialog() != DialogResult.OK)
                return;

            StartButtonEnabled = false;
            ResumeButtonEnabled = false;
            CurrGameProgram = null;
            try
            {
                M = Util.DeserializeMachineFromFile(openfiledialogFileSelect.FileName);
            }
            catch (Emu7800SerializationException ex)
            {
                LogLine("Error restoring machine state: " + ex);
                MessageBox.Show("File does not contain valid EMU7800 game state.", "Bad File", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            catch (IOException ex)
            {
                LogLine("Error restoring machine state: " + ex);
                MessageBox.Show("Unable to retrieve EMU7800 game state.", "Unexpected Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            ResumeButtonEnabled = true;
            ResetGameTitleLabel();
            LogLine("machine state restored");
        }

        void ComboboxHostSelectSelectedIndexChanged(object sender, EventArgs e)
        {
            _globalSettings.HostSelect = (string)comboboxHostSelect.SelectedItem;
        }

        #endregion
    }
}
