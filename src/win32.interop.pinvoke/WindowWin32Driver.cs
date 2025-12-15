// © Mike Murphy

using EMU7800.Shell;
using System;

namespace EMU7800.Win32.Interop;

public sealed class WindowWin32Driver
{
    public Window Window { get; private set; }
    public GraphicsDeviceD2DDriver GraphicsDevice { get; private set; }
    public GameControllersDInputXInputDriver GameControllers { get; private set; }
    public AudioDeviceWinmmDriver AudioDevice { get; private set; }

    readonly IntPtr _hWnd;

    public void ProcessEvents(bool startMaximized)
    {
        if (_hWnd == IntPtr.Zero)
            return;

        GameControllers.Initialize();

        Win32NativeMethods.Win32_ProcessEvents(this, _hWnd, startMaximized ? 3 /*SW_MAXIMIZE*/ : 1 /*SW_SHOWNORMAL*/);

        GameControllers.Shutdown();
        AudioDevice.Close();
        GraphicsDevice.Shutdown();
    }

    public WindowWin32Driver(Window window)
    {
        Window = window;
        _hWnd = Win32NativeMethods.Win32_CreateWindow();
        GraphicsDevice = new GraphicsDeviceD2DDriver(_hWnd);
        GameControllers = new GameControllersDInputXInputDriver(_hWnd, window);
        AudioDevice = new();

        window.OnAudioChanged(AudioDevice);
        window.OnControllersChanged(GameControllers);
    }
}