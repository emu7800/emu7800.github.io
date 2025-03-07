﻿// © Mike Murphy

using EMU7800.Core;
using EMU7800.Win32.Interop;

namespace EMU7800.D2D.Shell;

public sealed class InputAdapterJoystick(InputState inputState) : IInputAdapter
{
    public void ScreenResized(D2D_POINT_2F location, D2D_SIZE_F size)
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
                inputState.RaiseInput(playerNo, machineInput, down);
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
    }

    public void KeyboardKeyPressed(int playerNo, KeyboardKey key, bool down)
    {
        switch (key)
        {
            case KeyboardKey.Z:  // left fire mapped to 2600 fire
                inputState.RaiseInput(playerNo, MachineInput.Fire, down);
                break;
            case KeyboardKey.X:  // right fire mapped to booster trigger
                inputState.RaiseInput(playerNo, MachineInput.Fire2, down);
                break;
            case KeyboardKey.Left:
                inputState.RaiseInput(playerNo, MachineInput.Left, down);
                break;
            case KeyboardKey.Right:
                inputState.RaiseInput(playerNo, MachineInput.Right, down);
                break;
            case KeyboardKey.Up:
                inputState.RaiseInput(playerNo, MachineInput.Up, down);
                break;
            case KeyboardKey.Down:
                inputState.RaiseInput(playerNo, MachineInput.Down, down);
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