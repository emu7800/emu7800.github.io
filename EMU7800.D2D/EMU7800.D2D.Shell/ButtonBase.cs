// © Mike Murphy

using System;
using EMU7800.D2D.Interop;

namespace EMU7800.D2D.Shell
{
    public abstract class ButtonBase : ControlBase
    {
        #region Fields

        RectF _boundingRect;

        #endregion

        public event EventHandler<EventArgs> Pressed;
        public event EventHandler<EventArgs> Released;
        public event EventHandler<EventArgs> Clicked;

        public bool IsPressed
        {
            get { return IsPressedByPointerId.HasValue; }
        }

        #region ControlBase Overrides

        public override void LocationChanged()
        {
            base.LocationChanged();
            _boundingRect = ComputeBoundingRectangle();
        }

        public override void SizeChanged()
        {
            base.SizeChanged();
            _boundingRect = ComputeBoundingRectangle();
        }

        public override void MouseMoved(uint pointerId, int x, int y, int dx, int dy)
        {
            if (IsInBounds(x, y, _boundingRect))
            {
                if (!IsMouseOver)
                {
                    IsMouseOverPointerId = pointerId;
                }
            }
            else
            {
                if (IsMouseOver && IsMouseOverPointerId == pointerId)
                {
                    IsMouseOverPointerId = null;
                }
            }
        }

        public override void MouseButtonChanged(uint pointerId, int x, int y, bool down)
        {
            if (IsMouseOver && IsMouseOverPointerId == pointerId)
            {
                IsMouseOverPointerId = null;
            }

            if (IsInBounds(x, y, _boundingRect))
            {
                if (down && !IsPressed)
                {
                    IsPressedByPointerId = pointerId;
                    OnPressed();
                }
                else if (!down && IsPressed && IsPressedByPointerId == pointerId)
                {
                    IsPressedByPointerId = null;
                    IsMouseOverPointerId = null;
                    OnReleased();
                    OnClicked();
                }
            }
            else if (IsPressed && IsPressedByPointerId == pointerId)
            {
                IsPressedByPointerId = null;
                IsMouseOverPointerId = null;
                OnReleased();
            }
        }

        #endregion

        #region IDisposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Pressed = null;
                Released = null;
                Clicked = null;
            }
            base.Dispose(disposing);
        }

        #endregion

        protected uint? IsPressedByPointerId { get; set; }

        protected virtual RectF ComputeBoundingRectangle()
        {
            return Struct.ToRectF(Location, Size);
        }

        #region Helpers

        void OnClicked()
        {
            if (Clicked != null)
                Clicked(this, null);
        }

        void OnPressed()
        {
            if (Pressed != null)
                Pressed(this, null);
        }

        void OnReleased()
        {
            if (Released != null)
                Released(this, null);
        }

        #endregion
    }
}