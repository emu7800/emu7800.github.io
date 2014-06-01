// © Mike Murphy

using System;
using System.Linq;
using EMU7800.Core;
using EMU7800.D2D.Interop;

namespace EMU7800.D2D.Shell
{
    public sealed class GameControllersWrapper : IDisposable
    {
        #region Fields

        readonly GameControl _gameControl;
        readonly GamePage _gamePage;
        readonly GameProgramSelectionControl _gameProgramSelectionControl;
        readonly XInputDevice[] _xinputDevices;

        struct ThumbInput
        {
            public bool Left, Right, Up, Down;
        }
        readonly ThumbInput[] _lastThumbInput = new ThumbInput[4];

        #endregion

        public bool LeftJackHasAtariAdaptor { get; private set; }
        public bool RightJackHasAtariAdaptor { get; private set; }

        public void Poll()
        {
            for (var i = 0; i < _xinputDevices.Length; i++)
            {
                var xd = _xinputDevices[i];
                if (xd != null)
                    xd.Poll();
            }
        }

        public string GetControllerInfo(int controllerNo)
        {
            if (controllerNo < 0 || controllerNo >= _xinputDevices.Length || !_xinputDevices[controllerNo].IsControllerConnected)
                return null;
            return "XInput";
        }

        #region IDisposable Members

        public void Dispose()
        {
            for (var i = 0; i < _xinputDevices.Length; i++)
            {
                if (_xinputDevices[i] != null)
                {
                    _xinputDevices[i].Dispose();
                    _xinputDevices[i] = null;
                }
            }
        }

        #endregion

        #region Constructors

        public GameControllersWrapper(GameProgramSelectionControl gameProgramSelectionControl)
        {
            if (gameProgramSelectionControl == null)
                throw new ArgumentNullException("gameProgramSelectionControl");

            _gameProgramSelectionControl = gameProgramSelectionControl;

            LeftJackHasAtariAdaptor = false;
            RightJackHasAtariAdaptor = false;

            _xinputDevices = Enumerable.Range(0, 1).Select(i => new XInputDevice(i)).ToArray();
            _xinputDevices[0].ButtonChanged += HandleButtonChangedForSelectionPage;
            _xinputDevices[0].ThumbChanged += HandleThumbChangedForSelectionPage;
        }

        public GameControllersWrapper(GameControl gameControl, GamePage gamePage)
        {
            if (gameControl == null)
                throw new ArgumentNullException("gameControl");
            if (gamePage == null)
                throw new ArgumentNullException("gamePage");

            LeftJackHasAtariAdaptor = false;
            RightJackHasAtariAdaptor = false;

            _gameControl = gameControl;
            _gamePage = gamePage;

            _xinputDevices = Enumerable.Range(0, 4).Select(i => new XInputDevice(i)).ToArray();

            _xinputDevices[0].ButtonChanged += (xinput, down) => HandleButtonChanged(0, xinput, down);
            _xinputDevices[1].ButtonChanged += (xinput, down) => HandleButtonChanged(1, xinput, down);
            _xinputDevices[2].ButtonChanged += (xinput, down) => HandleButtonChanged(2, xinput, down);
            _xinputDevices[3].ButtonChanged += (xinput, down) => HandleButtonChanged(3, xinput, down);

            _xinputDevices[0].TriggerChanged += HandleTriggerChanged;
            _xinputDevices[1].TriggerChanged += HandleTriggerChanged;
            _xinputDevices[2].TriggerChanged += HandleTriggerChanged;
            _xinputDevices[3].TriggerChanged += HandleTriggerChanged;

            _xinputDevices[0].ThumbChanged += HandleThumbChangedForP0;
            _xinputDevices[1].ThumbChanged += (lx, ly, rx, ry) => HandleThumbChanged(1, lx, ly);
            _xinputDevices[2].ThumbChanged += (lx, ly, rx, ry) => HandleThumbChanged(2, lx, ly);
            _xinputDevices[3].ThumbChanged += (lx, ly, rx, ry) => HandleThumbChanged(3, lx, ly);
        }

        #endregion

        #region Helpers

        void HandleTriggerChanged(byte lt, byte rt)
        {
            switch (lt)
            {
                case 255:
                    _gameControl.RaiseMachineInput(MachineInput.Select, true);
                    break;
                case 0:
                    _gameControl.RaiseMachineInput(MachineInput.Select, false);
                    break;
            }
            switch (rt)
            {
                case 255:
                    _gameControl.RaiseMachineInput(MachineInput.Reset, true);
                    break;
                case 0:
                    _gameControl.RaiseMachineInput(MachineInput.Reset, false);
                    break;
            }
        }

