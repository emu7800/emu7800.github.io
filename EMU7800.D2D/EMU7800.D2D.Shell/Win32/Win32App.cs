// © Mike Murphy

using System;
using System.Diagnostics;
using EMU7800.D2D.Interop;
using EMU7800.Services;
using EMU7800.Services.Dto;

namespace EMU7800.D2D.Shell.Win32
{
    public sealed class Win32App : IDisposable
    {
        readonly TimerDevice _timerDevice = new TimerDevice();
        readonly PageBackStackHost _pageBackStack;
        readonly Win32Window _win;

        readonly bool[] _lastKeyInput = new bool[0x100];

        public Win32App(Win32Window win)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            _pageBackStack = new PageBackStackHost(new TitlePage());

            _win = win;
            _win.LURCycle += LURCycle;
            _win.VisibilityChanged += VisibilityChanged;
            _win.Resized += Resized;
            _win.KeyboardKeyPressed += KeyboardKeyPressed;
            _win.MouseMoved += MouseMoved;
            _win.MouseButtonChanged += MouseButtonChanged;
            _win.MouseWheelChanged += MouseWheelChanged;
        }

        public Win32App(Win32Window win, GameProgramInfoViewItem gpivi)
        {
            _pageBackStack = new PageBackStackHost(new GamePage(gpivi, true));

            _win = win;
            _win.LURCycle += LURCycle;
            _win.VisibilityChanged += VisibilityChanged;
            _win.Resized += Resized;
            _win.KeyboardKeyPressed += KeyboardKeyPressed;
            _win.MouseMoved += MouseMoved;
            _win.MouseButtonChanged += MouseButtonChanged;
            _win.MouseWheelChanged += MouseWheelChanged;
        }

        public void Run()
        {
            _win.ProcessEvents();
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_pageBackStack != null)
                    _pageBackStack.Dispose();
            }
        }

        #endregion

        void LURCycle(GraphicsDevice gd)
        {
            _pageBackStack.StartOfCycle();

            if (gd.IsDeviceResourcesRefreshed)
            {
                gd.IsDeviceResourcesRefreshed = false;
                _pageBackStack.LoadResources(gd);
            }

            _pageBackStack.Update(_timerDevice);

            gd.BeginDraw();
            _pageBackStack.Render(gd);
            gd.EndDraw();

            _timerDevice.Update();
        }

        void VisibilityChanged(bool isVisible)
        {
            if (isVisible)
                _pageBackStack.OnNavigatingHere();
            else
                _pageBackStack.OnNavigatingAway();
        }

        void Resized(SizeU size)
        {
            var sizef = Struct.ToSizeF(size.Width, size.Height);
            _pageBackStack.Resized(sizef);
        }

        void KeyboardKeyPressed(ushort vkey, bool down)
        {
            var lastDown = _lastKeyInput[vkey & 0xff];
            if (down == lastDown)
                return;
            _lastKeyInput[vkey & 0xff] = down;
            _pageBackStack.KeyboardKeyPressed((KeyboardKey)vkey, down);
        }

        void MouseMoved(int x, int y, int dx, int dy)
        {
            _pageBackStack.MouseMoved(0, x, y, dx, dy);
        }

        void MouseButtonChanged(int x, int y, bool down)
        {
            _pageBackStack.MouseButtonChanged(0, x, y, down);
        }

        void MouseWheelChanged(int x, int y, int delta)
        {
            _pageBackStack.MouseWheelChanged(0, x, y, delta);
        }

        static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var datastore = new DatastoreService();
            DatastoreService.DumpCrashReport(e.ExceptionObject as Exception);
            if (Debugger.IsAttached)
                Debugger.Break();
        }
    }
}
