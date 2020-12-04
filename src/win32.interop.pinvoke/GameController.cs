// © Mike Murphy

using EMU7800.Core;

namespace EMU7800.Win32.Interop
{
    public enum JoystickType { None, XInput, Usb, Stelladaptor, Daptor, Daptor2 };

    public delegate void ButtonChangedHandler(MachineInput input, bool down);
    public delegate void PaddlePositionChangedHandler(int paddleno, int valMax, int val);
    public delegate void DrivingPositionChangedHandler(MachineInput machineInput);

    public class GameController
    {
        static readonly MachineInput[] StelladaptorDrivingMachineInputMapping =
        {
            MachineInput.Driving0, MachineInput.Driving1, MachineInput.Driving2, MachineInput.Driving3
        };

        static readonly MachineInput[] Daptor2KeypadToMachineInputMapping =
        {
            MachineInput.NumPad1,    MachineInput.NumPad2, MachineInput.NumPad3,
            MachineInput.NumPad4,    MachineInput.NumPad5, MachineInput.NumPad6,
            MachineInput.NumPad7,    MachineInput.NumPad8, MachineInput.NumPad9,
            MachineInput.NumPadMult, MachineInput.NumPad0, MachineInput.NumPadHash,
            MachineInput.NumPad0,    MachineInput.NumPad0, MachineInput.NumPad0,
            MachineInput.NumPad0
        };

        public static readonly ButtonChangedHandler ButtonChangedHandlerDefault = (b, d) => {};
        public static readonly PaddlePositionChangedHandler PaddlePositionChangedHandlerDefault = (pn, vm, v) => {};
        public static readonly DrivingPositionChangedHandler DrivingPositionChangedHandlerDefault = mi => { };

        int _daptorMode;

        public string ProductName { get; internal set; } = string.Empty;
        public int InternalDeviceNumber { get; internal set; }
        public JoystickType JoystickType { get; internal set; } = JoystickType.None;

        public string Info => _daptorMode switch
        {
            0 => ProductName + " (2600 mode)",
            1 => ProductName + " (7800 mode)",
            2 => ProductName + " (Keypad mode)",
            _ => ProductName
        };

        public bool IsAtariAdaptor
            => JoystickType == JoystickType.Stelladaptor
            || JoystickType == JoystickType.Daptor
            || JoystickType == JoystickType.Daptor2;

        public ButtonChangedHandler ButtonChanged { get; set; } = ButtonChangedHandlerDefault;
        public PaddlePositionChangedHandler PaddlePositionChanged { get; set; } = PaddlePositionChangedHandlerDefault;
        public DrivingPositionChangedHandler DrivingPositionChanged { get; set; } = DrivingPositionChangedHandlerDefault;

