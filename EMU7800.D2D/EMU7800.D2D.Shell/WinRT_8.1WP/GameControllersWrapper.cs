// © Mike Murphy

using System;
using EMU7800.Core;
using EMU7800.D2D.Shell.WinRT;

namespace EMU7800.D2D.Shell
{
    public sealed class GameControllersWrapper : IDisposable
    {
        #region Fields

        const float JoystickThreshold = 0.4f;

        readonly GameControl _gameControl;
        readonly GamePage _gamePage;
        readonly GameProgramSelectionControl _gameProgramSelectionControl;
        readonly MogaController _mogaController;

        bool _lastLeft, _lastRight, _lastUp, _lastDown, _lastFire1, _lastFire2, _lastBack, _lastSelect, _lastReset;
        bool _lastLeft2, _lastRight2, _lastUp2, _lastDown2, _lastFire21, _lastFire22;

        bool _disposed;

        #endregion

        public bool LeftJackHasAtariAdaptor { get { return false; } }
        public bool RightJackHasAtariAdaptor { get { return false; } }

        public void Poll()
        {
            if (_disposed || !_mogaController.IsConnected)
                return;

            HandleMogaInput();
        }

        public string GetControllerInfo(int controllerNo)
        {
            if (controllerNo < 0 || controllerNo > 1 || _disposed || !_mogaController.IsConnected)
                return null;
            return "MOGA";
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
            _mogaController = AppView.MogaController;
        }

        public GameControllersWrapper(GameControl gameControl, GamePage gamePage)
        {
            if (gameControl == null)
                throw new ArgumentNullException("gameControl");
            if (gamePage == null)
                throw new ArgumentNullException("gamePage");

            _gameControl = gameControl;
            _gamePage = gamePage;
            _mogaController = AppView.MogaController;
        }

        #endregion

        #region Helpers

