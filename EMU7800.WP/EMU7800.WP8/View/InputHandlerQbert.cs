using EMU7800.Core;
using EMU7800.WP8.Interop;
using System;

namespace EMU7800.WP.View
{
    public class InputHandlerQbert : InputHandler
    {
        #region Fields

        const float JoystickThreshold = 0.2f;

        readonly Direct3DInterop _interop;
        readonly MogaController _mogaController;

        #endregion

        #region Constructors

        public InputHandlerQbert(MachineBase machine, Direct3DInterop interop, MogaController mogaController) : base(machine)
        {
            if (interop == null)
                throw new ArgumentNullException("interop");
            if (mogaController == null)
                throw new ArgumentNullException("mogaController");

            _interop = interop;
            _mogaController = mogaController;
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
            var xAxisValue = _mogaController.XAxisValue;
            var yAxisValue = _mogaController.YAxisValue;
            RaiseQbertifiedDirectionalInput(xAxisValue < -JoystickThreshold, yAxisValue > JoystickThreshold, xAxisValue > JoystickThreshold, yAxisValue < -JoystickThreshold);

            var fire1 = _mogaController.KeyCodeB == Moga.Windows.Phone.ControllerAction.Pressed;
            var fire2 = _mogaController.KeyCodeA == Moga.Windows.Phone.ControllerAction.Pressed;

            RaiseMachineInput(MachineInput.Fire,  fire1);
            RaiseMachineInput(MachineInput.Fire2, fire2);

            if (_mogaController.KeyCodeSelect == Moga.Windows.Phone.ControllerAction.Pressed)
                RaiseMachineInputWithButtonUpCounter(MachineInput.Select);

            if (_mogaController.KeyCodeReset  == Moga.Windows.Phone.ControllerAction.Pressed)
                RaiseMachineInputWithButtonUpCounter(MachineInput.Reset);
        }

        void HandleTouchScreenInput()
        {
            RaiseQbertifiedDirectionalInput(_interop.IsDPadLeft, _interop.IsDPadUp, _interop.IsDPadRight, _interop.IsDPadDown);

            RaiseMachineInput(MachineInput.Fire,  _interop.IsFire1);
            RaiseMachineInput(MachineInput.Fire2, _interop.IsFire2);
        }

        void RaiseQbertifiedDirectionalInput(bool origIsLeft, bool origIsUp, bool origIsRight, bool origIsDown)
        {
            RaiseMachineInput(MachineInput.Left,  origIsLeft  && origIsUp);
            RaiseMachineInput(MachineInput.Up,    origIsRight && origIsUp);
            RaiseMachineInput(MachineInput.Right, origIsRight && origIsDown);
            RaiseMachineInput(MachineInput.Down,  origIsLeft  && origIsDown);
        }

        #endregion
    }
}
