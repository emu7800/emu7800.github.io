// © Mike Murphy

using static EMU7800.Win32.Interop.Win32NativeMethods;

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
        public static readonly KeyboardKeyPressedHandler KeyboardKeyPressedHandlerDefault = (vk, d) => {};
        public static readonly MouseMovedHandler MouseMovedHandlerDefault = (x, y, dx, dy) => {};
        public static readonly MouseButtonChangedHandler MouseButtonChangedHandlerDefault = (x, y, down) => {};
        public static readonly MouseWheelChangedHandler MouseWheelChangedHandlerDefault = (x, y, delta) => {};
        public static readonly LURCycleHandler LURCycleHandlerDefault = () => {};
        public static readonly VisibilityChangedHandler VisibilityChangedHandlerDefault = iv => {};
        public static readonly ResizedHandler ResizeHandlerDefault = (w, h) => {};

        public static D2D.Interop.GraphicsDevice GraphicsDevice = new();
        public static System.IntPtr hWnd;

        public static KeyboardKeyPressedHandler KeyboardKeyPressed { get; set; } = KeyboardKeyPressedHandlerDefault;
        public static MouseMovedHandler MouseMoved { get; set; } = MouseMovedHandlerDefault;
        public static MouseButtonChangedHandler MouseButtonChanged { get; set; } = MouseButtonChangedHandlerDefault;
        public static MouseWheelChangedHandler MouseWheelChanged { get; set; } = MouseWheelChangedHandlerDefault;
        public static LURCycleHandler LURCycle { get; set; } = LURCycleHandlerDefault;
        public static VisibilityChangedHandler VisibilityChanged { get; set; } = VisibilityChangedHandlerDefault;
        public static ResizedHandler Resized { get; set; } = ResizeHandlerDefault;

        public static void Initialize()
        {
            GraphicsDevice.Initialize();
            Resized += (w, h) => GraphicsDevice.Resize(w, h);
            hWnd = Win32_Initialize();
            GraphicsDevice.AttachHwnd(hWnd);
        }

        public static void ProcessEvents()
            => Win32_ProcessEvents(hWnd);

        public static void Quit()
        {
            Win32_Quit();
            KeyboardKeyPressed = KeyboardKeyPressedHandlerDefault;
            MouseMoved         = MouseMovedHandlerDefault;
            MouseButtonChanged = MouseButtonChangedHandlerDefault;
            MouseWheelChanged  = MouseWheelChangedHandlerDefault;
            LURCycle           = LURCycleHandlerDefault;
            VisibilityChanged  = VisibilityChangedHandlerDefault;
            Resized            = ResizeHandlerDefault;
        }

        internal static KeyboardKeyPressedHandler RaiseKeyboardKeyPressedDelegate = new(RaiseKeyboardKeyPressed);
        internal static void RaiseKeyboardKeyPressed(ushort vKey, bool down) => KeyboardKeyPressed(vKey, down);

        internal static MouseMovedHandler RaiseMouseMovedDelegate = new(RaiseMouseMoved);
        internal static void RaiseMouseMoved(int x, int y, int dx, int dy) => MouseMoved(x, y, dx, dy);

        internal static MouseButtonChangedHandler RaiseMouseButtonChangedDelegate = new(RaiseMouseButtonChanged);
        internal static void RaiseMouseButtonChanged(int x, int y, bool down) => MouseButtonChanged(x, y, down);

        internal static MouseWheelChangedHandler RaiseMouseWheelChangedDelegate = new(RaiseMouseWheelChanged);
        internal static void RaiseMouseWheelChanged(int x, int y, int delta) => MouseWheelChanged(x, y, delta);

        internal static LURCycleHandler RaiseLURCycleDelegate = new(RaiseLURCycle);
        internal static void RaiseLURCycle() => LURCycle();

        internal static VisibilityChangedHandler RaiseVisibilityChangedDelegate = new(RaiseVisibilityChanged);
        internal static void RaiseVisibilityChanged(bool isVisible) => VisibilityChanged(isVisible);

        internal static ResizedHandler RaiseResizedDelegate = new(RaiseResized);
        internal static void RaiseResized(int w, int h) => Resized(w, h);
    }
}
