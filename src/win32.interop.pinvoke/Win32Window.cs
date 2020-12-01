// © Mike Murphy

using static EMU7800.Win32.Interop.Win32NativeMethods;
using static System.Console;

namespace EMU7800.Win32.Interop
{
    public delegate void KeyboardKeyPressedHandler(ushort vKey, bool down);
    public delegate void MouseMovedHandler(int x, int y, int dx, int dy);
    public delegate void MouseButtonChangedHandler(int x, int y, bool down);
    public delegate void MouseWheelChangedHandler(int x, int y, int delta);
    public delegate void LURCycleHandler();
    public delegate void VisibilityChangedHandler(bool isVisible);
    public delegate void ResizedHandler(int w, int h);

    public static class Win32Window
    {
        static readonly KeyboardKeyPressedHandler KeyboardKeyPressedHandlerDefault = (vk, d) => { };
        static readonly MouseMovedHandler MouseMovedHandlerDefault = (x, y, dx, dy) => { };
        static readonly MouseButtonChangedHandler MouseButtonChangedHandlerDefault = (x, y, down) => { };
        static readonly MouseWheelChangedHandler MouseWheelChangedHandlerDefault = (x, y, delta) => { };
        static readonly LURCycleHandler LURCycleHandlerDefault = () => { };
        static readonly VisibilityChangedHandler VisibilityChangedHandlerDefault = iv => { };
        static readonly ResizedHandler ResizeHandlerDefault = (w, h) => { };

        static System.IntPtr hWnd;

        public static KeyboardKeyPressedHandler KeyboardKeyPressed { get; set; } = KeyboardKeyPressedHandlerDefault;
        public static MouseMovedHandler MouseMoved { get; set; } = MouseMovedHandlerDefault;
        public static MouseButtonChangedHandler MouseButtonChanged { get; set; } = MouseButtonChangedHandlerDefault;
        public static MouseWheelChangedHandler MouseWheelChanged { get; set; } = MouseWheelChangedHandlerDefault;
        public static LURCycleHandler LURCycle { get; set; } = LURCycleHandlerDefault;
        public static VisibilityChangedHandler VisibilityChanged { get; set; } = VisibilityChangedHandlerDefault;
        public static ResizedHandler Resized { get; set; } = ResizeHandlerDefault;

        public static void ClearEventHandlers()
        {
            KeyboardKeyPressed = KeyboardKeyPressedHandlerDefault;
            MouseMoved         = MouseMovedHandlerDefault;
            MouseButtonChanged = MouseButtonChangedHandlerDefault;
            MouseWheelChanged  = MouseWheelChangedHandlerDefault;
            LURCycle           = LURCycleHandlerDefault;
            VisibilityChanged  = VisibilityChangedHandlerDefault;
            Resized            = ResizeHandlerDefault;
        }

        public static void Run()
        {
            Resized += (w, h) => GraphicsDevice.Resize(new(w, h));

            hWnd = Win32_Initialize();
            WriteLine($"Win32 initialized: hWnd=0x{hWnd:x8}");

            var hr = GraphicsDevice.Initialize(hWnd);
            WriteLine($"D2D initialized: HR=0x{hr:x8}");

            WriteLine("Win32 processing events...");
            Win32_ProcessEvents(hWnd);
            WriteLine("Win32 processing events completed");

            WriteLine("Shutting down D2D...");
            GraphicsDevice.Shutdown();

            WriteLine("Shutting down Win32...");
            Win32_Quit();

            WriteLine("Done");
        }
    }
}
