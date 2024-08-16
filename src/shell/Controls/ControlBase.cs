// © Mike Murphy

using EMU7800.Core;
using EMU7800.Win32.Interop;
using System;

namespace EMU7800.D2D.Shell;

public abstract class ControlBase : IDisposable
{
    public static readonly ControlBase Default = new ControlDefault();

    #region Fields

    static int _nextIdToProvision;
    readonly int _id = _nextIdToProvision++;

    D2D_POINT_2F _location;
    D2D_SIZE_F _size;

    #endregion

    public D2D_POINT_2F Location
    {
        get => _location;
        set
        {
            _location = value;
            LocationChanged();
        }
    }

    public D2D_SIZE_F Size
    {
        get => _size;
        set
        {
            _size = value;
            SizeChanged();
        }
    }

    public bool IsMouseOver => IsMouseOverPointerId >= 0;

    public bool IsVisible { get; set; }
    public bool IsEnabled { get; set; }

    public D2D_POINT_2F ToRightOf(int dx, int dy)
        => new(Location.X + Size.Width + dx, Location.Y + dy);

    public D2D_POINT_2F ToBottomOf(int dx, int dy)
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

    #region Object Overrides

    public override bool Equals(object? them)
        => them != null && _id == ((ControlBase)them)._id;

    public override int GetHashCode()
        => _id;

    public override string ToString()
        => $"EMU7800.D2D.Shell.ControlBase: ID={_id}";

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

    protected static bool IsInBounds(int x, int y, D2D_RECT_F bounds)
    {
        var outOfBounds = x < bounds.Left
            || x > bounds.Right
                || y < bounds.Top
                    || y > bounds.Bottom;
        return !outOfBounds;
    }

    class ControlDefault : ControlBase;
}