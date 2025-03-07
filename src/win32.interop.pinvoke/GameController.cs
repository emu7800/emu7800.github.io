﻿// © Mike Murphy

using EMU7800.Core;
using System;
using static EMU7800.Win32.Interop.DirectInputNativeMethods;
using static System.Console;

namespace EMU7800.Win32.Interop;

public enum JoystickType { None, XInput, Usb, Stelladaptor, Daptor, Daptor2 }

public class GameController
{
    static readonly MachineInput[] StelladaptorDrivingMachineInputMapping =
    [
        MachineInput.Driving0, MachineInput.Driving1, MachineInput.Driving2, MachineInput.Driving3
    ];

    static readonly MachineInput[] Daptor2KeypadToMachineInputMapping =
    [
        MachineInput.NumPad1,    MachineInput.NumPad2, MachineInput.NumPad3,
        MachineInput.NumPad4,    MachineInput.NumPad5, MachineInput.NumPad6,
        MachineInput.NumPad7,    MachineInput.NumPad8, MachineInput.NumPad9,
        MachineInput.NumPadMult, MachineInput.NumPad0, MachineInput.NumPadHash,
        MachineInput.NumPad0,    MachineInput.NumPad0, MachineInput.NumPad0,
        MachineInput.NumPad0
    ];

    static readonly Action<int, MachineInput, bool> ButtonChangedHandlerDefault = (cn, mi, d) => {};
    static readonly Action<int, int, int> PaddlePositionChangedHandlerDefault = (cn, pn, o) => {};
    static readonly Action<int, int, bool> PaddleButtonChangedHandlerDefault = (cn, pn, d) => {};
    static readonly Action<int, MachineInput> DrivingPositionChangedHandlerDefault = (cn, mi) => {};

    readonly int _controllerNo;
    int _daptorMode;

    public string ProductName { get; internal set; } = string.Empty;
    public int InternalDeviceNumber { get; internal set; }
    public JoystickType JoystickType { get; internal set; } = JoystickType.None;

    public string Info => ProductName + (IsDaptor ? $" ({ToDaptorModeStr(_daptorMode)} mode)" : string.Empty);

    public bool IsAtariAdaptor
        => JoystickType is JoystickType.Stelladaptor or JoystickType.Daptor or JoystickType.Daptor2;

    public bool IsDaptor
        => JoystickType is JoystickType.Daptor or JoystickType.Daptor2;

    public Action<int, MachineInput, bool> ButtonChanged { get; set; } = ButtonChangedHandlerDefault;
    public Action<int, int, int> PaddlePositionChanged { get; set; } = PaddlePositionChangedHandlerDefault;
    public Action<int, int, bool> PaddleButtonChanged { get; set; } = PaddleButtonChangedHandlerDefault;
    public Action<int, MachineInput> DrivingPositionChanged { get; set; } = DrivingPositionChangedHandlerDefault;

