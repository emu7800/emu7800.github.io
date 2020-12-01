// © Mike Murphy

using EMU7800.Core;
using EMU7800.Win32.Interop;

namespace EMU7800.D2D.Shell
{
    public sealed class InputAdapterNull : IInputAdapter
    {
        public void ScreenResized(D2D_POINT_2F location, D2D_SIZE_F size)
        {
        }

        public void JoystickChanged(int playerNo, MachineInput machineInput, bool down)
        {
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
}