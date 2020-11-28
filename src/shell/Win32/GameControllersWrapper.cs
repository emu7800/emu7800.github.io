// © Mike Murphy

using EMU7800.Core;
using EMU7800.Win32.Interop;

namespace EMU7800.D2D.Shell
{
    public sealed class GameControllersWrapper : IGameControllers
    {
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

        readonly GameControl _gameControl;
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

        public GameControllersWrapper(GameControl gameControl)
        {
            LeftJackHasAtariAdaptor = false;
            RightJackHasAtariAdaptor = false;

            _gameControl = gameControl;

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

                jd.Daptor2ModeChanged                 += mode                 => Daptor2ModeChanged(joystickNo, mode);
                jd.StelladaptorPaddlePositionChanged  += (paddleno, position) => StelladaptorPaddlePositionChanged(joystickNo, paddleno, position);
                jd.StelladaptorDrivingPositionChanged += position             => StelladaptorDrivingPositionChanged(joystickNo, position);
                jd.JoystickButtonChanged              += (buttonno, down)     => JoystickButtonChanged(joystickNo, buttonno, down);
                jd.JoystickDirectionalButtonChanged   += (button, down)       => JoystickDirectionalButtonChanged(joystickNo, button, down);
            }
        }

        #endregion

        #region Helpers

        void Daptor2ModeChanged(int joystickNo, int mode)
        {
            _daptor2Mode[joystickNo] = mode;
        }

        void JoystickDirectionalButtonChanged(int joystickNo, JoystickDirectionalButtonEnum button, bool down)
        {
            switch (button)
            {
                case JoystickDirectionalButtonEnum.Down:
                    _gameControl.JoystickChanged(joystickNo, MachineInput.Down, down);
                    _gameControl.ProLineJoystickChanged(joystickNo, MachineInput.Down, down);
                    break;
                case JoystickDirectionalButtonEnum.Up:
                    _gameControl.JoystickChanged(joystickNo, MachineInput.Up, down);
                    _gameControl.ProLineJoystickChanged(joystickNo, MachineInput.Up, down);
                    break;
                case JoystickDirectionalButtonEnum.Left:
                    _gameControl.JoystickChanged(joystickNo, MachineInput.Left, down);
                    _gameControl.ProLineJoystickChanged(joystickNo, MachineInput.Left, down);
                    break;
                case JoystickDirectionalButtonEnum.Right:
                    _gameControl.JoystickChanged(joystickNo, MachineInput.Right, down);
                    _gameControl.ProLineJoystickChanged(joystickNo, MachineInput.Right, down);
                    break;
            }
        }

        void JoystickButtonChanged(int joystickNo, int buttonno, bool down)
        {
            switch (_daptor2Mode[joystickNo])
            {
                case 1:
                    Daptor2ButtonChangedFor7800Mode(joystickNo, buttonno, down);
                    break;
                case 2:
                    Daptor2ButtonChangedForKeypadMode(joystickNo, buttonno, down);
                    break;
                default:
                    RegularJoystickButtonChanged(joystickNo, buttonno, down);
                    break;
            }
        }

        void RegularJoystickButtonChanged(int joystickNo, int buttonno, bool down)
        {
            switch (buttonno)
            {
                case 0:
                    _gameControl.JoystickChanged(joystickNo, MachineInput.Fire, down);
                    _gameControl.ProLineJoystickChanged(joystickNo, MachineInput.Fire2, down);
                    break;
                case 1:
                    _gameControl.JoystickChanged(joystickNo, MachineInput.Fire2, down);
                    _gameControl.ProLineJoystickChanged(joystickNo, MachineInput.Fire, down);
                    break;
            }
        }

        void Daptor2ButtonChangedFor7800Mode(int joystickNo, int buttonno, bool down)
        {
            switch (buttonno)
            {
                case 2:
                    _gameControl.JoystickChanged(joystickNo, MachineInput.Fire, down);
                    _gameControl.ProLineJoystickChanged(joystickNo, MachineInput.Fire2, down);
                    break;
                case 3:
                    _gameControl.JoystickChanged(joystickNo, MachineInput.Fire2, down);
                    _gameControl.ProLineJoystickChanged(joystickNo, MachineInput.Fire, down);
                    break;
            }
        }

        void StelladaptorPaddlePositionChanged(int joystickNo, int paddleno, int position)
        {
            const int AXISRANGE = 1000;
            const int StelladaptorPaddleRange = (int)((AXISRANGE << 1) * 0.34);
            var paddlePlayerNo = ((joystickNo << 1) | (paddleno & 1) & 3);
            _gameControl.PaddleChanged(paddlePlayerNo, StelladaptorPaddleRange, position);
        }

        void StelladaptorDrivingPositionChanged(int joystickNo, int position)
        {
            _gameControl.DrivingPaddleChanged(joystickNo, _stelladaptorDrivingMachineInputMapping[position & 3]);
        }

        void Daptor2ButtonChangedForKeypadMode(int joystickNo, int buttonno, bool down)
        {
            _gameControl.JoystickChanged(joystickNo, _daptor2KeypadToMachineInputMapping[buttonno & 0xf], down);
        }

        #endregion
    }
}
