using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EMU7800.Core;

namespace EMU7800.Win
{
    partial class ControlPanelForm
    {
        #region Event Handlers

        void TextboxOutputVisibleChanged(object sender, EventArgs e)
        {
            if (textboxOutput.Visible) textboxOutput.AppendText(EMU7800Application.Logger.GetMessages());
            textboxInput.Text = string.Empty;
        }

        void TextboxInputKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                e.Handled = true;
                IssueCommandFromInputTextbox();
            }
        }

        #endregion

        #region CommandLine Members

        public void ExecuteCommandLine(CommandLine cl)
        {
            if (cl.IsHelp)
            {
                LogLine("\n** General Commands **");
            }
            if (cl.CommandEquals("clear"))
            {
                ClClear(cl);
            }
            if (cl.CommandEquals("cpunop"))
            {
                ClCpuNop(cl);
            }
            if (cl.CommandEquals("gs"))
            {
                ClGs(cl);
            }
            if (cl.CommandEquals("ls"))
            {
                ClLs(cl);
            }
            if (cl.CommandEquals("rec"))
            {
                ClRec(cl);
            }
            if (cl.CommandEquals("pb"))
            {
                ClPb(cl);
            }
            if (cl.CommandEquals("run"))
            {
                ClRun(cl);
            }
            if (cl.CommandEquals("opacity"))
            {
                ClOpacity(cl);
            }
            if (cl.CommandEquals("joybuttons"))
            {
                ClJoyButtons(cl);
            }
            if (cl.CommandEquals("paddlefactor"))
            {
                ClPaddleFactor(cl);
            }
            if (cl.CommandEquals("fps"))
            {
                ClFps(cl);
            }
            if (cl.CommandEquals("save"))
            {
                ClSave(cl);
            }
            if (cl.CommandEquals("restore"))
            {
                ClRestore(cl);
            }

            if (M != null)
            {
                ExecuteMachineCommandLine(cl);
            }

            if (!cl.HasMatched && !cl.IsHelp)
            {
                LogLine("unrecognized command");
            }
        }

        void ClClear(CommandLine cl)
        {
            if (cl.IsCommandHelp)
            {
                LogLine("clear: clear log messages (64k limit)");
                return;
            }
            textboxOutput.Clear();
            LogLine("log cleared");
        }

        void ClCpuNop(CommandLine cl)
        {
            if (cl.IsCommandHelp || cl.Parms.Count > 2)
            {
                LogLine("cpunop [on]|[off]: turn on/off cpu NOP register dumping");
                return;
            }
            if (cl.Parms.Count.Equals(2))
            {
                _globalSettings.NOPRegisterDumping = cl.Parms[0].Equals("ON");
            }
            Log("CPU NOP register dumping: ");
            LogLine(_globalSettings.NOPRegisterDumping ? "ON" : "OFF");
        }

        void ClGs(CommandLine cl)
        {
            if (cl.IsCommandHelp)
            {
                LogLine("gs <attribute> <newvalue>: show/set game settings");
                return;
            }

            var gs = CurrGameProgram;
            if (gs == null)
            {
                LogLine("No GameProgram selected.");
                return;
            }

            if (cl.Parms.Count <= 1)
            {
                LogLine(gs.ToString());
                return;
            }

            var var = cl.Parms[1];
            var val = cl.Parms.Count == 2 ? new CommandLineParameter("?") : cl.Parms[2];
            if (var.Equals("title"))
            {
                gs.Title = val.StrValue;
            }
            else if (var.Equals("manufacturer"))
            {
                gs.Manufacturer = val.StrValue;
            }
            else if (var.Equals("year"))
            {
                gs.Year = val.StrValue;
            }
            else if (var.Equals("modelno"))
            {
                gs.ModelNo = val.StrValue;
            }
            else if (var.Equals("rarity"))
            {
                gs.Rarity = val.StrValue;
            }
            else if (var.Equals("carttype"))
            {
                try
                {
                    gs.CartType = (CartType)Enum.Parse(typeof(CartType), val.StrValue, true);
                }
                catch (ArgumentException)
                {
                    LogLine("Valid CartTypes:");
                    foreach (var typ in Enum.GetNames(typeof(CartType)))
                    {
                        Log(typ + " ");
                    }
                    LogLine(string.Empty);
                }
                return;
            }
            else if (var.Equals("machinetype"))
            {
                try
                {
                    gs.MachineType = (MachineType)Enum.Parse(typeof(MachineType), val.StrValue, true);
                }
                catch (ArgumentException)
                {
                    LogLine("Valid MachineTypes:");
                    foreach (var typ in Enum.GetNames(typeof(MachineType)))
                    {
                        Log(typ + " ");
                    }
                    LogLine(string.Empty);
                    return;
                }
            }
            else if (var.Equals("lcontroller") || var.Equals("rcontroller"))
            {
                var c = Controller.None;
                try
                {
                    c = (Controller)Enum.Parse(typeof(Controller), val.StrValue, true);
                }
                catch (ArgumentException)
                {
                }
                if (c != Controller.None)
                {
                    if (var.StrValue.Substring(0, 1).Equals("l", StringComparison.OrdinalIgnoreCase))
                    {
                        gs.LController = c;
                    }
                    else
                    {
                        gs.RController = c;
                    }
                }
                else
                {
                    LogLine("Valid Controllers:");
                    foreach (var typ in Enum.GetNames(typeof(Controller)))
                    {
                        Log(typ + " ");
                    }
                    LogLine(string.Empty);
                    return;
                }
            }
            else if (var.Equals("helpuri"))
            {
                gs.HelpUri = val.StrValue;
            }
            else
            {
                LogLine("bad parms");
                return;
            }

            CurrGameProgram = gs;
            LogLine(CurrGameProgram.ToString());
        }

        void ClLs(CommandLine cl)
        {
            if (cl.IsCommandHelp)
            {
                LogLine("ls: show files in outdir");
                return;
            }

            LogLine("files in outdir:");
            LogLine(_globalSettings.OutputDirectory);
            try
            {
                var files = new DirectoryInfo(_globalSettings.OutputDirectory).GetFiles();
                Log("FileName".PadRight(40, ' '));
                LogLine(" Size");
                LogLine(string.Empty.PadRight(45, '-'));
                foreach (var fi in files
                    .Where(fi => fi.Extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase)
                              || fi.Extension.Equals(".emu", StringComparison.OrdinalIgnoreCase)
                              || fi.Extension.Equals(".emurec", StringComparison.OrdinalIgnoreCase)))
                {
                    Log(fi.Name.PadRight(40, ' '));
                    LogLine(" {0}kb", fi.Length / 1024);
                }
            }
            catch (DirectoryNotFoundException)
            {
                Log("directory does not exist: ");
                LogLine(_globalSettings.OutputDirectory);
            }
        }

        void ClRec(CommandLine cl)
        {
            if (cl.IsCommandHelp)
            {
                LogLine("rec: start recording input");
                return;
            }
            if (!cl.CheckParms("ss"))
            {
                LogLine("bad args");
                return;
            }
            if (CurrGameProgram == null)
            {
                LogLine("No game program selected.");
                return;
            }

            var fnWithoutExt = Path.Combine(_globalSettings.OutputDirectory, cl.Parms[1].StrValue);
            var fn = GenerateOutFileName(fnWithoutExt, ".emurec");
            if (string.IsNullOrWhiteSpace(fn))
            {
                LogLine("Unable to generate output filename.");
                return;
            }
            _stagedInputRecorder = new InputRecorder(fn, CurrGameProgram.MD5, _logger);
            LogLine("Recording input from {0} to {1}.", CurrGameProgram.ToString(), fn);
            _globalSettings.Skip7800BIOS = true;
            checkboxSkip7800Bios.Checked = true;

            Start();

            LogLine("Recording completed to {0}", fn);
        }

        void ClPb(CommandLine cl)
        {
            if (cl.IsCommandHelp)
            {
                LogLine("pb <filename> [loop]: replay recording");
                return;
            }
            if (cl.Parms.Count <= 2 && !cl.CheckParms("ss") || cl.Parms.Count > 2 && !cl.CheckParms("sss"))
            {
                LogLine("bad args");
                return;
            }

            var loop = cl.Parms.Count >= 3 && cl.Parms[2].Equals("loop");
            var fn = Path.Combine(_globalSettings.OutputDirectory, cl.Parms[1].StrValue);
            if (!File.Exists(fn))
            {
                LogLine("File not found: {0}", fn);
                return;
            }

            while (true)
            {
                var ip = new InputPlayer(fn, _logger);
                if (!ip.ValidEmuRecFile)
                    break;
                _stagedInputPlayer = ip;
                _quitMachineOnInputEnded = loop;
                LogLine("Playback started from {0}", fn);
                _globalSettings.Skip7800BIOS = true;
                checkboxSkip7800Bios.Checked = true;
                var startTick = Environment.TickCount;

                Start();

                if (!loop)
                    break;

                var seconds = (Environment.TickCount - startTick) / 1000;
                if (seconds < 10)
                {
                    LogLine("Looping playback stopped.");
                    break;
                }
            }
        }

        void ClRun(CommandLine cl)
        {
            if (cl.IsCommandHelp)
            {
                LogLine("run: run ROM specified in GameSettings");
                return;
            }
            if (CurrGameProgram.DiscoveredRomFullName == null)
            {
                LogLine("GameSettings incomplete");
                return;
            }
            TextboxOutputVisibleChanged(this, null);
            ButtonStartClick(this, null);
        }

        void ClOpacity(CommandLine cl)
        {
            if (cl.IsCommandHelp)
            {
                LogLine("opacity [25-100]");
                return;
            }
            if (cl.Parms.Count <= 1)
            {
                LogLine("opacity " + Opacity * 100 + "%");
                return;
            }
            if (!cl.CheckParms("si"))
            {
                LogLine("bad parm");
                return;
            }

            var op = cl.Parms[1].IntValue;
            if (op > 100)
            {
                op = 100;
            }
            else if (op < 25)
            {
                op = 25;
            }
            Opacity = op / 100.0;
            LogLine("opacity set to " + Opacity * 100 + "%");
        }

        void ClJoyButtons(CommandLine cl)
        {
            if (cl.IsCommandHelp)
            {
                LogLine("joybuttons <trigger#> <booster#>");
                return;
            }
            if (cl.Parms.Count <= 1)
            {
                LogLine("joybuttons: trigger:{0} booster:{1}\n", _globalSettings.JoyBTrigger, _globalSettings.JoyBBooster);
                return;
            }
            if (!cl.CheckParms("sii"))
            {
                LogLine("bad parms");
                LogLine("usage: joybuttons [trigger#] [booster#]");
                return;
            }

            _globalSettings.JoyBTrigger = cl.Parms[1].IntValue;
            _globalSettings.JoyBBooster = cl.Parms[2].IntValue;
            LogLine("joystick button bindings set");
        }

        void ClPaddleFactor(CommandLine cl)
        {
            if (cl.IsCommandHelp)
            {
                LogLine("paddlefactor <0-100>");
                return;
            }
            if (cl.Parms.Count <= 1)
            {
                LogLine("paddlefactor: {0}", _globalSettings.PaddleFactor);
                return;
            }
            if (!cl.CheckParms("si") || cl.Parms[1].IntValue == 0)
            {
                LogLine("bad parms");
                LogLine("usage: paddlefactor <0-100>");
                return;
            }

            _globalSettings.PaddleFactor = cl.Parms[1].IntValue;
            LogLine("paddlefactor setting changed");
        }

        void ClFps(CommandLine cl)
        {
            if (cl.IsCommandHelp)
            {
                LogLine("fps [ratedelta]: adj. frames per second");
                return;
            }
            if (cl.Parms.Count <= 1)
            {
                LogLine(_globalSettings.FrameRateAdjust.ToString());
                return;
            }
            if (!cl.CheckParms("si") || cl.Parms.Count > 2)
            {
                LogLine("bad parm");
                return;
            }

            _globalSettings.FrameRateAdjust = cl.Parms[1].IntValue;
            LogLine("ok");
        }

        void ClSave(CommandLine cl)
        {
            if (cl.IsCommandHelp)
            {
                LogLine("save <filename>: save machine state");
                return;
            }
            if (M == null)
            {
                LogLine("no machine");
                return;
            }
            if (!cl.CheckParms("ss"))
            {
                LogLine("bad parm");
                return;
            }

            var fnWithoutExt = Path.Combine(_globalSettings.OutputDirectory, cl.Parms[1].StrValue);
            var fn = GenerateOutFileName(fnWithoutExt, ".emu");
            if (string.IsNullOrWhiteSpace(fn))
            {
                LogLine("Unable to generate output filename.");
                return;
            }
            try
            {
                Util.SerializeMachineToFile(M, fn);
            }
            catch (Exception ex)
            {
                if (Util.IsCriticalException(ex))
                    throw;
                LogLine("error saving machine state: ");
                LogLine(ex.ToString());
                return;
            }
            Log("machine state saved to ");
            LogLine(Path.GetFileName(fn));
        }

        void ClRestore(CommandLine cl)
        {
            if (cl.IsCommandHelp)
            {
                LogLine("restore <filename>: restore machine state");
                return;
            }
            if (!cl.CheckParms("ss"))
            {
                LogLine("bad parm");
                return;
            }

            CurrGameProgram = null;
            var fn = Path.Combine(_globalSettings.OutputDirectory, cl.Parms[1].StrValue);
            if (!File.Exists(fn))
            {
                StartButtonEnabled = false;
                ResumeButtonEnabled = false;
                LogLine("File not found: {0}", fn);
                return;
            }

            try
            {
                M = Util.DeserializeMachineFromFile(fn);
            }
            catch (Emu7800SerializationException)
            {
                StartButtonEnabled = false;
                ResumeButtonEnabled = false;
                var message = string.Format("Not a valid emu machine state file: {0}", fn);
                LogLine(message);
                return;
            }
            catch (Exception ex)
            {
                if (Util.IsCriticalException(ex))
                    throw;
                StartButtonEnabled = false;
                ResumeButtonEnabled = false;
                LogLine(ex.ToString());
                return;
            }
            M.Logger = _logger;
            StartButtonEnabled = false;
            ResumeButtonEnabled = true;
            ResetGameTitleLabel();
            LogLine("Machine state restored");
        }

        #region Machine CommandLine Members

        void ExecuteMachineCommandLine(CommandLine cl)
        {
            if (cl.IsHelp)
            {
                LogLine("\n** Machine Specific Commands **");
            }
            if (cl.CommandEquals("d"))
            {
                ClDisassemble(cl);
            }
            if (cl.CommandEquals("m"))
            {
                ClMemDump(cl);
            }
            if (cl.CommandEquals("poke"))
            {
                ClPokeMem(cl);
            }
            if (cl.CommandEquals("reset"))
            {
                ClReset(cl);
            }
            if (cl.CommandEquals("pc"))
            {
                ClSetProgramCounter(cl);
            }
            if (cl.CommandEquals("r"))
            {
                ClDumpRegisters(cl);
            }
            if (cl.CommandEquals("s"))
            {
                ClStepExecution(cl);
            }
        }

        void ClDisassemble(CommandLine cl)
        {
            if (cl.IsCommandHelp)
            {
                LogLine(" d <[$]fromaddr> [[$]toaddr]: disassemble memory");
                return;
            }
            if (cl.Parms.Count >= 3 && cl.CheckParms("sii"))
            {
                LogLine(M6502DASM.Disassemble(M.Mem, (ushort)cl.Parms[1].IntValue, (ushort)cl.Parms[2].IntValue));
            }
            else if (cl.Parms.Count >= 2 && cl.CheckParms("si"))
            {
                LogLine(M6502DASM.Disassemble(M.Mem, (ushort)cl.Parms[1].IntValue, (ushort)(cl.Parms[1].IntValue + 48)));
            }
            else
            {
                LogLine("bad parms");
            }
        }

        void ClMemDump(CommandLine cl)
        {
            if (cl.IsCommandHelp)
            {
                LogLine(" m <[$]fromaddr> [[$]toaddr]: dump memory");
                return;
            }
            if (cl.Parms.Count >= 3 && cl.CheckParms("sii"))
            {
                LogLine(M6502DASM.MemDump(M.Mem, (ushort)cl.Parms[1].IntValue, (ushort)cl.Parms[2].IntValue));
            }
            else if (cl.Parms.Count >= 2 && cl.CheckParms("si"))
            {
                LogLine(M6502DASM.MemDump(M.Mem, (ushort)cl.Parms[1].IntValue, (ushort)cl.Parms[1].IntValue));
            }
            else
            {
                LogLine("bad parms");
            }
        }

        void ClPokeMem(CommandLine cl)
        {
            if (cl.IsCommandHelp)
            {
                LogLine(" poke <[$]ataddr> <[$]dataval>: poke dataval to ataddr");
                return;
            }
            if (cl.Parms.Count < 3 || !cl.CheckParms("sii"))
            {
                LogLine("bad parms");
                return;
            }
            M.Mem[(ushort)cl.Parms[1].IntValue] = (byte)cl.Parms[2].IntValue;
            LogLine(string.Format("poke #${1:x2} at ${0:x4} complete", cl.Parms[1].IntValue, cl.Parms[2].IntValue));
        }

        void ClReset(CommandLine cl)
        {
            if (cl.IsCommandHelp)
            {
                LogLine(" reset: reset machine");
                return;
            }
            M.Reset();
        }

        void ClDumpRegisters(CommandLine cl)
        {
            if (cl.IsCommandHelp)
            {
                LogLine(" r: display CPU registers");
                return;
            }
            LogLine(M6502DASM.GetRegisters(M.CPU));
        }

        void ClSetProgramCounter(CommandLine cl)
        {
            if (cl.IsCommandHelp)
            {
                LogLine(" pc <addr>: change CPU program counter");
                return;
            }
            if (cl.Parms.Count <= 1)
            {
                LogLine(M6502DASM.GetRegisters(M.CPU));
            }
            else if (cl.CheckParms("si"))
            {
                M.CPU.PC = (ushort)cl.Parms[1].IntValue;
                LogLine(String.Format("PC changed to {0:x4}", M.CPU.PC));
            }
            else
            {
                LogLine("bad parm");
            }
        }

        void ClStepExecution(CommandLine cl)
        {
            if (cl.IsCommandHelp)
            {
                LogLine(" s [#steps] [stop PC]: step CPU execution");
                return;
            }
            if (cl.CheckParms("si"))
            {
                _ClStepExecution(cl.Parms[1].IntValue, 0);
            }
            else if (cl.CheckParms("sii"))
            {
                _ClStepExecution(cl.Parms[1].IntValue, (ushort)cl.Parms[2].IntValue);
            }
            else
            {
                _ClStepExecution(1, 0);
            }
        }

        void _ClStepExecution(int steps, ushort stopPC)
        {
            var sb = new StringBuilder();
            sb.Append(M6502DASM.Disassemble(M.Mem, M.CPU.PC, (ushort)(M.CPU.PC + 1)));
            sb.Append(M6502DASM.GetRegisters(M.CPU));
            sb.Append("\n");
            for (var i = 0; i < steps && M.CPU.PC != stopPC; i++)
            {
                M.CPU.RunClocks = 2;
                M.CPU.Execute();
                sb.Append(M6502DASM.Disassemble(M.Mem, M.CPU.PC, (ushort)(M.CPU.PC + 1)));
                sb.Append(M6502DASM.GetRegisters(M.CPU));
                sb.Append("\n");
            }
            LogLine(sb.ToString());
        }

        #endregion

        #endregion

        #region Helpers

        void IssueCommandFromInputTextbox()
        {
            var commandline = textboxInput.Text;
            textboxInput.Text = string.Empty;
            LogLine(string.Format(">{0}", commandline));
            ExecuteCommandLine(new CommandLine(commandline));
            textboxOutput.AppendText(EMU7800Application.Logger.GetMessages());
            textboxInput.Focus();
        }

        static string GenerateOutFileName(string fn, string ext)
        {
            var path1 = Path.GetDirectoryName(fn);
            if (string.IsNullOrWhiteSpace(path1))
                return null;
            var path2 = Path.GetFileNameWithoutExtension(fn);
            if (string.IsNullOrWhiteSpace(path2))
                return null;
            var stem = Path.Combine(path1, path2);
            var i = 0;
            var u = string.Empty;
            string outFileName;
            while (true)
            {
                outFileName = stem + u + ext;
                if (!File.Exists(outFileName))
                {
                    break;
                }
                u = (++i).ToString();
            }
            return outFileName;
        }

        void Log(string format, params object[] args)
        {
            _logger.Write(format, args);
        }

        void LogLine(string format, params object[] args)
        {
            _logger.WriteLine(format, args);
        }

        #endregion
    }
}
