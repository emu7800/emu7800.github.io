// © Mike Murphy

using EMU7800.Core;
using System;

namespace EMU7800.Shell;

public sealed class PageBackStackHost : IDisposable
{
    #region Fields

    PageBase _currentPage = new Nullpage();
    bool _pageChanged;
    SizeF _size;

    IAudioDeviceDriver _audioDevice = EmptyAudioDeviceDriver.Default;
    IGameControllersDriver _gameControllers = EmptyGameControllersDriver.Default;

    #endregion

    public bool StartOfCycle()
    {
        if (PageBackStackStateService.IsQuitPending)
        {
            return false;
        }

        if (PageBackStackStateService.IsPagePending)
        {
            _currentPage.OnNavigatingAway();
            _currentPage = PageBackStackStateService.GetPendingPage();
            _currentPage.OnNavigatingHere();
            _currentPage.Resized(_size);
            _currentPage.AudioChanged(_audioDevice);
            _currentPage.ControllersChanged(_gameControllers);
            _pageChanged = true;
        }

        if (PageBackStackStateService.IsDisposablePages)
        {
            DisposeAllDisposings();
        }

        return true;
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

    public void AudioChanged(IAudioDeviceDriver audioDevice)
    {
        _audioDevice = audioDevice;
    }

    public void ControllersChanged(IGameControllersDriver gameControllers)
    {
        _gameControllers = gameControllers;
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

    public void LoadResources(IGraphicsDeviceDriver graphicsDevice)
    {
        _currentPage.LoadResources(graphicsDevice);
    }

    public void Update(TimerDevice td)
    {
        _currentPage.Update(td);
    }

    public void Render(IGraphicsDeviceDriver graphicsDevice)
    {
        if (_pageChanged)
        {
            _currentPage.LoadResources(graphicsDevice);
            _pageChanged = false;
        }
        _currentPage.Render(graphicsDevice);
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