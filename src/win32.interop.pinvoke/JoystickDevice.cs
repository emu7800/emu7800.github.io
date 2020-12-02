// © Mike Murphy

using static EMU7800.Win32.Interop.DirectInputNativeMethods;
using static EMU7800.Win32.Interop.XInputNativeMethods;

namespace EMU7800.Win32.Interop
{
    public enum JoystickType { None, XInput, Usb, Stelladaptor, Daptor, Daptor2 };

    public enum JoystickDirectionalButtonEnum { Left, Right, Up, Down, Back, Start };

    public delegate void JoystickButtonChangedHandler(int buttonno, bool down);
    public delegate void JoystickDirectionalButtonChangedHandler(JoystickDirectionalButtonEnum button, bool down);
    public delegate void StelladaptorDrivingPositionChangedHandler(int position);
    public delegate void StelladaptorPaddlePositionChangedHandler(int paddleno, int position);
    public delegate void Daptor2ModeChangedHandler(int mode);

    public class JoystickDevice
    {
        public static readonly JoystickButtonChangedHandler JoystickButtonChangedHandlerDefault = (b, d) => {};
        public static readonly JoystickDirectionalButtonChangedHandler JoystickDirectionalButtonChangedHandlerDefault = (b, d) => {};
        public static readonly StelladaptorDrivingPositionChangedHandler StelladaptorDrivingPositionChangedHandlerDefault = p => {};
        public static readonly StelladaptorPaddlePositionChangedHandler StelladaptorPaddlePositionChangedHandlerDefault = (pa, po) => {};
        public static readonly Daptor2ModeChangedHandler Daptor2ModeChangedHandlerDefault = m => {};

        public string ProductName { get; } = string.Empty;
        public int InternalDeviceNumber { get; }
        public JoystickType JoystickType { get; } = JoystickType.None;

        public JoystickButtonChangedHandler JoystickButtonChanged { get; set; } = JoystickButtonChangedHandlerDefault;
        public JoystickDirectionalButtonChangedHandler JoystickDirectionalButtonChanged { get; set; } = JoystickDirectionalButtonChangedHandlerDefault;
        public StelladaptorDrivingPositionChangedHandler StelladaptorDrivingPositionChanged { get; set; } = StelladaptorDrivingPositionChangedHandlerDefault;
        public StelladaptorPaddlePositionChangedHandler StelladaptorPaddlePositionChanged { get; set; } = StelladaptorPaddlePositionChangedHandlerDefault;
        public Daptor2ModeChangedHandler Daptor2ModeChanged { get; set; } = Daptor2ModeChangedHandlerDefault;

        internal void RaiseEventsFromDirectInput(ref DIJOYSTATE2 currState, ref DIJOYSTATE2 prevState)
        {
            if (JoystickButtonChanged != JoystickButtonChangedHandlerDefault)
            {
                for (int i = 0; i < 4; i++)
                {
                    var prevDown = prevState.InterpretJoyButtonDown(i);
                    var currDown = currState.InterpretJoyButtonDown(i);
                    if (prevDown != currDown)
                    {
                        JoystickButtonChanged(i, currDown);
                    }
                }
            }

            if (JoystickDirectionalButtonChanged != JoystickDirectionalButtonChangedHandlerDefault)
            {
                var prevLeft = prevState.InterpretJoyLeft();
                var currLeft = currState.InterpretJoyLeft();
                if (prevLeft != currLeft)
                {
                    JoystickDirectionalButtonChanged(JoystickDirectionalButtonEnum.Left, currLeft);
                }

                var prevRight = prevState.InterpretJoyRight();
                var currRight = currState.InterpretJoyRight();
                if (prevRight != currRight)
                {
                    JoystickDirectionalButtonChanged(JoystickDirectionalButtonEnum.Right, currRight);
                }

                var prevUp = prevState.InterpretJoyUp();
                var currUp = currState.InterpretJoyUp();
                if (prevUp != currUp)
                {
                    JoystickDirectionalButtonChanged(JoystickDirectionalButtonEnum.Up, currUp);
                }

                var prevDown = prevState.InterpretJoyDown();
                var currDown = currState.InterpretJoyDown();
                if (prevDown != currDown)
                {
                    JoystickDirectionalButtonChanged(JoystickDirectionalButtonEnum.Down, currDown);
                }
            }

            if (JoystickType == JoystickType.Stelladaptor || JoystickType == JoystickType.Daptor || JoystickType == JoystickType.Daptor2)
            {
                if (StelladaptorDrivingPositionChanged != StelladaptorDrivingPositionChangedHandlerDefault)
                {
                    var prevPos = prevState.InterpretStelladaptorDrivingPosition();
                    var currPos = currState.InterpretStelladaptorDrivingPosition();
                    if (prevPos != currPos)
                    {
                        StelladaptorDrivingPositionChanged(currPos);
                    }
                }

                if (StelladaptorPaddlePositionChanged != StelladaptorPaddlePositionChangedHandlerDefault)
                {
                    var prevPos = prevState.InterpretStelladaptorPaddlePosition(0);
                    var currPos = currState.InterpretStelladaptorPaddlePosition(0);
                    if (prevPos != currPos)
                    {
                        StelladaptorPaddlePositionChanged(0, currPos);
                    }

                    prevPos = prevState.InterpretStelladaptorPaddlePosition(1);
                    currPos = currState.InterpretStelladaptorPaddlePosition(1);
                    if (prevPos != currPos)
                    {
                        StelladaptorPaddlePositionChanged(1, currPos);
                    }
                }

                if (Daptor2ModeChanged != Daptor2ModeChangedHandlerDefault)
                {
                    var prevMode = prevState.InterpretDaptor2Mode();
                    var currMode = currState.InterpretDaptor2Mode();
                    if (prevMode != currMode)
                    {
                        Daptor2ModeChanged(currMode);
                    }
                }
            }
        }

