// © Mike Murphy

using EMU7800.Core;
using EMU7800.Shell;
using System;

namespace EMU7800.Win32.Interop;

public sealed class GameControllersDInputXInputDriver : IGameControllersDriver
{
    public static GameControllersDInputXInputDriver Factory(IntPtr hWnd) => new(hWnd);
    GameControllersDInputXInputDriver() {}

    #region Fields

    readonly IntPtr _hWnd;

    #endregion

    #region IGameControllersDriver Members

    public GameController[] Controllers { get; private set; } = [];

    public void Initialize()
    {
        Shutdown();

        DirectInputNativeMethods.Initialize(_hWnd, out var joystickNames);

        Controllers = [new(0), new(1)];

        for (var i = 0; i < Controllers.Length; i++)
        {
            var c = Controllers[i];
            if (i < joystickNames.Length)
            {
                var joystickName = joystickNames[i];
                c.ProductName = joystickName;
                c.JoystickType = JoystickTypeFrom(joystickName);
                c.InternalDeviceNumber = i;
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

    public GameControllersDInputXInputDriver(IntPtr hWnd)
    {
        _hWnd = hWnd;
    }

    #endregion

    #region Helpers

    static void RaiseEventsFromDirectInput(GameController c)
    {
        DirectInputNativeMethods.Poll(c.InternalDeviceNumber, out var currState, out var prevState);

        var maybeNewDaptorMode = currState.InterpretDaptor2Mode();
        if (maybeNewDaptorMode != c.DaptorMode)
        {
            c.DaptorMode = maybeNewDaptorMode;
        }

        if (c.ButtonChanged != GameController.ButtonChangedHandlerDefault)
        {
            for (var i = 0; i < 0xf; i++)
            {
                var prevButtonDown = prevState.InterpretJoyButtonDown(i);
                var currButtonDown = currState.InterpretJoyButtonDown(i);
                if (prevButtonDown != currButtonDown)
                {
                    switch (c.DaptorMode)
                    {
                        // 7800 mode
                        case 1 when i == 2:
                            c.ButtonChanged(c.ControllerNo, MachineInput.Fire, currButtonDown);
                            break;
                        case 1:
                            {
                                if (i == 3)
                                {
                                    c.ButtonChanged(c.ControllerNo, MachineInput.Fire2, currButtonDown);
                                }
                                break;
                            }
                        // keypad mode
                        case 2:
                            c.ButtonChanged(c.ControllerNo, GameController.Daptor2KeypadToMachineInputMapping[i & 0xf], currButtonDown);
                            break;
                        // 2600/regular mode
                        default:
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
        }
    }

    static void RaiseEventsFromXinput(GameController c)
    {
        XInputNativeMethods.Poll(c.InternalDeviceNumber, out var currState, out var prevState);

        if (c.ButtonChanged != GameController.ButtonChangedHandlerDefault)
        {
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
    }

    static JoystickType JoystickTypeFrom(string name)
        => name switch
        {
            "Stelladaptor 2600-to-USB Interface" => JoystickType.Stelladaptor,
            "2600-daptor"                        => JoystickType.Daptor,
            "2600-daptor II"                     => JoystickType.Daptor2,
            "Controller (XBOX 360 For Windows)"
            or "Controller (Xbox 360 Wireless Receiver for Windows)"
                                                 => JoystickType.XInput,
            _ => name.Contains("XBOX", StringComparison.OrdinalIgnoreCase)
                    ? JoystickType.XInput
                    : name.Length > 0 ? JoystickType.Usb : JoystickType.XInput
        };

    #endregion
}
