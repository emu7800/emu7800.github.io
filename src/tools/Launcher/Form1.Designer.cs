namespace EMU7800.Launcher
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.textBoxRomPath = new System.Windows.Forms.TextBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.buttonStart = new System.Windows.Forms.Button();
            this.comboBoxMachineType = new System.Windows.Forms.ComboBox();
            this.comboBoxCartType = new System.Windows.Forms.ComboBox();
            this.comboBoxLeftController = new System.Windows.Forms.ComboBox();
            this.comboBoxRightController = new System.Windows.Forms.ComboBox();
            this.labelSize = new System.Windows.Forms.Label();
            this.textBoxMd5Key = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBoxRomPath
            // 
            this.textBoxRomPath.Location = new System.Drawing.Point(22, 41);
            this.textBoxRomPath.Name = "textBoxRomPath";
            this.textBoxRomPath.ReadOnly = true;
            this.textBoxRomPath.Size = new System.Drawing.Size(549, 20);
            this.textBoxRomPath.TabIndex = 0;
            this.textBoxRomPath.TabStop = false;
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Location = new System.Drawing.Point(22, 12);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(101, 23);
            this.buttonBrowse.TabIndex = 0;
            this.buttonBrowse.Text = "Select ROM File";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(496, 163);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(75, 23);
            this.buttonStart.TabIndex = 6;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // comboBoxMachineType
            // 
            this.comboBoxMachineType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMachineType.FormattingEnabled = true;
            this.comboBoxMachineType.Location = new System.Drawing.Point(22, 97);
            this.comboBoxMachineType.Name = "comboBoxMachineType";
            this.comboBoxMachineType.Size = new System.Drawing.Size(89, 21);
            this.comboBoxMachineType.TabIndex = 2;
            this.comboBoxMachineType.SelectedValueChanged += new System.EventHandler(this.comboBoxMachineType_SelectedValueChanged);
            // 
            // comboBoxCartType
            // 
            this.comboBoxCartType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCartType.FormattingEnabled = true;
            this.comboBoxCartType.Location = new System.Drawing.Point(117, 97);
            this.comboBoxCartType.Name = "comboBoxCartType";
            this.comboBoxCartType.Size = new System.Drawing.Size(454, 21);
            this.comboBoxCartType.TabIndex = 3;
            // 
            // comboBoxLeftController
            // 
            this.comboBoxLeftController.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLeftController.FormattingEnabled = true;
            this.comboBoxLeftController.Location = new System.Drawing.Point(21, 163);
            this.comboBoxLeftController.Name = "comboBoxLeftController";
            this.comboBoxLeftController.Size = new System.Drawing.Size(100, 21);
            this.comboBoxLeftController.TabIndex = 4;
            // 
            // comboBoxRightController
            // 
            this.comboBoxRightController.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRightController.FormattingEnabled = true;
            this.comboBoxRightController.Location = new System.Drawing.Point(127, 163);
            this.comboBoxRightController.Name = "comboBoxRightController";
            this.comboBoxRightController.Size = new System.Drawing.Size(100, 21);
            this.comboBoxRightController.TabIndex = 5;
            // 
            // labelSize
            // 
            this.labelSize.AutoSize = true;
            this.labelSize.Location = new System.Drawing.Point(267, 245);
            this.labelSize.Name = "labelSize";
            this.labelSize.Size = new System.Drawing.Size(41, 13);
            this.labelSize.TabIndex = 8;
            this.labelSize.Text = "0 bytes";
            // 
            // textBoxMd5Key
            // 
            this.textBoxMd5Key.Location = new System.Drawing.Point(23, 238);
            this.textBoxMd5Key.Name = "textBoxMd5Key";
            this.textBoxMd5Key.ReadOnly = true;
            this.textBoxMd5Key.Size = new System.Drawing.Size(204, 20);
            this.textBoxMd5Key.TabIndex = 9;
            this.textBoxMd5Key.TabStop = false;
            this.textBoxMd5Key.Text = "89b8b3df46733e0c4d57aeb9bb245e6f";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 219);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "MD5 Hash:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 78);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Machine Type:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(118, 78);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Cart Type:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(24, 144);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Left Controller:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(128, 144);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(82, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Right Controller:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(592, 284);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxMd5Key);
            this.Controls.Add(this.labelSize);
            this.Controls.Add(this.comboBoxRightController);
            this.Controls.Add(this.comboBoxLeftController);
            this.Controls.Add(this.comboBoxCartType);
            this.Controls.Add(this.comboBoxMachineType);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.textBoxRomPath);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(608, 323);
            this.MinimumSize = new System.Drawing.Size(608, 323);
            this.Name = "Form1";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EMU7800 Adhoc ROM Launcher";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxRomPath;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.ComboBox comboBoxMachineType;
        private System.Windows.Forms.ComboBox comboBoxCartType;
        private System.Windows.Forms.ComboBox comboBoxLeftController;
        private System.Windows.Forms.ComboBox comboBoxRightController;
        private System.Windows.Forms.Label labelSize;
        private System.Windows.Forms.TextBox textBoxMd5Key;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
    }
}

