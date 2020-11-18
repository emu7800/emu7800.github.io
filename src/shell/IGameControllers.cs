// © Mike Murphy

using System;

namespace EMU7800.D2D.Shell
{
    public interface IGameControllers : IDisposable
    {
        bool LeftJackHasAtariAdaptor { get; }
        bool RightJackHasAtariAdaptor { get; }
        void Poll() {}
        string GetControllerInfo(int controllerNo);
    }

    public sealed class EmptyGameControllers : IGameControllers
    {
        public static readonly IGameControllers Default = new EmptyGameControllers();

        public bool LeftJackHasAtariAdaptor { get => false; }
        public bool RightJackHasAtariAdaptor { get => false; }

        public void Dispose()
        {
        }

        public string GetControllerInfo(int controllerNo)
            => string.Empty;

        private EmptyGameControllers() {}
    }
}
