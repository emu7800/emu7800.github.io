using EMU7800.Core;
using Microsoft.Xna.Framework.Input;

namespace EMU7800.WP.View
{
    public class InputHandlerDPad : InputHandler
    {
        KeyboardState _lastKeyboardState;

        public InputHandlerDPad(MachineBase machine, bool hideFire2) : base(machine)
        {
            const int screenWidth = 800, screenHeight = 480;
            const int size = 50;
            const int size2 = 90;
            const int x = size + (size2 >> 1);
            const int y = screenHeight - (size + (size2 >> 1));

            var list = new System.Collections.Generic.List<InputBox>
            {
                new InputBox(x - (size >> 1), y - (size2 >> 1) - size, size, EmptyCircleTexture)
                {
                    InputRectangleNewWidth = size2 + (size << 1),
                    Pressed = () => RaiseMachineInput(MachineInput.Up, true),
                    Released = () => RaiseMachineInput(MachineInput.Up, false),
                },
                new InputBox(x + (size2 >> 1), y - (size >> 1), size, EmptyCircleTexture)
                {
                    InputRectangleNewHeight = size2 + (size << 1),
                    Pressed = () => RaiseMachineInput(MachineInput.Right, true),
                    Released = () => RaiseMachineInput(MachineInput.Right, false),
                },
                new InputBox(x - (size >> 1), y + (size2 >> 1), size, EmptyCircleTexture)
                {
                    InputRectangleNewWidth = size2 + (size << 1),
                    Pressed = () => RaiseMachineInput(MachineInput.Down, true),
                    Released = () => RaiseMachineInput(MachineInput.Down, false),
                },
                new InputBox(x - (size2 >> 1) - size, y - (size >> 1), size, EmptyCircleTexture)
                {
                    InputRectangleNewHeight = size2 + (size << 1),
                    Pressed = () => RaiseMachineInput(MachineInput.Left, true),
                    Released = () => RaiseMachineInput(MachineInput.Left, false),
                },
                new InputBox(screenWidth - size, screenHeight - 3 * size, size, EmptyCircleTexture)
                {
                    InputRectangleNewWidthAndHeight = size << 1,
                    Pressed = () => RaiseMachineInput(MachineInput.Fire, true),
                    Released = () => RaiseMachineInput(MachineInput.Fire, false),
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

            if (!hideFire2)
            {
                list.Add(new InputBox(screenWidth - 3 * size, screenHeight - size, size, EmptyCircleTexture)
                {
                    InputRectangleNewWidthAndHeight = size << 1,
                    Pressed = () => RaiseMachineInput(MachineInput.Fire2, true),
                    Released = () => RaiseMachineInput(MachineInput.Fire2, false),
                });
            }

            RegisterInputBoxen(list.ToArray());
        }

        public override void HandleKeyboardInput(KeyboardState ks)
        {
            HandleKey(ks, Keys.Up, MachineInput.Up);
            HandleKey(ks, Keys.Down, MachineInput.Down);
            HandleKey(ks, Keys.Left, MachineInput.Left);
            HandleKey(ks, Keys.Right, MachineInput.Right);
            HandleKey(ks, Keys.X, MachineInput.Fire);
            HandleKey(ks, Keys.Z, MachineInput.Fire2);
            HandleKey(ks, Keys.S, MachineInput.Select);
            HandleKey(ks, Keys.R, MachineInput.Reset);

            _lastKeyboardState = ks;
        }

        void HandleKey(KeyboardState newKeyboardState, Keys key, MachineInput machineInput)
        {
            if (newKeyboardState.IsKeyDown(key) && _lastKeyboardState.IsKeyUp(key))
            {
                RaiseMachineInput(machineInput, true);
            }
            else if (newKeyboardState.IsKeyUp(key) && _lastKeyboardState.IsKeyDown(key))
            {
                RaiseMachineInput(machineInput, false);
            }
        }
    }
}
