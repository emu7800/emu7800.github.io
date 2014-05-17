// © Mike Murphy

using System;
using System.Collections.Generic;
using EMU7800.Core;

namespace EMU7800.D2D.Shell
{
    public sealed class GameControllersWrapper : IDisposable
    {
        #region Fields

        readonly GameControl _gameControl;
        readonly GamePage _gamePage;
        readonly GameProgramSelectionControl _gameProgramSelectionControl;
        readonly HidControllerData[] _lastHidControllerData = new HidControllerData[StelladaptorHost.MAX_CONTROLLER_COUNT];
        readonly int[] _lastSeenDaptor2Mode = new int[StelladaptorHost.MAX_CONTROLLER_COUNT];

        bool _disposed;

        static readonly MachineInput[] _stelladaptorDrivingMachineInputMapping =
        {
            MachineInput.Driving0, MachineInput.Driving1, MachineInput.Driving2, MachineInput.Driving3
        };

        static readonly MachineInput[] _daptor2KeypadToMachineInputMapping =
        {
            MachineInput.NumPad1,    MachineInput.NumPad2, MachineInput.NumPad3,
            MachineInput.NumPad4,    MachineInput.NumPad5, MachineInput.NumPad6,
            MachineInput.NumPad7,    MachineInput.NumPad8, MachineInput.NumPad9,
            MachineInput.NumPadMult, MachineInput.NumPad0, MachineInput.NumPadHash
        };

        static readonly IDictionary<HidControllerType, string> _stelladaptorControllers = new Dictionary<HidControllerType, string>
        {
            { HidControllerType.None,         string.Empty },
            { HidControllerType.Stelladaptor, "Stelladaptor 2600-to-USB Interface"},
            { HidControllerType.A2600Daptor,  "2600-daptor"},
            { HidControllerType.A2600Daptor2, "2600-daptor II"},
        };

        static readonly string[] _daptor2ModeControllerNameSuffixes =
        {
            string.Empty, " (2600 mode)", " (7800 mode)", " (Keypad mode)"
        };

        #endregion

        public bool LeftJackHasAtariAdaptor { get; private set; }
        public bool RightJackHasAtariAdaptor { get; private set; }

        public void Poll()
        {
            if (_disposed)
                return;

            if (_gameControl == null)
                return;

            for (var i = 0; i < StelladaptorHost.MAX_CONTROLLER_COUNT; i++)
            {
                PollStelladaptorDevice(i);
            }
        }

        public string GetControllerInfo(int controllerNo)
        {
            if (controllerNo < 0 || _disposed)
                return null;
            var stelladaptorControllerNo = controllerNo;
            var hcd = StelladaptorHost.GetHidControllerData(stelladaptorControllerNo);
            if (hcd.Type == HidControllerType.None)
                return null;
            return _stelladaptorControllers[hcd.Type] + _daptor2ModeControllerNameSuffixes[_lastSeenDaptor2Mode[controllerNo & 1]];
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

            LeftJackHasAtariAdaptor = false;
            RightJackHasAtariAdaptor = false;
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

            StelladaptorHost.Start();
        }

        #endregion

        #region Helpers

        static int ToPaddlePlayerNo(int controllerNo, int paddleNo)
        {
            var paddlePlayerNo = ((controllerNo << 1) | (paddleNo & 1) & 3);
            return paddlePlayerNo;
        }

        void PollStelladaptorDevice(int controllerNo)
        {
            var hcd = StelladaptorHost.GetHidControllerData(controllerNo);
            var prevHcd = _lastHidControllerData[controllerNo];

            if (hcd.Buttons == prevHcd.Buttons
             && hcd.X == prevHcd.X
             && hcd.Y == prevHcd.Y
             && hcd.Type == prevHcd.Type)
                return;

            _lastHidControllerData[controllerNo] = hcd;

            _lastSeenDaptor2Mode[controllerNo & 1] = 0;

            if (controllerNo == 0)
                LeftJackHasAtariAdaptor  = hcd.Type != HidControllerType.None;
            if (controllerNo == 1)
                RightJackHasAtariAdaptor = hcd.Type != HidControllerType.None;

            switch (hcd.Type)
            {
                case HidControllerType.Stelladaptor:
                {
                    HandleHidControllerTypeStelladaptor(controllerNo, hcd);
                    break;
                }
                case HidControllerType.A2600Daptor:
                {
                    HandleHidControllerTypeA2600Daptor(controllerNo, hcd);
                    break;
                }
                case HidControllerType.A2600Daptor2:
                {
                    HandleHidControllerTypeA2600Daptor2(controllerNo, hcd);
                    break;
                }
            }
        }

