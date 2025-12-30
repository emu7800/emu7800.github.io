// © Mike Murphy

using EMU7800.Core;
using EMU7800.Shell;
using System;

namespace EMU7800.Win32.Interop;

public sealed class GameControllersDInputXInputDriver : IGameControllersDriver
{
    #region Fields

    readonly IntPtr _hWnd;
    readonly Window _window;
    readonly ILogger _logger;

    #endregion

    #region IGameControllersDriver Members

    public GameController[] Controllers { get; private set; } = [];

    public void Initialize()
    {
        Shutdown();

        Controllers = [new(0, _window), new(1, _window)];

        DirectInputNativeMethods.Initialize(_hWnd, out var joystickNames);

        for (var i = 0; i < Controllers.Length; i++)
        {
            var c = Controllers[i];
            if (i < joystickNames.Length)
            {
                var joystickName = joystickNames[i];
                c.ProductName = joystickName;
                c.JoystickType = GameController.JoystickTypeFrom(joystickName);
                c.InternalDeviceNumber = i;
                Info($"Joystick added: P{i + 1}: Name={c.ProductName} Type={c.JoystickType}");
            }
            else
            {
                c.ProductName = "(None)";
                c.JoystickType = JoystickType.None;
            }
        }
        for (int i = 0, j = 0; i < Controllers.Length; i++)
        {
            var c = Controllers[i];
            if (c.JoystickType == JoystickType.XInput)
            {
                var caps = new XInputNativeMethods.XINPUT_CAPABILITIES();
                do
                {
                    XInputNativeMethods.Initialize(j++, ref caps);
                }
                while (caps.Type == 0 && j < 4);
                if (j < 4)
                {
                    c.InternalDeviceNumber = j - 1;
                }
            }
        }
    }

    public void Poll()
    {
        for (var i = 0; i < Controllers.Length; i++)
        {
            var c = Controllers[i];
            switch (c.JoystickType)
            {
                case JoystickType.XInput:
                    RaiseEventsFromXinput(c);
                    break;
                case JoystickType.None:
                    break;
                default:
                    RaiseEventsFromDirectInput(c);
                    break;
            }
        }
    }

    public void Shutdown()
    {
        DirectInputNativeMethods.Shutdown();
    }

    #endregion

    #region Constructors

    public GameControllersDInputXInputDriver(IntPtr hWnd, Window window, ILogger logger)
      => (_hWnd, _window, _logger) = (hWnd, window, logger);

    #endregion

    #region Helpers

    void RaiseEventsFromDirectInput(GameController c)
    {
        DirectInputNativeMethods.Poll(c.InternalDeviceNumber, out var currState, out var prevState);

        if (c.JoystickType == JoystickType.Daptor2)
        {
            var maybeNewDaptorMode = currState.InterpretDaptor2Mode();
            if (maybeNewDaptorMode != c.Daptor2Mode && maybeNewDaptorMode != Daptor2Mode.Unknown)
            {
                c.Daptor2Mode = maybeNewDaptorMode;
                Info($"Daptor2 mode changed: P{c.ControllerNo + 1}: {maybeNewDaptorMode}");
            }
        }

        for (var i = 0; i < 0xf; i++)
        {
            var prevButtonDown = prevState.InterpretJoyButtonDown(i);
            var currButtonDown = currState.InterpretJoyButtonDown(i);
            if (prevButtonDown != currButtonDown)
            {
                if (c.Daptor2Mode == Daptor2Mode.A7800)
                {
                    switch (i)
                    {
                        case 2:
                            c.ButtonChanged(i, MachineInput.Fire, currButtonDown);
                            break;
                        case 3:
                            c.ButtonChanged(i, MachineInput.Fire2, currButtonDown);
                            break;
                    }
                }
                else if (c.Daptor2Mode == Daptor2Mode.Keypad)
                {
                    c.ButtonChanged(c.ControllerNo, GameController.Daptor2KeypadToMachineInputMapping[i & 0xf], currButtonDown);
                }
                else if (c.Daptor2Mode == Daptor2Mode.A2600 || c.IsStelladaptor)
                {
                    switch (i)
                    {
                        case 0:
                            c.ButtonChanged(c.ControllerNo, MachineInput.Fire, currButtonDown);
                            c.PaddleButtonChanged(c.ControllerNo, i, currButtonDown);
                            break;
                        case 1:
                            c.ButtonChanged(c.ControllerNo, MachineInput.Fire2, currButtonDown);
                            c.PaddleButtonChanged(c.ControllerNo, i, currButtonDown);
                            break;
                    }
                }
            }
        }

        var prevLeft = prevState.InterpretJoyLeft();
        var currLeft = currState.InterpretJoyLeft();
        if (prevLeft != currLeft)
        {
            c.ButtonChanged(c.ControllerNo, MachineInput.Left, currLeft);
        }

        var prevRight = prevState.InterpretJoyRight();
        var currRight = currState.InterpretJoyRight();
        if (prevRight != currRight)
        {
            c.ButtonChanged(c.ControllerNo, MachineInput.Right, currRight);
        }

        var prevUp = prevState.InterpretJoyUp();
        var currUp = currState.InterpretJoyUp();
        if (prevUp != currUp)
        {
            c.ButtonChanged(c.ControllerNo, MachineInput.Up, currUp);
        }

        var prevDown = prevState.InterpretJoyDown();
        var currDown = currState.InterpretJoyDown();
        if (prevDown != currDown)
        {
            c.ButtonChanged(c.ControllerNo, MachineInput.Down, currDown);
        }

        if (c.Daptor2Mode == Daptor2Mode.A2600 || c.IsStelladaptor)
        {
            var paddleno = c.ControllerNo << 1;
            var prevPos = prevState.InterpretStelladaptorPaddlePosition(paddleno);
            var currPos = currState.InterpretStelladaptorPaddlePosition(paddleno);
            if (prevPos != currPos)
            {
                var paddleohms = (1 << 20) - ((currPos + DirectInputNativeMethods.AXISRANGE) << 4);
                c.PaddlePositionChanged(c.ControllerNo, paddleno, paddleohms);
            }

            paddleno++;
            prevPos = prevState.InterpretStelladaptorPaddlePosition(paddleno);
            currPos = currState.InterpretStelladaptorPaddlePosition(paddleno);
            if (prevPos != currPos)
            {
                var paddleohms = (1 << 20) - ((currPos + DirectInputNativeMethods.AXISRANGE) << 4);
                c.PaddlePositionChanged(c.ControllerNo, paddleno, paddleohms);
            }

            prevPos = prevState.InterpretStelladaptorDrivingPosition(c.ControllerNo);
            currPos = currState.InterpretStelladaptorDrivingPosition(c.ControllerNo);
            if (prevPos != currPos)
            {
                c.DrivingPositionChanged(c.ControllerNo, GameController.StelladaptorDrivingMachineInputMapping[currPos]);
            }
        }
    }

