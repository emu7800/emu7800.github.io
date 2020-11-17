using System;
using System.Windows.Forms;

namespace EMU7800.Launcher
{
    static class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length > 0 && System.IO.File.Exists(args[0]))
            {
                D2D.Shell.Win32.Win32EntryPoint.StartGameProgram(args[0]);
            }
            else
            {
                Application.Run(new Form1());
            }

            return 0;
        }
    }
}
