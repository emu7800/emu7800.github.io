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

        public override bool OnTouchEvent(MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                case MotionEventActions.Up:
                    _pageBackStack.MouseButtonChanged((uint)e.DeviceId, (int)e.RawX, (int)e.RawY, e.Action == MotionEventActions.Down);
                    break;
                case MotionEventActions.Move:
                    _pageBackStack.MouseMoved((uint)e.DeviceId, (int)e.RawX, (int)e.RawY, 1, 1);
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("Action:{0} DeviceId:{1} XY:{2} {3}",
                        e.Action,
                        e.DeviceId,
                        e.RawX,
                        e.RawY);
                    break;
            }
            return true;
        }

        public void KeyboardKeyPressed(KeyboardKey key, bool down)
        {
            _pageBackStack.KeyboardKeyPressed(key, down);
        }

        #region Helpers

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

        #endregion
    }
}