using Microsoft.Xna.Framework.Input.Touch;
using EMU7800.Core;

namespace EMU7800.WP.View
{
    public class InputHandlerPaddle : InputHandler
    {
        #region Fields

        const int ScreenWidth = 800, ScreenHeight = 480;

        int _controllerPaddleXPos, _controllerPaddleXPosId;

        #endregion

        public override void HandleTouchLocationInput(TouchLocation tl)
        {
            switch (tl.State)
            {
                case TouchLocationState.Invalid:
                    break;
                case TouchLocationState.Released:
                    if (_controllerPaddleXPosId == tl.Id)
                        _controllerPaddleXPosId = -1;
                    break;
                case TouchLocationState.Pressed:
                    if (_controllerPaddleXPosId >= 0)
                        break;
                    _controllerPaddleXPosId = tl.Id;
                    RaiseMachinePaddleInput(ScreenWidth, _controllerPaddleXPos);
                    break;
                case TouchLocationState.Moved:
                    TouchLocation ptl;
                    if (_controllerPaddleXPosId == tl.Id && tl.TryGetPreviousLocation(out ptl))
                    {
                        var dx = (int)(tl.Position.X - ptl.Position.X);
                        _controllerPaddleXPos += dx;
                        if (_controllerPaddleXPos < 0)
                            _controllerPaddleXPos = 0;
                        else if (_controllerPaddleXPos > ScreenWidth)
                            _controllerPaddleXPos = ScreenWidth;
                        RaiseMachinePaddleInput(ScreenWidth, _controllerPaddleXPos);
                    }
                    break;
            }
        }

        #region Constructors

        public InputHandlerPaddle(MachineBase machine) : base(machine)
        {
            const int size = 50;
            const int size2 = 90;
            const int x = size + (size2 >> 1);

            var list = new System.Collections.Generic.List<InputBox>
            {
                new InputBox(ScreenWidth - size, ScreenHeight - 3 * size, size, EmptyCircleTexture)
                {
                    InputRectangleNewWidthAndHeight = size << 1,
                    Pressed = () => RaiseMachineInput(MachineInput.Fire, true),
                    Released = () => RaiseMachineInput(MachineInput.Fire, false),
                },
                new InputBox(x - (size >> 1), 0, size, SelectCircleTexture)
                {
                    InputRectangleNewWidth = 3 * size,
                    Pressed = () => RaiseMachineInput(MachineInput.Select, true),
                    Released = () => RaiseMachineInput(MachineInput.Select, false),
                },
                new InputBox(x - (size >> 1), size << 1, size, ResetCircleTexture)
                {
                    Pressed = () => RaiseMachineInput(MachineInput.Reset, true),
                    Released = () => RaiseMachineInput(MachineInput.Reset, false),
                },
            };

            RegisterInputBoxen(list.ToArray());

            _controllerPaddleXPosId = -1;
        }

        #endregion
    }
}
