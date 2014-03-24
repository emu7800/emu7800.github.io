namespace EMU7800.Win
{
    partial class ControlPanelForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlPanelForm));
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonResume = new System.Windows.Forms.Button();
            this.buttonQuit = new System.Windows.Forms.Button();
            this.tabcontrolControlPanel = new System.Windows.Forms.TabControl();
            this.tabpageGamePrograms = new System.Windows.Forms.TabPage();
            this.groupboxGameTitle = new System.Windows.Forms.GroupBox();
            this.labelGameTitle = new System.Windows.Forms.Label();
            this.progressbarRomCount = new System.Windows.Forms.ProgressBar();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.comboboxRomDir = new System.Windows.Forms.ComboBox();
            this.labelRomDir = new System.Windows.Forms.Label();
            this.labelRomCount = new System.Windows.Forms.Label();
            this.treeviewRomList = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.tabpageSettings = new System.Windows.Forms.TabPage();
            this.groupbox7800SpecificSettings = new System.Windows.Forms.GroupBox();
            this.checkboxHSC7800 = new System.Windows.Forms.CheckBox();
            this.checkboxSkip7800Bios = new System.Windows.Forms.CheckBox();
            this.numericupdownFrameRateAdjust = new System.Windows.Forms.NumericUpDown();
            this.comboboxHostSelect = new System.Windows.Forms.ComboBox();
            this.buttonLoadMachineState = new System.Windows.Forms.Button();
            this.labelFrameRateAdjust = new System.Windows.Forms.Label();
            this.labelHostSelect = new System.Windows.Forms.Label();
            this.tabpageKeyBindings = new System.Windows.Forms.TabPage();
            this.buttonKbCancel = new System.Windows.Forms.Button();
            this.labelKbKey = new System.Windows.Forms.Label();
            this.labelKbHostInput = new System.Windows.Forms.Label();
            this.buttonKbBindAction = new System.Windows.Forms.Button();
            this.comboboxKbKey = new System.Windows.Forms.ComboBox();
            this.comboboxKbHostInput = new System.Windows.Forms.ComboBox();
            this.tabpageConsole = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textboxInput = new System.Windows.Forms.TextBox();
            this.textboxOutput = new System.Windows.Forms.TextBox();
            this.tabpageHelp = new System.Windows.Forms.TabPage();
            this.linklabelGameHelp = new System.Windows.Forms.LinkLabel();
            this.linklabelReadMe = new System.Windows.Forms.LinkLabel();
            this.webbrowserHelp = new System.Windows.Forms.WebBrowser();
            this.backgroundworkerTreeViewLoader = new System.ComponentModel.BackgroundWorker();
            this.buttonStop = new System.Windows.Forms.Button();
            this.tabcontrolControlPanel.SuspendLayout();
            this.tabpageGamePrograms.SuspendLayout();
            this.groupboxGameTitle.SuspendLayout();
            this.tabpageSettings.SuspendLayout();
            this.groupbox7800SpecificSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericupdownFrameRateAdjust)).BeginInit();
            this.tabpageKeyBindings.SuspendLayout();
            this.tabpageConsole.SuspendLayout();
            this.tabpageHelp.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonStart
            // 
            this.buttonStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonStart.Location = new System.Drawing.Point(4, 436);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(75, 23);
            this.buttonStart.TabIndex = 0;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.ButtonStartClick);
            // 
            // buttonResume
            // 
            this.buttonResume.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonResume.Location = new System.Drawing.Point(85, 436);
            this.buttonResume.Name = "buttonResume";
            this.buttonResume.Size = new System.Drawing.Size(75, 23);
            this.buttonResume.TabIndex = 1;
            this.buttonResume.Text = "Resume";
            this.buttonResume.UseVisualStyleBackColor = true;
            this.buttonResume.Click += new System.EventHandler(this.ButtonResumeClick);
            // 
            // buttonQuit
            // 
            this.buttonQuit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonQuit.Location = new System.Drawing.Point(166, 436);
            this.buttonQuit.Name = "buttonQuit";
            this.buttonQuit.Size = new System.Drawing.Size(75, 23);
            this.buttonQuit.TabIndex = 2;
            this.buttonQuit.Text = "Quit";
            this.buttonQuit.UseVisualStyleBackColor = true;
            this.buttonQuit.Click += new System.EventHandler(this.ButtonQuitClick);
            // 
            // tabcontrolControlPanel
            // 
            this.tabcontrolControlPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabcontrolControlPanel.Controls.Add(this.tabpageGamePrograms);
            this.tabcontrolControlPanel.Controls.Add(this.tabpageSettings);
            this.tabcontrolControlPanel.Controls.Add(this.tabpageKeyBindings);
            this.tabcontrolControlPanel.Controls.Add(this.tabpageConsole);
            this.tabcontrolControlPanel.Controls.Add(this.tabpageHelp);
            this.tabcontrolControlPanel.Location = new System.Drawing.Point(4, 3);
            this.tabcontrolControlPanel.Name = "tabcontrolControlPanel";
            this.tabcontrolControlPanel.SelectedIndex = 0;
            this.tabcontrolControlPanel.Size = new System.Drawing.Size(486, 427);
            this.tabcontrolControlPanel.TabIndex = 3;
            // 
            // tabpageGamePrograms
            // 
            this.tabpageGamePrograms.Controls.Add(this.groupboxGameTitle);
            this.tabpageGamePrograms.Controls.Add(this.progressbarRomCount);
            this.tabpageGamePrograms.Controls.Add(this.buttonBrowse);
            this.tabpageGamePrograms.Controls.Add(this.comboboxRomDir);
            this.tabpageGamePrograms.Controls.Add(this.labelRomDir);
            this.tabpageGamePrograms.Controls.Add(this.labelRomCount);
            this.tabpageGamePrograms.Controls.Add(this.treeviewRomList);
            this.tabpageGamePrograms.Location = new System.Drawing.Point(4, 22);
            this.tabpageGamePrograms.Name = "tabpageGamePrograms";
            this.tabpageGamePrograms.Size = new System.Drawing.Size(478, 401);
            this.tabpageGamePrograms.TabIndex = 0;
            this.tabpageGamePrograms.Text = "Game Programs";
            this.tabpageGamePrograms.UseVisualStyleBackColor = true;
            // 
            // groupboxGameTitle
            // 
            this.groupboxGameTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupboxGameTitle.Controls.Add(this.labelGameTitle);
            this.groupboxGameTitle.Location = new System.Drawing.Point(7, 46);
            this.groupboxGameTitle.Name = "groupboxGameTitle";
            this.groupboxGameTitle.Size = new System.Drawing.Size(464, 47);
            this.groupboxGameTitle.TabIndex = 7;
            this.groupboxGameTitle.TabStop = false;
            this.groupboxGameTitle.Text = "Selected Game Program";
            // 
            // labelGameTitle
            // 
            this.labelGameTitle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelGameTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.labelGameTitle.Location = new System.Drawing.Point(29, 16);
            this.labelGameTitle.Name = "labelGameTitle";
            this.labelGameTitle.Size = new System.Drawing.Size(420, 22);
            this.labelGameTitle.TabIndex = 0;
            this.labelGameTitle.Text = "Text";
            this.labelGameTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // progressbarRomCount
            // 
            this.progressbarRomCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressbarRomCount.Location = new System.Drawing.Point(7, 381);
            this.progressbarRomCount.Name = "progressbarRomCount";
            this.progressbarRomCount.Size = new System.Drawing.Size(464, 17);
            this.progressbarRomCount.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressbarRomCount.TabIndex = 6;
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowse.Location = new System.Drawing.Point(443, 19);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(28, 21);
            this.buttonBrowse.TabIndex = 5;
            this.buttonBrowse.Text = "...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.ButtonBrowseClick);
            // 
            // comboboxRomDir
            // 
            this.comboboxRomDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboboxRomDir.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboboxRomDir.FormattingEnabled = true;
            this.comboboxRomDir.Location = new System.Drawing.Point(7, 19);
            this.comboboxRomDir.Name = "comboboxRomDir";
            this.comboboxRomDir.Size = new System.Drawing.Size(430, 21);
            this.comboboxRomDir.TabIndex = 4;
            this.comboboxRomDir.SelectedValueChanged += new System.EventHandler(this.ComboboxRomDirSelectedValueChanged);
            // 
            // labelRomDir
            // 
            this.labelRomDir.AutoSize = true;
            this.labelRomDir.Location = new System.Drawing.Point(4, 3);
            this.labelRomDir.Name = "labelRomDir";
            this.labelRomDir.Size = new System.Drawing.Size(114, 13);
            this.labelRomDir.TabIndex = 3;
            this.labelRomDir.Text = "Current ROM Directory";
            // 
            // labelRomCount
            // 
            this.labelRomCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelRomCount.AutoSize = true;
            this.labelRomCount.Location = new System.Drawing.Point(7, 388);
            this.labelRomCount.Name = "labelRomCount";
            this.labelRomCount.Size = new System.Drawing.Size(0, 13);
            this.labelRomCount.TabIndex = 1;
            // 
            // treeviewRomList
            // 
            this.treeviewRomList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeviewRomList.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeviewRomList.ImageIndex = 0;
            this.treeviewRomList.ImageList = this.imageList1;
            this.treeviewRomList.Location = new System.Drawing.Point(7, 99);
            this.treeviewRomList.Name = "treeviewRomList";
            this.treeviewRomList.SelectedImageIndex = 0;
            this.treeviewRomList.Size = new System.Drawing.Size(464, 276);
            this.treeviewRomList.TabIndex = 0;
            this.treeviewRomList.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TreeviewRomListNodeMouseClick);
            this.treeviewRomList.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TreeviewRomListNodeMouseDoubleClick);
            this.treeviewRomList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TreeviewRomListKeyDown);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "CLSDFOLD.BMP");
            this.imageList1.Images.SetKeyName(1, "OPENFOLD.BMP");
            this.imageList1.Images.SetKeyName(2, "ROM.BMP");
            // 
            // tabpageSettings
            // 
            this.tabpageSettings.Controls.Add(this.groupbox7800SpecificSettings);
            this.tabpageSettings.Controls.Add(this.numericupdownFrameRateAdjust);
            this.tabpageSettings.Controls.Add(this.comboboxHostSelect);
            this.tabpageSettings.Controls.Add(this.buttonLoadMachineState);
            this.tabpageSettings.Controls.Add(this.labelFrameRateAdjust);
            this.tabpageSettings.Controls.Add(this.labelHostSelect);
            this.tabpageSettings.Location = new System.Drawing.Point(4, 22);
            this.tabpageSettings.Name = "tabpageSettings";
            this.tabpageSettings.Size = new System.Drawing.Size(478, 401);
            this.tabpageSettings.TabIndex = 1;
            this.tabpageSettings.Text = "Settings";
            this.tabpageSettings.UseVisualStyleBackColor = true;
            // 
            // groupbox7800SpecificSettings
            // 
            this.groupbox7800SpecificSettings.Controls.Add(this.checkboxHSC7800);
            this.groupbox7800SpecificSettings.Controls.Add(this.checkboxSkip7800Bios);
            this.groupbox7800SpecificSettings.Location = new System.Drawing.Point(20, 99);
            this.groupbox7800SpecificSettings.Name = "groupbox7800SpecificSettings";
            this.groupbox7800SpecificSettings.Size = new System.Drawing.Size(160, 71);
            this.groupbox7800SpecificSettings.TabIndex = 6;
            this.groupbox7800SpecificSettings.TabStop = false;
            this.groupbox7800SpecificSettings.Text = "7800 Specific";
            // 
            // checkboxHSC7800
            // 
            this.checkboxHSC7800.AutoSize = true;
            this.checkboxHSC7800.Location = new System.Drawing.Point(15, 42);
            this.checkboxHSC7800.Name = "checkboxHSC7800";
            this.checkboxHSC7800.Size = new System.Drawing.Size(123, 17);
            this.checkboxHSC7800.TabIndex = 1;
            this.checkboxHSC7800.Text = "Use High Score Cart";
            this.checkboxHSC7800.UseVisualStyleBackColor = true;
            // 
            // checkboxSkip7800Bios
            // 
            this.checkboxSkip7800Bios.AutoSize = true;
            this.checkboxSkip7800Bios.Location = new System.Drawing.Point(15, 19);
            this.checkboxSkip7800Bios.Name = "checkboxSkip7800Bios";
            this.checkboxSkip7800Bios.Size = new System.Drawing.Size(112, 17);
            this.checkboxSkip7800Bios.TabIndex = 0;
            this.checkboxSkip7800Bios.Text = "Skip BIOS Startup";
            this.checkboxSkip7800Bios.UseVisualStyleBackColor = true;
            // 
            // numericupdownFrameRateAdjust
            // 
            this.numericupdownFrameRateAdjust.Location = new System.Drawing.Point(198, 65);
            this.numericupdownFrameRateAdjust.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericupdownFrameRateAdjust.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            -2147483648});
            this.numericupdownFrameRateAdjust.Name = "numericupdownFrameRateAdjust";
            this.numericupdownFrameRateAdjust.ReadOnly = true;
            this.numericupdownFrameRateAdjust.Size = new System.Drawing.Size(47, 20);
            this.numericupdownFrameRateAdjust.TabIndex = 2;
            this.numericupdownFrameRateAdjust.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // comboboxHostSelect
            // 
            this.comboboxHostSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboboxHostSelect.FormattingEnabled = true;
            this.comboboxHostSelect.Location = new System.Drawing.Point(16, 29);
            this.comboboxHostSelect.Name = "comboboxHostSelect";
            this.comboboxHostSelect.Size = new System.Drawing.Size(229, 21);
            this.comboboxHostSelect.TabIndex = 0;
            this.comboboxHostSelect.SelectedIndexChanged += new System.EventHandler(this.ComboboxHostSelectSelectedIndexChanged);
            // 
            // buttonLoadMachineState
            // 
            this.buttonLoadMachineState.Location = new System.Drawing.Point(20, 193);
            this.buttonLoadMachineState.Name = "buttonLoadMachineState";
            this.buttonLoadMachineState.Size = new System.Drawing.Size(164, 23);
            this.buttonLoadMachineState.TabIndex = 7;
            this.buttonLoadMachineState.Text = "Load Machine State";
            this.buttonLoadMachineState.UseVisualStyleBackColor = true;
            this.buttonLoadMachineState.Click += new System.EventHandler(this.ButtonLoadMachineStateClick);
            // 
            // labelFrameRateAdjust
            // 
            this.labelFrameRateAdjust.AutoSize = true;
            this.labelFrameRateAdjust.Location = new System.Drawing.Point(17, 65);
            this.labelFrameRateAdjust.Name = "labelFrameRateAdjust";
            this.labelFrameRateAdjust.Size = new System.Drawing.Size(120, 13);
            this.labelFrameRateAdjust.TabIndex = 1;
            this.labelFrameRateAdjust.Text = "Frame Rate Adjust (+/-):";
            // 
            // labelHostSelect
            // 
            this.labelHostSelect.AutoSize = true;
            this.labelHostSelect.Location = new System.Drawing.Point(15, 13);
            this.labelHostSelect.Name = "labelHostSelect";
            this.labelHostSelect.Size = new System.Drawing.Size(65, 13);
            this.labelHostSelect.TabIndex = 0;
            this.labelHostSelect.Text = "Host Select:";
            // 
            // tabpageKeyBindings
            // 
            this.tabpageKeyBindings.Controls.Add(this.buttonKbCancel);
            this.tabpageKeyBindings.Controls.Add(this.labelKbKey);
            this.tabpageKeyBindings.Controls.Add(this.labelKbHostInput);
            this.tabpageKeyBindings.Controls.Add(this.buttonKbBindAction);
            this.tabpageKeyBindings.Controls.Add(this.comboboxKbKey);
            this.tabpageKeyBindings.Controls.Add(this.comboboxKbHostInput);
            this.tabpageKeyBindings.Location = new System.Drawing.Point(4, 22);
            this.tabpageKeyBindings.Name = "tabpageKeyBindings";
            this.tabpageKeyBindings.Size = new System.Drawing.Size(478, 401);
            this.tabpageKeyBindings.TabIndex = 4;
            this.tabpageKeyBindings.Text = "Key Bindings";
            this.tabpageKeyBindings.UseVisualStyleBackColor = true;
            // 
            // buttonKbCancel
            // 
            this.buttonKbCancel.Enabled = false;
            this.buttonKbCancel.Location = new System.Drawing.Point(183, 218);
            this.buttonKbCancel.Name = "buttonKbCancel";
            this.buttonKbCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonKbCancel.TabIndex = 5;
            this.buttonKbCancel.Text = "Cancel";
            this.buttonKbCancel.UseVisualStyleBackColor = true;
            this.buttonKbCancel.Click += new System.EventHandler(this.ButtonKbCancelClick);
            // 
            // labelKbKey
            // 
            this.labelKbKey.AutoSize = true;
            this.labelKbKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelKbKey.Location = new System.Drawing.Point(30, 140);
            this.labelKbKey.Name = "labelKbKey";
            this.labelKbKey.Size = new System.Drawing.Size(136, 13);
            this.labelKbKey.TabIndex = 4;
            this.labelKbKey.Text = "<Bound DirectX Input Key>";
            // 
            // labelKbHostInput
            // 
            this.labelKbHostInput.AutoSize = true;
            this.labelKbHostInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelKbHostInput.Location = new System.Drawing.Point(30, 69);
            this.labelKbHostInput.Name = "labelKbHostInput";
            this.labelKbHostInput.Size = new System.Drawing.Size(56, 13);
            this.labelKbHostInput.TabIndex = 3;
            this.labelKbHostInput.Text = "Host Input";
            // 
            // buttonKbBindAction
            // 
            this.buttonKbBindAction.Location = new System.Drawing.Point(102, 218);
            this.buttonKbBindAction.Name = "buttonKbBindAction";
            this.buttonKbBindAction.Size = new System.Drawing.Size(75, 23);
            this.buttonKbBindAction.TabIndex = 2;
            this.buttonKbBindAction.Text = "<Unbind>";
            this.buttonKbBindAction.UseVisualStyleBackColor = true;
            this.buttonKbBindAction.Click += new System.EventHandler(this.ButtonKbBindActionClick);
            // 
            // comboboxKbKey
            // 
            this.comboboxKbKey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboboxKbKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboboxKbKey.FormattingEnabled = true;
            this.comboboxKbKey.Location = new System.Drawing.Point(33, 156);
            this.comboboxKbKey.MaxDropDownItems = 25;
            this.comboboxKbKey.Name = "comboboxKbKey";
            this.comboboxKbKey.Size = new System.Drawing.Size(225, 28);
            this.comboboxKbKey.TabIndex = 1;
            this.comboboxKbKey.SelectedIndexChanged += new System.EventHandler(this.ComboboxKbKeySelectedIndexChanged);
            // 
            // comboboxKbHostInput
            // 
            this.comboboxKbHostInput.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboboxKbHostInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboboxKbHostInput.FormattingEnabled = true;
            this.comboboxKbHostInput.ItemHeight = 20;
            this.comboboxKbHostInput.Location = new System.Drawing.Point(33, 85);
            this.comboboxKbHostInput.MaxDropDownItems = 25;
            this.comboboxKbHostInput.Name = "comboboxKbHostInput";
            this.comboboxKbHostInput.Size = new System.Drawing.Size(225, 28);
            this.comboboxKbHostInput.TabIndex = 0;
            this.comboboxKbHostInput.SelectedIndexChanged += new System.EventHandler(this.ComboboxKbHostInputSelectedIndexChanged);
            // 
            // tabpageConsole
            // 
            this.tabpageConsole.Controls.Add(this.label2);
            this.tabpageConsole.Controls.Add(this.label1);
            this.tabpageConsole.Controls.Add(this.textboxInput);
            this.tabpageConsole.Controls.Add(this.textboxOutput);
            this.tabpageConsole.Location = new System.Drawing.Point(4, 22);
            this.tabpageConsole.Name = "tabpageConsole";
            this.tabpageConsole.Size = new System.Drawing.Size(478, 401);
            this.tabpageConsole.TabIndex = 2;
            this.tabpageConsole.Text = "Console";
            this.tabpageConsole.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 358);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Command-line Input";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Output Message Log";
            // 
            // textboxInput
            // 
            this.textboxInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textboxInput.BackColor = System.Drawing.Color.Black;
            this.textboxInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textboxInput.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textboxInput.ForeColor = System.Drawing.Color.Lime;
            this.textboxInput.Location = new System.Drawing.Point(13, 375);
            this.textboxInput.Name = "textboxInput";
            this.textboxInput.Size = new System.Drawing.Size(459, 20);
            this.textboxInput.TabIndex = 0;
            this.textboxInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextboxInputKeyPress);
            // 
            // textboxOutput
            // 
            this.textboxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textboxOutput.BackColor = System.Drawing.Color.Black;
            this.textboxOutput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textboxOutput.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textboxOutput.ForeColor = System.Drawing.Color.Lime;
            this.textboxOutput.Location = new System.Drawing.Point(13, 29);
            this.textboxOutput.Multiline = true;
            this.textboxOutput.Name = "textboxOutput";
            this.textboxOutput.ReadOnly = true;
            this.textboxOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textboxOutput.Size = new System.Drawing.Size(459, 326);
            this.textboxOutput.TabIndex = 0;
            this.textboxOutput.TabStop = false;
            this.textboxOutput.WordWrap = false;
            this.textboxOutput.VisibleChanged += new System.EventHandler(this.TextboxOutputVisibleChanged);
            // 
            // tabpageHelp
            // 
            this.tabpageHelp.Controls.Add(this.linklabelGameHelp);
            this.tabpageHelp.Controls.Add(this.linklabelReadMe);
            this.tabpageHelp.Controls.Add(this.webbrowserHelp);
            this.tabpageHelp.Location = new System.Drawing.Point(4, 22);
            this.tabpageHelp.Name = "tabpageHelp";
            this.tabpageHelp.Size = new System.Drawing.Size(478, 401);
            this.tabpageHelp.TabIndex = 3;
            this.tabpageHelp.Text = "Help";
            this.tabpageHelp.UseVisualStyleBackColor = true;
            // 
            // linklabelGameHelp
            // 
            this.linklabelGameHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linklabelGameHelp.AutoSize = true;
            this.linklabelGameHelp.Location = new System.Drawing.Point(74, 378);
            this.linklabelGameHelp.Name = "linklabelGameHelp";
            this.linklabelGameHelp.Size = new System.Drawing.Size(105, 13);
            this.linklabelGameHelp.TabIndex = 1;
            this.linklabelGameHelp.TabStop = true;
            this.linklabelGameHelp.Text = "Selected Game Help";
            this.linklabelGameHelp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.linklabelGameHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinklabelGameHelpLinkClicked);
            // 
            // linklabelReadMe
            // 
            this.linklabelReadMe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linklabelReadMe.AutoSize = true;
            this.linklabelReadMe.Location = new System.Drawing.Point(3, 378);
            this.linklabelReadMe.Name = "linklabelReadMe";
            this.linklabelReadMe.Size = new System.Drawing.Size(53, 13);
            this.linklabelReadMe.TabIndex = 0;
            this.linklabelReadMe.TabStop = true;
            this.linklabelReadMe.Text = "README";
            this.linklabelReadMe.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.linklabelReadMe.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinklabelReadMeClicked);
            // 
            // webbrowserHelp
            // 
            this.webbrowserHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webbrowserHelp.Location = new System.Drawing.Point(-4, 3);
            this.webbrowserHelp.MinimumSize = new System.Drawing.Size(20, 20);
            this.webbrowserHelp.Name = "webbrowserHelp";
            this.webbrowserHelp.ScriptErrorsSuppressed = true;
            this.webbrowserHelp.Size = new System.Drawing.Size(476, 372);
            this.webbrowserHelp.TabIndex = 0;
            this.webbrowserHelp.TabStop = false;
            this.webbrowserHelp.Url = new System.Uri("", System.UriKind.Relative);
            this.webbrowserHelp.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.WebbrowserHelpDocumentCompleted);
            // 
            // backgroundworkerTreeViewLoader
            // 
            this.backgroundworkerTreeViewLoader.WorkerReportsProgress = true;
            this.backgroundworkerTreeViewLoader.WorkerSupportsCancellation = true;
            this.backgroundworkerTreeViewLoader.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundworkerTreeViewLoaderDoWork);
            this.backgroundworkerTreeViewLoader.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BackgroundworkerTreeViewLoaderProgressChanged);
            this.backgroundworkerTreeViewLoader.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundworkerTreeViewLoaderRunWorkerCompleted);
            // 
            // buttonStop
            // 
            this.buttonStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.buttonStop.Location = new System.Drawing.Point(390, 438);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(99, 23);
            this.buttonStop.TabIndex = 0;
            this.buttonStop.TabStop = false;
            this.buttonStop.Text = "Stop Scanning";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // ControlPanelForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(492, 473);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.tabcontrolControlPanel);
            this.Controls.Add(this.buttonQuit);
            this.Controls.Add(this.buttonResume);
            this.Controls.Add(this.buttonStart);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(500, 500);
            this.Name = "ControlPanelForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "EMU7800 Control Panel";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ControlPanelForm_FormClosing);
            this.Load += new System.EventHandler(this.ControlPanelForm_Load);
            this.tabcontrolControlPanel.ResumeLayout(false);
            this.tabpageGamePrograms.ResumeLayout(false);
            this.tabpageGamePrograms.PerformLayout();
            this.groupboxGameTitle.ResumeLayout(false);
            this.tabpageSettings.ResumeLayout(false);
            this.tabpageSettings.PerformLayout();
            this.groupbox7800SpecificSettings.ResumeLayout(false);
            this.groupbox7800SpecificSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericupdownFrameRateAdjust)).EndInit();
            this.tabpageKeyBindings.ResumeLayout(false);
            this.tabpageKeyBindings.PerformLayout();
            this.tabpageConsole.ResumeLayout(false);
            this.tabpageConsole.PerformLayout();
            this.tabpageHelp.ResumeLayout(false);
            this.tabpageHelp.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonResume;
        private System.Windows.Forms.Button buttonQuit;
        private System.Windows.Forms.TabControl tabcontrolControlPanel;
        private System.Windows.Forms.TabPage tabpageGamePrograms;
        private System.Windows.Forms.TreeView treeviewRomList;
        private System.Windows.Forms.Label labelRomCount;
        private System.Windows.Forms.Label labelRomDir;
        private System.Windows.Forms.ComboBox comboboxRomDir;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.TabPage tabpageSettings;
        private System.Windows.Forms.TabPage tabpageConsole;
        private System.Windows.Forms.Label labelFrameRateAdjust;
        private System.Windows.Forms.Label labelHostSelect;
        private System.Windows.Forms.Button buttonLoadMachineState;
        private System.Windows.Forms.ComboBox comboboxHostSelect;
        private System.Windows.Forms.NumericUpDown numericupdownFrameRateAdjust;
        private System.Windows.Forms.CheckBox checkboxHSC7800;
        private System.Windows.Forms.CheckBox checkboxSkip7800Bios;
        private System.Windows.Forms.GroupBox groupbox7800SpecificSettings;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textboxInput;
        private System.Windows.Forms.TextBox textboxOutput;
        private System.Windows.Forms.ProgressBar progressbarRomCount;
        private System.Windows.Forms.GroupBox groupboxGameTitle;
        private System.Windows.Forms.Label labelGameTitle;
        private System.Windows.Forms.TabPage tabpageHelp;
        private System.Windows.Forms.WebBrowser webbrowserHelp;
        private System.Windows.Forms.LinkLabel linklabelReadMe;
        private System.Windows.Forms.LinkLabel linklabelGameHelp;
        private System.Windows.Forms.Button buttonStart;
        private System.ComponentModel.BackgroundWorker backgroundworkerTreeViewLoader;
        private System.Windows.Forms.TabPage tabpageKeyBindings;
        private System.Windows.Forms.Label labelKbKey;
        private System.Windows.Forms.Label labelKbHostInput;
        private System.Windows.Forms.Button buttonKbBindAction;
        private System.Windows.Forms.ComboBox comboboxKbKey;
        private System.Windows.Forms.ComboBox comboboxKbHostInput;
        private System.Windows.Forms.Button buttonKbCancel;
        private System.Windows.Forms.Button buttonStop;
    }
}