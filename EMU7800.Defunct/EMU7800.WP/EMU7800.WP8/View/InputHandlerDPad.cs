using EMU7800.Core;
using EMU7800.WP8.Interop;
using System;

namespace EMU7800.WP.View
{
    public class InputHandlerDPad : InputHandler
    {
        #region Fields

        const float JoystickThreshold = 0.4f;

        readonly Direct3DInterop _interop;
        readonly MogaController _mogaController;
        readonly bool _hideFire2;

        #endregion

        #region Constructors

        public InputHandlerDPad(MachineBase machine, Direct3DInterop interop, MogaController mogaController, bool hideFire2) : base(machine)
        {
            if (interop == null)
                throw new ArgumentNullException("interop");
            if (mogaController == null)
                throw new ArgumentNullException("mogaController");

            _interop = interop;
            _mogaController = mogaController;
            _hideFire2 = hideFire2;
        }

        #endregion

        public override void Update()
        {
            base.Update();

            _mogaController.Poll();

            if (_mogaController.IsConnected)
            {
                HandleMogaInput();
            }
            else
            {
                HandleTouchScreenInput();
            }
        }

        #region Helpers

        void HandleMogaInput()
        {
            RaiseMachineInput(MachineInput.Left,  _mogaController.XAxisValue < -JoystickThreshold);
            RaiseMachineInput(MachineInput.Right, _mogaController.XAxisValue >  JoystickThreshold);
            RaiseMachineInput(MachineInput.Up,    _mogaController.YAxisValue >  JoystickThreshold);
            RaiseMachineInput(MachineInput.Down,  _mogaController.YAxisValue < -JoystickThreshold);

            var fire1 = _mogaController.KeyCodeB == Moga.Windows.Phone.ControllerAction.Pressed;
            var fire2 = _mogaController.KeyCodeA == Moga.Windows.Phone.ControllerAction.Pressed;

            if (_hideFire2)
            {
                RaiseMachineInput(MachineInput.Fire, fire1 || fire2);
            }
            else
            {
                RaiseMachineInput(MachineInput.Fire,  fire1);
                RaiseMachineInput(MachineInput.Fire2, fire2);
            }

            if (_mogaController.KeyCodeSelect == Moga.Windows.Phone.ControllerAction.Pressed)
                RaiseMachineInputWithButtonUpCounter(MachineInput.Select);

            if (_mogaController.KeyCodeReset == Moga.Windows.Phone.ControllerAction.Pressed)
                RaiseMachineInputWithButtonUpCounter(MachineInput.Reset);

            RaiseOppositePlayerMachineInput(MachineInput.Left,  _mogaController.ZAxisValue  < -JoystickThreshold);
            RaiseOppositePlayerMachineInput(MachineInput.Right, _mogaController.ZAxisValue  >  JoystickThreshold);
            RaiseOppositePlayerMachineInput(MachineInput.Up,    _mogaController.RZAxisValue >  JoystickThreshold);
            RaiseOppositePlayerMachineInput(MachineInput.Down,  _mogaController.RZAxisValue < -JoystickThreshold);
        }

        void HandleTouchScreenInput()
        {
            RaiseMachineInput(MachineInput.Left,  _interop.IsDPadLeft);
            RaiseMachineInput(MachineInput.Up,    _interop.IsDPadUp);
            RaiseMachineInput(MachineInput.Right, _interop.IsDPadRight);
            RaiseMachineInput(MachineInput.Down,  _interop.IsDPadDown);

            if (_hideFire2)
            {
                RaiseMachineInput(MachineInput.Fire, _interop.IsFire1 || _interop.IsFire2);
            }
            else
            {
                RaiseMachineInput(MachineInput.Fire,  _interop.IsFire1);
                RaiseMachineInput(MachineInput.Fire2, _interop.IsFire2);
            }
        }

        #endregion
    }
}
