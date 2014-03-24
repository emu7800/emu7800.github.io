/*
 * HostBase.cs
 * 
 * Abstraction of an emulated machine host.
 * 
 * Copyright © 2004-2008 Mike Murphy
 * 
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using EMU7800.Core;

namespace EMU7800.Win
{
    public abstract class HostBase
    {
        #region Fields

        protected MachineBase M { get; private set; }

        readonly ILogger _logger;
        readonly GlobalSettings _globalSettings;

        // for emulating driving controllers
        readonly int[] _rotationState = new int[2];
        readonly int[] _rotationDirection = new int[2];

        long _postedMsgExpireFrameCount;
        bool _audioOpened;
        string _postedMsg = string.Empty;

        #endregion

        #region Public Properties

        public string PostedMsg
        {
            get { return (_postedMsgExpireFrameCount > M.FrameNumber) ? _postedMsg : string.Empty; }
            set
            {
                if (value == null) return;
                _postedMsg = value.PadRight(32);
                _postedMsgExpireFrameCount = M.FrameNumber + 3*M.FrameHZ;
            }
        }

        public int EffectiveFPS { get; protected set; }

        public string OutputDirectory
        {
            get { return _globalSettings.OutputDirectory; }
        }

        public int CurrentKeyboardPlayerNo { get; private set; }
        public bool Muted { get; private set; }
        public bool Paused { get; private set; }
        public bool Ended { get; private set; }
        public int ClipStart { get; private set; }
        public int LeftOffset { get; private set; }

        #endregion

        #region Constructors

        protected HostBase(MachineBase m, ILogger logger)
        {
            if (m == null)
                throw new ArgumentNullException("m");
            if (logger == null)
                throw new ArgumentNullException("logger");

            M = m;

            _logger = logger;
            _globalSettings = new GlobalSettings(logger);

            EffectiveFPS = M.FrameHZ + _globalSettings.FrameRateAdjust;
        }

        #endregion

        #region Public Methods

        public void RaiseInput(MachineInput input, bool down)
        {
            RaiseInput(CurrentKeyboardPlayerNo, input, down);
        }

        public void RaiseInput(int playerNo, MachineInput input, bool down)
        {
            var mi = M.InputState;

            if (playerNo < 2)
            {
                var currentControllerJack = (playerNo == 0) ? M.InputState.LeftControllerJack : M.InputState.RightControllerJack;
                if (currentControllerJack == Controller.Driving)
                {
                    switch (input)
                    {
                        case MachineInput.Left:
                            _rotationDirection[playerNo] = down ? -1 : 0;
                            return;
                        case MachineInput.Right:
                            _rotationDirection[playerNo] = down ? 1 : 0;
                            return;
                    }
                }
            }

            if (Paused && down)
            {
                switch (input)
                {
                    case MachineInput.Pause:
                    case MachineInput.PanLeft:
                    case MachineInput.PanRight:
                    case MachineInput.PanDown:
                    case MachineInput.PanUp:
                    case MachineInput.LeftDifficulty:
                    case MachineInput.RightDifficulty:
                    case MachineInput.Color:
                    case MachineInput.SaveMachine:
                    case MachineInput.Mute:
                    case MachineInput.End:
                        break;
                    default:
                        Paused = false;
                        PostedMsg = "Resumed";
                        break;
                }
            }

            mi.RaiseInput(playerNo, input, down);

            switch (input)
            {
                case MachineInput.Color:
                    if (down) PostedMsg = mi.IsGameBWConsoleSwitchSet ? "B/W" : "Color";
                    break;
                case MachineInput.LeftDifficulty:
                    if (down) PostedMsg = "Left Difficulty: " + (mi.IsLeftDifficultyAConsoleSwitchSet ? "A (Pro)" : "B (Novice)");
                    break;
                case MachineInput.RightDifficulty:
                    if (down) PostedMsg = "Right Difficulty: " + (mi.IsRightDifficultyAConsoleSwitchSet ? "A (Pro)" : "B (Novice)");
                    break;
                case MachineInput.SetKeyboardToPlayer1:
                    if (down) SetKeyboardToPlayerNo(0);
                    break;
                case MachineInput.SetKeyboardToPlayer2:
                    if (down) SetKeyboardToPlayerNo(1);
                    break;
                case MachineInput.SetKeyboardToPlayer3:
                    if (down) SetKeyboardToPlayerNo(2);
                    break;
                case MachineInput.SetKeyboardToPlayer4:
                    if (down) SetKeyboardToPlayerNo(3);
                    break;
                case MachineInput.PanLeft:
                    if (down) LeftOffset++;
                    break;
                case MachineInput.PanRight:
                    if (down) LeftOffset--;
                    break;
                case MachineInput.PanDown:
                    if (down) ClipStart--;
                    break;
                case MachineInput.PanUp:
                    if (down) ClipStart++;
                    break;
                case MachineInput.SaveMachine:
                    if (down) SaveMachineState();
                    break;
                case MachineInput.Mute:
                    if (down)
                    {
                        Muted = !Muted;
                        PostedMsg = Muted ? "Mute" : "Mute Off";
                    }
                    break;
                case MachineInput.Pause:
                    if (down && !Paused)
                    {
                        Paused = true;
                        PostedMsg = "Paused";
                    }
                    break;
                case MachineInput.End:
                    if (down) Ended = true;
                    break;
            }
        }

        public void RaiseLightGunInput(int scanline, int hpos)
        {
            M.InputState.RaiseLightgunPos(CurrentKeyboardPlayerNo, scanline, hpos);
        }

        public void RaisePaddleInput(int valMax, int val)
        {
            RaisePaddleInput(CurrentKeyboardPlayerNo, valMax, val);
        }

        public void RaisePaddleInput(int playerNo, int valMax, int val)
        {
            M.InputState.RaisePaddleInput(playerNo, valMax, val);
        }

        public void RaiseEmulatedDrivingInput()
        {
            for (var playerNo = 0; playerNo < 2; playerNo++)
            {
                var dir = _rotationDirection[playerNo];
                if (dir == 0)
                    continue;

                _rotationState[playerNo] += dir;
                switch (_rotationState[playerNo] & 3)
                {
                    case 0:
                        M.InputState.RaiseInput(playerNo, MachineInput.Driving0, true);
                        break;
                    case 1:
                        M.InputState.RaiseInput(playerNo, MachineInput.Driving1, true);
                        break;
                    case 2:
                        M.InputState.RaiseInput(playerNo, MachineInput.Driving2, true);
                        break;
                    case 3:
                        M.InputState.RaiseInput(playerNo, MachineInput.Driving3, true);
                        break;
                }
            }
        }

        /// <exception cref="ApplicationException">Thrown if the audio device cannot be opened.</exception>
        public int EnqueueAudio(FrameBuffer frameBuffer, int soundSampleRate)
        {
            if (!_audioOpened)
            {
                var mmsyserr = WinmmNativeMethods.Open(soundSampleRate, frameBuffer.SoundBufferByteLength, 30);
                if (mmsyserr != 0)
                {
                    var message = string.Format("Unable to open audio device (MMSYSERR={0}); Windows Audio service may be stopped or no audio device available.", mmsyserr);
                    throw new ApplicationException(message);
                }
                _audioOpened = true;
            }
            return WinmmNativeMethods.Enqueue(frameBuffer);
        }

        public void CloseAudio()
        {
            WinmmNativeMethods.Close();
            _audioOpened = false;
        }

        public static Dictionary<DirectX.Key, MachineInput> CreateDefaultKeyBindings()
        {
            return new Dictionary<DirectX.Key, MachineInput>
            {
                { DirectX.Key.Escape,       MachineInput.End },
                { DirectX.Key.Z,            MachineInput.Fire2 },
                { DirectX.Key.X,            MachineInput.Fire },
                { DirectX.Key.Up,           MachineInput.Up },
                { DirectX.Key.Left,         MachineInput.Left },
                { DirectX.Key.Right,        MachineInput.Right },
                { DirectX.Key.Down,         MachineInput.Down },
                { DirectX.Key.NumPad7,      MachineInput.NumPad7 },
                { DirectX.Key.NumPad8,      MachineInput.NumPad8 },
                { DirectX.Key.NumPad9,      MachineInput.NumPad9 },
                { DirectX.Key.NumPad4,      MachineInput.NumPad4 },
                { DirectX.Key.NumPad5,      MachineInput.NumPad5 },
                { DirectX.Key.NumPad6,      MachineInput.NumPad6 },
                { DirectX.Key.NumPad1,      MachineInput.NumPad1 },
                { DirectX.Key.NumPad2,      MachineInput.NumPad2 },
                { DirectX.Key.NumPad3,      MachineInput.NumPad3 },
                { DirectX.Key.Multiply,     MachineInput.NumPadMult },
                { DirectX.Key.NumPad0,      MachineInput.NumPad0 },
                { DirectX.Key.Divide,       MachineInput.NumPadHash },
                { DirectX.Key.D1,           MachineInput.LeftDifficulty },
                { DirectX.Key.D2,           MachineInput.RightDifficulty },
                { DirectX.Key.F1,           MachineInput.SetKeyboardToPlayer1 },
                { DirectX.Key.F2,           MachineInput.SetKeyboardToPlayer2 },
                { DirectX.Key.F3,           MachineInput.SetKeyboardToPlayer3 },
                { DirectX.Key.F4,           MachineInput.SetKeyboardToPlayer4 },
                { DirectX.Key.F5,           MachineInput.PanLeft },
                { DirectX.Key.F6,           MachineInput.PanRight },
                { DirectX.Key.F7,           MachineInput.PanUp },
                { DirectX.Key.F8,           MachineInput.PanDown },
                { DirectX.Key.F11,          MachineInput.SaveMachine },
                { DirectX.Key.F12,          MachineInput.TakeScreenshot },
                { DirectX.Key.C,            MachineInput.Color },
                { DirectX.Key.F,            MachineInput.ShowFrameStats },
                { DirectX.Key.M,            MachineInput.Mute },
                { DirectX.Key.P,            MachineInput.Pause },
                { DirectX.Key.R,            MachineInput.Reset },
                { DirectX.Key.S,            MachineInput.Select },
                { DirectX.Key.Q,            MachineInput.LeftPaddleSwap },
                { DirectX.Key.W,            MachineInput.GameControllerSwap },
                { DirectX.Key.E,            MachineInput.RightPaddleSwap },
            };
        }

        public Dictionary<DirectX.Key, MachineInput> UpdateKeyBindingsFromGlobalSettings(Dictionary<DirectX.Key, MachineInput> keyBindings)
        {
            foreach (var keyHostInput in _globalSettings.KeyBindings.Split(';')
                .Select(binding => binding.Split(','))
                .Where(keyHostInput => (keyHostInput.Length.Equals(2) && keyHostInput[0] != null) && keyHostInput[1] != null))
            {
                DirectX.Key key;
                if (!Enum.TryParse(keyHostInput[0], true, out key))
                    continue;
                MachineInput machineInput;
                if (!Enum.TryParse(keyHostInput[1], true, out machineInput))
                    continue;

                var priorKey = key;
                foreach (var keyVal in keyBindings.Where(keyVal => keyVal.Value.Equals(machineInput)))
                {
                    priorKey = keyVal.Key;
                    break;
                }
                if (priorKey.Equals(key))
                    continue;

                foreach (var kv in keyBindings.Where(x => x.Value == machineInput).ToList())
                    keyBindings.Remove(kv.Key);
                keyBindings.Remove(key);
                keyBindings.Add(key, machineInput);
            }
            return keyBindings;
        }

        #endregion

        #region Virtual Members

        public virtual void Run()
        {
            Muted = false;
            Paused = false;
            Ended = false;
            _audioOpened = false;
            CurrentKeyboardPlayerNo = 0;
            ClipStart = M.FirstScanline;
            LeftOffset = 0;
            _postedMsgExpireFrameCount = 0;
        }

        #endregion

        #region Helpers

        void SetKeyboardToPlayerNo(int playerNo)
        {
            M.InputState.ClearInputByPlayer(playerNo);
            CurrentKeyboardPlayerNo = playerNo;
            PostedMsg = string.Format("Keyboard to Player {0}", CurrentKeyboardPlayerNo + 1);
        }

        void SaveMachineState()
        {
            var fileName = string.Format("emu7800 machinestate {0:yyyy}.{0:MM}-{0:dd}.{0:HH}{0:mm}{0:ss}.emu", DateTime.Now);
            var fullName = Path.Combine(_globalSettings.OutputDirectory, fileName);
            try
            {
                Util.SerializeMachineToFile(M, fullName);
            }
            catch (Exception ex)
            {
                if (Util.IsCriticalException(ex))
                    throw;
                _logger.WriteLine("machine state save error: {0}", ex);
                PostedMsg = "Error saving machine state";
                return;
            }

            _logger.WriteLine("Machine state saved: " + fullName);
            PostedMsg = "Machine state saved";
        }

        #endregion
    }
}
