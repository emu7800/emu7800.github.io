// © Mike Murphy

using EMU7800.Core;
using System;

namespace EMU7800.Shell;

public sealed class PageBackStackHost : IDisposable
{
    #region Fields

    readonly PageBackStackStateService _stateService = new();

    PageBase _currentPage = new Nullpage();
    bool _pageChanged;
    SizeF _size;

    IAudioDeviceDriver _audioDevice = EmptyAudioDeviceDriver.Default;
    IGameControllersDriver _gameControllers = EmptyGameControllersDriver.Default;

    #endregion

    public bool StartOfCycle()
    {
        if (_stateService.IsQuitPending)
        {
            return false;
        }

        if (_stateService.IsPagePending)
        {
            _currentPage.OnNavigatingAway();
            _currentPage = _stateService.GetPendingPage();
            _currentPage.OnNavigatingHere();
            _currentPage.Resized(_size);
            _currentPage.InjectDependency(_stateService);
            _currentPage.InjectDependency(_audioDevice);
            _currentPage.InjectDependency(_gameControllers);
            _pageChanged = true;
        }

        if (_stateService.IsDisposablePages)
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

    public void InjectDependency(object dependency)
    {
        _currentPage.InjectDependency(dependency);
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

    PageBackStackHost() {}

    public PageBackStackHost(PageBase startPage)
      => _stateService.Push(startPage);

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
            while (_stateService.Pop())
            {
            }
            DisposeAllDisposings();
            _currentPage.Dispose();
        }
    }

    void DisposeAllDisposings()
    {
        while (_stateService.IsDisposablePages)
        {
            var disposingPage = _stateService.GetNextDisposablePage();
            disposingPage.Dispose();
        }
    }

    #endregion
}