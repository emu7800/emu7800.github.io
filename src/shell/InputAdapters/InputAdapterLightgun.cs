// © Mike Murphy

using EMU7800.Core;
using EMU7800.Win32.Interop;

namespace EMU7800.D2D.Shell;

public sealed class InputAdapterLightgun(InputState inputState, int startingScanline, MachineType machineType) : IInputAdapter
{
    D2D_POINT_2F _location;
    D2D_SIZE_F _size;
    float _sfx, _sfy, _tx, _ty;

    public void ScreenResized(D2D_POINT_2F location, D2D_SIZE_F size)
    {
        _location = location;
        _size = size;

        var pitch = machineType switch
        {
            MachineType.A2600NTSC or MachineType.A2600PAL => 160,
            _ => 320
        };

        _sfx = size.Width > 0 ? pitch / size.Width : 0;
        _sfy = size.Height > 0 ? 230 / size.Height : 0;
    }

    public void JoystickChanged(int playerNo, MachineInput machineInput, bool down)
    {
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
            case KeyboardKey.X:
            case KeyboardKey.Z:
                RaiseLightgunPos(playerNo);
                inputState.RaiseInput(playerNo, MachineInput.Fire, down);
                break;
        }
    }

    public void MouseMoved(int playerNo, int x, int y, int dx, int dy)
    {
        _tx = x - _location.X;
        _ty = y - _location.Y;
    }

    public void MouseButtonChanged(int playerNo, int x, int y, bool down, bool touchMode)
    {
        RaiseLightgunPos(playerNo);
        inputState.RaiseInput(playerNo, MachineInput.Fire, down);
    }

    public void Update(TimerDevice td)
    {
    }

    void RaiseLightgunPos(int playerNo)
    {
        if (_tx < 0 || _ty < 0 || _tx > _size.Width || _ty > _size.Height)
            return;
        var scanline = (int)(_ty * _sfy) + startingScanline;
        var hpos = (int)(_tx * _sfx);
        inputState.RaiseLightgunPos(playerNo, scanline, hpos);
    }
}