        internal void RaiseEventsFromDirectInput()
        {
            DirectInputNativeMethods.Poll(InternalDeviceNumber, out var currState, out var prevState);

            _daptorMode = currState.InterpretDaptor2Mode();

            if (ButtonChanged != ButtonChangedHandlerDefault)
            {
                for (int i = 0; i < 0xf; i++)
                {
                    var prevButtonDown = prevState.InterpretJoyButtonDown(i);
                    var currButtonDown = currState.InterpretJoyButtonDown(i);
                    if (prevButtonDown != currButtonDown)
                    {
                        if (_daptorMode == 1) // 7800 mode
                        {
                            if (i == 2)
                            {
                                ButtonChanged(MachineInput.Fire, currButtonDown);
                            }
                            else if (i == 3)
                            {
                                ButtonChanged(MachineInput.Fire2, currButtonDown);
                            }
                        }
                        else if (_daptorMode == 2) // keypad mode
                        {
                            ButtonChanged(Daptor2KeypadToMachineInputMapping[i & 0xf], currButtonDown);
                        }
                        else // 2600/regular mode
                        {
                            if (i == 0)
                            {
                                ButtonChanged(MachineInput.Fire, currButtonDown);
                            }
                            else if (i == 1)
                            {
                                ButtonChanged(MachineInput.Fire2, currButtonDown);
                            }
                        }
                    }
                }

                var prevLeft = prevState.InterpretJoyLeft();
                var currLeft = currState.InterpretJoyLeft();
                if (prevLeft != currLeft)
                {
                    ButtonChanged(MachineInput.Left, currLeft);
                }

                var prevRight = prevState.InterpretJoyRight();
                var currRight = currState.InterpretJoyRight();
                if (prevRight != currRight)
                {
                    ButtonChanged(MachineInput.Right, currRight);
                }

                var prevUp = prevState.InterpretJoyUp();
                var currUp = currState.InterpretJoyUp();
                if (prevUp != currUp)
                {
                    ButtonChanged(MachineInput.Up, currUp);
                }

                var prevDown = prevState.InterpretJoyDown();
                var currDown = currState.InterpretJoyDown();
                if (prevDown != currDown)
                {
                    ButtonChanged(MachineInput.Down, currDown);
                }
            }

            if (JoystickType == JoystickType.Stelladaptor || JoystickType == JoystickType.Daptor || JoystickType == JoystickType.Daptor2)
            {
                if (DrivingPositionChanged != DrivingPositionChangedHandlerDefault)
                {
                    var prevPos = prevState.InterpretStelladaptorDrivingPosition();
                    var currPos = currState.InterpretStelladaptorDrivingPosition();
                    if (prevPos != currPos)
                    {
                        DrivingPositionChanged(StelladaptorDrivingMachineInputMapping[currPos & 3]);
                    }
                }

                if (PaddlePositionChanged != PaddlePositionChangedHandlerDefault)
                {
                    const int AXISRANGE = 1000;
                    const int StelladaptorPaddleRange = (int)((AXISRANGE << 1) * 0.34);

                    var prevPos = prevState.InterpretStelladaptorPaddlePosition(0);
                    var currPos = currState.InterpretStelladaptorPaddlePosition(0);
                    if (prevPos != currPos)
                    {
                        PaddlePositionChanged(0, StelladaptorPaddleRange, currPos);
                    }

                    prevPos = prevState.InterpretStelladaptorPaddlePosition(1);
                    currPos = currState.InterpretStelladaptorPaddlePosition(1);
                    if (prevPos != currPos)
                    {
                        PaddlePositionChanged(1, StelladaptorPaddleRange, currPos);
                    }
                }
            }
        }

        internal void RaiseEventsFromXinput()
        {
            XInputNativeMethods.Poll(InternalDeviceNumber, out var currState, out var prevState);

            if (ButtonChanged != ButtonChangedHandlerDefault)
            {
                for (int i = 0; i < 4; i++)
                {
                    var prevButton = prevState.InterpretButtonDown(i);
                    var currButton = currState.InterpretButtonDown(i);
                    if (prevButton != currButton)
                    {
                        if (i == 0)
                        {
                            ButtonChanged(MachineInput.Fire, currButton);
                        }
                        else if (i == 1)
                        {
                            ButtonChanged(MachineInput.Fire2, currButton);
                        }
                    }
                }

                var prevLeft = prevState.InterpretJoyLeft();
                var currLeft = currState.InterpretJoyLeft();
                if (prevLeft != currLeft)
                {
                    ButtonChanged(MachineInput.Left, currLeft);
                }

                var prevRight = prevState.InterpretJoyRight();
                var currRight = currState.InterpretJoyRight();
                if (prevRight != currRight)
                {
                    ButtonChanged(MachineInput.Right, currRight);
                }

                var prevUp = prevState.InterpretJoyUp();
                var currUp = currState.InterpretJoyUp();
                if (prevUp != currUp)
                {
                    ButtonChanged(MachineInput.Up, currUp);
                }

                var prevDown = prevState.InterpretJoyDown();
                var currDown = currState.InterpretJoyDown();
                if (prevDown != currDown)
                {
                    ButtonChanged(MachineInput.Down, currDown);
                }

                var prevBack = prevState.InterpretButtonBack();
                var currBack = currState.InterpretButtonBack();
                if (prevBack != currBack)
                {
                    ButtonChanged(MachineInput.End, currBack);
                }

                var prevStart = prevState.InterpretButtonStart();
                var currStart = currState.InterpretButtonStart();
                if (prevStart != currStart)
                {
                    ButtonChanged(MachineInput.Start, currStart);
                }
            }
        }

        internal GameController() {}
    }
}