        void HandleMogaInput()
        {
            _mogaController.Poll();

            var left  = _mogaController.XAxisValue < -JoystickThreshold;
            var right = _mogaController.XAxisValue > JoystickThreshold;
            var up    = _mogaController.YAxisValue > JoystickThreshold;
            var down  = _mogaController.YAxisValue < -JoystickThreshold;
            var fire1 = _mogaController.KeyCodeB == Moga.Windows.Phone.ControllerAction.Pressed;
            var fire2 = _mogaController.KeyCodeA == Moga.Windows.Phone.ControllerAction.Pressed;

            var left2  = _mogaController.ZAxisValue  < -JoystickThreshold;
            var right2 = _mogaController.ZAxisValue  > JoystickThreshold;
            var up2    = _mogaController.RZAxisValue > JoystickThreshold;
            var down2  = _mogaController.RZAxisValue < -JoystickThreshold;
            var fire21 = _mogaController.KeyCodeX == Moga.Windows.Phone.ControllerAction.Pressed;
            var fire22 = _mogaController.KeyCodeY == Moga.Windows.Phone.ControllerAction.Pressed;

            var back   = _mogaController.KeyCodeL1     == Moga.Windows.Phone.ControllerAction.Pressed;
            var select = _mogaController.KeyCodeSelect == Moga.Windows.Phone.ControllerAction.Pressed;
            var reset  = _mogaController.KeyCodeReset  == Moga.Windows.Phone.ControllerAction.Pressed
                      || _mogaController.KeyCodeR1     == Moga.Windows.Phone.ControllerAction.Pressed;

            if (_gameControl != null)
            {
                if (left != _lastLeft)
                {
                    _gameControl.JoystickChanged(0, MachineInput.Left, left);
                    _gameControl.ProLineJoystickChanged(0, MachineInput.Left, left);
                }

                if (right != _lastRight)
                {
                    _gameControl.JoystickChanged(0, MachineInput.Right, right);
                    _gameControl.ProLineJoystickChanged(0, MachineInput.Right, right);
                }

                if (up != _lastUp)
                {
                    _gameControl.JoystickChanged(0, MachineInput.Up, up);
                    _gameControl.ProLineJoystickChanged(0, MachineInput.Up, up);
                }

                if (down != _lastDown)
                {
                    _gameControl.JoystickChanged(0, MachineInput.Down, down);
                    _gameControl.ProLineJoystickChanged(0, MachineInput.Down, down);
                }

                if (fire1 != _lastFire1 || fire2 != _lastFire2)
                {
                    _gameControl.JoystickChanged(0, MachineInput.Fire, fire1 || fire2);
                }
                if (fire1 != _lastFire1)
                {
                    _gameControl.ProLineJoystickChanged(0, MachineInput.Fire, fire1);
                }
                if (fire2 != _lastFire2)
                {
                    _gameControl.ProLineJoystickChanged(0, MachineInput.Fire2, fire2);
                }

                if (left2 != _lastLeft2)
                {
                    _gameControl.JoystickChanged(1, MachineInput.Left, left2);
                    _gameControl.ProLineJoystickChanged(1, MachineInput.Left, left2);
                }

                if (right2 != _lastRight2)
                {
                    _gameControl.JoystickChanged(1, MachineInput.Right, right2);
                    _gameControl.ProLineJoystickChanged(1, MachineInput.Right, right2);
                }

                if (up2 != _lastUp2)
                {
                    _gameControl.JoystickChanged(1, MachineInput.Up, up2);
                    _gameControl.ProLineJoystickChanged(1, MachineInput.Up, up2);
                }

                if (down2 != _lastDown2)
                {
                    _gameControl.JoystickChanged(1, MachineInput.Down, down2);
                    _gameControl.ProLineJoystickChanged(1, MachineInput.Down, down2);
                }

                if (fire21 != _lastFire21 || fire22 != _lastFire22)
                {
                    _gameControl.JoystickChanged(1, MachineInput.Fire, fire21 || fire22);
                }
                if (fire21 != _lastFire21)
                {
                    _gameControl.ProLineJoystickChanged(1, MachineInput.Fire, fire21);
                }
                if (fire22 != _lastFire22)
                {
                    _gameControl.ProLineJoystickChanged(1, MachineInput.Fire2, fire22);
                }

                if (back != _lastBack)
                {
                    _gamePage.KeyboardKeyPressed(KeyboardKey.Escape, back);
                }
                if (select != _lastSelect)
                {
                    _gameControl.RaiseMachineInput(MachineInput.Select, select);
                }
                if (reset != _lastReset)
                {
                    _gameControl.RaiseMachineInput(MachineInput.Reset, reset);
                }
            }

            if (_gameProgramSelectionControl != null)
            {
                if (left != _lastLeft)
                {
                    _gameProgramSelectionControl.KeyboardKeyPressed(KeyboardKey.Left, left);
                }
                if (right != _lastRight)
                {
                    _gameProgramSelectionControl.KeyboardKeyPressed(KeyboardKey.Right, right);
                }
                if (up != _lastUp)
                {
                    _gameProgramSelectionControl.KeyboardKeyPressed(KeyboardKey.Up, up);
                }
                if (down != _lastDown)
                {
                    _gameProgramSelectionControl.KeyboardKeyPressed(KeyboardKey.Down, down);
                }
                if (reset != _lastReset)
                {
                    _gameProgramSelectionControl.KeyboardKeyPressed(KeyboardKey.Enter, reset);
                }
            }

            _lastLeft   = left;
            _lastRight  = right;
            _lastUp     = up;
            _lastDown   = down;
            _lastFire1  = fire1;
            _lastFire2  = fire2;

            _lastBack   = back;
            _lastSelect = select;
            _lastReset  = reset;

            _lastLeft2  = left2;
            _lastRight2 = right2;
            _lastUp2    = up2;
            _lastDown2  = down2;
            _lastFire21 = fire21;
            _lastFire22 = fire22;
        }

        #endregion
    }
}
