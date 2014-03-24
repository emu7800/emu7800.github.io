using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EMU7800.Core;

namespace EMU7800.Win
{
    partial class ControlPanelForm
    {
        #region Fields

        readonly RomFileAccessor _romFileAccessor = new RomFileAccessor();
        TreeNode _treenodeTitle, _treenodeUnknown;
        bool _doubleClickReady;
        int _romFileCount;
        Dictionary<string, TreeNode> _manuIndex, _yearIndex, _rareIndex, _machIndex, _contIndex;
#if DEBUG
        Dictionary<string, TreeNode> _cartIndex;
#endif

        bool _requestWorkerToStop;
        long _workerStartTick;

        #endregion

        #region Event Handlers

        void ComboboxRomDirSelectedValueChanged(object sender, EventArgs e)
        {
            _requestWorkerToStop = false;
            _workerStartTick = Environment.TickCount;
            if (!backgroundworkerTreeViewLoader.IsBusy)
            {
                BackgroundworkerTreeViewLoaderProgressStart();
                backgroundworkerTreeViewLoader.RunWorkerAsync(comboboxRomDir.Text.Trim());
            }
        }

        void ButtonBrowseClick(object sender, EventArgs e)
        {
            var ofdRomSelect = new OpenFileDialog
            {
                Title = "Select ROM File",
                Filter = "ROM Archive (*.zip)|*.zip|ROMs (*.bin)|*.bin|A78 ROMs (*.a78)|*.a78|A26 ROMs (*.a26)|*.a26",
                FilterIndex = 1
            };

            var romDir = comboboxRomDir.Text.Trim();
            ofdRomSelect.InitialDirectory = Directory.Exists(romDir) ? romDir : _globalSettings.RomDirectory;

            if (ofdRomSelect.ShowDialog() != DialogResult.OK)
                return;
            Application.DoEvents();
            GameSelectByFileName(ofdRomSelect.FileName);
        }

