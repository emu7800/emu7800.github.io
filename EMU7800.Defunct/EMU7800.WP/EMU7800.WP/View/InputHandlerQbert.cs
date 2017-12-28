using EMU7800.Core;

namespace EMU7800.WP.View
{
    public class InputHandlerQbert : InputHandler
    {
        public InputHandlerQbert(MachineBase machine) : base(machine)
        {
            const int screenWidth = 800, screenHeight = 480;
            const int size = 50;
            const int size2 = 90;
            const int x = size + (size2 >> 1);
            const int y = screenHeight - (size + (size2 >> 1));

            var list = new System.Collections.Generic.List<InputBox>
            {
                new InputBox(x - (size2 >> 1) - size, y - (size2 >> 1) - size, size, EmptyCircleTexture)
                {
                    InputRectangleNewWidthAndHeight = size << 1,
                    Pressed = () => RaiseMachineInput(MachineInput.Left, true),
                    Released = () => RaiseMachineInput(MachineInput.Left, false),
                },
                new InputBox(x + (size2 >> 1), y - (size2 >> 1) - size, size, EmptyCircleTexture)
                {
                    InputRectangleNewWidthAndHeight = size << 1,
                    Pressed = () => RaiseMachineInput(MachineInput.Up, true),
                    Released = () => RaiseMachineInput(MachineInput.Up, false),
                },
                new InputBox(x - (size2 >> 1) - size, y + (size2 >> 1), size, EmptyCircleTexture)
                {
                    InputRectangleNewWidthAndHeight = size << 1,
                    Pressed = () => RaiseMachineInput(MachineInput.Down, true),
                    Released = () => RaiseMachineInput(MachineInput.Down, false),
                },
                new InputBox(x + (size2 >> 1), y + (size2 >> 1), size, EmptyCircleTexture)
                {
                    InputRectangleNewWidthAndHeight = size << 1,
                    Pressed = () => RaiseMachineInput(MachineInput.Right, true),
                    Released = () => RaiseMachineInput(MachineInput.Right, false)
                },
                new InputBox(screenWidth - size, screenHeight - 3 * size, size, EmptyCircleTexture)
                {
                    InputRectangleNewWidthAndHeight = size << 1,
                    Pressed = () => RaiseMachineInput(MachineInput.Fire, true),
                    Released = () => RaiseMachineInput(MachineInput.Fire, false)
                },
                new InputBox(x - (size >> 1), 0, size, SelectCircleTexture)
                {
                    InputRectangleNewWidth = 3 * size,
                    Pressed = () => RaiseMachineInput(MachineInput.Select, true),
                    Released = () => RaiseMachineInput(MachineInput.Select, false)
                },
                new InputBox(x - (size >> 1), size << 1, size, ResetCircleTexture)
                {
                    Pressed = () => RaiseMachineInput(MachineInput.Reset, true),
                    Released = () => RaiseMachineInput(MachineInput.Reset, false)
                },
            };

            RegisterInputBoxen(list.ToArray());
        }
    }
}
