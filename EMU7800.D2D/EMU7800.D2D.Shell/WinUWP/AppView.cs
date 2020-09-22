// © Mike Murphy

using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Graphics.Display;
using Windows.UI.Core;
using EMU7800.D2D.Interop;

namespace EMU7800.D2D.Shell.WinRT
{
    public sealed class AppView : IFrameworkView
    {
        #region Fields

        readonly TimerDevice _timerDevice = new TimerDevice();
        readonly PageBackStackHost _pageBackStack;
        readonly GraphicsDevice _graphicsDevice;

        readonly bool[] _lastKeyInput = new bool[0x100];

        bool _windowClosed, _windowVisible;
        int _lastMouseX, _lastMouseY;
        int _lastMousePointerId;
        float _scaleFactor;

        #endregion

        public AppView()
        {
            _pageBackStack = new PageBackStackHost(new TitlePage());
            _graphicsDevice = new GraphicsDevice();
        }

        public void Initialize(CoreApplicationView applicationView)
        {
            applicationView.Activated += ApplicationViewOnActivated;
            CoreApplication.Resuming += CoreApplicationOnResuming;
            CoreApplication.Suspending += CoreApplicationOnSuspending;
        }

        public void SetWindow(CoreWindow window)
        {
            window.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);

            window.KeyDown += (sender, args) => WindowOnKeyChanged(args, true);
            window.KeyUp += (sender, args) => WindowOnKeyChanged(args, false);
            window.PointerMoved += (sender, args) => WindowOnPointerMoved(args);
            window.PointerPressed += (sender, args) => WindowOnPointerChanged(args, true);
            window.PointerReleased += (sender, args) => WindowOnPointerChanged(args, false);
            window.PointerWheelChanged += WindowOnPointerWheelChanged;
            window.SizeChanged += WindowOnSizeChanged;

            window.Closed += WindowOnClosed;
            window.VisibilityChanged += WindowOnVisibilityChanged;

            DisplayInformation.DisplayContentsInvalidated += DisplayInformationOnDisplayContentsInvalidated;

            var displayInformation = DisplayInformation.GetForCurrentView();
            displayInformation.DpiChanged += DisplayInformationOnDpiChanged;

            _scaleFactor = GetScaleFactor(window.Bounds.Width);

            var size = Struct.ToSizeF((float)window.Bounds.Width * _scaleFactor, (float)window.Bounds.Height * _scaleFactor);
            _pageBackStack.Resized(size);

            var logicalDpi = displayInformation.LogicalDpi / _scaleFactor;
            _graphicsDevice.Initialize(window, logicalDpi, 0);
        }

        public void Load(string entryPoint)
        {
        }

        public void Run()
        {
            var coreWindow = CoreWindow.GetForCurrentThread();
            var dispatcher = coreWindow.Dispatcher;

            while (!_windowClosed)
            {
                if (_windowVisible)
                {
                    dispatcher.ProcessEvents(CoreProcessEventsOption.ProcessAllIfPresent);
                    RunOneLURCycle();
                    _graphicsDevice.Present();
                    _timerDevice.Update();
                }
                else
                {
                    dispatcher.ProcessEvents(CoreProcessEventsOption.ProcessOneAndAllPending);
                }
            }
        }

        public void Uninitialize()
        {
        }

        #region CoreAppWin Event Handlers

        void ApplicationViewOnActivated(CoreApplicationView sender, IActivatedEventArgs args)
        {
            var coreWindow = CoreWindow.GetForCurrentThread();
            coreWindow.Activated += CoreWindowOnActivated;
            coreWindow.Activate();
        }

        void WindowOnPointerMoved(PointerEventArgs args)
        {
            var current = args.CurrentPoint;
            var pointerId = (int)current.PointerId;
            var pos = current.Position;
            var x = (int)(pos.X * _scaleFactor);
            var y = (int)(pos.Y * _scaleFactor);

            if (_lastMousePointerId != pointerId)
            {
                _lastMouseX = x;
                _lastMouseY = y;
            }
            var dx = x - _lastMouseX;
            var dy = y - _lastMouseY;

            _lastMousePointerId = pointerId;
            _lastMouseX = x;
            _lastMouseY = y;

            _pageBackStack.MouseMoved(pointerId, x, y, dx, dy);
        }

        void WindowOnPointerChanged(PointerEventArgs args, bool down)
        {
            var current = args.CurrentPoint;
            var pointerId = (int)current.PointerId;
            var pos = current.Position;
            var x = (int)(pos.X * _scaleFactor);
            var y = (int)(pos.Y * _scaleFactor);
            _pageBackStack.MouseButtonChanged(pointerId, x, y, down);
        }