    static void RaiseEventsFromXinput(GameController c)
    {
        XInputNativeMethods.Poll(c.InternalDeviceNumber, out var currState, out var prevState);

        for (var i = 0; i < 4; i++)
        {
            var prevButton = prevState.InterpretButtonDown(i);
            var currButton = currState.InterpretButtonDown(i);
            if (prevButton != currButton)
            {
                switch (i)
                {
                    case 0:
                        c.ButtonChanged(c.ControllerNo, MachineInput.Fire, currButton);
                        break;
                    case 1:
                        c.ButtonChanged(c.ControllerNo, MachineInput.Fire2, currButton);
                        break;
                }
            }
        }

        var prevLeft = prevState.InterpretJoyLeft();
        var currLeft = currState.InterpretJoyLeft();
        if (prevLeft != currLeft)
        {
            c.ButtonChanged(c.ControllerNo, MachineInput.Left, currLeft);
        }

        var prevRight = prevState.InterpretJoyRight();
        var currRight = currState.InterpretJoyRight();
        if (prevRight != currRight)
        {
            c.ButtonChanged(c.ControllerNo, MachineInput.Right, currRight);
        }

        var prevUp = prevState.InterpretJoyUp();
        var currUp = currState.InterpretJoyUp();
        if (prevUp != currUp)
        {
            c.ButtonChanged(c.ControllerNo, MachineInput.Up, currUp);
        }

        var prevDown = prevState.InterpretJoyDown();
        var currDown = currState.InterpretJoyDown();
        if (prevDown != currDown)
        {
            c.ButtonChanged(c.ControllerNo, MachineInput.Down, currDown);
        }

        var prevBack = prevState.InterpretButtonBack();
        var currBack = currState.InterpretButtonBack();
        if (prevBack != currBack)
        {
            c.ButtonChanged(c.ControllerNo, MachineInput.End, currBack);
        }

        var prevStart = prevState.InterpretButtonStart();
        var currStart = currState.InterpretButtonStart();
        if (prevStart != currStart)
        {
            c.ButtonChanged(c.ControllerNo, MachineInput.Start, currStart);
        }

        var prevSelect = prevState.InterpretLeftShoulderButton();
        var currSelect = currState.InterpretLeftShoulderButton();
        if (prevSelect != currSelect)
        {
            c.ButtonChanged(c.ControllerNo, MachineInput.Select, currSelect);
        }

        var prevReset = prevState.InterpretRightShoulderButton();
        var currReset = currState.InterpretRightShoulderButton();
        if (prevReset != currReset)
        {
            c.ButtonChanged(c.ControllerNo, MachineInput.Reset, currReset);
        }
    }

    void Info(string message)
      => _logger.Log(3, message);

    #endregion
}
