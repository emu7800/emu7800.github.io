// © Mike Murphy

using EMU7800.Services;
using EMU7800.Services.Dto;
using EMU7800.Win32.Interop;
using System;
using System.Diagnostics;

#pragma warning disable CA1822 // Mark members as static

namespace EMU7800.D2D.Shell.Win32
{
    public sealed class Win32App : IDisposable
    {
        readonly TimerDevice _timerDevice = new();
        readonly PageBackStackHost _pageBackStack;
        readonly bool[] _lastKeyInput = new bool[0x100];

        bool _resourcesLoaded;

        public Win32App()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            _pageBackStack = new PageBackStackHost(new TitlePage());
            WireUpEvents();
        }

        public Win32App(GameProgramInfoViewItem gpivi)
        {
            _pageBackStack = new PageBackStackHost(new GamePage(gpivi, true));
            WireUpEvents();
        }

        public void Run(bool startMaximized = true)
            => Win32Window.Run(startMaximized);

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                _pageBackStack?.Dispose();
            }
        }

        #endregion

        void WireUpEvents()
        {
            Win32Window.LURCycle           += LURCycle;
            Win32Window.VisibilityChanged  += VisibilityChanged;
            Win32Window.Resized            += (w, h)         => _pageBackStack.Resized(new(w, h));
            Win32Window.KeyboardKeyPressed += KeyboardKeyPressed;
            Win32Window.MouseMoved         += (x, y, dx, dy) => _pageBackStack.MouseMoved(0, x, y, dx, dy);
            Win32Window.MouseButtonChanged += (x, y, down)   => _pageBackStack.MouseButtonChanged(0, x, y, down);
            Win32Window.MouseWheelChanged  += (x, y, delta)  => _pageBackStack.MouseWheelChanged(0, x, y, delta);
            Win32Window.DeviceChanged      += () => GameControllers.Initialize();

            foreach (var gc in GameControllers.Controllers)
            {
                gc.ButtonChanged          += (cn, mi, down)  => _pageBackStack.ControllerButtonChanged(cn, mi, down);
                gc.PaddlePositionChanged  += (cn, pn, o)     => _pageBackStack.PaddlePositionChanged(cn, pn, o);
                gc.DrivingPositionChanged += (cn, mi)        => _pageBackStack.DrivingPositionChanged(cn, mi);
            }
        }

        void LURCycle()
        {
            _pageBackStack.StartOfCycle();

            if (!_resourcesLoaded)
            {
                _pageBackStack.LoadResources();
                _resourcesLoaded = true;
            }

            GameControllers.Poll();

            _pageBackStack.Update(_timerDevice);

            GraphicsDevice.BeginDraw();
            _pageBackStack.Render();
            GraphicsDevice.EndDraw();

            _timerDevice.Update();
        }

        void VisibilityChanged(bool isVisible)
        {
            if (isVisible)
                _pageBackStack.OnNavigatingHere();
            else
                _pageBackStack.OnNavigatingAway();
        }

        void KeyboardKeyPressed(ushort vkey, bool down)
        {
            var lastDown = _lastKeyInput[vkey & 0xff];
            if (down == lastDown)
                return;
            _lastKeyInput[vkey & 0xff] = down;
            _pageBackStack.KeyboardKeyPressed((KeyboardKey)vkey, down);
        }

        static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                DatastoreService.DumpCrashReport(ex);
            if (Debugger.IsAttached)
                Debugger.Break();
        }
    }
}
