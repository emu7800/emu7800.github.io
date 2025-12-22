using EMU7800.Core;
using EMU7800.Shell;
using System;
using static EMU7800.SDL3.Interop.SDL3;

namespace EMU7800.SDL3.Interop;

public sealed class GameControllersSDL3InputDriver : IGameControllersDriver
{
    #region Fields

    readonly Window _window;
    readonly ILogger _logger;

    uint[] _instanceIds = [];

    #endregion

    #region IGameControllersDriver Members

    public GameController[] Controllers { get; private set; } = [];

    public void Initialize()
    {
        Shutdown();

        Controllers = [new(0, _window), new(1, _window)];
        _instanceIds = new uint[Controllers.Length];

        var gamepads = SDL_GetGamepads(out int count);
        for (var i = 0; i < Controllers.Length; i++)
        {
            var c = Controllers[i];
            if (i < count)
            {
                unsafe
                {
                    var gamepadptr = (uint*)(gamepads + i);
                    AddController(i, *gamepadptr);
                }
            }
            else
            {
                c.ProductName = "(None)";
                c.JoystickType = JoystickType.None;
            }
        }
    }

    public void AddController(uint instanceId)
    {
        for (var i = 0; i < Controllers.Length; i++)
        {
            if (Controllers[i].JoystickType == JoystickType.None)
            {
                AddController(i, instanceId);
                return;
            }
        }
        Info($"Controller {instanceId} ignored, no free slots.");
    }

    public void Poll()
    {
    }

    public void RemoveController(uint instanceId)
    {
        var gamepad = SDL_GetGamepadFromID(instanceId);
        SDL_CloseGamepad(gamepad);

        for (var i = 0; i < Controllers.Length; i++)
        {
            if (_instanceIds[i] == instanceId)
            {
                _instanceIds[i] = 0;
                var c = Controllers[i];
                Info($"Controller removed: P{i + 1}: {instanceId} {c.ProductName} {c.JoystickType}");
                c.ProductName = "(None)";
                c.JoystickType = JoystickType.None;
                return;
            }
        }
        Info($"Controller {instanceId} ignored, not previously added.");
    }

    public void ButtonChanged(uint instanceId, MachineInput machineInput, bool down)
    {
        var controllerno = GetControllerNo(instanceId);
        if (controllerno < 0)
            return;
        Controllers[controllerno].ButtonChanged(controllerno, machineInput, down);
    }

    public void Shutdown()
    {
        for (var i = 0; i < Controllers.Length; i++)
        {
            var c = Controllers[i];
            var instanceId = _instanceIds[i];
            var gamepad = SDL_GetGamepadFromID(instanceId);
            if (gamepad != IntPtr.Zero)
            {
                SDL_CloseGamepad(gamepad);
                Info($"Controller closed: P{i + 1}: {c.ProductName} {c.JoystickType}");
            }
        }
        Controllers = [];
        _instanceIds = [];
    }

    #endregion

    #region Constructors

    public GameControllersSDL3InputDriver(Window window, ILogger logger)
      => (_window, _logger) = (window, logger);

    #endregion

    #region Helpers

    void AddController(int i, uint instanceId)
    {
        var c = Controllers[i];
        _instanceIds[i] = instanceId;
        var gamepad = SDL_OpenGamepad(instanceId);
        var gamepadName = SDL_GetGamepadName(gamepad);
        c.InternalDeviceNumber = i;
        c.JoystickType = JoystickTypeFrom(gamepadName);
        c.ProductName = gamepadName;
        var gamepadType = SDL_GetGamepadTypeForID(instanceId);
        Info($"Controller added: P{i + 1}: {instanceId} {c.ProductName} {c.JoystickType} {gamepadType}");
    }

    public int GetControllerNo(uint instanceId)
    {
        for (var i = 0; i < Controllers.Length; i++)
        {
            if (_instanceIds[i] == instanceId)
                return i;
        }
        return -1;
    }

    static JoystickType JoystickTypeFrom(string name)
        => name switch
        {
            "Stelladaptor 2600-to-USB Interface" => JoystickType.Stelladaptor,
            "2600-daptor" => JoystickType.Daptor,
            "2600-daptor II" => JoystickType.Daptor2,
            "Controller (XBOX 360 For Windows)"
            or "Controller (Xbox 360 Wireless Receiver for Windows)"
                                                 => JoystickType.XInput,
            _ => name.Contains("XBOX", StringComparison.OrdinalIgnoreCase)
                    ? JoystickType.XInput
                    : name.Length > 0 ? JoystickType.Usb : JoystickType.XInput
        };

    void Info(string message)
      => _logger.Log(3, message);

    #endregion
}