// © Mike Murphy

using EMU7800.Core;
using EMU7800.Win32.Interop;

namespace EMU7800.D2D.Shell;

public sealed class InputAdapterDrivingPaddle(InputState inputState) : IInputAdapter
{
    static readonly MachineInput[] _mapping = [MachineInput.Driving0, MachineInput.Driving1, MachineInput.Driving2, MachineInput.Driving3];
    static readonly int RotCounterRate = (int)System.Diagnostics.Stopwatch.Frequency / 10;
    readonly int[] _direction = new int[2];
    readonly int[] _curGrayCode = new int[2];

    int _rotCounter;

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
                inputState.RaiseInput(playerNo, MachineInput.Fire, down);
                break;
            case MachineInput.Left:
                _direction[playerNo] = down ? -1 : 0;
                break;
            case MachineInput.Right:
                _direction[playerNo] = down ? 1 : 0;
                break;
        }
    }

    public void ProLineJoystickChanged(int playerNo, MachineInput machineInput, bool down)
    {
    }

    public void PaddleChanged(int playerNo, int ohms)
    {
    }

    public void PaddleButtonChanged(int playerNo, bool down)
    {
    }

    public void DrivingPaddleChanged(int playerNo, MachineInput machineInput)
    {
        inputState.RaiseInput(playerNo, machineInput, true);
    }

    public void KeyboardKeyPressed(int playerNo, KeyboardKey key, bool down)
    {
        switch (key)
        {
            case KeyboardKey.X:
            case KeyboardKey.Z:
            case KeyboardKey.Up:
                inputState.RaiseInput(playerNo, MachineInput.Fire, down);
                break;
            case KeyboardKey.Left:
                _direction[playerNo] = down ? -1 : 0;
                break;
            case KeyboardKey.Right:
                _direction[playerNo] = down ? 1 : 0;
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
        _rotCounter -= td.DeltaTicks;
        if (_rotCounter <= 0)
        {
            _rotCounter += RotCounterRate;
            for (var playerNo = 0; playerNo < 2; playerNo++)
            {
                if (_direction[playerNo] != 0)
                {
                    _curGrayCode[playerNo] += _direction[playerNo];
                    _curGrayCode[playerNo] &= 3;
                    inputState.RaiseInput(playerNo, _mapping[_curGrayCode[playerNo]], true);
                }
            }
        }
    }
}