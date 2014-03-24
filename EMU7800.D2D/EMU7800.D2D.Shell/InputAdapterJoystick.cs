// � Mike Murphy

using EMU7800.Core;
using EMU7800.D2D.Interop;

namespace EMU7800.D2D.Shell
{
    public sealed class InputAdapterJoystick : IInputAdapter
    {
        readonly InputState _inputState;
        readonly int _jackNo;

        public void ScreenResized(PointF location, SizeF size)
        {
        }

        public void JoystickChanged(int playerNo, MachineInput machineInput, bool down)
        {
            switch (machineInput)
            {
                case MachineInput.Left:
                case MachineInput.Right:
                case MachineInput.Up:
                case MachineInput.Down:
                case MachineInput.Fire:
                case MachineInput.Fire2:
                    _inputState.RaiseInput(_jackNo, machineInput, down);
                    break;
            }
        }

        public void ProLineJoystickChanged(int playerNo, MachineInput machineInput, bool down)
        {
        }

        public void PaddleChanged(int playerNo, int valMax, int val)
        {
        }

        public void DrivingPaddleChanged(int playerNo, MachineInput machineInput)
        {
        }

        public void KeyboardKeyPressed(int playerNo, KeyboardKey key, bool down)
        {
            switch (key)
            {
                case KeyboardKey.Z:  // left fire mapped to 2600 fire
                    _inputState.RaiseInput(_jackNo, MachineInput.Fire, down);
                    break;
                case KeyboardKey.X:  // right fire mapped to booster trigger
                    _inputState.RaiseInput(_jackNo, MachineInput.Fire2, down);
                    break;
                case KeyboardKey.Left:
                    _inputState.RaiseInput(_jackNo, MachineInput.Left, down);
                    break;
                case KeyboardKey.Right:
                    _inputState.RaiseInput(_jackNo, MachineInput.Right, down);
                    break;
                case KeyboardKey.Up:
                    _inputState.RaiseInput(_jackNo, MachineInput.Up, down);
                    break;
                case KeyboardKey.Down:
                    _inputState.RaiseInput(_jackNo, MachineInput.Down, down);
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
        }

        public InputAdapterJoystick(InputState inputState, int jackNo)
        {
            _inputState = inputState;
            _jackNo = jackNo;
        }
    }
}