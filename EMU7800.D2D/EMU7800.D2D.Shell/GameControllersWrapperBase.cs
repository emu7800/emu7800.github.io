// © Mike Murphy

using System;

#nullable disable

namespace EMU7800.D2D.Shell
{
    public class GameControllersWrapperBase : IDisposable
    {
        public bool LeftJackHasAtariAdaptor { get; protected set; }
        public bool RightJackHasAtariAdaptor { get; protected set; }

        public virtual void Poll() {}

        public virtual string GetControllerInfo(int controllerNo)
            => string.Empty;

        public virtual void Dispose() {}
    }
}
