// © Mike Murphy

using EMU7800.Core;
using EMU7800.Win32.Interop;
using System;

namespace EMU7800.D2D.Shell;

public sealed class PageBackStackHost : IDisposable
{
    #region Fields

    PageBase _currentPage = new Nullpage();
    bool _pageChanged;
    D2D_SIZE_F _size;

    #endregion

    public void StartOfCycle()
    {
        if (PageBackStackStateService.IsPagePending)
        {
            _currentPage.OnNavigatingAway();
            _currentPage = PageBackStackStateService.GetPendingPage();
            _currentPage.OnNavigatingHere();
            _currentPage.Resized(_size);
            _pageChanged = true;
        }

        if (PageBackStackStateService.IsDisposablePages)
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

    public void ControllerButtonChanged(int controllerNo, MachineInput input, bool down)
    {
        _currentPage.ControllerButtonChanged(controllerNo, input, down);
    }

    public void PaddlePositionChanged(int controllerNo, int paddleNo, int ohms)
    {
        _currentPage.PaddlePositionChanged(controllerNo, paddleNo, ohms);
    }

    public void PaddleButtonChanged(int controllerNo, int paddleNo, bool down)
    {
        _currentPage.PaddleButtonChanged(controllerNo, paddleNo, down);
    }

    public void DrivingPositionChanged(int controllerNo, MachineInput input)
    {
        _currentPage.DrivingPositionChanged(controllerNo, input);
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
        PageBackStackStateService.Push(startPage);
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
            while (PageBackStackStateService.Pop())
            {
            }
            DisposeAllDisposings();
            _currentPage.Dispose();
        }
    }

    static void DisposeAllDisposings()
    {
        while (PageBackStackStateService.IsDisposablePages)
        {
            var disposingPage = PageBackStackStateService.GetNextDisposablePage();
            disposingPage.Dispose();
        }
    }

    #endregion
}