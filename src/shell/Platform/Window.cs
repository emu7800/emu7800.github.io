// © Mike Murphy

using EMU7800.Core;
using EMU7800.Services.Dto;
using System;

namespace EMU7800.Shell;

public interface IWindowDriver
{
    public void StartWindowAndProcessEvents(bool startMaximized);
}

public sealed class EmptyWindowDriver : IWindowDriver
{
    public static readonly EmptyWindowDriver Default = new();
    EmptyWindowDriver() {}

    #region IWindowDriver Members

    public void StartWindowAndProcessEvents(bool _) {}

    #endregion
}

public static class Window
{
    public static Func<IWindowDriver> DriverFactory { get; set; } = () => EmptyWindowDriver.Default;
    static IWindowDriver _driver = EmptyWindowDriver.Default;

    #region Fields

    static readonly TimerDevice _timerDevice = new();
    static readonly bool[] _lastKeyInput = new bool[0x100];

    static PageBackStackHost _pageBackStack = new(new TitlePage());
    static bool _resourcesLoaded;

    #endregion

    public static void Start(bool startMaximized)
    {
        _driver = DriverFactory();
        _driver.StartWindowAndProcessEvents(startMaximized);
    }

    public static void Start(bool startMaximized, PageBase? startPage = null)
    {
        if (startPage is not null)
        {
            _pageBackStack = new PageBackStackHost(startPage);
        }
        Start(startMaximized);
    }

    public static void Start(bool startMaximized, GameProgramInfoViewItem gpivi)
      => Start(startMaximized, new GamePage(gpivi, true));

    #region Callbacks

    public static void OnButtonChanged(int controllerNo, MachineInput input, bool down)
      => _pageBackStack.ControllerButtonChanged(controllerNo, input, down);

    public static void OnDeviceChanged()
      => GameControllers.Initialize();

    public static void OnDrivingPositionChanged(int controllerNo, MachineInput input)
      => _pageBackStack.DrivingPositionChanged(controllerNo, input);

    public static void OnIterate()
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

    public static void OnKeyboardKeyPressed(ushort vkey, bool down)
    {
        var lastDown = _lastKeyInput[vkey & 0xff];
        if (down == lastDown)
            return;
        _lastKeyInput[vkey & 0xff] = down;
        _pageBackStack.KeyboardKeyPressed((KeyboardKey)vkey, down);
    }

    public static void OnMouseButtonChanged(int x, int y, bool down)
      => _pageBackStack.MouseButtonChanged(0, x, y, down);

    public static void OnMouseMoved(int x, int y, int dx, int dy)
      => _pageBackStack.MouseMoved(0, x, y, dx, dy);

    public static void OnMouseWheelChanged(int x, int y, int delta)
      => _pageBackStack.MouseWheelChanged(0, x, y, delta);

    public static void OnPaddleButtonChanged(int controllerNo, int paddleNo, bool down)
      => _pageBackStack.PaddleButtonChanged(controllerNo, paddleNo, down);

    public static void OnPaddlePositionChanged(int controllerNo, int paddleNo, int ohms)
      => _pageBackStack.PaddlePositionChanged(controllerNo, paddleNo, ohms);

    public static void OnResized(int w, int h)
    {
        GraphicsDevice.Resize(new(w, h));
        _pageBackStack.Resized(new(w, h));
    }

    public static void OnVisibilityChanged(bool isVisible)
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

    #endregion
}