        void TreeviewRomListKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Return)
                return;
            e.Handled = true;
            SetSelection(treeviewRomList.SelectedNode);
            TreeviewRomListNodeMouseDoubleClick(sender, null);
        }

        void TreeviewRomListNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            SetSelection(e.Node);
        }

        void TreeviewRomListNodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (_doubleClickReady && CurrGameProgram.DiscoveredRomFullName != null)
            {
                ButtonStartClick(sender, e);
            }
            _doubleClickReady = false;
        }

        void buttonStop_Click(object sender, EventArgs e)
        {
            buttonStop.Enabled = false;
            _requestWorkerToStop = true;
            LogLine("ROM directory scan requested stopped by user");
        }

        #endregion

        #region BackgroundWorkerTreeViewLoader

        void BackgroundworkerTreeViewLoaderDoWork(object sender, DoWorkEventArgs e)
        {
            _romFileCount = 0;
            var worker = (BackgroundWorker)sender;
            var newRomDir = (string)e.Argument;

            var task1 = Task.Factory.StartNew(() =>
            {
                var fileList = new List<string>();
                try
                {
                    fileList.AddRange(_romFileAccessor.GetRomFullNames(newRomDir));
                }
                catch (IOException)
                {
                    e.Cancel = true;
                }
                return fileList;
            });
            WaitOnTaskUntilCompleteOrCancelled(task1, worker);
            if (worker.CancellationPending || _requestWorkerToStop || task1.Result == null)
            {
                e.Cancel = true;
                return;
            }

            var romFiles = task1.Result;
            worker.ReportProgress(-1, romFiles.Count);

            foreach (var romFileName in romFiles)
            {
                var fullName = romFileName;
                var task2 = Task.Factory.StartNew(() =>
                {
                    var gp = _gameProgramLibrary.TryRecognizeRom(fullName);
                    if (gp != null)
                        _romFileCount++;
                    return gp;
                });
                WaitOnTaskUntilCompleteOrCancelled(task2, worker);
                if (worker.CancellationPending || _requestWorkerToStop)
                {
                    e.Cancel = true;
                    break;
                }

                worker.ReportProgress(1, task2.Result);
            }

            e.Result = newRomDir;
        }

        void WaitOnTaskUntilCompleteOrCancelled(Task task, BackgroundWorker worker)
        {
            while (true)
            {
                task.Wait(100);
                if (_requestWorkerToStop || task.IsCompleted || worker.CancellationPending)
                    break;
                worker.ReportProgress(1, null);
            }
        }

        void BackgroundworkerTreeViewLoaderProgressStart()
        {
            Cursor = Cursors.AppStarting;

            buttonStart.Enabled = false;
            buttonResume.Enabled = false;

            comboboxRomDir.Enabled = false;
            buttonBrowse.Enabled = false;

            progressbarRomCount.Minimum = 0;
            progressbarRomCount.Value = 0;
            progressbarRomCount.Visible = false;
            buttonStop.Visible = false;

            const string message = "Examining ROM Directory...";
            labelRomCount.Text = message;
            LogLine(message);
            labelRomCount.Visible = true;

            treeviewRomList.BeginUpdate();
            treeviewRomList.Nodes.Clear();

            _treenodeTitle = new TreeNode("Title", 0, 1);
            treeviewRomList.Nodes.Add(_treenodeTitle);

            _manuIndex = AddTreeSubRoot(treeviewRomList, "Manufacturer", new[] {
                "Absolute",
                "Activision",
                "Apollo",
                "Atari",
                "Avalon Hill",
                "Bitcorp",
                "Bomb",
                "CBS Electronics",
                "CCE",
                "Coleco",
                "CommaVid",
                "Data Age",
                "Epyx",
                "Exus",
                "Froggo",
                "HomeVision",
                "Hozer Video Games",
                "Imagic",
                "ITT Family Games",
                "Konami",
                "Mattel",
                "Milton Bradley",
                "Mystique",
                "Mythicon",
                "Panda",
                "Parker Bros",
                "Playaround",
                "Sears",
                "Sega",
                "Spectravideo",
                "Starsoft",
                "Suntek",
                "Telegames",
                "Telesys",
                "Tigervision",
                "US Games",
                "Video Gems",
                "Xonox",
                "Zellers",
                "Zimag",
                "20th Century Fox"});

            var al = new List<string>();
            for (var i = 1977; i <= DateTime.Today.Year; i++)
            {
                al.Add(i.ToString());
            }
            _yearIndex = AddTreeSubRoot(treeviewRomList, "Year", al);

            _rareIndex = AddTreeSubRoot(treeviewRomList, "Rarity", new[] {
                "Common", "Uncommon", "Scarce", "Rare", "Extremely Rare",
                "Unbelievably Rare", "Prototype", "Unreleased Prototype", "Homebrew"});

            _machIndex = AddTreeSubRoot(treeviewRomList, "Machine Type", new[] {
                GetMachineTypeString(MachineType.A2600NTSC, true),
                GetMachineTypeString(MachineType.A2600PAL, true),
                GetMachineTypeString(MachineType.A7800NTSC, true),
                GetMachineTypeString(MachineType.A7800PAL, true)});

            _contIndex = AddTreeSubRoot(treeviewRomList, "Controller", new[] {
                "Joystick", "ProLineJoystick", "Paddles", "Driving", "Keypad", "Lightgun", "BoosterGrip"});
#if DEBUG
            var cartList = Enum.GetNames(typeof(CartType));
            _cartIndex = AddTreeSubRoot(treeviewRomList, "Cartridge Type", cartList);
#endif
            _treenodeUnknown = new TreeNode("Unknown", 0, 1);
            treeviewRomList.Nodes.Add(_treenodeUnknown);
        }

        void BackgroundworkerTreeViewLoaderProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage < 0)
            {
                progressbarRomCount.Minimum = 0;
                progressbarRomCount.Value = 0;
                progressbarRomCount.Maximum = (int)e.UserState;
                return;
            }

            var progressBarMode = (Environment.TickCount - _workerStartTick > 1000);
            progressbarRomCount.Visible = buttonStop.Visible = buttonStop.Enabled = progressBarMode;
            labelRomCount.Visible = !progressBarMode;

            var gp = e.UserState as GameProgram;
            if (gp == null)
                return;

            progressbarRomCount.Value = _romFileCount;

            var tn = new TreeNode(BuildTitle(gp.Title, gp.Manufacturer, GetMachineTypeString(gp.MachineType, false), gp.Year), 2, 2) { Tag = gp };
            _treenodeTitle.Nodes.Add(tn);

            AddTreeNode(_manuIndex, gp, gp.Manufacturer, gp.Title, GetMachineTypeString(gp.MachineType, false), gp.Year);
            AddTreeNode(_yearIndex, gp, gp.Year, gp.Title, gp.Manufacturer, GetMachineTypeString(gp.MachineType, false));
            AddTreeNode(_rareIndex, gp, gp.Rarity, gp.Title, gp.Manufacturer, GetMachineTypeString(gp.MachineType, false), gp.Year);
            AddTreeNode(_machIndex, gp, GetMachineTypeString(gp.MachineType, true), gp.Title, gp.Manufacturer, GetMachineTypeString(gp.MachineType, false), gp.Year);
            AddTreeNode(_contIndex, gp, gp.LController.ToString(), gp.Title, gp.Manufacturer, GetMachineTypeString(gp.MachineType, false), gp.Year);
