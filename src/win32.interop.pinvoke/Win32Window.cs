// © Mike Murphy

using EMU7800.Shell;
using System;

namespace EMU7800.Win32.Interop;

public static class Win32Window
{
    static IntPtr hWnd;

    public static Action<ushort, bool> KeyboardKeyPressed { get; set; } = (vk, d) => { };
    public static Action<int, int, int, int> MouseMoved { get; set; } = (x, y, dx, dy) => { };
    public static Action<int, int, bool> MouseButtonChanged { get; set; } = (x, y, down) => { };
    public static Action<int, int, int> MouseWheelChanged { get; set; } = (x, y, delta) => { };
    public static Action LURCycle { get; set; } = () => { };
    public static Action<bool> VisibilityChanged { get; set; } = iv => { };
    public static Action<int, int> Resized { get; set; } = (w, h) => { };
    public static Action DeviceChanged { get; set; } = () => { };

    public static void Run(bool startMaximized = true)
    {
        hWnd = Win32NativeMethods.Win32_CreateWindow();
        if (hWnd == IntPtr.Zero)
            return;

        AudioDevice.DriverFactory     = AudioDeviceWinmmDriver.Factory;
        DynamicBitmap.DriverFactory   = DynamicBitmapD2DDriver.Factory;
        StaticBitmap.DriverFactory    = StaticBitmapD2DDriver.Factory;
        TextFormat.DriverFactory      = TextFormatD2DDriver.Factory;
        TextLayout.DriverFactory      = TextLayoutD2DDriver.Factory;
        GraphicsDevice.DriverFactory  = () => GraphicsDeviceD2DDriver.Factory(hWnd);
        GameControllers.DriverFactory = () => GameControllersDInputXInputDriver.Factory(hWnd);

        Resized += (w, h) => GraphicsDevice.Resize(new(w, h));

        GraphicsDevice.Initialize();
        GameControllers.Initialize();

        Win32NativeMethods.Win32_ProcessEvents(hWnd, startMaximized ? 3 /*SW_MAXIMIZE*/ : 1 /*SW_SHOWNORMAL*/);

        GameControllers.Shutdown();
        GraphicsDevice.Shutdown();
    }
}