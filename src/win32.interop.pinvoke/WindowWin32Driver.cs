// © Mike Murphy

using EMU7800.Core;
using EMU7800.Shell;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace EMU7800.Win32.Interop;

public sealed partial class WindowWin32Driver : IWindowDriver
{
    readonly ILogger _logger;
    readonly bool _openConsole;
    IntPtr _hWnd;

    #region IWindowDriver Members

    public void Start(Window window, bool startMaximized)
    {
        if (_openConsole)
        {
            AllocConsole();
            _logger.Log(5, "Opened console window.");
        }

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

    public WindowWin32Driver(string[] args, ILogger logger)
      => (_logger, _openConsole) = (logger, args.Any(arg => CiOneOf(arg, "-c", "/c")));

    #endregion

    #region Helpers

    static bool CiOneOf(string arg, params string[] items)
      => items.Any(item => item.Equals(arg, StringComparison.OrdinalIgnoreCase));

    [LibraryImport("Kernel32.dll")]
    internal static partial void AllocConsole();

    #endregion
}