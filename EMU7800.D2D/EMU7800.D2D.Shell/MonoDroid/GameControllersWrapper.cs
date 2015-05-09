// © Mike Murphy

using System;

namespace EMU7800.D2D.Shell
{
    public sealed class GameControllersWrapper : IDisposable
    {
        #region Fields

        readonly GameControl _gameControl;
        readonly GamePage _gamePage;
        readonly GameProgramSelectionControl _gameProgramSelectionControl;

        #endregion

        public bool LeftJackHasAtariAdaptor { get { return false; } }
        public bool RightJackHasAtariAdaptor { get { return false; } }

        public void Poll()
        {
        }

        public string GetControllerInfo(int controllerNo)
        {
            return null;
        }

        #region IDisposable Members

        public void Dispose()
        {
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
    }
}
