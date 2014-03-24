/*
 * ControlPanelForm.cs
 * 
 * The main user interface form.
 * 
 * Copyright © 2005 Mike Murphy
 * 
 */
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using EMU7800.Core;

namespace EMU7800.Win
{
    public partial class ControlPanelForm : Form
    {
        #region Fields

        readonly GameProgramLibrary _gameProgramLibrary;
        readonly GlobalSettings _globalSettings;
        readonly HostFactory _hostFactory;
        readonly ILogger _logger;

        GameProgram CurrGameProgram { get; set; }
        MachineBase M { get; set; }
        string ReadMeUri { get; set; }

        bool _quitMachineOnInputEnded;
        InputPlayer _stagedInputPlayer;
        InputRecorder _stagedInputRecorder;

        #endregion

        #region Constructors

        public ControlPanelForm()
        {
            InitializeComponent();

            _logger = EMU7800Application.Logger;
            _globalSettings = new GlobalSettings(_logger);
            _gameProgramLibrary = new GameProgramLibrary(_logger);
            _hostFactory = new HostFactory(_logger);

            var items = _hostFactory.GetRegisteredHostNames().ToArray();
            comboboxHostSelect.Items.AddRange(items);

            ResetGameTitleLabel();

            // Game Programs TabPage
            groupboxGameTitle.Text = string.Empty;
            labelGameTitle.Text = EMU7800Application.Copyright;
            StartButtonEnabled = true;
            ResumeButtonEnabled = true;

            // Settings TabPage
            comboboxHostSelect.SelectedItem = _globalSettings.HostSelect;
            if (comboboxHostSelect.SelectedIndex < 0 && comboboxHostSelect.Items.Count > 0) comboboxHostSelect.SelectedIndex = 0;
            numericupdownFrameRateAdjust.DataBindings.Add("Value", _globalSettings, "FrameRateAdjust");
            checkboxSkip7800Bios.DataBindings.Add("Checked", _globalSettings, "Skip7800BIOS");
            checkboxHSC7800.DataBindings.Add("Checked", _globalSettings, "Use7800HSC");

            // Help TabPage
            var fn = Path.Combine(Directory.GetCurrentDirectory(), "README\\README.html");
            ReadMeUri = File.Exists(fn) ? fn : null;
            linklabelReadMe.Enabled = ReadMeUri != null;
            if (ReadMeUri != null)
            {
                LinklabelReadMeClicked(this, null);
            }
            else
            {
                LogLine("README not found at: " + fn);
            }
        }

        #endregion

        #region Event Handlers

        void ControlPanelForm_Load(object sender, EventArgs e)
        {
            int width, height;
            if (!int.TryParse(_globalSettings.GetUserValue("ControlPanelFormWidth"), out width)) width = 0;
            if (!int.TryParse(_globalSettings.GetUserValue("ControlPanelFormHeight"), out height)) height = 0;

            Size = new Size(width < 500 ? 500 : width, height < 500 ? 500 : height);

            LoadComboBoxRomDirectories();
            AddRomDirectoryToComboBoxIfNecessary(_globalSettings.RomDirectory);
            InitializeKeyBindingsComboBoxes();
            Show();
            // triggers LoadTreeView(); prior Show() ensures appearance on progress bar
            comboboxRomDir.SelectedIndex = 0;
        }

        void ButtonStartClick(object sender, EventArgs e)
        {
            Start();
        }

        void ButtonResumeClick(object sender, EventArgs e)
        {
            Resume();
        }

        void ButtonQuitClick(object sender, EventArgs e)
        {
            SaveComboBoxRomDirectories();
            Application.Exit();
        }

        void ControlPanelForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _globalSettings.SetUserValue("ControlPanelFormWidth", Size.Width);
            _globalSettings.SetUserValue("ControlPanelFormHeight", Size.Height);
        }

        #endregion

        #region Helpers

        bool StartButtonEnabled
        {

            get { return buttonStart.Enabled; }
            set { buttonStart.Enabled = value && CurrGameProgram != null; }
        }

        bool ResumeButtonEnabled
        {
            get { return buttonResume.Enabled; }
            set { buttonResume.Enabled = value && M != null; }
        }

