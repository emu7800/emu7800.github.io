// © Mike Murphy

using EMU7800.Core;
using EMU7800.Win32.Interop;

namespace EMU7800.D2D.Shell;

public sealed class InputAdapterProlineJoystick(InputState inputState, int jackNo) : IInputAdapter
{
    readonly InputState _inputState = inputState;
    readonly int _jackNo = jackNo;

    public void ScreenResized(D2D_POINT_2F location, D2D_SIZE_F size)
    {
    }

    public void JoystickChanged(int playerNo, MachineInput machineInput, bool down)
    {
    }

    public void ProLineJoystickChanged(int playerNo, MachineInput machineInput, bool down)
    {
        switch (machineInput)
        {
            case MachineInput.Left:
            case MachineInput.Right:
            case MachineInput.Up:
            case MachineInput.Down:
                _inputState.RaiseInput(_jackNo, machineInput, down);
                break;
            case MachineInput.Fire:
                _inputState.RaiseInput(_jackNo, MachineInput.Fire2, down);
                break;
            case MachineInput.Fire2:
                _inputState.RaiseInput(_jackNo, MachineInput.Fire, down);
                break;
        }
    }

    public void PaddleChanged(int playerNo, int ohms)
    {
    }

    public void DrivingPaddleChanged(int playerNo, MachineInput machineInput)
    {
    }

    public void KeyboardKeyPressed(int playerNo, KeyboardKey key, bool down)
    {
        switch (key)
        {
            case KeyboardKey.Z:  // left fire button
                _inputState.RaiseInput(_jackNo, MachineInput.Fire2, down);
                break;
            case KeyboardKey.X:  // right fire button
                _inputState.RaiseInput(_jackNo, MachineInput.Fire, down);
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
}