// © Mike Murphy

using EMU7800.Core;
using EMU7800.Shell;
using System;

namespace EMU7800.Win32.Interop;

public sealed partial class WindowWin32Driver : IWindowDriver
{
    readonly ILogger _logger;
    IntPtr _hWnd;

    #region IWindowDriver Members

    public void Start(Window window, bool startMaximized)
    {
        _hWnd = Win32NativeMethods.Win32_CreateWindow();

        if (_hWnd == IntPtr.Zero)
            return;

        WindowDevices devices = new(
            window,
            new GraphicsDeviceD2DDriver(_hWnd),
            new AudioDeviceWinmmDriver(),
            new GameControllersDInputXInputDriver(_hWnd, window));

        window.OnAudioChanged(devices.AudioDevice);
        window.OnControllersChanged(devices.GameControllers);

        devices.GameControllers.Initialize();

        Win32NativeMethods.Win32_ProcessEvents(devices, _hWnd, startMaximized ? 3 /*SW_MAXIMIZE*/ : 1 /*SW_SHOWNORMAL*/);

        devices.GameControllers.Shutdown();
        devices.AudioDevice.Close();
        devices.GraphicsDevice.Shutdown();
    }

    #endregion

    #region Constructors

    public WindowWin32Driver(ILogger logger)
      => _logger = logger;

    #endregion
}