// © Mike Murphy

using EMU7800.Shell;
using System;

namespace EMU7800.Win32.Interop;

public sealed class WindowWin32Driver : IWindowDriver
{
    public static WindowWin32Driver Factory() => new();
    WindowWin32Driver() {}

    IntPtr hWnd;

    public void StartWindowAndProcessEvents(bool startMaximized)
    {
        hWnd = Win32NativeMethods.Win32_CreateWindow();
        if (hWnd == IntPtr.Zero)
            return;

        AudioDevice.DriverFactory     = AudioDeviceWinmmDriver.Factory;
        GraphicsDevice.DriverFactory  = () => GraphicsDeviceD2DDriver.Factory(hWnd);
        GameControllers.DriverFactory = () => GameControllersDInputXInputDriver.Factory(hWnd);

        GraphicsDevice.Initialize();
        GameControllers.Initialize();

        Win32NativeMethods.Win32_ProcessEvents(hWnd, startMaximized ? 3 /*SW_MAXIMIZE*/ : 1 /*SW_SHOWNORMAL*/);

        GameControllers.Shutdown();
        GraphicsDevice.Shutdown();
    }
}