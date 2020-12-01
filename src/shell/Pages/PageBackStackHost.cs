// © Mike Murphy

using EMU7800.Win32.Interop;
using System;

namespace EMU7800.D2D.Shell
{
    public sealed class PageBackStackHost : IDisposable
    {
        #region Fields

        readonly PageBackStackStateService _pageStateService = new();

        PageBase _currentPage = new Nullpage();
        bool _pageChanged;
        D2D_SIZE_F _size;

        #endregion

        public void StartOfCycle()
        {
            if (_pageStateService.IsPagePending)
            {
                _currentPage.OnNavigatingAway();
                _currentPage = _pageStateService.GetPendingPage();
                _currentPage.OnNavigatingHere();
                _currentPage.Resized(_size);
                _pageChanged = true;
            }

            if (_pageStateService.IsDisposablePages)
                DisposeAllDisposings();
        }

        public void OnNavigatingAway()
        {
            _currentPage.OnNavigatingAway();
        }

        public void OnNavigatingHere()
        {
            _currentPage.OnNavigatingHere();
        }

        public void Resized(D2D_SIZE_F size)
        {
            _size = size;
            _currentPage.Resized(size);
        }

        public void KeyboardKeyPressed(KeyboardKey key, bool down)
        {
            _currentPage.KeyboardKeyPressed(key, down);
        }

        public void MouseMoved(int pointerId, int x, int y, int dx, int dy)
        {
            _currentPage.MouseMoved(pointerId, x, y, dx, dy);
        }

        public void MouseButtonChanged(int pointerId, int x, int y, bool down)
        {
            _currentPage.MouseButtonChanged(pointerId, x, y, down);
        }

        public void MouseWheelChanged(int pointerId, int x, int y, int delta)
        {
            _currentPage.MouseWheelChanged(pointerId, x, y, delta);
        }

        public void LoadResources()
        {
            _currentPage.LoadResources();
        }

        public void Update(TimerDevice td)
        {
            _currentPage.Update(td);
        }

        public void Render()
        {
            if (_pageChanged)
            {
                _currentPage.LoadResources();
                _pageChanged = false;
            }
            _currentPage.Render();
        }

        #region Constructors

        public PageBackStackHost(PageBase startPage)
        {
            _pageStateService.Push(startPage);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                while (_pageStateService.Pop())
                {
                }
                DisposeAllDisposings();
                _currentPage.Dispose();
            }
        }

        void DisposeAllDisposings()
        {
            while (_pageStateService.IsDisposablePages)
            {
                var disposingPage = _pageStateService.GetNextDisposablePage();
                disposingPage.Dispose();
            }
        }

        #endregion
    }
}