        internal void RaiseEventsFromXinput(ref XINPUT_STATE currState, ref XINPUT_STATE prevState)
        {
            if (JoystickButtonChanged != JoystickButtonChangedHandlerDefault)
            {
                for (int i = 0; i < 4; i++)
                {
                    var prevDown = prevState.InterpretButtonDown(i);
                    var currDown = currState.InterpretButtonDown(i);
                    if (prevDown != currDown)
                    {
                        JoystickButtonChanged(i, currDown);
                    }
                }
            }

            if (JoystickDirectionalButtonChanged != JoystickDirectionalButtonChangedHandlerDefault)
            {
                var prevLeft = prevState.InterpretJoyLeft();
                var currLeft = currState.InterpretJoyLeft();
                if (prevLeft != currLeft)
                {
                    JoystickDirectionalButtonChanged(JoystickDirectionalButtonEnum.Left, currLeft);
                }

                var prevRight = prevState.InterpretJoyRight();
                var currRight = currState.InterpretJoyRight();
                if (prevRight != currRight)
                {
                    JoystickDirectionalButtonChanged(JoystickDirectionalButtonEnum.Right, currRight);
                }

                var prevUp = prevState.InterpretJoyUp();
                var currUp = currState.InterpretJoyUp();
                if (prevUp != currUp)
                {
                    JoystickDirectionalButtonChanged(JoystickDirectionalButtonEnum.Up, currUp);
                }

                var prevDown = prevState.InterpretJoyDown();
                var currDown = currState.InterpretJoyDown();
                if (prevDown != currDown)
                {
                    JoystickDirectionalButtonChanged(JoystickDirectionalButtonEnum.Down, currDown);
                }

                var prevBack = prevState.InterpretButtonBack();
                var currBack = currState.InterpretButtonBack();
                if (prevBack != currBack)
                {
                    JoystickDirectionalButtonChanged(JoystickDirectionalButtonEnum.Back, currBack);
                }

                var prevStart = prevState.InterpretButtonStart();
                var currStart = currState.InterpretButtonStart();
                if (prevStart != currStart)
                {
                    JoystickDirectionalButtonChanged(JoystickDirectionalButtonEnum.Start, currStart);
                }
            }
        }

        public JoystickDevice(string joystickName, int internalDeviceNumber)
        {
            ProductName = joystickName;
            InternalDeviceNumber = internalDeviceNumber;
            if (joystickName == "Stelladaptor 2600-to-USB Interface")
            {
                JoystickType = JoystickType.Stelladaptor;
            }
            else if (joystickName == "2600-daptor")
            {
                JoystickType = JoystickType.Daptor;
            }
            else if (joystickName == "2600-daptor II")
            {
                JoystickType = JoystickType.Daptor2;
            }
            else if (joystickName.Length > 0)
            {
                JoystickType = JoystickType.Usb;
            }
            else
            {
                JoystickType = JoystickType.None;
            }
        }

        public JoystickDevice(int internalDeviceNumber)
        {
            ProductName = "XBox Default";
            InternalDeviceNumber = internalDeviceNumber;
            JoystickType = JoystickType.XInput;
            XInputNativeMethods.Initialize(InternalDeviceNumber, out var _);
        }

        public void ClearEventHandlers()
        {
            JoystickButtonChanged = JoystickButtonChangedHandlerDefault;
            JoystickDirectionalButtonChanged = JoystickDirectionalButtonChangedHandlerDefault;
            StelladaptorDrivingPositionChanged = StelladaptorDrivingPositionChangedHandlerDefault;
            StelladaptorPaddlePositionChanged = StelladaptorPaddlePositionChangedHandlerDefault;
            Daptor2ModeChanged = Daptor2ModeChangedHandlerDefault;
        }

        public void Poll()
        {
            switch (JoystickType)
            {
                case JoystickType.Usb:
                case JoystickType.Stelladaptor:
                case JoystickType.Daptor:
                case JoystickType.Daptor2:
                    DirectInputNativeMethods.Poll(InternalDeviceNumber, out var currDiState, out var prevDiState);
                    RaiseEventsFromDirectInput(ref currDiState, ref prevDiState);
                    break;
                case JoystickType.XInput:
                    XInputNativeMethods.Poll(InternalDeviceNumber, out var currXiState, out var prevXiState);
                    RaiseEventsFromXinput(ref currXiState, ref prevXiState);
                    break;
                default:
                    break;
            }
        }
    }
}
