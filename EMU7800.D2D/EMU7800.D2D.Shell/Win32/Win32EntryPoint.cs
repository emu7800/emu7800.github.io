using System;
using EMU7800.D2D.Interop;
using EMU7800.Services.Dto;

namespace EMU7800.D2D.Shell.Win32
{
    public sealed class Win32EntryPoint
    {
        [STAThread]
        public static int Main()
        {
            using (var win = new Win32Window())
            using (var app = new Win32App(win))
            {
                app.Run();
            }
            return 0;
        }

        public static void StartGameProgram(GameProgramInfoViewItem gpivi)
        {
            using (var win = new Win32Window())
            using (var app = new Win32App(win, gpivi))
            {
                app.Run();
            }
        }
    }
}