#if DEBUG
            AddTreeNode(_cartIndex, gp, gp.CartType.ToString(), gp.Title, gp.Manufacturer, GetMachineTypeString(gp.MachineType, false), gp.Year);
#endif
        }

        void BackgroundworkerTreeViewLoaderRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                labelRomCount.Text = "ROM directory does not exist.";
                treeviewRomList.Nodes.Clear();
            }
            else
            {
                for (var i = 0; i < treeviewRomList.Nodes.Count; )
                {
                    var c = treeviewRomList.Nodes[i];
                    if (PruneTree(c))
                        c.Remove();
                    else
                        i++;
                }
                var message = string.Format("{0} ROM file{1} recognized", _romFileCount, (_romFileCount != 1 ? "s" : ""));
                labelRomCount.Text = message;
                LogLine(message);
                treeviewRomList.Sorted = true;

                _globalSettings.RomDirectory = (string)e.Result;
            }

            treeviewRomList.EndUpdate();
            treeviewRomList.Update();

            progressbarRomCount.Visible = false;
            buttonStop.Visible = false;
            labelRomCount.Visible = true;

            comboboxRomDir.Enabled = true;
            buttonBrowse.Enabled = true;

            Cursor = Cursors.Arrow;
        }

        #endregion

        #region Helpers

        void GameSelectByFileName(string fn)
        {
            var fi = new FileInfo(fn);
            string directoryName, fullName;
            try
            {
                directoryName = fi.DirectoryName;
                fullName = fi.FullName;
            }
            catch (IOException)
            {
                return;
            }

            AddRomDirectoryToComboBoxIfNecessary(directoryName);

            // This will reload the TreeView via cmbROMDir_SelectedValueChanged
            comboboxRomDir.SelectedItem = directoryName;

            if (!_romFileAccessor.IsValidRomFileName(fn))
                return;

            while (backgroundworkerTreeViewLoader.IsBusy)
            {
                Application.DoEvents();
            }

            var selected = SelectTitle(fullName);
            if (selected)
                return;

            var gp = _gameProgramLibrary.GetGameProgramFromFullName(fullName);
            if (gp == null)
            {
                var md5 = _gameProgramLibrary.ComputeMD5Digest(fullName);
                if (string.IsNullOrWhiteSpace(md5))
                {
                    LogLine("Unable to compute MD5 for {0}", fi.FullName);
                    return;
                }
                gp = new GameProgram(md5);
            }
            gp.DiscoveredRomFullName = fi.FullName;

            LogLine("Unrecognized ROM: prefilled GameSettings fields as follows:\n{0}", gp.ToString());

            if (!_gameProgramLibrary.ContainsKey(gp.MD5))
                _gameProgramLibrary.Add(gp.MD5, gp);

            CurrGameProgram = gp;
            UpdateGameTitleLabel();

            MessageBox.Show("Use Console Tab to specify custom attributes.", "ROM Not Recognized", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        void AddRomDirectoryToComboBoxIfNecessary(string romDir)
        {
            romDir = romDir.Trim();
            if (comboboxRomDir.Items.Cast<string>().Any(item => item.Equals(romDir, StringComparison.OrdinalIgnoreCase)))
                return;
            comboboxRomDir.Items.Add(romDir);
        }

        void LoadComboBoxRomDirectories()
        {
            var romDirectories = _globalSettings.GetUserValue("ROMDirectories");
            foreach (var romDir in romDirectories.Split(';')
                .Where(romDir => !romDir.Trim().Length.Equals(0)))
            {
                AddRomDirectoryToComboBoxIfNecessary(romDir);
            }
        }

        void SaveComboBoxRomDirectories()
        {
            var sb = new StringBuilder();
            if (comboboxRomDir.SelectedItem != null)
            {
                // this will cause the subsequent foreach to add a dup, but it will be filtered on the next load.
                // this keeps the currently selected romdir at the top of the list.
                sb.Append(comboboxRomDir.SelectedItem);
            }
            foreach (string item in comboboxRomDir.Items)
            {
                if (!sb.Length.Equals(0))
                    sb.Append(';');
                sb.Append(item);
            }
            _globalSettings.SetUserValue("ROMDirectories", sb.ToString());
        }

        void SetSelection(TreeNode n)
        {
            _doubleClickReady = false;
            if (n == null || n.Tag == null)
                return;

            _doubleClickReady = true;

            var gp = n.Tag as GameProgram;
            if (gp != CurrGameProgram && gp != null)
            {
                CurrGameProgram = gp;
                LogLine(CurrGameProgram.ToString());
            }

            UpdateGameTitleLabel();
            StartButtonEnabled = true;
            ResumeButtonEnabled = false;
        }

        // Remove TreeNodes that have no dependencies
        static bool PruneTree(TreeNode p)
        {
            var score = 0;
            if (p.Nodes.Count > 0)
            {
                for (var i = 0; i < p.Nodes.Count; )
                {
                    var c = p.Nodes[i];
                    if (PruneTree(c))
                    {
                        c.Remove();
                    }
                    else
                    {
                        score++;
                        i++;
                    }
                }
            }
            return (score == 0 && p.Tag == null);
        }

        bool SelectTitle(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return false;

            var md5 = _gameProgramLibrary.ComputeMD5Digest(fullName);
            if (md5 == null)
                return false;

            foreach (var tn in _treenodeTitle.Nodes.Cast<TreeNode>()
                .Where(tn => tn.Tag != null && md5 == ((GameProgram) tn.Tag).MD5))
            {
                treeviewRomList.SelectedNode = tn;
                SetSelection(tn);
                return true;
            }
            foreach (var tn in _treenodeUnknown.Nodes.Cast<TreeNode>()
                .Where(tn => tn.Tag != null && md5 == ((GameProgram) tn.Tag).MD5))
            {
                treeviewRomList.SelectedNode = tn;
                return true;
            }
            return false;
        }

        static Dictionary<string, TreeNode> AddTreeSubRoot(TreeView root, string label, IEnumerable<string> subList)
        {
            var tnparent = new TreeNode(label, 0, 1);
            root.Nodes.Add(tnparent);
            var index = new Dictionary<string, TreeNode>();
            TreeNode tn;
            foreach (var s in subList)
            {
                tn = new TreeNode(s, 0, 1);
                tnparent.Nodes.Add(tn);
                index.Add(s, tn);
            }
            tn = new TreeNode("Other", 0, 1);
            tnparent.Nodes.Add(tn);
            index.Add("Other", tn);
            return index;
        }

        static void AddTreeNode(IDictionary<string, TreeNode> index, GameProgram gs, string key, params string[] titlebits)
        {
            var tn = new TreeNode(BuildTitle(titlebits), 2, 2) { Tag = gs };
            if (key == null || !index.ContainsKey(key))
                key = "Other";
            index[key].Nodes.Add(tn);
        }

        static string BuildTitle(params string[] titlebits)
        {
            var title = new StringBuilder();
            for (var i = 0; i < titlebits.Length; i++)
            {
                if (string.IsNullOrEmpty(titlebits[i])) continue;
                if (i > 0)
                    title.Append(", ");
                title.Append(titlebits[i]);
            }
            return title.ToString();
        }

        static string GetMachineTypeString(MachineType machineType, bool verbose)
        {
            var mts = string.Empty;
            switch (machineType)
            {
                case MachineType.A2600NTSC:
                    mts = verbose ? "VCS (2600) NTSC (N.American)" : "VCS";
                    break;
                case MachineType.A2600PAL:
                    mts = verbose ? "VCS (2600) PAL (European)" : "VCS PAL";
                    break;
                case MachineType.A7800NTSC:
                    mts = verbose ? "ProSystem (7800) NTSC (N.American)" : "ProSystem";
                    break;
                case MachineType.A7800PAL:
                    mts = verbose ? "ProSystem (7800) PAL (European)" : "ProSystem PAL";
                    break;
            }
            return mts;
        }

        #endregion
    }
}