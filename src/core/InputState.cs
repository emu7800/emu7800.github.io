/*
 * InputState.cs
 *
 * Class containing the input state of the console and its controllers,
 * mapping emulator input devices to external input.
 *
 * Copyright © 2003-2010 Mike Murphy
 *
 */
using System;

namespace EMU7800.Core
{
    public class InputState
    {
        #region Fields

        const int
            LeftControllerJackIndex     = 0,
            RightControllerJackIndex    = 1,
            ConsoleSwitchIndex          = 2,
            ControllerActionStateIndex  = 3,
            OhmsIndex                   = ControllerActionStateIndex + 4,
            LightgunPositionIndex       = ControllerActionStateIndex + 4,
            InputStateSize              = ControllerActionStateIndex + 8 + 1;

        // For driving controllers
        readonly byte[] _rotGrayCodes = { 0x0f, 0x0d, 0x0c, 0x0e };
        readonly int[] _rotState = new int[2];

        readonly int[] _nextInputState = new int[InputStateSize];
        readonly int[] _inputState = new int[InputStateSize];

        #endregion

        #region Public Members

        /// <summary>
        /// Enables the incoming input state buffer to be populated prior to the start of the frame.
        /// Useful for input playback senarios.
        /// </summary>
        public Action<int[]> InputAdvancing { get; set; } = nis => {};

        /// <summary>
        /// Enables access to the input state buffer.
        /// Useful for input recording senarios.
        /// </summary>
        public Action<int[]> InputAdvanced { get; set; } = nis => {};

        public void CaptureInputState()
        {
            InputAdvancing(_nextInputState);
            Buffer.BlockCopy(_nextInputState, 0, _inputState, 0, InputStateSize * sizeof(int));
            InputAdvanced(_inputState);
        }

        public Controller LeftControllerJack
        {
            get => (Controller)_nextInputState[LeftControllerJackIndex];
            set => _nextInputState[LeftControllerJackIndex] = (int)value;
        }

        public Controller RightControllerJack
        {
            get => (Controller)_nextInputState[RightControllerJackIndex];
            set => _nextInputState[RightControllerJackIndex] = (int)value;
        }

        public bool IsGameBWConsoleSwitchSet
            => (_nextInputState[ConsoleSwitchIndex] & (1 << (int)ConsoleSwitch.GameBW)) != 0;

        public bool IsLeftDifficultyAConsoleSwitchSet
            => (_nextInputState[ConsoleSwitchIndex] & (1 << (int)ConsoleSwitch.LeftDifficultyA)) != 0;

        public bool IsRightDifficultyAConsoleSwitchSet
            => (_nextInputState[ConsoleSwitchIndex] & (1 << (int)ConsoleSwitch.RightDifficultyA)) != 0;

