﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using EMU7800.Core;
using EMU7800.Services;
using EMU7800.Services.Dto;

namespace EMU7800.Launcher
{
    public partial class Form1 : Form
    {
        bool _isa78format;
        GameProgramInfo _a78gameProgramInfo;

        readonly DropDownItem<MachineType>[] _machineTypes =
        {
            new DropDownItem<MachineType> { Value = MachineType.A2600NTSC, DisplayName = "2600 NTSC" },
            new DropDownItem<MachineType> { Value = MachineType.A2600PAL,  DisplayName = "2600 PAL" },
            new DropDownItem<MachineType> { Value = MachineType.A7800NTSC, DisplayName = "7800 NTSC" },
            new DropDownItem<MachineType> { Value = MachineType.A7800PAL,  DisplayName = "7800 PAL" }
        };

        readonly DropDownItem<CartType>[] _cartTypes26 =
        {
            new DropDownItem<CartType> { Value = CartType.A2K,     DisplayName = "A2K: 2KB Non-Bankswitched" },
            new DropDownItem<CartType> { Value = CartType.A4K,     DisplayName = "A4K: 4KB Non-Bankswitched" },
            new DropDownItem<CartType> { Value = CartType.A8K,     DisplayName = "A8K: 8KB Bankswitched" },
            new DropDownItem<CartType> { Value = CartType.A8KR,    DisplayName = "A8KR: 9KB Bankswitched w/128 bytes RAM" },
            new DropDownItem<CartType> { Value = CartType.A16K,    DisplayName = "A16K: 16KB Bankswitched" },
            new DropDownItem<CartType> { Value = CartType.A16KR,   DisplayName = "A16KR: 16KB Bankswitched w/128 bytes RAM" },
            new DropDownItem<CartType> { Value = CartType.A32K,    DisplayName = "A32K: 32KB Bankswitched" },
            new DropDownItem<CartType> { Value = CartType.A32KR,   DisplayName = "A32KR: 32KB Bankswitched w/128 bytes RAM" },
            new DropDownItem<CartType> { Value = CartType.DC8K,    DisplayName = "DC8K: 8KB Bankswitched Decathlon" },
            new DropDownItem<CartType> { Value = CartType.PB8K,    DisplayName = "PB8K: 8KB Bankswitched Parker Brothers" },
            new DropDownItem<CartType> { Value = CartType.TV8K,    DisplayName = "TV8K: 8KB Bankswitched Tigervision" },
            new DropDownItem<CartType> { Value = CartType.CBS12K,  DisplayName = "CBS12K: 12KB Bankswitched w/128 bytes RAM CBS" },
            new DropDownItem<CartType> { Value = CartType.MN16K,   DisplayName = "MN16K: 16KB Bankswitched w/2KB RAM M-Network" },
            new DropDownItem<CartType> { Value = CartType.DPC,     DisplayName = "DPC: Pitfall II" },
            new DropDownItem<CartType> { Value = CartType.M32N12K, DisplayName = "M32N12K: 32 in 1" }
        };

        readonly DropDownItem<CartType>[] _cartTypes78 =
        {
            new DropDownItem<CartType> { Value = CartType.A7808,  DisplayName = "A7808: 8KB Non-Bankswitched" },
            new DropDownItem<CartType> { Value = CartType.A7816,  DisplayName = "A7816: 16KB Non-Bankswitched" },
            new DropDownItem<CartType> { Value = CartType.A7832,  DisplayName = "7832: 32KB Non-Bankswitched" },
            new DropDownItem<CartType> { Value = CartType.A7832P, DisplayName = "A7832P: 32KB Non-Bankswitched w/Pokey" },
            new DropDownItem<CartType> { Value = CartType.A7848,  DisplayName = "7848: 48KB Non-Bankswitched" },
            new DropDownItem<CartType> { Value = CartType.A78SGP, DisplayName = "78SGP: SuperGame Bankswitched w/Pokey" },
            new DropDownItem<CartType> { Value = CartType.A78SG,  DisplayName = "78SG: SuperGame Bankswitched" },
            new DropDownItem<CartType> { Value = CartType.A78SGR, DisplayName = "78SGR: SuperGame Bankswitched w/16KB RAM" },
            new DropDownItem<CartType> { Value = CartType.A78S9,  DisplayName = "78S9: SuperGame S9 Bankswitched" },
            new DropDownItem<CartType> { Value = CartType.A78S4,  DisplayName = "78S4: SuperGame S4 Bankswitched" },
            new DropDownItem<CartType> { Value = CartType.A78S4R, DisplayName = "78S4R: SuperGame S4 Bankswitched w/8KB RAM" },
            new DropDownItem<CartType> { Value = CartType.A78AB,  DisplayName = "78AB: Absolute Bankswitched" },
            new DropDownItem<CartType> { Value = CartType.A78AC,  DisplayName = "78AC: Activision Bankswitched" }
        };

