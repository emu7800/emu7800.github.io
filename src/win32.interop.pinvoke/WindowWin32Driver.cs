// © Mike Murphy

using EMU7800.Shell;
using System;

namespace EMU7800.Win32.Interop;

public static class WindowWin32Driver
{
    static IntPtr hWnd;

    public static void StartWindowAndProcessEvents(bool startMaximized)
    {
        hWnd = Win32NativeMethods.Win32_CreateWindow();
        if (hWnd == IntPtr.Zero)
            return;

        AudioDevice.DriverFactory     = AudioDeviceWinmmDriver.Factory;
        GameControllers.DriverFactory = () => GameControllersDInputXInputDriver.Factory(hWnd);

        GraphicsDevice.Initialize(new GraphicsDeviceD2DDriver(hWnd));
        GameControllers.Initialize();

        Win32NativeMethods.Win32_ProcessEvents(hWnd, startMaximized ? 3 /*SW_MAXIMIZE*/ : 1 /*SW_SHOWNORMAL*/);

        GameControllers.Shutdown();
        GraphicsDevice.Shutdown();
    }
}