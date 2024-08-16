// © Mike Murphy

using EMU7800.Core;
using EMU7800.Win32.Interop;
using System;

namespace EMU7800.D2D.Shell;

public abstract class PageBase : IDisposable
{
    public static readonly PageBase Default = new PageDefault();

    #region Fields

    static int _nextIdToProvision;
    readonly int _id = _nextIdToProvision++;

    #endregion

    protected readonly ControlCollection Controls = new();

    public virtual void OnNavigatingHere()
    {
    }

    public virtual void OnNavigatingAway()
    {
    }

    public virtual void Resized(D2D_SIZE_F size)
    {
    }

    public virtual void KeyboardKeyPressed(KeyboardKey key, bool down)
    {
        Controls.KeyboardKeyPressed(key, down);
    }

    public virtual void MouseMoved(int pointerId, int x, int y, int dx, int dy)
    {
        Controls.MouseMoved(pointerId, x, y, dx, dy);
    }

    public virtual void MouseButtonChanged(int pointerId, int x, int y, bool down)
    {
        Controls.MouseButtonChanged(pointerId, x, y, down);
    }

    public virtual void MouseWheelChanged(int pointerId, int x, int y, int delta)
    {
        Controls.MouseWheelChanged(pointerId, x, y, delta);
    }

    public virtual void ControllerButtonChanged(int controllerNo, MachineInput input, bool down)
    {
        Controls.ControllerButtonChanged(controllerNo, input, down);
    }

    public virtual void PaddlePositionChanged(int controllerNo, int paddleNo, int ohms)
    {
        Controls.PaddlePositionChanged(controllerNo, paddleNo, ohms);
    }

    public virtual void PaddleButtonChanged(int controllerNo, int paddleNo, bool down)
    {
        Controls.PaddleButtonChanged(controllerNo, paddleNo, down);
    }

    public virtual void DrivingPositionChanged(int controllerNo, MachineInput input)
    {
        Controls.DrivingPositionChanged(controllerNo, input);
    }

    public virtual void LoadResources()
    {
        Controls.LoadResources();
    }

    public virtual void Update(TimerDevice td)
    {
        Controls.Update(td);
    }

    public virtual void Render()
    {
        Controls.Render();
    }

    #region PageStateService Accessors

    protected static void PushPage(PageBase pageToPush)
    {
        PageBackStackStateService.Push(pageToPush);
    }

    protected static void ReplacePage(PageBase replacePage)
    {
        PageBackStackStateService.Replace(replacePage);
    }

    protected static bool PopPage()
    {
        return PageBackStackStateService.Pop();
    }

    #endregion

    #region Object Overrides

    public override bool Equals(object? them)
        => them != null && _id == ((PageBase)them)._id;

    public override int GetHashCode()
        => _id;

    public override string ToString()
        => $"EMU7800.D2D.Shell.PageBase: ID={_id}";

    #endregion

    #region Disposable Members

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Controls.Dispose();
        }
    }

    class PageDefault : PageBase;

    #endregion
}