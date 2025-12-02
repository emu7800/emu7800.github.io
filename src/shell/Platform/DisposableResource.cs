using System;

namespace EMU7800.Shell;

public abstract class DisposableResource : IDisposable
{
    protected bool _resourceDisposed;

    public int HR { get; init; } = 0;

    protected virtual void Dispose(bool disposing) {}

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
