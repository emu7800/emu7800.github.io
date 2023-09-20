// © Mike Murphy

using EMU7800.Core;
using EMU7800.Win32.Interop;
using static System.Console;

namespace EMU7800.D2D.Shell;

public sealed class InputAdapterPaddle(InputState inputState, int jackNo) : IInputAdapter
{
    readonly InputState _inputState = inputState;
    readonly int _jackNo = jackNo;

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

    public void PaddleChanged(int playerNo, int ohms)
    {
        if (!_emulationOff[playerNo & 1])
        {
            _emulationOff[playerNo & 1] = true;
            WriteLine("Real paddle input detected, turning off emulated paddle input from mouse movement");
        }
        _inputState.RaisePaddleInput(ToPaddlePlayerNo(playerNo), ohms);
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

        var ohms = 1000000 * (_currentXWidth - _currentXPosition[playerNo & 1]) / _currentXWidth;

        _inputState.RaisePaddleInput(ToPaddlePlayerNo(playerNo), ohms);
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
    }

    int ToPaddlePlayerNo(int playerNo)
        => (_jackNo << 1) | (playerNo & 1);
}