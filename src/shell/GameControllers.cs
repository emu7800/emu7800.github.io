// © Mike Murphy

using EMU7800.Core;
using EMU7800.Win32.Interop;

namespace EMU7800.D2D.Shell
{
    public sealed class GameControllers
    {
        public static readonly GameControllers Default = new GameControllers();

        #region Fields

        static readonly MachineInput[] _stelladaptorDrivingMachineInputMapping =
        {
            MachineInput.Driving0, MachineInput.Driving1, MachineInput.Driving2, MachineInput.Driving3
        };

        static readonly MachineInput[] _daptor2KeypadToMachineInputMapping =
        {
            MachineInput.NumPad1,    MachineInput.NumPad2, MachineInput.NumPad3,
            MachineInput.NumPad4,    MachineInput.NumPad5, MachineInput.NumPad6,
            MachineInput.NumPad7,    MachineInput.NumPad8, MachineInput.NumPad9,
            MachineInput.NumPadMult, MachineInput.NumPad0, MachineInput.NumPadHash,
            MachineInput.NumPad0,    MachineInput.NumPad0, MachineInput.NumPad0,
            MachineInput.NumPad0
        };

        readonly int[] _daptor2Mode = new int[2];

        #endregion

        public bool LeftJackHasAtariAdaptor { get; private set; }
        public bool RightJackHasAtariAdaptor { get; private set; }

        public void Poll()
        {
            JoystickDeviceList.Poll();
        }

