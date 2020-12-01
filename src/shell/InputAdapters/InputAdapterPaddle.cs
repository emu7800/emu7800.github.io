// © Mike Murphy

using EMU7800.Core;
using EMU7800.Win32.Interop;

namespace EMU7800.D2D.Shell
{
    public sealed class InputAdapterPaddle : IInputAdapter
    {
        readonly InputState _inputState;
        readonly int _jackNo;

        const float EmulationRotationalVelocity= 1f; // factor of _currentXWidth per second
        readonly int[] _emulationDirection = new int[2];
        readonly bool[] _emulationOff = new bool[2];

        readonly int[] _currentXPosition = new int[2];
        int _currentXLocation, _currentXWidth = 1;
        int _startYForPaddleInput;

        public void ScreenResized(D2D_POINT_2F location, D2D_SIZE_F size)
        {
            _currentXLocation = (int)location.X;
            if (size.Width > 0)
                _currentXWidth = (int)size.Width;
            _startYForPaddleInput = (int)(location.Y + size.Height)*2/3;
        }

        public void JoystickChanged(int playerNo, MachineInput machineInput, bool down)
        {
            switch (machineInput)
            {
                case MachineInput.Fire:
                case MachineInput.Fire2:
                    _inputState.RaiseInput(ToPaddlePlayerNo(playerNo), MachineInput.Fire, down);
                    break;
                case MachineInput.Left:
                    _emulationDirection[playerNo & 1] += down ? -1 : 1;
                    break;
                case MachineInput.Right:
                    _emulationDirection[playerNo & 1] += down ? 1 : -1;
                    break;
            }
        }

        public void ProLineJoystickChanged(int playerNo, MachineInput machineInput, bool down)
        {
        }

        public void PaddleChanged(int playerNo, int valMax, int val)
        {
            _emulationOff[playerNo & 1] = true;
            _inputState.RaisePaddleInput(ToPaddlePlayerNo(playerNo), valMax, val);
        }

        public void DrivingPaddleChanged(int playerNo, MachineInput machineInput)
        {
        }

        public void KeyboardKeyPressed(int playerNo, KeyboardKey key, bool down)
        {
            switch (key)
            {
                case KeyboardKey.Z:
                case KeyboardKey.X:
                    _inputState.RaiseInput(ToPaddlePlayerNo(playerNo), MachineInput.Fire, down);
                    break;
                case KeyboardKey.Left:
                    _emulationDirection[playerNo & 1] += down ? -1 : 1;
                    break;
                case KeyboardKey.Right:
                    _emulationDirection[playerNo & 1] += down ? 1 : -1;
                    break;
            }
        }

        public void MouseMoved(int playerNo, int x, int y, int dx, int dy)
        {
            if (_emulationOff[playerNo & 1])
                return;

            if (y < _startYForPaddleInput)
                return;

            var tx = x - _currentXLocation;
            if (tx < 0)
                tx = 0;
            else if (tx > _currentXWidth)
                tx = _currentXWidth;

            _currentXPosition[playerNo & 1] = tx;
            _inputState.RaisePaddleInput(ToPaddlePlayerNo(playerNo), _currentXWidth, _currentXPosition[playerNo & 1]);
        }

        public void MouseButtonChanged(int playerNo, int x, int y, bool down, bool touchMode)
        {
            if (touchMode)
                return;
            MouseMoved(playerNo, x, y, 0, 0);
            _inputState.RaiseInput(ToPaddlePlayerNo(playerNo), MachineInput.Fire, down);
        }

        public void Update(TimerDevice td)
        {
            for (var i = 0; i < 2; i++)
            {
                if (_emulationOff[i] || _emulationDirection[i] == 0)
                    continue;

                _currentXPosition[i] += (int)(td.DeltaInSeconds * EmulationRotationalVelocity * _currentXWidth * _emulationDirection[i]);

                if (_currentXPosition[i] < 0)
                    _currentXPosition[i] = 0;
                else if (_currentXPosition[i] > _currentXWidth)
                    _currentXPosition[i] = _currentXWidth;

                _inputState.RaisePaddleInput(ToPaddlePlayerNo(i), _currentXWidth, _currentXPosition[i]);
            }
        }

        public InputAdapterPaddle(InputState inputState, int jackNo)
        {
            _inputState = inputState;
            _jackNo = jackNo;
        }

        int ToPaddlePlayerNo(int playerNo)
        {
            var paddlePlayerNo = (_jackNo << 1) | (playerNo & 1);
            return paddlePlayerNo;
        }
    }
}