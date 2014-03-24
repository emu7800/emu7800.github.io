// © Mike Murphy

using EMU7800.Core;
using EMU7800.D2D.Interop;

namespace EMU7800.D2D.Shell
{
    public sealed class InputAdapterLightgun : IInputAdapter
    {
        readonly InputState _inputState;
        readonly int _jackNo, _startingScanline, _pitch = 320;

        PointF _location;
        SizeF _size;
        float _sfx, _sfy;

        public void ScreenResized(PointF location, SizeF size)
        {
            _location = location;
            _size = size;
            _sfx = size.Width > 0 ? _pitch / size.Width : 0;
            _sfy = size.Height > 0 ? 230 / size.Height : 0;
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
            switch (key)
            {
                case KeyboardKey.X:
                case KeyboardKey.Z:
                    _inputState.RaiseInput(_jackNo, MachineInput.Fire, down);
                    break;
            }
        }

        public void MouseMoved(int playerNo, int x, int y, int dx, int dy)
        {
        }

        public void MouseButtonChanged(int playerNo, int x, int y, bool down, bool touchMode)
        {
            var tx = x - _location.X;
            var ty = y - _location.Y;
            if (tx < 0 || ty < 0 || tx > _size.Width || ty > _size.Height)
                return;
            var scanline = (int)(ty * _sfy) + _startingScanline;
            var hpos = (int)(tx * _sfx);
            _inputState.RaiseLightgunPos(_jackNo, scanline, hpos);

            _inputState.RaiseInput(_jackNo, MachineInput.Fire, down);
        }

        public void Update(TimerDevice td)
        {
        }

        public InputAdapterLightgun(InputState inputState, int jackNo, int startingScanline, MachineType machineType)
        {
            _inputState = inputState;
            _jackNo = jackNo;
            _startingScanline = startingScanline;
            switch (machineType)
            {
                case MachineType.A2600NTSC:
                case MachineType.A2600PAL:
                    _pitch = 160;
                    break;
            }
        }
    }
}