        public void RaiseInput(int playerNo, MachineInput input, bool down)
        {
            switch (input)
            {
                case MachineInput.Fire:
                    SetControllerActionState(playerNo, ControllerAction.Trigger, down);
                    break;
                case MachineInput.Fire2:
                    SetControllerActionState(playerNo, ControllerAction.Trigger2, down);
                    break;
                case MachineInput.Left:
                    SetControllerActionState(playerNo, ControllerAction.Left, down);
                    if (down) SetControllerActionState(playerNo, ControllerAction.Right, false);
                    break;
                case MachineInput.Up:
                    SetControllerActionState(playerNo, ControllerAction.Up, down);
                    if (down) SetControllerActionState(playerNo, ControllerAction.Down, false);
                    break;
                case MachineInput.Right:
                    SetControllerActionState(playerNo, ControllerAction.Right, down);
                    if (down) SetControllerActionState(playerNo, ControllerAction.Left, false);
                    break;
                case MachineInput.Down:
                    SetControllerActionState(playerNo, ControllerAction.Down, down);
                    if (down) SetControllerActionState(playerNo, ControllerAction.Up, false);
                    break;
                case MachineInput.NumPad7:
                    SetControllerActionState(playerNo, ControllerAction.Keypad7, down);
                    break;
                case MachineInput.NumPad8:
                    SetControllerActionState(playerNo, ControllerAction.Keypad8, down);
                    break;
                case MachineInput.NumPad9:
                    SetControllerActionState(playerNo, ControllerAction.Keypad9, down);
                    break;
                case MachineInput.NumPad4:
                    SetControllerActionState(playerNo, ControllerAction.Keypad4, down);
                    break;
                case MachineInput.NumPad5:
                    SetControllerActionState(playerNo, ControllerAction.Keypad5, down);
                    break;
                case MachineInput.NumPad6:
                    SetControllerActionState(playerNo, ControllerAction.Keypad6, down);
                    break;
                case MachineInput.NumPad1:
                    SetControllerActionState(playerNo, ControllerAction.Keypad1, down);
                    break;
                case MachineInput.NumPad2:
                    SetControllerActionState(playerNo, ControllerAction.Keypad2, down);
                    break;
                case MachineInput.NumPad3:
                    SetControllerActionState(playerNo, ControllerAction.Keypad3, down);
                    break;
                case MachineInput.NumPadMult:
                    SetControllerActionState(playerNo, ControllerAction.KeypadA, down);
                    break;
                case MachineInput.NumPad0:
                    SetControllerActionState(playerNo, ControllerAction.Keypad0, down);
                    break;
                case MachineInput.NumPadHash:
                    SetControllerActionState(playerNo, ControllerAction.KeypadP, down);
                    break;
                case MachineInput.Driving0:
                    SetControllerActionState(playerNo, ControllerAction.Driving0, true);
                    SetControllerActionState(playerNo, ControllerAction.Driving1, false);
                    SetControllerActionState(playerNo, ControllerAction.Driving2, false);
                    SetControllerActionState(playerNo, ControllerAction.Driving3, false);
                    break;
                case MachineInput.Driving1:
                    SetControllerActionState(playerNo, ControllerAction.Driving0, false);
                    SetControllerActionState(playerNo, ControllerAction.Driving1, true);
                    SetControllerActionState(playerNo, ControllerAction.Driving2, false);
                    SetControllerActionState(playerNo, ControllerAction.Driving3, false);
                    break;
                case MachineInput.Driving2:
                    SetControllerActionState(playerNo, ControllerAction.Driving0, false);
                    SetControllerActionState(playerNo, ControllerAction.Driving1, false);
                    SetControllerActionState(playerNo, ControllerAction.Driving2, true);
                    SetControllerActionState(playerNo, ControllerAction.Driving3, false);
                    break;
                case MachineInput.Driving3:
                    SetControllerActionState(playerNo, ControllerAction.Driving0, false);
                    SetControllerActionState(playerNo, ControllerAction.Driving1, false);
                    SetControllerActionState(playerNo, ControllerAction.Driving2, false);
                    SetControllerActionState(playerNo, ControllerAction.Driving3, true);
                    break;
                case MachineInput.Reset:
                    SetConsoleSwitchState(ConsoleSwitch.GameReset, down);
                    break;
                case MachineInput.Select:
                    SetConsoleSwitchState(ConsoleSwitch.GameSelect, down);
                    break;
                case MachineInput.Color:
                    if (down) ToggleConsoleSwitchState(ConsoleSwitch.GameBW);
                    break;
                case MachineInput.LeftDifficulty:
                    if (down) ToggleConsoleSwitchState(ConsoleSwitch.LeftDifficultyA);
                    break;
                case MachineInput.RightDifficulty:
                    if (down) ToggleConsoleSwitchState(ConsoleSwitch.RightDifficultyA);
                    break;
                case MachineInput.Pause:
                    SetConsoleSwitchState(ConsoleSwitch.Pause, down);
                    break;
            }
        }

        public void RaisePaddleInput(int playerNo, int ohms)
        {
            if (ohms >= 0 && ohms < 1000000)
            {
                _nextInputState[OhmsIndex + (playerNo & 3)] = ohms;
            }
        }

        public void RaiseLightgunPos(int playerNo, int scanline, int hpos)
        {
            var i = LightgunPositionIndex + ((playerNo & 1) << 1);
            _nextInputState[i++] = scanline;
            _nextInputState[i] = hpos;
        }

        public void ClearAllInput()
        {
            _nextInputState[ConsoleSwitchIndex] = 0;
            ClearLeftJackInput();
            ClearRightJackInput();
        }

        public void ClearInputByPlayer(int playerNo)
        {
            _nextInputState[OhmsIndex + (playerNo & 3)] = 0;
            _nextInputState[ControllerActionStateIndex + (playerNo & 3)] = 0;
            _nextInputState[LightgunPositionIndex + ((playerNo & 1) << 1)] = _nextInputState[LightgunPositionIndex + ((playerNo & 1) << 1) + 1] = 0;
        }

