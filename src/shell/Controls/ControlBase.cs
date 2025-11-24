// © Mike Murphy

using EMU7800.Core;
using System;

namespace EMU7800.Shell;

public abstract class ControlBase : IDisposable
{
    public static readonly ControlBase Default = new ControlDefault();

    public PointF Location
    {
        get;
        set
        {
            field = value;
            LocationChanged();
        }
    }

    public SizeF Size
    {
        get;
        set
        {
            field = value;
            SizeChanged();
        }
    }

    public bool IsMouseOver => IsMouseOverPointerId >= 0;

    public bool IsVisible { get; set; }
    public bool IsEnabled { get; set; }

    public PointF ToRightOf(int dx, int dy)
        => new(Location.X + Size.Width + dx, Location.Y + dy);

    public PointF ToBottomOf(int dx, int dy)
        => new(Location.X + dx, Location.Y + Size.Height + dy);

    protected ControlBase()
    {
        IsVisible = true;
        IsEnabled = true;
    }

    #region ControlBase Virtuals

    public virtual void KeyboardKeyPressed(KeyboardKey key, bool down)
    {
    }

    public virtual void MouseMoved(int pointerId, int x, int y, int dx, int dy)
    {
    }

    public virtual void MouseButtonChanged(int pointerId, int x, int y, bool down)
    {
    }

    public virtual void MouseWheelChanged(int pointerId, int x, int y, int delta)
    {
    }

    public virtual void ControllerButtonChanged(int controllerNo, MachineInput input, bool down)
    {
    }

    public virtual void PaddlePositionChanged(int controllerNo, int paddleNo, int ohms)
    {
    }

    public virtual void PaddleButtonChanged(int controllerNo, int paddleNo, bool down)
    {
    }

    public virtual void DrivingPositionChanged(int controllerNo, MachineInput input)
    {
    }

    public virtual void LocationChanged()
    {
    }

    public virtual void SizeChanged()
    {
    }

    public virtual void LoadResources()
    {
        DisposeResources();
        CreateResources();
    }

    public virtual void Update(TimerDevice td)
    {
    }

    public virtual void Render()
    {
    }

    protected virtual void CreateResources()
    {
    }

    protected virtual void DisposeResources()
    {
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisposeResources();
        }
    }

    #endregion

    protected int IsMouseOverPointerId { get; set; } = -1;

    protected static void SafeDispose(ref StaticBitmap bitmap)
    {
        if (bitmap == StaticBitmap.Default)
            return;
        bitmap.Dispose();
        bitmap = StaticBitmap.Default;
    }

    protected static void SafeDispose(ref DynamicBitmap bitmap)
    {
        if (bitmap == DynamicBitmap.Default)
            return;
        bitmap.Dispose();
        bitmap = DynamicBitmap.Default;
    }

    protected static void SafeDispose(ref TextLayout textLayout)
    {
        if (textLayout == TextLayout.Default)
            return;
        textLayout.Dispose();
        textLayout = TextLayout.Default;
    }

    protected static bool IsInBounds(int x, int y, RectF bounds)
    {
        var outOfBounds = x < bounds.Left
            || x > bounds.Right
                || y < bounds.Top
                    || y > bounds.Bottom;
        return !outOfBounds;
    }

    class ControlDefault : ControlBase;
}