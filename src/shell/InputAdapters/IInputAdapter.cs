// © Mike Murphy

using EMU7800.Core;
using EMU7800.Win32.Interop;

namespace EMU7800.D2D.Shell
{
    public interface IInputAdapter
    {
        void ScreenResized(D2D_POINT_2F location, D2D_SIZE_F size);
        void JoystickChanged(int playerNo, MachineInput machineInput, bool down);
        void ProLineJoystickChanged(int playerNo, MachineInput machineInput, bool down);
        void PaddleChanged(int playerNo, int valMax, int val);
        void DrivingPaddleChanged(int playerNo, MachineInput machineInput);
        void KeyboardKeyPressed(int playerNo, KeyboardKey key, bool down);
        void MouseMoved(int playerNo, int x, int y, int dx, int dy);
        void MouseButtonChanged(int playerNo, int x, int y, bool down, bool touchMode);
        void Update(TimerDevice td);
    }
}