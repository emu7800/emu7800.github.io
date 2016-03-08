// © Mike Murphy

using EMU7800.D2D.Interop;

namespace EMU7800.D2D.Shell
{
    public sealed class ControlCollection : ControlBase
    {
        #region Fields

        const int ArrayAllocationChunkSize = 8;
        ControlBase[] _controls = new ControlBase[ArrayAllocationChunkSize];

        #endregion

        #region ControlBase Overrides

        public override void KeyboardKeyPressed(KeyboardKey key, bool down)
        {
            if (!IsVisible)
                return;
            for (var i = 0; i < _controls.Length; i++)
            {
                var control = _controls[i];
                if (control == null)
                    break;
                if (control.IsVisible && control.IsEnabled)
                    control.KeyboardKeyPressed(key, down);
            }
        }

        public override void MouseMoved(uint pointerId, int x, int y, int dx, int dy)
        {
            if (!IsVisible)
                return;
            for (var i = 0; i < _controls.Length; i++)
            {
                var control = _controls[i];
                if (control == null)
                    break;
                if (control.IsVisible && control.IsEnabled)
                    control.MouseMoved(pointerId, x, y, dx, dy);
            }
        }

        public override void MouseButtonChanged(uint pointerId, int x, int y, bool down)
        {
            if (!IsVisible)
                return;
            for (var i = 0; i < _controls.Length; i++)
            {
                var control = _controls[i];
                if (control == null)
                    break;
                if (control.IsVisible && control.IsEnabled)
                    control.MouseButtonChanged(pointerId, x, y, down);
            }
        }

        public override void MouseWheelChanged(uint pointerId, int x, int y, int delta)
        {
            if (!IsVisible)
                return;
            for (var i = 0; i < _controls.Length; i++)
            {
                var control = _controls[i];
                if (control == null)
                    break;
                if (control.IsVisible && control.IsEnabled)
                    control.MouseWheelChanged(pointerId, x, y, delta);
            }
        }

        public override void LoadResources(GraphicsDevice gd)
        {
            for (var i = 0; i < _controls.Length; i++)
            {
                var control = _controls[i];
                if (control == null)
                    break;
                control.LoadResources(gd);
            }
        }

        public override void Update(TimerDevice td)
        {
            if (!IsVisible)
                return;
            for (var i = 0; i < _controls.Length; i++)
            {
                var control = _controls[i];
                if (control == null)
                    break;
                if (control.IsVisible && control.IsEnabled)
                    control.Update(td);
            }
        }

        public override void Render(GraphicsDevice gd)
        {
            if (!IsVisible)
                return;
            for (var i = 0; i < _controls.Length; i++)
            {
                var control = _controls[i];
                if (control == null)
                    break;
                if (control.IsVisible && control.IsEnabled)
                    control.Render(gd);
            }
        }

        #endregion

        #region Public Members

        public void Add(params ControlBase[] controls)
        {
            if (controls == null)
                return;
            foreach (var control in controls)
                Add(control);
        }

        public void Add(ControlBase control)
        {
            if (control == null)
                return;
            for (var i = 0; i < _controls.Length; i++)
            {
                if (_controls[i] != null)
                    continue;
                _controls[i] = control;
                return;
            }
            var j = IncreaseArraySizeByArrayAllocationChunkSize();
            _controls[j] = control;
        }

        public void Remove(ControlBase control)
        {
            if (control == null)
                return;
            for (var i = 0; i < _controls.Length; i++)
            {
                if (!control.Equals(_controls[i]))
                    continue;
                for (var j = i; j < _controls.Length - 1; j++)
                {
                    var nextControl = _controls[j + 1];
                    _controls[j] = nextControl;
                    if (nextControl == null)
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
                    if (_controls[i] == null)
                        break;
                    _controls[i].Dispose();
                    _controls[i] = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Helpers

        int IncreaseArraySizeByArrayAllocationChunkSize()
        {
            var nControls = new ControlBase[_controls.Length + ArrayAllocationChunkSize];
            _controls.CopyTo(nControls, 0);
            _controls = nControls;
            return _controls.Length - ArrayAllocationChunkSize;
        }

        #endregion
    }
}