        void HandleHidControllerTypeStelladaptor(int controllerNo, HidControllerData hcd)
        {
            var isPossibleJoystickInput = (hcd.X == 0x00 || hcd.X == 0x7f || hcd.X == 0xff)
                                       && (hcd.Y == 0x00 || hcd.Y == 0x7f || hcd.Y == 0xff);

            if (isPossibleJoystickInput)
            {
                _gameControl.JoystickChanged(controllerNo, MachineInput.Fire,  hcd[0]);
                _gameControl.JoystickChanged(controllerNo, MachineInput.Fire2, hcd[1]);  // in case it works for booster grip
                _gameControl.JoystickChanged(controllerNo, MachineInput.Left,  hcd.X == 0x00);
                _gameControl.JoystickChanged(controllerNo, MachineInput.Right, hcd.X == 0xff);
                _gameControl.JoystickChanged(controllerNo, MachineInput.Up,    hcd.Y == 0x00);
                _gameControl.JoystickChanged(controllerNo, MachineInput.Down,  hcd.Y == 0xff);

                _gameControl.ProLineJoystickChanged(controllerNo, MachineInput.Fire,  hcd[1]);
                _gameControl.ProLineJoystickChanged(controllerNo, MachineInput.Fire2, hcd[0]);
                _gameControl.ProLineJoystickChanged(controllerNo, MachineInput.Left,  hcd.X == 0x00);
                _gameControl.ProLineJoystickChanged(controllerNo, MachineInput.Right, hcd.X == 0xff);
                _gameControl.ProLineJoystickChanged(controllerNo, MachineInput.Up,    hcd.Y == 0x00);
                _gameControl.ProLineJoystickChanged(controllerNo, MachineInput.Down,  hcd.Y == 0xff);
            }

            const int paddleRange = (int)(0xff * 0.34);
            _gameControl.PaddleChanged(ToPaddlePlayerNo(controllerNo, 0), paddleRange, hcd.X);
            _gameControl.PaddleChanged(ToPaddlePlayerNo(controllerNo, 1), paddleRange, hcd.Y);
            _gameControl.PaddleButtonChanged(ToPaddlePlayerNo(controllerNo, 0), hcd[0]);
            _gameControl.PaddleButtonChanged(ToPaddlePlayerNo(controllerNo, 1), hcd[1]);

            _gameControl.DrivingPaddleChanged(controllerNo, _stelladaptorDrivingMachineInputMapping[ToDrivingPosition(hcd.Y)]);
        }

        void HandleHidControllerTypeA2600Daptor(int controllerNo, HidControllerData hcd)
        {
            var isPossibleJoystickInput = (hcd.X == 0x000 || hcd.X == 0x7ff || hcd.X == 0xfff)
                                       && (hcd.Y == 0x000 || hcd.Y == 0x7ff || hcd.Y == 0xfff);

            if (isPossibleJoystickInput)
            {
                _gameControl.JoystickChanged(controllerNo, MachineInput.Fire,  hcd[0]);
                _gameControl.JoystickChanged(controllerNo, MachineInput.Fire2, hcd[1]);  // in case it works for booster grip
                _gameControl.JoystickChanged(controllerNo, MachineInput.Left,  hcd.X == 0x000);
                _gameControl.JoystickChanged(controllerNo, MachineInput.Right, hcd.X == 0xfff);
                _gameControl.JoystickChanged(controllerNo, MachineInput.Up,    hcd.Y == 0x000);
                _gameControl.JoystickChanged(controllerNo, MachineInput.Down,  hcd.Y == 0xfff);

                _gameControl.ProLineJoystickChanged(controllerNo, MachineInput.Fire,  hcd[1]);
                _gameControl.ProLineJoystickChanged(controllerNo, MachineInput.Fire2, hcd[0]);
                _gameControl.ProLineJoystickChanged(controllerNo, MachineInput.Left,  hcd.X == 0x000);
                _gameControl.ProLineJoystickChanged(controllerNo, MachineInput.Right, hcd.X == 0xfff);
                _gameControl.ProLineJoystickChanged(controllerNo, MachineInput.Up,    hcd.Y == 0x000);
                _gameControl.ProLineJoystickChanged(controllerNo, MachineInput.Down,  hcd.Y == 0xfff);
            }

            const int paddleRange = (int)(0xfff * 0.34);
            _gameControl.PaddleChanged(ToPaddlePlayerNo(controllerNo, 0), paddleRange, hcd.X);
            _gameControl.PaddleChanged(ToPaddlePlayerNo(controllerNo, 1), paddleRange, hcd.Y);
            _gameControl.PaddleButtonChanged(ToPaddlePlayerNo(controllerNo, 0), hcd[0]);
            _gameControl.PaddleButtonChanged(ToPaddlePlayerNo(controllerNo, 1), hcd[1]);

            _gameControl.DrivingPaddleChanged(controllerNo, _stelladaptorDrivingMachineInputMapping[ToDrivingPosition(hcd.Y)]);
        }

