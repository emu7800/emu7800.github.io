// © Mike Murphy

using System;
using static EMU7800.Win32.Interop.Win32NativeMethods;
using static System.Console;

namespace EMU7800.Win32.Interop
{
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
            Resized += (w, h) => GraphicsDevice.Resize(new(w, h));

            hWnd = Win32_Initialize();
            WriteLine($"Win32 initialized: hWnd=0x{hWnd:x8}");

            var hr = GraphicsDevice.Initialize(hWnd);
            WriteLine($"D2D initialized: HR=0x{hr:x8}");

            GameControllers.Initialize(hWnd);

            WriteLine("Win32 processing events...");
            Win32_ProcessEvents(hWnd, startMaximized ? 3 /*SW_MAXIMIZE*/ : 1 /*SW_SHOWNORMAL*/);
            WriteLine("Win32 processing events completed");

            WriteLine("Shutting down game controllers, D2D, Win32...");

            GameControllers.Shutdown();
            GraphicsDevice.Shutdown();
            Win32_Quit();

            WriteLine("Done");
        }
    }
}
