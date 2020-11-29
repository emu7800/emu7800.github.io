using EMU7800.D2D.Interop;
using System;

namespace EMU7800.Win32.Interop
{
    public delegate void KeyboardKeyPressedHandler(ushort vKey, bool down);
    public delegate void MouseMovedHandler(int x, int y, int dx, int dy);
    public delegate void MouseButtonChangedHandler(int x, int y, bool down);
    public delegate void MouseWheelChangedHandler(int x, int y, int delta);
    public delegate void LURCycleHandler();
    public delegate void VisibilityChangedHandler(bool isVisible);
    public delegate void ResizeHandler(SizeU size);

    public class Win32Window
    {
        public static readonly KeyboardKeyPressedHandler KeyboardKeyPressedHandlerDefault = (vk, d) => {};
        public static readonly MouseMovedHandler MouseMovedHandlerDefault = (x, y, dx, dy) => {};
        public static readonly MouseButtonChangedHandler MouseButtonChangedHandlerDefault = (x, y, down) => {};
        public static readonly MouseWheelChangedHandler MouseWheelChangedHandlerDefault = (x, y, delta) => {};
        public static readonly LURCycleHandler LURCycleHandlerDefault = () => {};
        public static readonly VisibilityChangedHandler VisibilityChangedHandlerDefault = iv => {};
        public static readonly ResizeHandler ResizeHandlerDefault = s => {};

        #region Fields

        readonly GraphicsDevice _graphicsDevice = new();
        readonly IntPtr _hWnd;

        #endregion

        public KeyboardKeyPressedHandler KeyboardKeyPressed { get; set; } = KeyboardKeyPressedHandlerDefault;
        public MouseMovedHandler MouseMoved { get; set; } = MouseMovedHandlerDefault;
        public MouseButtonChangedHandler MouseButtonChanged { get; set; } = MouseButtonChangedHandlerDefault;
        public MouseWheelChangedHandler MouseWheelChanged { get; set; } = MouseWheelChangedHandlerDefault;
        public LURCycleHandler LURCycle { get; set; } = LURCycleHandlerDefault;
        public VisibilityChangedHandler VisibilityChanged { get; set; } = VisibilityChangedHandlerDefault;
        public ResizeHandler Resized { get; set; } = ResizeHandlerDefault;

        public void ProcessEvents()
        {
            // call native.process
        }

        public void Quit()
        {
            // call native.quit
        }

        #region Constructors

        public Win32Window()
        {
            // call native.init(...)
            // set hWnd
            _graphicsDevice.Initialize();
            //_graphicsDevice.AttachHwnd(_hWnd);
        }

        #endregion
    }
}