        void HandleHidControllerTypeA2600Daptor2(int controllerNo, HidControllerData hcd)
        {
            int mode;
            if (hcd[4] && hcd[5])
            {
                // 2600 mode
                // Constant on/off flash
                // Joystick, Driving, Paddle (same as A2600Daptor)
                mode = 1;
            }
            else if (!hcd[4] && hcd[5])
            {
                // 7800 mode
                // 2 flashes on, then off a bit
                // ProLineJoystick, CBS Booster Grip
                mode = 2;
            }
            else if (hcd[4] && !hcd[5])
            {
                // Keypad mode
                // Mostly on, with a flash off
                // Keyboard, Video Touch (Star Raiders), Kids
                mode = 3;
            }
            else
            {
                // Boot to mouse mode (same as 2600 mode)
                mode = 1;
            }

            _lastSeenDaptor2Mode[controllerNo & 1] = mode;

            var isPossibleJoystickInput = (hcd.X == 0x000 || hcd.X == 0x7ff || hcd.X == 0xfff)
                                       && (hcd.Y == 0x000 || hcd.Y == 0x7ff || hcd.Y == 0xfff);

            switch (mode)
            {
                case 1:
                    if (isPossibleJoystickInput)
                    {
                        _gameControl.JoystickChanged(controllerNo, MachineInput.Fire,  hcd[0]);
                        _gameControl.JoystickChanged(controllerNo, MachineInput.Fire2, hcd[1]);  // in case it works for a booster grip
                        _gameControl.JoystickChanged(controllerNo, MachineInput.Left,  hcd.X == 0x000);
                        _gameControl.JoystickChanged(controllerNo, MachineInput.Right, hcd.X == 0xfff);
                        _gameControl.JoystickChanged(controllerNo, MachineInput.Up,    hcd.Y == 0x000);
                        _gameControl.JoystickChanged(controllerNo, MachineInput.Down,  hcd.Y == 0xfff);

                        _gameControl.ProLineJoystickChanged(controllerNo, MachineInput.Fire,  hcd[1]);
                        _gameControl.ProLineJoystickChanged(controllerNo, MachineInput.Fire2, hcd[0]);
                        _gameControl.ProLineJoystickChanged(controllerNo, MachineInput.Left,  hcd.X == 0x000);
                        _gameControl.ProLineJoystickChanged(controllerNo, MachineInput.Right, hcd.X == 0xfff);
                        _gameControl.ProLineJoystickChanged(controllerNo, MachineInput.Up,    hcd.Y == 0x000);
                        _gameControl.ProLineJoystickChanged(controllerNo, MachineInput.Down,  hcd.Y == 0xfff);
                    }

                    const int paddleRange = (int)(0xfff * 0.34);
                    _gameControl.PaddleChanged(ToPaddlePlayerNo(controllerNo, 0), paddleRange, hcd.X);
                    _gameControl.PaddleChanged(ToPaddlePlayerNo(controllerNo, 1), paddleRange, hcd.Y);
                    _gameControl.PaddleButtonChanged(ToPaddlePlayerNo(controllerNo, 0), hcd[0]);
                    _gameControl.PaddleButtonChanged(ToPaddlePlayerNo(controllerNo, 1), hcd[1]);

                    _gameControl.DrivingPaddleChanged(controllerNo, _stelladaptorDrivingMachineInputMapping[ToDrivingPosition(hcd.Y)]);
                    break;

                case 2:
                    if (isPossibleJoystickInput)
                    {
                        // for booster grip
                        _gameControl.JoystickChanged(controllerNo, MachineInput.Fire,  hcd[2]);
                        _gameControl.JoystickChanged(controllerNo, MachineInput.Fire2, hcd[3]);
                        _gameControl.JoystickChanged(controllerNo, MachineInput.Left,  hcd.X == 0x000);
                        _gameControl.JoystickChanged(controllerNo, MachineInput.Right, hcd.X == 0xfff);
                        _gameControl.JoystickChanged(controllerNo, MachineInput.Up,    hcd.Y == 0x000);
                        _gameControl.JoystickChanged(controllerNo, MachineInput.Down,  hcd.Y == 0xfff);

                        _gameControl.ProLineJoystickChanged(controllerNo, MachineInput.Fire,  hcd[3]);
                        _gameControl.ProLineJoystickChanged(controllerNo, MachineInput.Fire2, hcd[2]);
                        _gameControl.ProLineJoystickChanged(controllerNo, MachineInput.Left,  hcd.X == 0x000);
                        _gameControl.ProLineJoystickChanged(controllerNo, MachineInput.Right, hcd.X == 0xfff);
                        _gameControl.ProLineJoystickChanged(controllerNo, MachineInput.Up,    hcd.Y == 0x000);
                        _gameControl.ProLineJoystickChanged(controllerNo, MachineInput.Down,  hcd.Y == 0xfff);
                    }
                    break;

                case 3:
                    for (var i = 0; i < _daptor2KeypadToMachineInputMapping.Length; i++)
                    {
                        _gameControl.JoystickChanged(controllerNo, _daptor2KeypadToMachineInputMapping[i], hcd[i]);
                    }
                    break;
            }
        }

        static int ToDrivingPosition(ushort y)
        {
            int position;
            switch (y >> 4)
            {
                case 0x00:
                    position = 3; // up
                    break;
                case 0x7f:
                    position = 0; // center
                    break;
                case 0xff:
                    position = 1; // down
                    break;
                default:
                    position = 2; // half-down
                    break;
            }
            return position;
        }

        #endregion
    }
}