        void WindowOnKeyChanged(KeyEventArgs args, bool down)
        {
            var lastDown = _lastKeyInput[(int)args.VirtualKey & 0xff];
            if (down == lastDown)
                return;
            _lastKeyInput[(int)args.VirtualKey & 0xff] = down;
            _pageBackStack.KeyboardKeyPressed((KeyboardKey)args.VirtualKey, down);
        }

        void WindowOnPointerWheelChanged(CoreWindow sender, PointerEventArgs args)
        {
            var current = args.CurrentPoint;
            var pointerId = (int)current.PointerId;
            var pos = current.Position;
            var x = (int)(pos.X * _scaleFactor);
            var y = (int)(pos.Y * _scaleFactor);
            var delta = current.Properties.MouseWheelDelta;
            _pageBackStack.MouseWheelChanged(pointerId, x, y, delta);
        }

        void WindowOnSizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
        {
            _scaleFactor = GetScaleFactor(args.Size.Width);

            var size = Struct.ToSizeF((float)args.Size.Width * _scaleFactor, (float)args.Size.Height * _scaleFactor);
            _pageBackStack.Resized(size);

            _graphicsDevice.UpdateForWindowSizeChange();

            var resizeManager = CoreWindowResizeManager.GetForCurrentView();
            resizeManager.NotifyLayoutCompleted();
        }

        void DisplayInformationOnDpiChanged(DisplayInformation displayInformation, object args)
        {
            var logicalDpi = displayInformation.LogicalDpi / _scaleFactor;
            _graphicsDevice.SetDpi(logicalDpi);
        }

        void DisplayInformationOnDisplayContentsInvalidated(DisplayInformation displayInformation, object args)
        {
            _graphicsDevice.ValidateDevice();
        }

        void CoreWindowOnActivated(CoreWindow sender, WindowActivatedEventArgs args)
        {
            switch (args.WindowActivationState)
            {
                case CoreWindowActivationState.Deactivated:
                    _pageBackStack.OnNavigatingAway();
                    System.Diagnostics.Debug.WriteLine("EMU7800.D2D.Shell.WinRT.AppView.CoreWindowOnActivated: Deactivated: PageBackStack.OnNavigatingAway()");
                    break;
                case CoreWindowActivationState.CodeActivated:
                case CoreWindowActivationState.PointerActivated:
                    _pageBackStack.OnNavigatingHere();
                    System.Diagnostics.Debug.WriteLine("EMU7800.D2D.Shell.WinRT.AppView.CoreWindowOnActivated: CodeActivated/PointerActivated: PageBackStack.OnNavigatingHere()");
                    break;
            }
            args.Handled = true;
        }

        void WindowOnVisibilityChanged(CoreWindow sender, VisibilityChangedEventArgs args)
        {
            _windowVisible = args.Visible;
            switch (_windowVisible)
            {
                case true:
                    _pageBackStack.OnNavigatingHere();
                    System.Diagnostics.Debug.WriteLine("EMU7800.D2D.Shell.WinRT.AppView.WindowOnVisibilityChanged: IsVisible=true: PageBackStack.OnNavigatingHere()");
                    break;
                case false:
                    _pageBackStack.OnNavigatingAway();
                    System.Diagnostics.Debug.WriteLine("EMU7800.D2D.Shell.WinRT.AppView.WindowOnVisibilityChanged: IsVisible=false: PageBackStack.OnNavigatingAway()");
                    break;
            }
        }

        void CoreApplicationOnResuming(object sender, object o)
        {
            _pageBackStack.OnNavigatingHere();
            System.Diagnostics.Debug.WriteLine("EMU7800.D2D.Shell.WinRT.AppView.CoreApplicationOnResuming: PageBackStack.OnNavigatingHere()");
        }

        void CoreApplicationOnSuspending(object sender, SuspendingEventArgs suspendingEventArgs)
        {
            _pageBackStack.OnNavigatingAway();
            System.Diagnostics.Debug.WriteLine("EMU7800.D2D.Shell.WinRT.AppView.CoreApplicationOnSuspending: PageBackStack.OnNavigatingAway()");

            // Windows App Certification Kit 3.1 requirement
            _graphicsDevice.Trim();
        }

        void WindowOnClosed(CoreWindow sender, CoreWindowEventArgs args)
        {
            _windowClosed = true;
        }

        #endregion

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

        float GetScaleFactor(double width)
        {
            // This application has an assumption the width will be at least 800 pixels.
            // WinX scaling on mobile devices can make this width less than this.
            // In these situations, apply a scaling factor to reach this minimum.
            return width < 800f ? 800f / (float)width : 1f;
        }

        #endregion
    }
}
