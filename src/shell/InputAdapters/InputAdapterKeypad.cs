// © Mike Murphy

using EMU7800.Core;
using EMU7800.Win32.Interop;

namespace EMU7800.D2D.Shell;

public sealed class InputAdapterKeypad(InputState inputState) : IInputAdapter
{
    public void ScreenResized(D2D_POINT_2F location, D2D_SIZE_F size)
    {
    }

    public void JoystickChanged(int playerNo, MachineInput machineInput, bool down)
    {
        inputState.RaiseInput(playerNo, machineInput, down);
    }

    public void ProLineJoystickChanged(int playerNo, MachineInput machineInput, bool down)
    {
        inputState.RaiseInput(playerNo, machineInput, down);
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
            case KeyboardKey.NumberPad0:
                inputState.RaiseInput(playerNo, MachineInput.NumPad0, down);
                break;
            case KeyboardKey.NumberPad1:
                inputState.RaiseInput(playerNo, MachineInput.NumPad1, down);
                break;
            case KeyboardKey.NumberPad2:
                inputState.RaiseInput(playerNo, MachineInput.NumPad2, down);
                break;
            case KeyboardKey.NumberPad3:
                inputState.RaiseInput(playerNo, MachineInput.NumPad3, down);
                break;
            case KeyboardKey.NumberPad4:
                inputState.RaiseInput(playerNo, MachineInput.NumPad4, down);
                break;
            case KeyboardKey.NumberPad5:
                inputState.RaiseInput(playerNo, MachineInput.NumPad5, down);
                break;
            case KeyboardKey.NumberPad6:
                inputState.RaiseInput(playerNo, MachineInput.NumPad6, down);
                break;
            case KeyboardKey.NumberPad7:
                inputState.RaiseInput(playerNo, MachineInput.NumPad7, down);
                break;
            case KeyboardKey.NumberPad8:
                inputState.RaiseInput(playerNo, MachineInput.NumPad8, down);
                break;
            case KeyboardKey.NumberPad9:
                inputState.RaiseInput(playerNo, MachineInput.NumPad9, down);
                break;
            case KeyboardKey.Multiply:
                inputState.RaiseInput(playerNo, MachineInput.NumPadMult, down);
                break;
            case KeyboardKey.Add:
                inputState.RaiseInput(playerNo, MachineInput.NumPadHash, down);
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