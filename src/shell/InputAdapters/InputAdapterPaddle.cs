// © Mike Murphy

using EMU7800.Core;
using EMU7800.Win32.Interop;

namespace EMU7800.D2D.Shell;

public sealed class InputAdapterPaddle(InputState inputState) : IInputAdapter
{
    readonly int[] _emulationDirection = new int[4];

    readonly int[] _currentXPosition = new int[4];
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
    }

    public void ProLineJoystickChanged(int playerNo, MachineInput machineInput, bool down)
    {
    }

    public void PaddleChanged(int playerNo, int ohms)
    {
        inputState.RaisePaddleInput(playerNo, ohms);
    }

    public void PaddleButtonChanged(int playerNo, bool down)
    {
        inputState.RaiseInput(playerNo, MachineInput.Fire, down);
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
                inputState.RaiseInput(playerNo, MachineInput.Fire, down);
                break;
            case KeyboardKey.Left:
                _emulationDirection[playerNo] = down ? -1 : 0;
                break;
            case KeyboardKey.Right:
                _emulationDirection[playerNo] = down ? 1 : 0;
                break;
        }
    }

    public void MouseMoved(int playerNo, int x, int y, int dx, int dy)
    {
        if (y < _startYForPaddleInput)
            return;

        var tx = x - _currentXLocation;
        if (tx < 0)
            tx = 0;
        else if (tx > _currentXWidth)
            tx = _currentXWidth;

        _currentXPosition[playerNo] = tx;

        var ohms = 1000000 * (_currentXWidth - _currentXPosition[playerNo]) / _currentXWidth;

        inputState.RaisePaddleInput(playerNo, ohms);
    }

    public void MouseButtonChanged(int playerNo, int x, int y, bool down, bool touchMode)
    {
        if (touchMode)
            return;
        MouseMoved(playerNo, x, y, 0, 0);
        inputState.RaiseInput(playerNo, MachineInput.Fire, down);
    }

    public void Update(TimerDevice td)
    {
        for (var i = 0; i < 4; i++)
        {
            if (_emulationDirection[i] == 0)
                continue;

            const int EmulationRotationalVelocity = 1;

            _currentXPosition[i] += (int)(td.DeltaInSeconds * EmulationRotationalVelocity * _currentXWidth * _emulationDirection[i]);

            if (_currentXPosition[i] < 0)
                _currentXPosition[i] = 0;
            else if (_currentXPosition[i] > _currentXWidth)
                _currentXPosition[i] = _currentXWidth;

            var ohms = 1000000 * (_currentXWidth - _currentXPosition[i]) / _currentXWidth;

            inputState.RaisePaddleInput(i, ohms);
        }
    }
}