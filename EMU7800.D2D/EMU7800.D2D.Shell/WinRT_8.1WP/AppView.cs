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

        bool _windowClosed, _windowVisible;
        int _lastMouseX, _lastMouseY;
        uint _lastMousePointerId;

        const int
            DXGI_MODE_ROTATION_UNSPECIFIED = 0,
            DXGI_MODE_ROTATION_IDENTITY    = 1,
            DXGI_MODE_ROTATION_ROTATE90    = 2,
            DXGI_MODE_ROTATION_ROTATE180   = 3,
            DXGI_MODE_ROTATION_ROTATE270   = 4;

        static readonly MogaController _mogaController = new MogaController();

        #endregion

        public static MogaController MogaController { get { return _mogaController; } }

        public static FileOpenPickerContinuationEventArgs CapturedFileOpenPickerContinuationEventArgs { get; set; }

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
            window.PointerMoved += (sender, args) => WindowOnPointerMoved(args);
            window.PointerPressed += (sender, args) => WindowOnPointerChanged(args, true);
            window.PointerReleased += (sender, args) => WindowOnPointerChanged(args, false);

            window.Closed += WindowOnClosed;
            window.VisibilityChanged += WindowOnVisibilityChanged;

            var displayInformation = DisplayInformation.GetForCurrentView();

            DisplayInformation.DisplayContentsInvalidated += DisplayInformationOnDisplayContentsInvalidated;

            var size = Struct.ToSizeF((float)window.Bounds.Width, (float)window.Bounds.Height);
            _pageBackStack.Resized(size);

            var logicalDpi = displayInformation.LogicalDpi;
            var dxgiModeRotation = ComputeDxgiModeRotation(displayInformation);
            _graphicsDevice.Initialize(window, logicalDpi, dxgiModeRotation);
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
            _mogaController.Closing();
        }

        #region CoreAppWin Event Handlers

        void ApplicationViewOnActivated(CoreApplicationView sender, IActivatedEventArgs args)
        {
            var continuationEventArgs = args as FileOpenPickerContinuationEventArgs;
            if (CapturedFileOpenPickerContinuationEventArgs == null && continuationEventArgs != null)
            {
                CapturedFileOpenPickerContinuationEventArgs = continuationEventArgs;
            }

            var coreWindow = CoreWindow.GetForCurrentThread();
            coreWindow.Activated += CoreWindowOnActivated;
            coreWindow.Activate();

            _mogaController.Launching();
        }

        void WindowOnPointerMoved(PointerEventArgs args)
        {
            var current = args.CurrentPoint;
            var pointerId = current.PointerId;
            var pos = current.Position;
            var x = (int)pos.X;
            var y = (int)pos.Y;

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
            var pointerId = current.PointerId;
            var pos = current.Position;
            var x = (int)pos.X;
            var y = (int)pos.Y;
            _pageBackStack.MouseButtonChanged(pointerId, x, y, down);
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
                    System.Diagnostics.Debug.WriteLine("EMU7800.D2D.Shell.WinRT.AppView.CoreWindowOnActivated: Deactivated: PageBackStack.OnNavigatingAway(): WindowActivationState={0}", args.WindowActivationState);
                    break;
                case CoreWindowActivationState.CodeActivated:
                case CoreWindowActivationState.PointerActivated:
                    _pageBackStack.OnNavigatingHere();
                    System.Diagnostics.Debug.WriteLine("EMU7800.D2D.Shell.WinRT.AppView.CoreWindowOnActivated: CodeActivated/PointerActivated: PageBackStack.OnNavigatingHere(): WindowActivationState={0}", args.WindowActivationState);
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
                    System.Diagnostics.Debug.WriteLine("EMU7800.D2D.Shell.WinRT.AppView.WindowOnVisibilityChanged: IsVisible=true: PageBackStack.OnNavigatingHere(): Visible={0}", args.Visible);
                    break;
                case false:
                    _pageBackStack.OnNavigatingAway();
                    System.Diagnostics.Debug.WriteLine("EMU7800.D2D.Shell.WinRT.AppView.WindowOnVisibilityChanged: IsVisible=false: PageBackStack.OnNavigatingAway(): Visible={0}", args.Visible);
                    break;
            }
        }

        void CoreApplicationOnResuming(object sender, object o)
        {
            _mogaController.Activated();
            _pageBackStack.OnNavigatingHere();
            System.Diagnostics.Debug.WriteLine("EMU7800.D2D.Shell.WinRT.AppView.CoreApplicationOnResuming: PageBackStack.OnNavigatingHere()");
        }

        void CoreApplicationOnSuspending(object sender, SuspendingEventArgs suspendingEventArgs)
        {
            _mogaController.Deactivated();
            _pageBackStack.OnNavigatingAway();
            _graphicsDevice.Trim(); // Windows App Certification Kit 3.1 requirement
            System.Diagnostics.Debug.WriteLine("EMU7800.D2D.Shell.WinRT.AppView.CoreApplicationOnSuspending: PageBackStack.OnNavigatingAway()");
        }

        void WindowOnClosed(CoreWindow sender, CoreWindowEventArgs args)
        {
            _windowClosed = true;
            _mogaController.Closing();
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

        static int ComputeDxgiModeRotation(DisplayInformation displayInformation)
        {
            switch (displayInformation.NativeOrientation)
            {
                case DisplayOrientations.Landscape:
                    switch (displayInformation.CurrentOrientation)
                    {
                        case DisplayOrientations.Landscape:
                            return DXGI_MODE_ROTATION_IDENTITY;
                        case DisplayOrientations.Portrait:
                            return DXGI_MODE_ROTATION_ROTATE270;
                        case DisplayOrientations.LandscapeFlipped:
                            return DXGI_MODE_ROTATION_ROTATE180;
                        case DisplayOrientations.PortraitFlipped:
                            return DXGI_MODE_ROTATION_ROTATE90;
                    }
                    break;
                case DisplayOrientations.Portrait:
                    switch (displayInformation.CurrentOrientation)
                    {
                        case DisplayOrientations.Landscape:
                            return DXGI_MODE_ROTATION_ROTATE90;
                        case DisplayOrientations.Portrait:
                            return DXGI_MODE_ROTATION_IDENTITY;
                        case DisplayOrientations.LandscapeFlipped:
                            return DXGI_MODE_ROTATION_ROTATE270;
                        case DisplayOrientations.PortraitFlipped:
                            return DXGI_MODE_ROTATION_ROTATE180;
                    }
                    break;
            }
            return DXGI_MODE_ROTATION_UNSPECIFIED;
        }

        #endregion
    }
}
