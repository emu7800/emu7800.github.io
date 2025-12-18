// © Mike Murphy

using EMU7800.Core;
using EMU7800.Services;
using EMU7800.Services.Dto;
using System.Collections.Generic;

namespace EMU7800.Shell;

public sealed class Window
{
    #region Fields

    readonly TimerDevice _timerDevice = new();
    readonly bool[] _lastKeyInput = new bool[0x100];

    readonly PageBackStackHost _pageBackStack;
    readonly ILogger _logger;

    bool _resourcesLoaded;

    #endregion

    public void OnButtonChanged(int controllerNo, MachineInput input, bool down)
      => _pageBackStack.ControllerButtonChanged(controllerNo, input, down);

    public void OnDrivingPositionChanged(int controllerNo, MachineInput input)
      => _pageBackStack.DrivingPositionChanged(controllerNo, input);

    public bool OnIterate(IGraphicsDeviceDriver graphicsDevice, IGameControllersDriver gameControllers)
    {
        if (!_pageBackStack.StartOfCycle())
        {
            _logger.Log(3, "Stop requested.");
            return false;
        }

        if (!_resourcesLoaded)
        {
            _pageBackStack.LoadResources(graphicsDevice);
            _resourcesLoaded = true;
        }

        gameControllers.Poll();

        _pageBackStack.Update(_timerDevice);

        graphicsDevice.BeginDraw();

        _pageBackStack.Render(graphicsDevice);

        graphicsDevice.EndDraw();

        _timerDevice.Update();

        return true;
    }

    public void OnKeyboardKeyPressed(ushort vkey, bool down)
    {
        var lastDown = _lastKeyInput[vkey & 0xff];
        if (down == lastDown)
            return;
        _lastKeyInput[vkey & 0xff] = down;
        _pageBackStack.KeyboardKeyPressed((KeyboardKey)vkey, down);
    }

    public void OnMouseButtonChanged(int x, int y, bool down)
      => _pageBackStack.MouseButtonChanged(0, x, y, down);

    public void OnMouseMoved(int x, int y, int dx, int dy)
      => _pageBackStack.MouseMoved(0, x, y, dx, dy);

    public void OnMouseWheelChanged(int x, int y, int delta)
      => _pageBackStack.MouseWheelChanged(0, x, y, delta);

    public void OnPaddleButtonChanged(int controllerNo, int paddleNo, bool down)
      => _pageBackStack.PaddleButtonChanged(controllerNo, paddleNo, down);

    public void OnPaddlePositionChanged(int controllerNo, int paddleNo, int ohms)
      => _pageBackStack.PaddlePositionChanged(controllerNo, paddleNo, ohms);

    public void OnResized(IGraphicsDeviceDriver graphicsDevice, int w, int h)
    {
        graphicsDevice.Resize(new(w, h));
        _pageBackStack.Resized(new(w, h));
    }

    public void OnVisibilityChanged(bool isVisible)
    {
        if (isVisible)
        {
            _pageBackStack.OnNavigatingHere();
        }
        else
        {
            _pageBackStack.OnNavigatingAway();
        }
    }

    public void OnAudioChanged(IAudioDeviceDriver audioDevice)
      => _pageBackStack.AudioChanged(audioDevice);

    public void OnControllersChanged(IGameControllersDriver gameControllers)
      => _pageBackStack.ControllersChanged(gameControllers);

    #region Constructors

    public Window(ILogger logger)
      : this(new(logger), logger) {}

    public Window(DatastoreService datastoreSvc, ILogger logger)
      : this(new TitlePage(), datastoreSvc, logger) {}

    public Window(GameProgramInfoViewItem gpivi, List<ImportedSpecialBinaryInfo> specialBinaries, DatastoreService datastoreSvc, ILogger logger)
      : this(new GamePage(gpivi, specialBinaries, true), datastoreSvc, logger) {}

    public Window(PageBase startPage, DatastoreService datastoreSvc, ILogger logger)
      => (_pageBackStack, _logger) = (new PageBackStackHost(startPage, datastoreSvc, logger), logger);

    #endregion
}