        void Start()
        {
            Hide();

            var use7800Bios = !_globalSettings.Skip7800BIOS;

            var hscManager = _globalSettings.Use7800HSC ? new HSC7800Factory(_gameProgramLibrary, _logger) : null;
            var hsc = (hscManager != null) ? hscManager.CreateHSC7800() : null;

            var nopRegisterDumping = _globalSettings.NOPRegisterDumping;

            if (_stagedInputRecorder != null)
            {
            }
            else if (_stagedInputPlayer != null)
            {
                if (_stagedInputPlayer.ValidEmuRecFile)
                {
                    CurrGameProgram = _gameProgramLibrary.GetGameProgramFromMd5(_stagedInputPlayer.MD5);
                    if (CurrGameProgram == null || CurrGameProgram.DiscoveredRomFullName == null)
                    {
                        _logger.WriteLine("Input playback file references a ROM that is unknown.");
                    }
                }
            }

            if (CurrGameProgram == null || CurrGameProgram.DiscoveredRomFullName == null)
            {
                Show();
                return;
            }

            var machineFactory = new MachineFactory(_gameProgramLibrary, hsc, _logger);
            try
            {
                M = machineFactory.BuildMachine(CurrGameProgram.DiscoveredRomFullName, use7800Bios);
            }
            catch (Exception ex)
            {
                if (Util.IsCriticalException(ex))
                    throw;
                LogLine(ex.ToString());
                MessageBox.Show(ex.ToString(), "Machine Creation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Show();
                return;
            }

            M.NOPRegisterDumping = nopRegisterDumping;

            if (_stagedInputRecorder != null)
            {
                M.InputState.InputAdvanced = _stagedInputRecorder.OnInputAdvanced;
            }
            else if (_stagedInputPlayer != null)
            {
                M.InputState.InputAdvancing = _stagedInputPlayer.OnInputAdvancing;
            }

            try
            {
                var host = _hostFactory.Create(_globalSettings.HostSelect, M);
                if (_quitMachineOnInputEnded && _stagedInputPlayer != null)
                    _stagedInputPlayer.InputEnded += delegate { host.RaiseInput(MachineInput.End, true); };
                host.Run();
            }
            catch (Exception ex)
            {
                if (Util.IsCriticalException(ex))
                    throw;
                LogLine(ex.ToString());
                MessageBox.Show(ex.ToString(), "Machine/Host Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Show();
            }

            if (_stagedInputRecorder != null)
            {
                _stagedInputRecorder.Close();
                _stagedInputRecorder.Dispose();
                _stagedInputRecorder = null;
            }
            if (_stagedInputPlayer != null)
            {
                _stagedInputPlayer.Close();
                _stagedInputPlayer.Dispose();
                _stagedInputPlayer = null;
            }

            if (hscManager != null)
                hscManager.SaveRam();

            StartButtonEnabled = true;
            ResumeButtonEnabled = true;
        }

        void Resume()
        {
            Hide();

            M.InputState.InputAdvancing = M.InputState.InputAdvanced = null;
            try
            {
                var host = _hostFactory.Create(_globalSettings.HostSelect, M);
                host.Run();
            }
            catch (Exception ex)
            {
                if (Util.IsCriticalException(ex))
                    throw;
                LogLine(ex.ToString());
                MessageBox.Show(ex.ToString(), "Host Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Show();
            }

            StartButtonEnabled = true;
            ResumeButtonEnabled = true;
        }

        void ResetGameTitleLabel()
        {
            groupboxGameTitle.Text = string.Empty;
            labelGameTitle.Text = EMU7800Application.Copyright;
            linklabelGameHelp.Visible = false;
        }

        void UpdateGameTitleLabel()
        {
            var gp = CurrGameProgram;
            if (gp == null || (gp.Title ?? string.Empty).Trim().Length <= 0)
                return;
            groupboxGameTitle.Text = "Selected Game Program";
            labelGameTitle.Text = gp.Title;
            linklabelGameHelp.Text = labelGameTitle.Text + " Game Help";
            linklabelGameHelp.Visible = gp.HelpUri != null;
        }

        #endregion
    }
}