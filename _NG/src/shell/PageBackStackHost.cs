// © Mike Murphy

using System;
using EMU7800.D2D.Interop;

namespace EMU7800.D2D.Shell
{
    public sealed class PageBackStackHost : IDisposable
    {
        #region Fields

        readonly PageBackStackStateService _pageStateService = new PageBackStackStateService();

        PageBase _currentPage = new Nullpage();
        bool _pageChanged;
        SizeF _size;

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

        public void Resized(SizeF size)
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

        public void LoadResources(GraphicsDevice gd)
        {
            _currentPage.LoadResources(gd);
        }

        public void Update(TimerDevice td)
        {
            _currentPage.Update(td);
        }

        public void Render(GraphicsDevice gd)
        {
            if (_pageChanged)
            {
                _currentPage.LoadResources(gd);
                _pageChanged = false;
            }
            _currentPage.Render(gd);
        }

        #region Constructors

        public PageBackStackHost(PageBase startPage)
        {
            _pageStateService.Push(startPage ?? throw new ArgumentNullException(nameof(startPage)));
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
