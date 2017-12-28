using EMU7800.Core;
using Windows.UI.Core;

namespace EMU7800.WP.View
{
    public class InputHandlerPaddle : InputHandler
    {
        #region Fields

        uint _pressedCounter;
        int _controllerPaddleXPos, _controllerPaddleLastXPos, _controllerPaddleXPosId = -1;

        #endregion

        public override void OnPointerPressed(PointerEventArgs args)
        {
            // top half of the screen is reserved for paddle fire button that can be held down
            var h = ScreenHeight >> 1;
            if (h > 0 && args.CurrentPoint.Position.Y < h)
            {
                RaiseMachineInput(MachineInput.Fire, true);
                return;
            }

            _pressedCounter = 0;
            if (_controllerPaddleXPosId >= 0)
                return;

            _controllerPaddleXPosId = (int)args.CurrentPoint.PointerId;
            _controllerPaddleLastXPos = (int)args.CurrentPoint.Position.X;
        }

        public override void OnPointerMoved(PointerEventArgs args)
        {
            var w = ScreenWidth;
            if (w <= 0)
                return;
            if (_controllerPaddleXPosId != (int)args.CurrentPoint.PointerId)
                return;

            var x = (int)args.CurrentPoint.Position.X;
            var dx = x - _controllerPaddleLastXPos;
            _controllerPaddleLastXPos = x;

            _controllerPaddleXPos += dx;

            if (_controllerPaddleXPos < 0)
                _controllerPaddleXPos = 0;
            else if (_controllerPaddleXPos > w)
                _controllerPaddleXPos = w;

            RaiseMachinePaddleInput(w, _controllerPaddleXPos);
        }

        public override void OnPointerReleased(PointerEventArgs args)
        {
            // top half of the screen is reserved for paddle fire button that can be held down
            var h = ScreenHeight >> 1;
            if (h > 0 && args.CurrentPoint.Position.Y < h)
            {
                RaiseMachineInput(MachineInput.Fire, false);
                return;
            }

            // interpret taps on the bottom half of the screen as fire button press and relesae
            if (_controllerPaddleXPosId == (int)args.CurrentPoint.PointerId)
                _controllerPaddleXPosId = -1;
            if (_pressedCounter > 10)
                return;
            RaiseMachineInputWithButtonUpCounter(MachineInput.Fire);
        }

        public override void Update()
        {
            base.Update();
            _pressedCounter++;
        }

        #region Constructors

        public InputHandlerPaddle(MachineBase machine) : base(machine)
        {
        }

        #endregion
    }
}
