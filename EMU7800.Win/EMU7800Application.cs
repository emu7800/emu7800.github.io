/*
 * EMU7800Application.cs
 * 
 * Main application class for EMU7800.
 * 
 * Copyright © 2004-2005 Mike Murphy
 * 
 */

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace EMU7800.Win
{
    public sealed class EMU7800Application
    {
        public static string Title
        {
            get
            {
                var executingAssembly = Assembly.GetExecutingAssembly();
                var obj = executingAssembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (obj.Length == 0)
                    return string.Empty;
                var attr = obj[0] as AssemblyTitleAttribute;
                return (attr != null) ? attr.Title : string.Empty;
            }
        }

        public static string Version
        {
            get
            {
                var executingAssembly = Assembly.GetExecutingAssembly();
                return executingAssembly.GetName().Version.ToString();
            }
        }

        public static string Copyright
        {
            get
            {
                var executingAssembly = Assembly.GetExecutingAssembly();
                var obj = executingAssembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (obj.Length == 0)
                    return string.Empty;
                var attr = obj[0] as AssemblyCopyrightAttribute;
                return (attr != null) ? attr.Copyright : string.Empty;
            }
        }

        public static Version ClrVersion
        {
            [DebuggerStepThrough]
            get { return Environment.Version; }
        }

        public static OperatingSystem OSVersion
        {
            [DebuggerStepThrough]
            get { return Environment.OSVersion; }
        }

        public static ControlPanelFormLogger Logger = new ControlPanelFormLogger();

        public EMU7800Application()
        {
            Logger.Write(Title);
            Logger.Write(" v");
            Logger.Write(Version);
#if DEBUG
            Logger.Write(" DEBUG");
#endif
            Logger.WriteLine("");
            Logger.WriteLine(Copyright);

            Logger.WriteLine("CLR Version: {0}", ClrVersion);
            Logger.WriteLine("OS Version: {0}", OSVersion);
            Logger.WriteLine("High resolution timer available: {0}", Stopwatch.IsHighResolution ? "yes" : "no");
            Logger.WriteLine("Timer frequency {0} ticks per second", Stopwatch.Frequency);
            Logger.WriteLine("Process is {0}-bit", Util.IsProcess32Bit ? "32" : "64");
            Logger.WriteLine("Current directory: {0}", Environment.CurrentDirectory);

            Logger.WriteLine("Available Hosts:");
            var hostFactory = new HostFactory(Logger);
            foreach (var name in hostFactory.GetRegisteredHostNames())
            {
                Logger.WriteLine(name);
            }
        }

        public int Run(string[] args)
        {
            var settings = new GlobalSettings(Logger);
            if (args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += ApplicationOnThreadException;
                Application.Run(new ControlPanelForm());
                settings.Save();
                return 0;
            }

            var fullName = args[0];
            var gpl = new GameProgramLibrary(Logger);
            var gp = gpl.TryRecognizeRom(fullName);
            if (gp == null)
                return 1;
            var hsc7800Factory = new HSC7800Factory(gpl, Logger);
            var hsc = settings.Use7800HSC ? hsc7800Factory.CreateHSC7800() : null;
            var nopRegisterDumping = settings.NOPRegisterDumping;
            var machineFactory = new MachineFactory(gpl, hsc, Logger);
            var m = machineFactory.BuildMachine(gp.DiscoveredRomFullName, !settings.Skip7800BIOS);
            m.NOPRegisterDumping = nopRegisterDumping;
            var hostFactory = new HostFactory(Logger);
            try
            {
                var host = hostFactory.Create(args.Length > 1 ? args[1] : settings.HostSelect, m);
                host.Run();
            }
            catch (Exception ex)
            {
                if (Util.IsCriticalException(ex))
                    throw;
                return 1;
            }

            if (hsc != null)
                hsc7800Factory.SaveRam();

            return 0;
        }

        void ApplicationOnThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Logger.WriteLine("Unhandled exception:");
            Logger.WriteLine(e.Exception.ToString());
            MessageBox.Show("An unexpected exception occurred, see Console log for details.", "EMU7800: Unexpected Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        [STAThread]
        public static int Main(string[] args)
        {
            var emu7800Application = new EMU7800Application();
            return emu7800Application.Run(args);
        }
    }
}