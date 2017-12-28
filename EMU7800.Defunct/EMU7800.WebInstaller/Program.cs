using System;
using System.Windows.Forms;

namespace EMU7800.WebInstaller
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WebInstaller());
        }
    }
}
