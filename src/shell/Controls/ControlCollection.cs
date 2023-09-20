// © Mike Murphy

using EMU7800.Core;
using System.Linq;

namespace EMU7800.D2D.Shell;

public sealed class ControlCollection : ControlBase
{
    #region Fields

    const int ArrayAllocationChunkSize = 8;
    ControlBase[] _controls = CreateEmptyControlsArray(ArrayAllocationChunkSize);

    #endregion

    #region ControlBase Overrides

    public override void KeyboardKeyPressed(KeyboardKey key, bool down)
    {
        if (!IsVisible)
            return;
        for (var i = 0; i < _controls.Length; i++)
        {
            var control = _controls[i];
            if (control == Default)
                break;
            if (control.IsVisible && control.IsEnabled)
                control.KeyboardKeyPressed(key, down);
        }
    }

    public override void MouseMoved(int pointerId, int x, int y, int dx, int dy)
    {
        if (!IsVisible)
            return;
        for (var i = 0; i < _controls.Length; i++)
        {
            var control = _controls[i];
            if (control == Default)
                break;
            if (control.IsVisible && control.IsEnabled)
                control.MouseMoved(pointerId, x, y, dx, dy);
        }
    }

    public override void MouseButtonChanged(int pointerId, int x, int y, bool down)
    {
        if (!IsVisible)
            return;
        for (var i = 0; i < _controls.Length; i++)
        {
            var control = _controls[i];
            if (control == Default)
                break;
            if (control.IsVisible && control.IsEnabled)
                control.MouseButtonChanged(pointerId, x, y, down);
        }
    }

    public override void MouseWheelChanged(int pointerId, int x, int y, int delta)
    {
        if (!IsVisible)
            return;
        for (var i = 0; i < _controls.Length; i++)
        {
            var control = _controls[i];
            if (control == Default)
                break;
            if (control.IsVisible && control.IsEnabled)
                control.MouseWheelChanged(pointerId, x, y, delta);
        }
    }

    public override void ControllerButtonChanged(int controllerNo, MachineInput input, bool down)
    {
        if (!IsVisible)
            return;
        for (var i = 0; i < _controls.Length; i++)
        {
            var control = _controls[i];
            if (control == Default)
                break;
            if (control.IsVisible && control.IsEnabled)
                control.ControllerButtonChanged(controllerNo, input, down);
        }
    }

    public override void PaddlePositionChanged(int controllerNo, int paddleNo, int ohms)
    {
        if (!IsVisible)
            return;
        for (var i = 0; i < _controls.Length; i++)
        {
            var control = _controls[i];
            if (control == Default)
                break;
            if (control.IsVisible && control.IsEnabled)
                control.PaddlePositionChanged(controllerNo, paddleNo, ohms);
        }
    }

    public override void DrivingPositionChanged(int controllerNo, MachineInput input)
    {
        if (!IsVisible)
            return;
        for (var i = 0; i < _controls.Length; i++)
        {
            var control = _controls[i];
            if (control == Default)
                break;
            if (control.IsVisible && control.IsEnabled)
                control.DrivingPositionChanged(controllerNo, input);
        }
    }

    public override void LoadResources()
    {
        for (var i = 0; i < _controls.Length; i++)
        {
            var control = _controls[i];
            if (control == Default)
                break;
            control.LoadResources();
        }
    }

    public override void Update(TimerDevice td)
    {
        if (!IsVisible)
            return;
        for (var i = 0; i < _controls.Length; i++)
        {
            var control = _controls[i];
            if (control == Default)
                break;
            if (control.IsVisible && control.IsEnabled)
                control.Update(td);
        }
    }

    public override void Render()
    {
        if (!IsVisible)
            return;
        for (var i = 0; i < _controls.Length; i++)
        {
            var control = _controls[i];
            if (control == Default)
                break;
            if (control.IsVisible && control.IsEnabled)
                control.Render();
        }
    }

    #endregion

    #region Public Members

    public void Add(params ControlBase[] controls)
    {
        foreach (var control in controls)
        {
            Add(control);
        }
    }

    public void Add(ControlBase control)
    {
        if (control == Default)
            return;
        for (var i = 0; i < _controls.Length; i++)
        {
            if (_controls[i] != Default)
                continue;
            _controls[i] = control;
            return;
        }
        var j = IncreaseArraySizeByArrayAllocationChunkSize();
        _controls[j] = control;
    }

    public void Remove(ControlBase control)
    {
        if (control == Default)
            return;
        for (var i = 0; i < _controls.Length; i++)
        {
            if (!control.Equals(_controls[i]))
                continue;
            for (var j = i; j < _controls.Length - 1; j++)
            {
                var nextControl = _controls[j + 1];
                _controls[j] = nextControl;
                if (nextControl == Default)
                    break;
            }
            break;
        }
    }

    #endregion

    #region IDisposable Members

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            for (var i = 0; i < _controls.Length; i++)
            {
                using (_controls[i]) {}
                _controls[i] = Default;
            }
        }
        base.Dispose(disposing);
    }

    #endregion

    #region Helpers

    int IncreaseArraySizeByArrayAllocationChunkSize()
    {
        var nControls = CreateEmptyControlsArray(_controls.Length + ArrayAllocationChunkSize);
        _controls.CopyTo(nControls, 0);
        _controls = nControls;
        return _controls.Length - ArrayAllocationChunkSize;
    }

    static ControlBase[] CreateEmptyControlsArray(int size)
        => Enumerable.Range(0, size).Select(i => Default).ToArray();

    #endregion
}