    internal void RaiseEventsFromDirectInput()
    {
        Poll(InternalDeviceNumber, out var currState, out var prevState);

        var maybeNewDaptorMode = currState.InterpretDaptor2Mode();
        if (maybeNewDaptorMode != _daptorMode)
        {
            WriteLine($"P{_controllerNo + 1} DaptorMode changed from {ToDaptorModeStr(_daptorMode)}:{_daptorMode} mode to {ToDaptorModeStr(maybeNewDaptorMode)}:{maybeNewDaptorMode} mode");
            _daptorMode = maybeNewDaptorMode;
        }

        if (ButtonChanged != ButtonChangedHandlerDefault)
        {
            for (var i = 0; i < 0xf; i++)
            {
                var prevButtonDown = prevState.InterpretJoyButtonDown(i);
                var currButtonDown = currState.InterpretJoyButtonDown(i);
                if (prevButtonDown != currButtonDown)
                {
                    switch (_daptorMode)
                    {
                        // 7800 mode
                        case 1 when i == 2:
                            ButtonChanged(_controllerNo, MachineInput.Fire, currButtonDown);
                            break;
                        case 1:
                        {
                            if (i == 3)
                            {
                                ButtonChanged(_controllerNo, MachineInput.Fire2, currButtonDown);
                            }
                            break;
                        }
                        // keypad mode
                        case 2:
                            ButtonChanged(_controllerNo, Daptor2KeypadToMachineInputMapping[i & 0xf], currButtonDown);
                            break;
                        // 2600/regular mode
                        default:
                        {
                            switch (i)
                            {
                                case 0:
                                    ButtonChanged(_controllerNo, MachineInput.Fire, currButtonDown);
                                    PaddleButtonChanged(_controllerNo, i, currButtonDown);
                                    break;
                                case 1:
                                    ButtonChanged(_controllerNo, MachineInput.Fire2, currButtonDown);
                                    PaddleButtonChanged(_controllerNo, i, currButtonDown);
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
                ButtonChanged(_controllerNo, MachineInput.Left, currLeft);
            }

            var prevRight = prevState.InterpretJoyRight();
            var currRight = currState.InterpretJoyRight();
            if (prevRight != currRight)
            {
                ButtonChanged(_controllerNo, MachineInput.Right, currRight);
            }

            var prevUp = prevState.InterpretJoyUp();
            var currUp = currState.InterpretJoyUp();
            if (prevUp != currUp)
            {
                ButtonChanged(_controllerNo, MachineInput.Up, currUp);
            }

            var prevDown = prevState.InterpretJoyDown();
            var currDown = currState.InterpretJoyDown();
            if (prevDown != currDown)
            {
                ButtonChanged(_controllerNo, MachineInput.Down, currDown);
            }
        }

        switch (JoystickType)
        {
            case JoystickType.Stelladaptor or JoystickType.Daptor or JoystickType.Daptor2:
            {
                if (DrivingPositionChanged != DrivingPositionChangedHandlerDefault)
                {
                    var prevPos = prevState.InterpretStelladaptorDrivingPosition();
                    var currPos = currState.InterpretStelladaptorDrivingPosition();
                    if (prevPos != currPos)
                    {
                        DrivingPositionChanged(_controllerNo, StelladaptorDrivingMachineInputMapping[currPos & 3]);
                    }
                }

                if (PaddlePositionChanged != PaddlePositionChangedHandlerDefault)
                {
                    var prevPos = prevState.InterpretStelladaptorPaddlePosition(0);
                    var currPos = currState.InterpretStelladaptorPaddlePosition(0);
                    if (prevPos != currPos)
                    {
                        // 1 MOhm resistance range
                        var ohms = (AXISRANGE - currPos) * 1000000 / (AXISRANGE << 1);
                        PaddlePositionChanged(_controllerNo, 0, ohms);
                    }

                    prevPos = prevState.InterpretStelladaptorPaddlePosition(1);
                    currPos = currState.InterpretStelladaptorPaddlePosition(1);
                    if (prevPos != currPos)
                    {
                        // 1 MOhm resistance range
                        var ohms = (AXISRANGE - currPos) * 1000000 / (AXISRANGE << 1);
                        PaddlePositionChanged(_controllerNo, 1, ohms);
                    }
                }

                // TODO: find paddle buttons and raise PattleButtonChanged(controllerNo, paddleNo, down)

                break;
            }
        }
    }

    internal void RaiseEventsFromXinput()
    {
        XInputNativeMethods.Poll(InternalDeviceNumber, out var currState, out var prevState);

        if (ButtonChanged != ButtonChangedHandlerDefault)
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
                            ButtonChanged(_controllerNo, MachineInput.Fire, currButton);
                            break;
                        case 1:
                            ButtonChanged(_controllerNo, MachineInput.Fire2, currButton);
                            break;
                    }
                }
            }

            var prevLeft = prevState.InterpretJoyLeft();
            var currLeft = currState.InterpretJoyLeft();
            if (prevLeft != currLeft)
            {
                ButtonChanged(_controllerNo, MachineInput.Left, currLeft);
            }

            var prevRight = prevState.InterpretJoyRight();
            var currRight = currState.InterpretJoyRight();
            if (prevRight != currRight)
            {
                ButtonChanged(_controllerNo, MachineInput.Right, currRight);
            }

            var prevUp = prevState.InterpretJoyUp();
            var currUp = currState.InterpretJoyUp();
            if (prevUp != currUp)
            {
                ButtonChanged(_controllerNo, MachineInput.Up, currUp);
            }

            var prevDown = prevState.InterpretJoyDown();
            var currDown = currState.InterpretJoyDown();
            if (prevDown != currDown)
            {
                ButtonChanged(_controllerNo, MachineInput.Down, currDown);
            }

            var prevBack = prevState.InterpretButtonBack();
            var currBack = currState.InterpretButtonBack();
            if (prevBack != currBack)
            {
                ButtonChanged(_controllerNo, MachineInput.End, currBack);
            }

            var prevStart = prevState.InterpretButtonStart();
            var currStart = currState.InterpretButtonStart();
            if (prevStart != currStart)
            {
                ButtonChanged(_controllerNo, MachineInput.Start, currStart);
            }

            var prevSelect = prevState.InterpretLeftShoulderButton();
            var currSelect = currState.InterpretLeftShoulderButton();
            if (prevSelect != currSelect)
            {
                ButtonChanged(_controllerNo, MachineInput.Select, currSelect);
            }

            var prevReset = prevState.InterpretRightShoulderButton();
            var currReset = currState.InterpretRightShoulderButton();
            if (prevReset != currReset)
            {
                ButtonChanged(_controllerNo, MachineInput.Reset, currReset);
            }
        }
    }

    internal GameController(int controllerNo)
        => _controllerNo = controllerNo;

    static string ToDaptorModeStr(int daptorMode)
        => daptorMode switch
        {
            0 => "2600",
            1 => "7800",
            2 => "keypad",
            _ => "unknown"
        };
}