        readonly DropDownItem<Controller>[] _controllers =
        {
            new DropDownItem<Controller> { Value = Controller.Joystick,        DisplayName = "Joystick" },
            new DropDownItem<Controller> { Value = Controller.Paddles,         DisplayName = "Paddles" },
            new DropDownItem<Controller> { Value = Controller.Keypad,          DisplayName = "Keypad" },
            new DropDownItem<Controller> { Value = Controller.Driving,         DisplayName = "Driving" },
            new DropDownItem<Controller> { Value = Controller.BoosterGrip,     DisplayName = "Booster Grip" },
            new DropDownItem<Controller> { Value = Controller.ProLineJoystick, DisplayName = "ProLine Joystick" },
            new DropDownItem<Controller> { Value = Controller.Lightgun,        DisplayName = "Lightgun" }
        };

        public Form1()
        {
            InitializeComponent();
        }

        void Form1_Load(object sender, EventArgs e)
        {
            comboBoxMachineType.DataSource    = _machineTypes;
            comboBoxLeftController.DataSource = _controllers;
            comboBoxRightController.DataSource = _controllers;

            comboBoxMachineType.DisplayMember     = "DisplayName";
            comboBoxCartType.DisplayMember        = "DisplayName";
            comboBoxLeftController.DisplayMember  = "DisplayName";
            comboBoxRightController.DisplayMember = "DisplayName";
            comboBoxMachineType.ValueMember       = "Value";
            comboBoxCartType.ValueMember          = "Value";
            comboBoxLeftController.ValueMember    = "Value";
            comboBoxRightController.ValueMember   = "Value";

            textBoxMd5Key.Text = labelSize.Text = string.Empty;

            buttonStart.Enabled = false;
        }

        void buttonBrowse_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = string.Empty;
            var result = openFileDialog1.ShowDialog();
            if (result != DialogResult.OK)
                return;

            textBoxRomPath.Text = openFileDialog1.FileName;

            var fileExists = File.Exists(textBoxRomPath.Text);
            buttonStart.Enabled = fileExists;

            if (!fileExists)
                return;

            using (var fs = new FileStream(textBoxRomPath.Text, FileMode.Open))
            using (var br = new BinaryReader(fs))
            {
                var bytes = br.ReadBytes(1000000);
                var svc = new RomBytesService();
                textBoxMd5Key.Text = svc.ToMD5Key(bytes);
                labelSize.Text = $"Size {bytes.Length} / 0x{bytes.Length:X4} bytes";
                _isa78format = svc.IsA78Format(bytes);
                _a78gameProgramInfo = _isa78format ? svc.ToGameProgramInfoFromA78Format(bytes) : null;
                if (_a78gameProgramInfo != null)
                {
                    comboBoxMachineType.SelectedItem = _machineTypes.FirstOrDefault(mt => mt.Value == _a78gameProgramInfo.MachineType);
                    comboBoxCartType.SelectedItem = _cartTypes78.FirstOrDefault(ct => ct.Value == _a78gameProgramInfo.CartType);
                    comboBoxLeftController.SelectedItem = _controllers.FirstOrDefault(c => c.Value == _a78gameProgramInfo.LController);
                    comboBoxRightController.SelectedItem = _controllers.FirstOrDefault(c => c.Value == _a78gameProgramInfo.RController);
                }
            }
        }

        void comboBoxMachineType_SelectedValueChanged(object sender, EventArgs e)
        {
            switch (((DropDownItem<MachineType>)comboBoxMachineType.SelectedItem).Value)
            {
                case MachineType.A2600NTSC:
                case MachineType.A2600PAL:
                    comboBoxCartType.DataSource = _cartTypes26;
                    break;
                case MachineType.A7800NTSC:
                case MachineType.A7800PAL:
                    comboBoxCartType.DataSource = _cartTypes78;
                    break;
            }
        }

        async void buttonStart_Click(object sender, EventArgs e)
        {
            buttonBrowse.Enabled = false;
            buttonStart.Enabled = false;

            var gpivi = new GameProgramInfoViewItem
            {
                Title = string.Empty,
                SubTitle = string.Empty,
                ImportedGameProgramInfo = new ImportedGameProgramInfo
                {
                    GameProgramInfo = new GameProgramInfo
                    {
                        Author = string.Empty,
                        CartType = ((DropDownItem<CartType>)comboBoxCartType.SelectedItem).Value,
                        MachineType = ((DropDownItem<MachineType>)comboBoxMachineType.SelectedItem).Value,
                        LController = ((DropDownItem<Controller>)comboBoxLeftController.SelectedItem).Value,
                        RController = ((DropDownItem<Controller>)comboBoxRightController.SelectedItem).Value,
                        Manufacturer = string.Empty,
                        HelpUri = string.Empty,
                        MD5 = textBoxMd5Key.Text,
                        ModelNo = string.Empty,
                        Qualifier = string.Empty
                    },
                    PersistedStateExists = false,
                    StorageKeySet = new HashSet<string> { textBoxRomPath.Text }
                }
            };

            await Task.Factory.StartNew(() => StartGameProgram(gpivi));

            buttonBrowse.Enabled = true;
            buttonStart.Enabled = true;
        }

        void StartGameProgram(GameProgramInfoViewItem gpivi)
        {
            try
            {
                D2D.Shell.Win32.Win32EntryPoint.StartGameProgram(gpivi);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
            }
        }
    }
}
