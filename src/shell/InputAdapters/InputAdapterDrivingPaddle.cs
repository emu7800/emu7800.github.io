// © Mike Murphy

using EMU7800.Core;
using EMU7800.Win32.Interop;

namespace EMU7800.D2D.Shell
{
    public sealed class InputAdapterDrivingPaddle : IInputAdapter
    {
        static readonly MachineInput[] _mapping = { MachineInput.Driving0, MachineInput.Driving1, MachineInput.Driving2, MachineInput.Driving3 };

        readonly InputState _inputState;
        readonly int _jackNo;
        readonly int _rotCounterRate = (int)System.Diagnostics.Stopwatch.Frequency / 10;

        int _direction, _rotCounter, _curGrayCode;
        bool _emulationOff;

        public void ScreenResized(D2D_POINT_2F location, D2D_SIZE_F size)
        {
        }

        public void JoystickChanged(int playerNo, MachineInput machineInput, bool down)
        {
            switch (machineInput)
            {
                case MachineInput.Fire:
                case MachineInput.Fire2:
                case MachineInput.Up:
                    _inputState.RaiseInput(_jackNo, MachineInput.Fire, down);
                    break;
                case MachineInput.Left:
                    _direction += down ? -1 : 1;
                    break;
                case MachineInput.Right:
                    _direction += down ? 1 : -1;
                    break;
            }
        }

        public void ProLineJoystickChanged(int playerNo, MachineInput machineInput, bool down)
        {
        }

        public void PaddleChanged(int playerNo, int ohms)
        {
        }

        public void DrivingPaddleChanged(int playerNo, MachineInput machineInput)
        {
            _emulationOff = true;
            _inputState.RaiseInput(_jackNo, machineInput, true);
        }

        public void KeyboardKeyPressed(int playerNo, KeyboardKey key, bool down)
        {
            switch (key)
            {
                case KeyboardKey.X:
                case KeyboardKey.Z:
                case KeyboardKey.Up:
                    _inputState.RaiseInput(_jackNo, MachineInput.Fire, down);
                    break;
                case KeyboardKey.Left:
                    _direction += down ? -1 : 1;
                    break;
                case KeyboardKey.Right:
                    _direction += down ? 1 : -1;
                    break;
            }
        }

        public void MouseMoved(int playerNo, int x, int y, int dx, int dy)
        {
        }

        public void MouseButtonChanged(int playerNo, int x, int y, bool down, bool touchMode)
        {
        }

        public void Update(TimerDevice td)
        {
            if (_emulationOff)
                return;

            _rotCounter -= td.DeltaTicks;
            if (_rotCounter > 0)
                return;
            _rotCounter += _rotCounterRate;

            if (_direction == 0)
                return;

            _curGrayCode += _direction;
            _curGrayCode &= 3;

            _inputState.RaiseInput(_jackNo, _mapping[_curGrayCode], true);
        }

        public InputAdapterDrivingPaddle(InputState inputState, int jackNo)
        {
            _inputState = inputState;
            _jackNo = jackNo;
        }
    }
}