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
    readonly uint[] _instanceIds = [0, 0];
    readonly MachineInput[] _lastHAxisChange = [MachineInput.End, MachineInput.End];
    readonly MachineInput[] _lastVAxisChange = [MachineInput.End, MachineInput.End];

    #endregion

    #region IGameControllersDriver Members

    public GameController[] Controllers { get; private set; } = [];

    public void Initialize()
    {
        Shutdown();
        Controllers = [new(0, _window), new(1, _window)];
    }

    public void AddGamepad(uint instanceId)
    {
        for (var i = 0; i < Controllers.Length; i++)
        {
            if (Controllers[i].JoystickType == JoystickType.None)
            {
                AddGamepad(i, instanceId);
                return;
            }
        }
        Info($"Gamepad {instanceId} ignored, two controllers already added.");
    }

    public void AddJoystick(uint instanceId)
    {
        for (var i = 0; i < Controllers.Length; i++)
        {
            if (Controllers[i].JoystickType == JoystickType.None)
            {
                AddJoystick(i, instanceId);
                return;
            }
        }
        Info($"Joystick {instanceId} ignored, two controllers already added.");
    }

    public void Poll()
    {
        for (var i = 0; i < Controllers.Length; i++)
        {
            var c = Controllers[i];
            if (c.JoystickType != JoystickType.Daptor2 || c.Daptor2Mode != Daptor2Mode.Unknown)
                continue;
            var joystick = SDL_GetJoystickFromID(_instanceIds[i]);
            var value = SDL_GetJoystickAxis(joystick, 2);
            AxisChanged(_instanceIds[i], 2, value);
        }
    }

    public void RemoveGamepad(uint instanceId)
    {
        var gamepad = SDL_GetGamepadFromID(instanceId);
        if (gamepad == IntPtr.Zero)
            return;

        SDL_CloseGamepad(gamepad);

        for (var i = 0; i < Controllers.Length; i++)
        {
            if (_instanceIds[i] == instanceId)
            {
                _instanceIds[i] = 0;
                var c = Controllers[i];
                Info($"Gamepad removed: P{i + 1}: {instanceId} {c.ProductName} {c.JoystickType}");
                c.ProductName = "(None)";
                c.JoystickType = JoystickType.None;
                return;
            }
        }
    }

    public void RemoveJoystick(uint instanceId)
    {
        var joystick = SDL_GetJoystickFromID(instanceId);
        if (joystick == IntPtr.Zero)
            return;

        SDL_CloseJoystick(joystick);

        for (var i = 0; i < Controllers.Length; i++)
        {
            if (_instanceIds[i] == instanceId)
            {
                _instanceIds[i] = 0;
                var c = Controllers[i];
                Info($"Joystick removed: P{i + 1}: {instanceId} {c.ProductName} {c.JoystickType}");
                c.ProductName = "(None)";
                c.JoystickType = JoystickType.None;
                return;
            }
        }
    }

    public void ButtonChanged(uint instanceId, MachineInput machineInput, bool down)
    {
        var controllerno = GetControllerNo(instanceId);
        if (controllerno < 0)
            return;
        Controllers[controllerno].ButtonChanged(controllerno, machineInput, down);
    }

    public void ButtonChanged(uint instanceId, byte button, bool down)
    {
        var controllerno = GetControllerNo(instanceId);
        if (controllerno < 0)
            return;

        var c = Controllers[controllerno];

        switch (c.Daptor2Mode)
        {
            case Daptor2Mode.A7800:
                switch (button)
                {
                    case 2:
                        c.ButtonChanged(controllerno, MachineInput.Fire, down);
                        break;
                    case 3:
                        c.ButtonChanged(controllerno, MachineInput.Fire2, down);
                        break;
                }
                break;
            case Daptor2Mode.Keypad:
                c.ButtonChanged(controllerno, GameController.Daptor2KeypadToMachineInputMapping[button & 0xf], down);
                break;
            case Daptor2Mode.A2600:
            default:
                switch (button)
                {
                    case 0:
                        c.ButtonChanged(controllerno, MachineInput.Fire, down);
                        c.PaddleButtonChanged(controllerno, button, down);
                        break;
                    case 1:
                        c.ButtonChanged(controllerno, MachineInput.Fire2, down);
                        c.PaddleButtonChanged(controllerno, button, down);
                        break;
                }
                break;
        }
    }

    public void AxisChanged(uint instanceId, byte axis, short value)
    {
        if (axis > 2)
            return;

        var controllerno = GetControllerNo(instanceId);
        if (controllerno < 0)
            return;

        var c = Controllers[controllerno];

        if (axis == 2)
        {
            var newDaptor2Mode = value switch
            {
                -32768 /* 8000 */ => Daptor2Mode.A2600,
                -28672 /* 9000 */ => Daptor2Mode.A7800,
                -24576 /* A000 */ => Daptor2Mode.Keypad,
                _ => Daptor2Mode.Unknown,
            };
            if (c.Daptor2Mode != newDaptor2Mode && newDaptor2Mode != Daptor2Mode.Unknown)
            {
                c.Daptor2Mode = newDaptor2Mode;
                Info($"Daptor2 mode changed: P{controllerno + 1}: {newDaptor2Mode}");
            }
            return;
        }

        const int
            DEADZONE = 32760
            ;

        switch (c.Daptor2Mode)
        {
            case Daptor2Mode.A7800:
            case Daptor2Mode.Keypad:
                break;
            case Daptor2Mode.A2600:
            default:
                var paddlepos = (1 << 20) - ((value + 32768) << 4);
                switch (axis)
                {
                    case 0:
                        if (value < -DEADZONE)
                        {
                            switch (_lastHAxisChange[controllerno])
                            {
                                case MachineInput.Left:
                                    break;
                                case MachineInput.Right:
                                    c.ButtonChanged(controllerno, MachineInput.Right, false);
                                    c.ButtonChanged(controllerno, MachineInput.Left, true);
                                    _lastHAxisChange[controllerno] = MachineInput.Left;
                                    break;
                                case MachineInput.End:
                                    c.ButtonChanged(controllerno, MachineInput.Left, true);
                                    _lastHAxisChange[controllerno] = MachineInput.Left;
                                    break;
                            }
                        }
                        else if (value > DEADZONE)
                        {
                            switch (_lastHAxisChange[controllerno])
                            {
                                case MachineInput.Right:
                                    break;
                                case MachineInput.Left:
                                    c.ButtonChanged(controllerno, MachineInput.Left, false);
                                    c.ButtonChanged(controllerno, MachineInput.Right, true);
                                    _lastHAxisChange[controllerno] = MachineInput.Right;
                                    break;
                                case MachineInput.End:
                                    c.ButtonChanged(controllerno, MachineInput.Right, true);
                                    _lastHAxisChange[controllerno] = MachineInput.Right;
                                    break;
                            }
                        }
                        else if (_lastHAxisChange[controllerno] != MachineInput.End)
                        {
                            c.ButtonChanged(controllerno, _lastHAxisChange[controllerno], false);
                            _lastHAxisChange[controllerno] = MachineInput.End;
                        }
                        c.PaddlePositionChanged(controllerno, controllerno << 1, paddlepos);
                        break;
                    case 1:
                        if (value < -DEADZONE)
                        {
                            switch (_lastVAxisChange[controllerno])
                            {
                                case MachineInput.Up:
                                    break;
                                case MachineInput.Down:
                                    c.ButtonChanged(controllerno, MachineInput.Down, false);
                                    c.ButtonChanged(controllerno, MachineInput.Up, true);
                                    _lastVAxisChange[controllerno] = MachineInput.Up;
                                    break;
                                case MachineInput.End:
                                    c.ButtonChanged(controllerno, MachineInput.Up, true);
                                    _lastVAxisChange[controllerno] = MachineInput.Up;
                                    break;
                            }
                        }
                        else if (value > DEADZONE)
                        {
                            switch (_lastVAxisChange[controllerno])
                            {
                                case MachineInput.Down:
                                    break;
                                case MachineInput.Up:
                                    c.ButtonChanged(controllerno, MachineInput.Up, false);
                                    c.ButtonChanged(controllerno, MachineInput.Down, true);
                                    _lastVAxisChange[controllerno] = MachineInput.Down;
                                    break;
                                case MachineInput.End:
                                    c.ButtonChanged(controllerno, MachineInput.Down, true);
                                    _lastVAxisChange[controllerno] = MachineInput.Down;
                                    break;
                            }
                        }
                        else if (_lastVAxisChange[controllerno] != MachineInput.End)
                        {
                            c.ButtonChanged(controllerno, _lastVAxisChange[controllerno], false);
                            _lastVAxisChange[controllerno] = MachineInput.End;
                        }
                        c.PaddlePositionChanged(controllerno, (controllerno << 1) + 1, paddlepos);
                        break;
                }
                if (controllerno == axis)
                {
                    var pos = value switch
                    {
                        < -DEADZONE => 3, // up
                        > DEADZONE  => 1, // down
                        _           => 0  // center
                    };
                    c.DrivingPositionChanged(c.ControllerNo, GameController.StelladaptorDrivingMachineInputMapping[pos]);
                }
                break;
        }
    }

    public void Shutdown()
    {
        for (var i = 0; i < Controllers.Length; i++)
        {
            RemoveGamepad(_instanceIds[i]);
            RemoveJoystick(_instanceIds[i]);
        }

        Controllers = [];

        for (var i = 0; i < _instanceIds.Length; i++)
        {
            _instanceIds[i] = 0;
            _lastHAxisChange[i] = _lastVAxisChange[i] = MachineInput.End;
        }
    }

    #endregion

    #region Constructors

    public GameControllersSDL3InputDriver(Window window, ILogger logger)
      => (_window, _logger) = (window, logger);

    #endregion

    #region Helpers

    void AddGamepad(int i, uint instanceId)
    {
        var c = Controllers[i];
        _instanceIds[i] = instanceId;
        var gamepad = SDL_OpenGamepad(instanceId);
        var gamepadName = SDL_GetGamepadName(gamepad);
        c.InternalDeviceNumber = i;
        c.JoystickType = GameController.JoystickTypeFrom(gamepadName);
        c.ProductName = gamepadName;
        var gamepadType = SDL_GetGamepadTypeForID(instanceId);
        Info($"Gamepad added: P{i + 1}: {instanceId} {c.ProductName} {c.JoystickType} {gamepadType}");
    }

    void AddJoystick(int i, uint instanceId)
    {
        var c = Controllers[i];
        _instanceIds[i] = instanceId;
        var joystick = SDL_OpenJoystick(instanceId);
        var joystickName = SDL_GetJoystickName(joystick);
        c.InternalDeviceNumber = i;
        c.JoystickType= GameController.JoystickTypeFrom(joystickName);
        c.ProductName= joystickName;
        var joystickType = SDL_GetJoystickTypeForID(instanceId);
        Info($"Joystick added: P{i + 1}: {instanceId} {c.ProductName} {c.JoystickType} {joystickType}");
    }

    int GetControllerNo(uint instanceId)
    {
        for (var i = 0; i < _instanceIds.Length; i++)
        {
            if (_instanceIds[i] == instanceId)
                return i;
        }
        return -1;
    }

    void Info(string message)
      => _logger.Log(3, message);

    #endregion
}