        public void ClearLeftJackInput()
        {
            _nextInputState[OhmsIndex] = _nextInputState[OhmsIndex + 1] = 0;
            _nextInputState[ControllerActionStateIndex] = 0;
            _nextInputState[ControllerActionStateIndex] = LeftControllerJack switch
            {
                Controller.Paddles => _nextInputState[ControllerActionStateIndex + 1] = 0,
                _                  => 0,
            };
            _nextInputState[LightgunPositionIndex] = _nextInputState[LightgunPositionIndex + 1] = 0;
        }

        public void ClearRightJackInput()
        {
            _nextInputState[OhmsIndex + 2] = _nextInputState[OhmsIndex + 3] = 0;
            switch (RightControllerJack)
            {
                case Controller.Paddles:
                    _nextInputState[ControllerActionStateIndex + 2] = _nextInputState[ControllerActionStateIndex + 3] = 0;
                    break;
                default:
                    _nextInputState[ControllerActionStateIndex + 1] = 0;
                    break;
            }
            _nextInputState[LightgunPositionIndex + 2] = _nextInputState[LightgunPositionIndex + 3] = 0;
        }

        #endregion

        #region Serialization Members

        public InputState()
        {
        }

        public InputState(DeserializationContext input)
        {
            input.CheckVersion(1);
            _rotState = input.ReadIntegers(2);
            _nextInputState = input.ReadIntegers(InputStateSize);
            _inputState = input.ReadIntegers(InputStateSize);
        }

        public void GetObjectData(SerializationContext output)
        {
            output.WriteVersion(1);
            output.Write(_rotState);
            output.Write(_nextInputState);
            output.Write(_inputState);
        }

        #endregion

        #region Internal Members

        internal bool SampleCapturedConsoleSwitchState(ConsoleSwitch consoleSwitch)
            => (_inputState[ConsoleSwitchIndex] & (1 << (int)consoleSwitch)) != 0;

        internal bool SampleCapturedControllerActionState(int playerno, ControllerAction action)
            => (_inputState[ControllerActionStateIndex + (playerno & 3)] & (1 << (int)action)) != 0;

        internal int SampleCapturedOhmState(int playerNo)
            => _inputState[OhmsIndex + (playerNo & 3)];

        internal void SampleCapturedLightGunPosition(int playerNo, out int scanline, out int hpos)
        {
            var i = LightgunPositionIndex + ((playerNo & 1) << 1);
            scanline = _inputState[i++];
            hpos = _inputState[i];
        }

        internal byte SampleCapturedDrivingState(int playerNo)
        {
            if      (SampleCapturedControllerActionState(playerNo, ControllerAction.Driving0))
                _rotState[playerNo] = 0;
            else if (SampleCapturedControllerActionState(playerNo, ControllerAction.Driving1))
                _rotState[playerNo] = 1;
            else if (SampleCapturedControllerActionState(playerNo, ControllerAction.Driving2))
                _rotState[playerNo] = 2;
            else if (SampleCapturedControllerActionState(playerNo, ControllerAction.Driving3))
                _rotState[playerNo] = 3;
            return _rotGrayCodes[_rotState[playerNo]];
        }

        #endregion

        #region Object Overrides

        public override string ToString()
            => "EMU7800.Core.InputState";

        #endregion

        #region Helpers

        void SetControllerActionState(int playerNo, ControllerAction action, bool value)
        {
            if (value)
            {
                _nextInputState[ControllerActionStateIndex + (playerNo & 3)] |= (1 << (int)action);
            }
            else
            {
                _nextInputState[ControllerActionStateIndex + (playerNo & 3)] &= ~(1 << (int)action);
            }
        }

        void SetConsoleSwitchState(ConsoleSwitch consoleSwitch, bool value)
        {
            if (value)
            {
                _nextInputState[ConsoleSwitchIndex] |= (byte)(1 << (byte)consoleSwitch);
            }
            else
            {
                _nextInputState[ConsoleSwitchIndex] &= (byte)~(1 << (byte)consoleSwitch);
            }
        }

        void ToggleConsoleSwitchState(ConsoleSwitch consoleSwitch)
        {
            var consoleSwitchState = (_nextInputState[ConsoleSwitchIndex] & (1 << (int) consoleSwitch)) != 0;
            SetConsoleSwitchState(consoleSwitch, !consoleSwitchState);
        }

        #endregion
    }
}