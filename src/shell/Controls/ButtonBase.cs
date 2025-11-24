// © Mike Murphy

using System;

namespace EMU7800.Shell;

public abstract class ButtonBase : ControlBase
{
    static readonly EventArgs DefaultEventArgs = EventArgs.Empty;
    static readonly EventHandler<EventArgs> DefaultEventHandler = (s, o) => {};

    #region Fields

    RectF _boundingRect;

    #endregion

    public event EventHandler<EventArgs> Pressed = DefaultEventHandler;
    public event EventHandler<EventArgs> Released = DefaultEventHandler;
    public event EventHandler<EventArgs> Clicked = DefaultEventHandler;

    public bool IsPressed => IsPressedByPointerId >= 0;

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

    public override void MouseMoved(int pointerId, int x, int y, int dx, int dy)
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
                IsMouseOverPointerId = -1;
            }
        }
    }

    public override void MouseButtonChanged(int pointerId, int x, int y, bool down)
    {
        if (IsMouseOver && IsMouseOverPointerId == pointerId)
        {
            IsMouseOverPointerId = -1;
        }

        if (IsInBounds(x, y, _boundingRect))
        {
            switch (down)
            {
                case true when !IsPressed:
                    IsPressedByPointerId = pointerId;
                    OnPressed();
                    break;
                case false when IsPressed && IsPressedByPointerId == pointerId:
                    IsPressedByPointerId = -1;
                    IsMouseOverPointerId = -1;
                    OnReleased();
                    OnClicked();
                    break;
            }
        }
        else if (IsPressed && IsPressedByPointerId == pointerId)
        {
            IsPressedByPointerId = -1;
            IsMouseOverPointerId = -1;
            OnReleased();
        }
    }

    #endregion

    #region IDisposable Members

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Pressed = DefaultEventHandler;
            Released = DefaultEventHandler;
            Clicked = DefaultEventHandler;
        }
        base.Dispose(disposing);
    }

    #endregion

    protected int IsPressedByPointerId { get; set; } = -1;

    protected virtual RectF ComputeBoundingRectangle()
        => new(Location, Size);

    #region Helpers

    void OnClicked()
        => Clicked(this, DefaultEventArgs);

    void OnPressed()
        => Pressed(this, DefaultEventArgs);

    void OnReleased()
        => Released(this, DefaultEventArgs);

    #endregion
}