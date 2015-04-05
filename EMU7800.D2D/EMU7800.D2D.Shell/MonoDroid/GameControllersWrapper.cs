// © Mike Murphy

using System;
using EMU7800.Core;

namespace EMU7800.D2D.Shell
{
    public sealed class GameControllersWrapper : IDisposable
    {
        #region Fields

        const float JoystickThreshold = 0.4f;

        readonly GameControl _gameControl;
        readonly GamePage _gamePage;
        readonly GameProgramSelectionControl _gameProgramSelectionControl;

        bool _lastLeft, _lastRight, _lastUp, _lastDown, _lastFire1, _lastFire2, _lastBack, _lastSelect, _lastReset;
        bool _lastLeft2, _lastRight2, _lastUp2, _lastDown2, _lastFire21, _lastFire22;

        bool _disposed;

        #endregion

        public bool LeftJackHasAtariAdaptor { get { return false; } }
        public bool RightJackHasAtariAdaptor { get { return false; } }

        public void Poll()
        {
            if (_disposed)
                return;
        }

        public string GetControllerInfo(int controllerNo)
        {
            if (controllerNo < 0 || controllerNo > 0 || _disposed)
                return null;
            return "FIXME";
        }

        #region IDisposable Members

        public void Dispose()
        {
            _disposed = true;
        }

        #endregion

        #region Constructors

        public GameControllersWrapper(GameProgramSelectionControl gameProgramSelectionControl)
        {
            if (gameProgramSelectionControl == null)
                throw new ArgumentNullException("gameProgramSelectionControl");

            _gameProgramSelectionControl = gameProgramSelectionControl;
        }

        public GameControllersWrapper(GameControl gameControl, GamePage gamePage)
        {
            if (gameControl == null)
                throw new ArgumentNullException("gameControl");
            if (gamePage == null)
                throw new ArgumentNullException("gamePage");

            _gameControl = gameControl;
            _gamePage = gamePage;
        }

        #endregion

        #region Helpers
        #endregion
    }
}
