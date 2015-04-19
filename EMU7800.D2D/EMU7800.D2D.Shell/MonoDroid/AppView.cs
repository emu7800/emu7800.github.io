using Android.Content;
using Android.Views;
using EMU7800.D2D.Interop;
using EMU7800.D2D.Shell;
using OpenTK;
using OpenTK.Platform.Android;
using System;

namespace EMU7800.D2D
{
    public sealed class AppView : AndroidGameView
    {
        #region Fields

        readonly TimerDevice _timerDevice = new TimerDevice();
        readonly PageBackStackHost _pageBackStack;
        readonly GraphicsDevice _graphicsDevice;

        readonly bool[] _lastKeyInput = new bool[0x100];

        bool _windowClosed;
        int _lastMouseX, _lastMouseY;
        uint _lastMousePointerId;

        #endregion

        public AppView(Context context) : base(context)
        {
            _pageBackStack = new PageBackStackHost(new TitlePage());
            _graphicsDevice = new GraphicsDevice();
        }

        protected override void CreateFrameBuffer()
        {
            _graphicsDevice.Initialize(this);
            base.CreateFrameBuffer();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Run();
        }

        protected override void OnResize(EventArgs e)
        {
            var size = Struct.ToSizeF(Width, Height);
            _pageBackStack.Resized(size);

            _graphicsDevice.UpdateForWindowSizeChange();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            if (Visible)
            {
                RunOneLURCycle();
                _graphicsDevice.Present();
                _timerDevice.Update();
            }
        }

        public override bool OnGenericMotionEvent(MotionEvent e)
        {
            return true;
        }

        public override bool OnKeyUp(Keycode keyCode, KeyEvent e)
        {
            return OnKeyChanged(keyCode, e, false);
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            return OnKeyChanged(keyCode, e, true);
        }

        #region Helpers

        bool OnKeyChanged(Keycode keyCode, KeyEvent e, bool down)
        {
            var virtualKey = ToKeyboardKey(keyCode);
            var lastDown = _lastKeyInput[(int)virtualKey & 0xff];
            if (down != lastDown)
            {
                _lastKeyInput[(int)virtualKey & 0xff] = down;
                _pageBackStack.KeyboardKeyPressed(virtualKey, down);
            }
            return true;
        }

        void RunOneLURCycle()
        {
            _pageBackStack.StartOfCycle();

            if (_graphicsDevice.IsDeviceResourcesRefreshed)
            {
                _graphicsDevice.IsDeviceResourcesRefreshed = false;
                _pageBackStack.LoadResources(_graphicsDevice);
            }

            _pageBackStack.Update(_timerDevice);

            _graphicsDevice.BeginDraw();
            _pageBackStack.Render(_graphicsDevice);
            _graphicsDevice.EndDraw();
        }

        KeyboardKey ToKeyboardKey(Keycode keyCode)
        {
            switch (keyCode)
            {
                case Keycode.Z:                 return KeyboardKey.Z;
                case Keycode.X:                 return KeyboardKey.X;
                case Keycode.DpadLeft:          return KeyboardKey.Left;
                case Keycode.DpadUp:            return KeyboardKey.Up;
                case Keycode.DpadRight:         return KeyboardKey.Right;
                case Keycode.DpadDown:          return KeyboardKey.Down;
                case Keycode.Num0:              return KeyboardKey.Number0;
                case Keycode.Num1:              return KeyboardKey.Number1;
                case Keycode.Num2:              return KeyboardKey.Number2;
                case Keycode.Num3:              return KeyboardKey.Number3;
                case Keycode.Num4:              return KeyboardKey.Number4;
                case Keycode.Num5:              return KeyboardKey.Number5;
                case Keycode.Num6:              return KeyboardKey.Number6;
                case Keycode.Num7:              return KeyboardKey.Number7;
                case Keycode.Num8:              return KeyboardKey.Number8;
                case Keycode.Num9:              return KeyboardKey.Number9;
                case Keycode.Numpad0:           return KeyboardKey.NumberPad0;
                case Keycode.Numpad1:           return KeyboardKey.NumberPad1;
                case Keycode.Numpad2:           return KeyboardKey.NumberPad2;
                case Keycode.Numpad3:           return KeyboardKey.NumberPad3;
                case Keycode.Numpad4:           return KeyboardKey.NumberPad4;
                case Keycode.Numpad5:           return KeyboardKey.NumberPad5;
                case Keycode.Numpad6:           return KeyboardKey.NumberPad6;
                case Keycode.Numpad7:           return KeyboardKey.NumberPad7;
                case Keycode.Numpad8:           return KeyboardKey.NumberPad8;
                case Keycode.Numpad9:           return KeyboardKey.NumberPad9;
                case Keycode.NumpadMultiply:    return KeyboardKey.Multiply;
                case Keycode.NumpadDivide:      return KeyboardKey.Divide;
                case Keycode.NumpadAdd:         return KeyboardKey.Add;
                case Keycode.Q:                 return KeyboardKey.Q;
                case Keycode.W:                 return KeyboardKey.W;
                case Keycode.E:                 return KeyboardKey.E;
                case Keycode.H:                 return KeyboardKey.H;
                case Keycode.P:                 return KeyboardKey.P;
                case Keycode.F1:                return KeyboardKey.F1;
                case Keycode.F2:                return KeyboardKey.F2;
                case Keycode.F3:                return KeyboardKey.F3;
                case Keycode.F4:                return KeyboardKey.F4;
                default:                        return KeyboardKey.None;
            }
        }

        #endregion
    }
}