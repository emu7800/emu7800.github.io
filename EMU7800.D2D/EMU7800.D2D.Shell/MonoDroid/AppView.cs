using Android.Content;
using Android.Util;
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

        bool _windowClosed, _windowVisible;
        int _lastMouseX, _lastMouseY;
        uint _lastMousePointerId;

        #endregion

        public AppView(Context context) : base(context)
        {
            _pageBackStack = new PageBackStackHost(new TitlePage());
            _graphicsDevice = new GraphicsDevice();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Run the render loop
            Run();
        }

        protected override void CreateFrameBuffer()
        {
            //_graphicsDevice.Initialize()

            // the default GraphicsMode that is set consists of (16, 16, 0, 0, 2, false)
            try
            {
                Log.Verbose("GLCube", "Loading with default settings");

                // if you don't call this, the context won't be created
                base.CreateFrameBuffer();
                return;
            }
            catch (Exception ex)
            {
                Log.Verbose("GLCube", "{0}", ex);
            }

            // this is a graphics setting that sets everything to the lowest mode possible so
            // the device returns a reliable graphics setting.
            try
            {
                Log.Verbose("GLCube", "Loading with custom Android settings (low mode)");
                GraphicsMode = new AndroidGraphicsMode(0, 0, 0, 0, 0, false);

                // if you don't call this, the context won't be created
                base.CreateFrameBuffer();
                return;
            }
            catch (Exception ex)
            {
                Log.Verbose("GLCube", "{0}", ex);
            }
            throw new Exception("Can't load egl, aborting");
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            if (_windowVisible)
            {
                RunOneLURCycle();
                _graphicsDevice.Present();
                _timerDevice.Update();
            }
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