// © Mike Murphy

using EMU7800.Core;
using EMU7800.D2D.Interop;

namespace EMU7800.D2D.Shell
{
    public sealed class InputAdapterKeypad : IInputAdapter
    {
        readonly InputState _inputState;
        readonly int _jackNo;

        public void ScreenResized(PointF location, SizeF size)
        {
        }

        public void JoystickChanged(int playerNo, MachineInput machineInput, bool down)
        {
            _inputState.RaiseInput(_jackNo, machineInput, down);
        }

        public void ProLineJoystickChanged(int playerNo, MachineInput machineInput, bool down)
        {
            _inputState.RaiseInput(_jackNo, machineInput, down);
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
                case KeyboardKey.NumberPad0:
                    _inputState.RaiseInput(_jackNo, MachineInput.NumPad0, down);
                    break;
                case KeyboardKey.NumberPad1:
                    _inputState.RaiseInput(_jackNo, MachineInput.NumPad1, down);
                    break;
                case KeyboardKey.NumberPad2:
                    _inputState.RaiseInput(_jackNo, MachineInput.NumPad2, down);
                    break;
                case KeyboardKey.NumberPad3:
                    _inputState.RaiseInput(_jackNo, MachineInput.NumPad3, down);
                    break;
                case KeyboardKey.NumberPad4:
                    _inputState.RaiseInput(_jackNo, MachineInput.NumPad4, down);
                    break;
                case KeyboardKey.NumberPad5:
                    _inputState.RaiseInput(_jackNo, MachineInput.NumPad5, down);
                    break;
                case KeyboardKey.NumberPad6:
                    _inputState.RaiseInput(_jackNo, MachineInput.NumPad6, down);
                    break;
                case KeyboardKey.NumberPad7:
                    _inputState.RaiseInput(_jackNo, MachineInput.NumPad7, down);
                    break;
                case KeyboardKey.NumberPad8:
                    _inputState.RaiseInput(_jackNo, MachineInput.NumPad8, down);
                    break;
                case KeyboardKey.NumberPad9:
                    _inputState.RaiseInput(_jackNo, MachineInput.NumPad9, down);
                    break;
                case KeyboardKey.Multiply:
                    _inputState.RaiseInput(_jackNo, MachineInput.NumPadMult, down);
                    break;
                case KeyboardKey.Add:
                    _inputState.RaiseInput(_jackNo, MachineInput.NumPadHash, down);
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

        public InputAdapterKeypad(InputState inputState, int jackNo)
        {
            _inputState = inputState;
            _jackNo = jackNo;
        }
    }
}