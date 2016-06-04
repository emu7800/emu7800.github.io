// © Mike Murphy

using System;
using Windows.ApplicationModel.Core;

namespace EMU7800.D2D.Shell.WinRT
{
    public sealed class WinRTEntryPoint
    {
        [MTAThread]
        public static int Main()
        {
            var appViewSource = new AppViewSource();
            CoreApplication.Run(appViewSource);
            return 0;
        }
    }
}
