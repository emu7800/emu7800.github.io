// © Mike Murphy

using EMU7800.Core;
using System;

namespace EMU7800.Shell;

public abstract class PageBase : IDisposable
{
    public static readonly PageBase Default = new PageDefault();

    PageBackStackStateService? _stateService;

    protected readonly ControlCollection Controls = new();

    public virtual void OnNavigatingHere()
    {
    }

    public virtual void OnNavigatingAway()
    {
    }

    public virtual void Resized(SizeF size)
    {
    }

    public virtual void InjectDependency(object dependency)
    {
        Controls.InjectDependency(dependency);
        if (dependency is  PageBackStackStateService stateService)
            _stateService = stateService;
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

    public virtual void LoadResources(IGraphicsDeviceDriver graphicsDevice)
    {
        Controls.LoadResources(graphicsDevice);
    }

    public virtual void Update(TimerDevice td)
    {
        Controls.Update(td);
    }

    public virtual void Render(IGraphicsDeviceDriver graphicsDevice)
    {
        Controls.Render(graphicsDevice);
    }

    #region PageStateService Accessors

    protected void PushPage(PageBase pageToPush)
    {
        _stateService?.Push(pageToPush);
    }

    protected void ReplacePage(PageBase replacePage)
    {
        _stateService?.Replace(replacePage);
    }

    protected bool PopPage()
    {
        return _stateService?.Pop() ?? false;
    }

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