        void HandleButtonChanged(int playerNo, XInputButton xinput, bool down)
        {
            switch (xinput)
            {
                case XInputButton.Back:
                    _gamePage.KeyboardKeyPressed(KeyboardKey.Escape, down);
                    return;
                case XInputButton.LThumb:
                    _gameControl.JoystickChanged(playerNo, MachineInput.Fire, down);
                    return;
                case XInputButton.RThumb:
                    var remappedPlayerNo = (playerNo == 0) ? 1 : playerNo;
                    _gameControl.JoystickChanged(remappedPlayerNo, MachineInput.Fire, down);
                    return;
            }
            JoystickChanged(playerNo, xinput, down);
        }

        void HandleButtonChangedForSelectionPage(XInputButton xinput, bool down)
        {
            switch (xinput)
            {
                case XInputButton.DUp:
                    _gameProgramSelectionControl.KeyboardKeyPressed(KeyboardKey.Up, down);
                    return;
                case XInputButton.DDown:
                    _gameProgramSelectionControl.KeyboardKeyPressed(KeyboardKey.Down, down);
                    return;
                case XInputButton.DLeft:
                    _gameProgramSelectionControl.KeyboardKeyPressed(KeyboardKey.Left, down);
                    return;
                case XInputButton.DRight:
                    _gameProgramSelectionControl.KeyboardKeyPressed(KeyboardKey.Right, down);
                    return;
                case XInputButton.A:
                case XInputButton.B:
                case XInputButton.X:
                case XInputButton.Y:
                case XInputButton.Start:
                    _gameProgramSelectionControl.KeyboardKeyPressed(KeyboardKey.Enter, down);
                    return;
            }
        }

        void HandleThumbChangedForP0(short lx, short ly, short rx, short ry)
        {
            HandleThumbChanged(0, lx, ly);
            HandleThumbChanged(1, rx, ry);
        }

        void HandleThumbChangedForSelectionPage(short x, short y, short rx, short ry)
        {
            const int deadzone = 0x4000;

            bool left = false, right = false;
            if (x < -deadzone)
                left = true;
            else if (x > deadzone)
                right = true;

            bool up = false, down = false;
            if (y < -deadzone)
                down = true;
            else if (y > deadzone)
                up = true;

            if (left != _lastThumbInput[0].Left)
            {
                _lastThumbInput[0].Left = left;
                _gameProgramSelectionControl.KeyboardKeyPressed(KeyboardKey.Left, left);
            }
            if (right != _lastThumbInput[0].Right)
            {
                _lastThumbInput[0].Right = right;
                _gameProgramSelectionControl.KeyboardKeyPressed(KeyboardKey.Right, right);
            }
            if (up != _lastThumbInput[0].Up)
            {
                _lastThumbInput[0].Up = up;
                _gameProgramSelectionControl.KeyboardKeyPressed(KeyboardKey.Up, up);
            }
            if (down != _lastThumbInput[0].Down)
            {
                _lastThumbInput[0].Down = down;
                _gameProgramSelectionControl.KeyboardKeyPressed(KeyboardKey.Down, down);
            }
        }

        void HandleThumbChanged(int playerNo, short x, short y)
        {
            const int deadzone = 0x4000;

            bool left = false, right = false;
            if (x < -deadzone)
                left = true;
            else if (x > deadzone)
                right = true;

            bool up = false, down = false;
            if (y < -deadzone)
                down = true;
            else if (y > deadzone)
                up = true;

            if (left != _lastThumbInput[playerNo].Left)
            {
                _lastThumbInput[playerNo].Left = left;
                JoystickChanged(playerNo, XInputButton.DLeft, left);
            }
            if (right != _lastThumbInput[playerNo].Right)
            {
                _lastThumbInput[playerNo].Right = right;
                JoystickChanged(playerNo, XInputButton.DRight, right);
            }
            if (up != _lastThumbInput[playerNo].Up)
            {
                _lastThumbInput[playerNo].Up = up;
                JoystickChanged(playerNo, XInputButton.DUp, up);
            }
            if (down != _lastThumbInput[playerNo].Down)
            {
                _lastThumbInput[playerNo].Down = down;
                JoystickChanged(playerNo, XInputButton.DDown, down);
            }
        }

        void JoystickChanged(int playerNo, XInputButton xinput, bool down)
        {
            switch (xinput)
            {
                case XInputButton.A:
                    _gameControl.JoystickChanged(playerNo, MachineInput.Fire2, down);
                    break;
                case XInputButton.B:
                    _gameControl.JoystickChanged(playerNo, MachineInput.Fire, down);
                    break;
                case XInputButton.X:
                case XInputButton.Y:
                    _gamePage.KeyboardKeyPressed(KeyboardKey.W, down);
                    break;
                case XInputButton.DLeft:
                    _gameControl.JoystickChanged(playerNo, MachineInput.Left, down);
                    break;
                case XInputButton.DUp:
                    _gameControl.JoystickChanged(playerNo, MachineInput.Up, down);
                    break;
                case XInputButton.DRight:
                    _gameControl.JoystickChanged(playerNo, MachineInput.Right, down);
                    break;
                case XInputButton.DDown:
                    _gameControl.JoystickChanged(playerNo, MachineInput.Down, down);
                    break;
            }
        }

        #endregion
    }
}