        public string GetControllerInfo(int controllerNo)
        {
            if (controllerNo < 0 || controllerNo >= JoystickDeviceList.Joysticks.Length)
                return string.Empty;

            switch (JoystickDeviceList.Joysticks[controllerNo].JoystickType)
            {
                case JoystickType.Daptor2:
                    var daptor2Mode = string.Empty;
                    switch (_daptor2Mode[controllerNo])
                    {
                        case 0: daptor2Mode = " (2600 mode)";   break;
                        case 1: daptor2Mode = " (7800 mode)";   break;
                        case 2: daptor2Mode = " (Keypad mode)"; break;
                    }
                    return JoystickDeviceList.Joysticks[controllerNo].ProductName + daptor2Mode;
                default:
                    return JoystickDeviceList.Joysticks[controllerNo].ProductName;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            JoystickDeviceList.Close();
        }

        #endregion

        #region Constructors

        private GameControllers()
        {
            LeftJackHasAtariAdaptor = false;
            RightJackHasAtariAdaptor = false;
        }

        public GameControllers(GameControl gameControl)
        {
            LeftJackHasAtariAdaptor = false;
            RightJackHasAtariAdaptor = false;

            JoystickDeviceList.Initialize();

            for (var i = 0; i < JoystickDeviceList.Joysticks.Length; i++)
            {
                var joystickNo = i;
                var jd = JoystickDeviceList.Joysticks[joystickNo];

                if (jd.JoystickType == JoystickType.Daptor2
                    || jd.JoystickType == JoystickType.Daptor
                        || jd.JoystickType == JoystickType.Stelladaptor)
                {
                    if (joystickNo == 0)
                        LeftJackHasAtariAdaptor = true;
                    if (joystickNo == 1)
                        RightJackHasAtariAdaptor = true;
               }

                jd.Daptor2ModeChanged                 += mode                 => _daptor2Mode[joystickNo] = mode;
                jd.StelladaptorPaddlePositionChanged  += (paddleno, position) => StelladaptorPaddlePositionChanged(gameControl, joystickNo, paddleno, position);
                jd.StelladaptorDrivingPositionChanged += position             => StelladaptorDrivingPositionChanged(gameControl, joystickNo, position);
                jd.JoystickButtonChanged              += (buttonno, down)     => JoystickButtonChanged(gameControl, _daptor2Mode, joystickNo, buttonno, down);
                jd.JoystickDirectionalButtonChanged   += (button, down)       => JoystickDirectionalButtonChanged(gameControl, joystickNo, button, down);
            }
        }

        #endregion

        #region Helpers

        static void JoystickDirectionalButtonChanged(GameControl gameControl, int joystickNo, JoystickDirectionalButtonEnum button, bool down)
        {
            switch (button)
            {
                case JoystickDirectionalButtonEnum.Down:
                    gameControl.JoystickChanged(joystickNo, MachineInput.Down, down);
                    gameControl.ProLineJoystickChanged(joystickNo, MachineInput.Down, down);
                    break;
                case JoystickDirectionalButtonEnum.Up:
                    gameControl.JoystickChanged(joystickNo, MachineInput.Up, down);
                    gameControl.ProLineJoystickChanged(joystickNo, MachineInput.Up, down);
                    break;
                case JoystickDirectionalButtonEnum.Left:
                    gameControl.JoystickChanged(joystickNo, MachineInput.Left, down);
                    gameControl.ProLineJoystickChanged(joystickNo, MachineInput.Left, down);
                    break;
                case JoystickDirectionalButtonEnum.Right:
                    gameControl.JoystickChanged(joystickNo, MachineInput.Right, down);
                    gameControl.ProLineJoystickChanged(joystickNo, MachineInput.Right, down);
                    break;
            }
        }

        static void JoystickButtonChanged(GameControl gameControl, int[] daptor2Mode, int joystickNo, int buttonno, bool down)
        {
            switch (daptor2Mode[joystickNo % daptor2Mode.Length])
            {
                case 1:
                    Daptor2ButtonChangedFor7800Mode(gameControl, joystickNo, buttonno, down);
                    break;
                case 2:
                    Daptor2ButtonChangedForKeypadMode(gameControl, joystickNo, buttonno, down);
                    break;
                default:
                    RegularJoystickButtonChanged(gameControl, joystickNo, buttonno, down);
                    break;
            }
        }

        static void RegularJoystickButtonChanged(GameControl gameControl, int joystickNo, int buttonno, bool down)
        {
            switch (buttonno)
            {
                case 0:
                    gameControl.JoystickChanged(joystickNo, MachineInput.Fire, down);
                    gameControl.ProLineJoystickChanged(joystickNo, MachineInput.Fire2, down);
                    break;
                case 1:
                    gameControl.JoystickChanged(joystickNo, MachineInput.Fire2, down);
                    gameControl.ProLineJoystickChanged(joystickNo, MachineInput.Fire, down);
                    break;
            }
        }

        static void Daptor2ButtonChangedFor7800Mode(GameControl gameControl, int joystickNo, int buttonno, bool down)
        {
            switch (buttonno)
            {
                case 2:
                    gameControl.JoystickChanged(joystickNo, MachineInput.Fire, down);
                    gameControl.ProLineJoystickChanged(joystickNo, MachineInput.Fire2, down);
                    break;
                case 3:
                    gameControl.JoystickChanged(joystickNo, MachineInput.Fire2, down);
                    gameControl.ProLineJoystickChanged(joystickNo, MachineInput.Fire, down);
                    break;
            }
        }

        static void StelladaptorPaddlePositionChanged(GameControl gameControl, int joystickNo, int paddleno, int position)
        {
            const int AXISRANGE = 1000;
            const int StelladaptorPaddleRange = (int)((AXISRANGE << 1) * 0.34);
            var paddlePlayerNo = (joystickNo << 1) | paddleno & 1 & 3;
            gameControl.PaddleChanged(paddlePlayerNo, StelladaptorPaddleRange, position);
        }

        static void StelladaptorDrivingPositionChanged(GameControl gameControl, int joystickNo, int position)
        {
            gameControl.DrivingPaddleChanged(joystickNo, _stelladaptorDrivingMachineInputMapping[position & 3]);
        }

        static void Daptor2ButtonChangedForKeypadMode(GameControl gameControl, int joystickNo, int buttonno, bool down)
        {
            gameControl.JoystickChanged(joystickNo, _daptor2KeypadToMachineInputMapping[buttonno & 0xf], down);
        }

        #endregion
    }
}
