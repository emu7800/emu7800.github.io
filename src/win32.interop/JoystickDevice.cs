// © Mike Murphy

using static EMU7800.Win32.Interop.DirectInputNativeMethods;

namespace EMU7800.Win32.Interop
{
    public enum JoystickDirectionalButtonEnum { Left, Right, Up, Down };

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

        public JoystickType JoystickType { get; private set; } = JoystickType.Normal;

        public JoystickButtonChangedHandler JoystickButtonChanged { get; set; } = JoystickButtonChangedHandlerDefault;
        public JoystickDirectionalButtonChangedHandler JoystickDirectionalButtonChanged { get; set; } = JoystickDirectionalButtonChangedHandlerDefault;
        public StelladaptorDrivingPositionChangedHandler StelladaptorDrivingPositionChanged { get; set; } = StelladaptorDrivingPositionChangedHandlerDefault;
        public StelladaptorPaddlePositionChangedHandler StelladaptorPaddlePositionChanged { get; set; } = StelladaptorPaddlePositionChangedHandlerDefault;
        public Daptor2ModeChangedHandler Daptor2ModeChanged { get; set; } = Daptor2ModeChangedHandlerDefault;

        internal void RaiseEvents(ref DIJOYSTATE2 currState, ref DIJOYSTATE2 prevState)
        {
            if (JoystickButtonChanged != JoystickButtonChangedHandlerDefault)
            {
                for (int i = 0; i < 16; i++)
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

            if (JoystickType == JoystickType.Normal)
                return;

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

        public JoystickDevice(JoystickType joystickType)
            => JoystickType = joystickType;
    }
}
