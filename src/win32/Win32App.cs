﻿// © Mike Murphy

using EMU7800.Services;
using EMU7800.Services.Dto;
using EMU7800.Win32.Interop;
using System;
using System.Diagnostics;

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

#pragma warning disable CA1822 // Mark members as static
        public void Run()
            => Win32Window.Run();
#pragma warning restore CA1822 // Mark members as static

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

        void WireUpEvents()
        {
            Win32Window.LURCycle           += LURCycle;
            Win32Window.VisibilityChanged  += VisibilityChanged;
            Win32Window.Resized            += Resized;
            Win32Window.KeyboardKeyPressed += KeyboardKeyPressed;
            Win32Window.MouseMoved         += MouseMoved;
            Win32Window.MouseButtonChanged += MouseButtonChanged;
            Win32Window.MouseWheelChanged  += MouseWheelChanged;
        }

        void LURCycle()
        {
            _pageBackStack.StartOfCycle();

            if (!_resourcesLoaded)
            {
                _pageBackStack.LoadResources();
                _resourcesLoaded = true;
            }

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

        void Resized(int width, int height)
        {
            _pageBackStack.Resized(new(width,height));
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
            if (e.ExceptionObject is Exception ex)
                DatastoreService.DumpCrashReport(ex);
            if (Debugger.IsAttached)
                Debugger.Break();
        }
    }
}