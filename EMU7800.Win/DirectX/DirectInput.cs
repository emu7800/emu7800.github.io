/*
 * DirectInput
 *
 * Utility class for acquiring input using DirectInput
 *
 * Copyright © 2007, 2012 Mike Murphy
 *
 */
using System;
using EMU7800.Core;

namespace EMU7800.Win.DirectX
{
    public delegate void JoystickChangedHandler(int joystickno, bool left, bool right, bool up, bool down, bool fire, bool fire2, bool fire3);
    public delegate void StelladaptorDrivingChangedHandler(int deviceno, int position, bool fire);
    public delegate void StelladaptorPaddleChangedHandler(int paddleno, int val);
    public delegate void StelladaptorPaddleButtonChangedHandler(int paddleno, bool fire);
    public delegate void Daptor2KeypadChangedHandler(int deviceno, MachineInput key, bool down);
    public delegate void MousePaddleChangedHandler(int val);
    public delegate void MouseChangedHandler(int dx, int dy);
    public delegate void MouseButtonChangedHandler(bool fire);
    public delegate void KeyboardChangedHandler(Key key, bool down);

    internal class DirectInput : IDisposable
    {
        #region Fields

        const int
            JoystickRange    = 1000,
            JoystickDeadzone = 100;

        struct LastJoystickInput
        {
            public bool Left, Right, Up, Down, Fire, Fire2, Fire3;
        };

        struct LastDrivingInput
        {
            public int Position;
            public bool Fire;
        }

        struct LastPaddleInput
        {
            public int Val;
            public bool Fire;
        };

        class LastDaptor2KeypadInput
        {
            public readonly bool[] KeyPressed = new bool[12];
        }

        readonly LastJoystickInput[] _lastJoystickInput = new LastJoystickInput[2];
        readonly LastDrivingInput[] _lastStelladaptorDrivingInput = new LastDrivingInput[2];
        readonly LastPaddleInput[] _lastStelladaptorPaddleInput = new LastPaddleInput[4];
        readonly LastDaptor2KeypadInput[] _lastDaptor2KeypadInput = new LastDaptor2KeypadInput[2];
        readonly bool[] _isStelladaptor = new bool[2];
        readonly bool[] _isDaptor2 = new bool[2];
        readonly int[] _daptor2Mode = new int[2];
        readonly int _joyBTriggerGlobalSetting,  _joyBBoosterGlobalSetting;
        readonly int[] _joyFireButtonNo = new int[2];
        readonly int[] _joyFire2ButtonNo = new int[2];
        readonly int[] _joyFire3ButtonNo = new int[2];
        readonly bool[] _lastKeyboardInput = new bool[0x100];
        LastPaddleInput _lastMousePaddleInput;

        // for DaptorKeypadChanged
        readonly MachineInput[] _keypadToMachineInputMapping =
        {
            MachineInput.NumPad1,    MachineInput.NumPad2, MachineInput.NumPad3,
            MachineInput.NumPad4,    MachineInput.NumPad5, MachineInput.NumPad6,
            MachineInput.NumPad7,    MachineInput.NumPad8, MachineInput.NumPad9,
            MachineInput.NumPadMult, MachineInput.NumPad0, MachineInput.NumPadHash
        };

        #endregion

        #region Public Members

        public JoystickChangedHandler JoystickChanged;
        public StelladaptorDrivingChangedHandler StelladaptorDrivingChanged;
        public StelladaptorPaddleChangedHandler StelladaptorPaddleChanged;
        public StelladaptorPaddleButtonChangedHandler StelladaptorPaddleButtonChanged;
        public Daptor2KeypadChangedHandler Daptor2KeypadChanged;
        public MousePaddleChangedHandler MousePaddleChanged;
        public MouseChangedHandler MouseChanged;
        public MouseButtonChangedHandler MouseButtonChanged;
        public KeyboardChangedHandler KeyboardChanged;

        public int MousePaddleRange { get; private set; }
        public static int StelladaptorPaddleRange { get; private set; }

        public bool IsStelladaptor(int deviceno)
        {
            return _isStelladaptor[deviceno & 1];
        }

        public bool IsDaptor2(int deviceno)
        {
            return _isDaptor2[deviceno & 1];
        }

        /// <summary>
        /// Returns the 2600-daptor II mode: 0=2600 mode, 1=7800 mode, 2=keypad mode.
        /// Defaults to 0 when 2600-daptor not recognized.
        /// </summary>
        /// <param name="deviceno"></param>
        public int GetDaptor2Mode(int deviceno)
        {
            return _isDaptor2[deviceno & 1] ? _daptor2Mode[deviceno & 1] : 0;
        }

        public string GetDaptor2ModeText(int mode)
        {
            switch (mode)
            {
                case 0:  return "2600";
                case 1:  return "7800";
                case 2:  return "KPad";
                default: return string.Empty;
            }
        }

        public int LastHResult
        {
            get { return DirectInputNativeMethods.HResult; }
        }

        public bool Poll(bool reacquireIfNecessary)
        {
            if (!DirectInputNativeMethods.Poll(reacquireIfNecessary))
                return false;

            if (StelladaptorPaddleChanged != null)
            {
                if (_isStelladaptor[0] || _isDaptor2[0])
                {
                    RaiseStelladaptorPaddleChangedIfNecessary(0);
                    RaiseStelladaptorPaddleChangedIfNecessary(1);
                }
                if (_isStelladaptor[1] || _isDaptor2[1])
                {
                    RaiseStelladaptorPaddleChangedIfNecessary(2);
                    RaiseStelladaptorPaddleChangedIfNecessary(3);
                }
            }

            if (StelladaptorPaddleButtonChanged != null)
            {
                if (_isStelladaptor[0] || _isDaptor2[0])
                {
                    RaiseStelladaptorPaddleButtonChangedIfNecessary(0);
                    RaiseStelladaptorPaddleButtonChangedIfNecessary(1);
                }
                if (_isStelladaptor[1] || _isDaptor2[1])
                {
                    RaiseStelladaptorPaddleButtonChangedIfNecessary(2);
                    RaiseStelladaptorPaddleButtonChangedIfNecessary(3);
                }
            }

            if (StelladaptorDrivingChanged != null)
            {
                if (_isStelladaptor[0] || _isDaptor2[0])
                {
                    RaiseStelladaptorDrivingChangedIfNecessary(0);
                }
                if (_isStelladaptor[1] || _isDaptor2[1])
                {
                    RaiseStelladaptorDrivingChangedIfNecessary(1);
                }
            }

            if (Daptor2KeypadChanged != null)
            {
                if (_isDaptor2[0])
                {
                    RaiseDaptorKeypadButtonChangedIfNecessary(0);
                }
                if (_isDaptor2[1])
                {
                    RaiseDaptorKeypadButtonChangedIfNecessary(1);
                }
            }

            if (JoystickChanged != null)
            {
                RaiseJoystickChangedIfNecessary(0);
                RaiseJoystickChangedIfNecessary(1);
            }

            if (MousePaddleChanged != null)
                RaiseMousePaddleChangedIfNecessary();

            if (MouseChanged != null)
                RaiseMouseChangedIfNecessary();
            if (MouseButtonChanged != null)
                RaiseMouseButtonChangedIfNecessary();

            if (KeyboardChanged != null)
                RaiseKeyboardChanged();

            return true;
        }

        #endregion

        #region Constructors

        private DirectInput()
        {
            MousePaddleRange = JoystickRange;

            var globalSettings = new GlobalSettings(new NullLogger());
            _joyBTriggerGlobalSetting = globalSettings.JoyBTrigger;
            _joyBBoosterGlobalSetting= globalSettings.JoyBBooster;

            var pf = globalSettings.PaddleFactor / 100.0f;
            if (pf < 0.01f)
                pf = 0.01f;
            else if (pf > 1.0f)
                pf = 1.0f;
            StelladaptorPaddleRange = (int)((JoystickRange << 1) * pf);

            for (var i = 0; i < _lastDaptor2KeypadInput.Length; i++)
                _lastDaptor2KeypadInput[i] = new LastDaptor2KeypadInput();
        }

        public DirectInput(IntPtr handle, bool fullScreen) : this()
        {
            DirectInputNativeMethods.Initialize(handle, fullScreen, JoystickRange);

            for (var deviceno = 0; deviceno < 2; deviceno++)
            {
                _joyFireButtonNo[deviceno] = _joyBTriggerGlobalSetting;
                _joyFire2ButtonNo[deviceno] = _joyBBoosterGlobalSetting;
                _joyFire3ButtonNo[deviceno] = 0;

                _isStelladaptor[deviceno] = DirectInputNativeMethods.IsStelladaptor(deviceno);
                _isDaptor2[deviceno] = DirectInputNativeMethods.Is2600daptorII(deviceno);

                if (!_isStelladaptor[deviceno] && !_isDaptor2[deviceno])
                    continue;

                var z = 0;
                for (var i = 0; i < 5; i++)
                {
                    int x, y;
                    DirectInputNativeMethods.Poll(true);
                    DirectInputNativeMethods.ReadJoystickPosition(deviceno, out x, out y, out z);
                    if (z != 0)
                        break;
                    // need fixed duration to allow time for the z-axis input to surface after init
                    System.Threading.Thread.Sleep(1);
                }

                switch (z)
                {
                    case -1000:
                        _daptor2Mode[deviceno] = 0; // 2600 mode
                        break;
                    case -875:
                        _daptor2Mode[deviceno] = 1; // 7800 mode
                        break;
                    case -750:
                        _daptor2Mode[deviceno] = 2; // keypad mode
                        break;
                }

                if (_isDaptor2[deviceno] && _daptor2Mode[deviceno] == 1 /* 7800 mode */)
                {
                    // 2600-daptor II:
                    // B0 = 2600 fire button (on with either 7800 LeftFire or 7800 RightFire)
                    // B2 = 7800 LeftFire,  BoosterTop
                    // B3 = 7800 RightFire, BoosterHandle
                    _joyFireButtonNo[deviceno] = 2;
                    _joyFire2ButtonNo[deviceno] = 3;
                    _joyFire3ButtonNo[deviceno] = 0;
                    StelladaptorPaddleRange = (int)((JoystickRange << 1) * 0.34);
                }
                else if (_isStelladaptor[deviceno] || _isDaptor2[deviceno])
                {
                    _joyFireButtonNo[deviceno] = 0;
                    _joyFire2ButtonNo[deviceno] = 1;
                    StelladaptorPaddleRange = (int)((JoystickRange << 1) * 0.34);
                }
            }
        }

        #endregion

        #region Disposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
            DirectInputNativeMethods.Shutdown();
        }

        ~DirectInput()
        {
            Dispose(false);
        }

        #endregion

        #region Helpers

        void RaiseJoystickChangedIfNecessary(int deviceno)
        {
            int x, y, z;
            DirectInputNativeMethods.ReadJoystickPosition(deviceno, out x, out y, out z);

            var left = x < -JoystickDeadzone;
            var right = x > JoystickDeadzone;
            var up = y < -JoystickDeadzone;
            var down = y > JoystickDeadzone;

            // Fire3 is only for 2600-daptor II support, to collect the 2600 fire button input alongside the other two BoosterGrip buttons.

            var fire1 = DirectInputNativeMethods.ReadJoystickButtonState(deviceno, _joyFireButtonNo[deviceno]);
            var fire2 = DirectInputNativeMethods.ReadJoystickButtonState(deviceno, _joyFire2ButtonNo[deviceno]);
            var fire3 = DirectInputNativeMethods.ReadJoystickButtonState(deviceno, _joyFire3ButtonNo[deviceno]);

            if (left == _lastJoystickInput[deviceno].Left
                && right == _lastJoystickInput[deviceno].Right
                && up == _lastJoystickInput[deviceno].Up
                && down == _lastJoystickInput[deviceno].Down
                && fire1 == _lastJoystickInput[deviceno].Fire
                && fire2 == _lastJoystickInput[deviceno].Fire2
                && fire3 == _lastJoystickInput[deviceno].Fire3)
                    return;

            _lastJoystickInput[deviceno].Left = left;
            _lastJoystickInput[deviceno].Right = right;
            _lastJoystickInput[deviceno].Up = up;
            _lastJoystickInput[deviceno].Down = down;
            _lastJoystickInput[deviceno].Fire = fire1;
            _lastJoystickInput[deviceno].Fire2 = fire2;
            _lastJoystickInput[deviceno].Fire3 = fire3;
            JoystickChanged(deviceno, left, right, up, down, fire1, fire2, fire3);
        }

        void RaiseStelladaptorDrivingChangedIfNecessary(int deviceno)
        {
            int x, y, z;
            DirectInputNativeMethods.ReadJoystickPosition(deviceno, out x, out y, out z);

            int position;
            if (y < -JoystickDeadzone)
                position = 3;                   // up
            else if (y > (JoystickRange - JoystickDeadzone))
                position = 1;                   // down (full)
            else if (y > JoystickDeadzone)      // down (half)
                position = 2;
            else
                position = 0;                   // center

            var fire = DirectInputNativeMethods.ReadJoystickButtonState(deviceno, _joyFireButtonNo[deviceno]);

            if (_lastStelladaptorDrivingInput[deviceno].Position == position && _lastStelladaptorDrivingInput[deviceno].Fire == fire)
                return;

            _lastStelladaptorDrivingInput[deviceno].Position = position;
            _lastStelladaptorDrivingInput[deviceno].Fire = fire;

            StelladaptorDrivingChanged(deviceno, position, fire);
        }

        void RaiseStelladaptorPaddleChangedIfNecessary(int paddleno)
        {
            var deviceno = paddleno >> 1;
            paddleno &= 3;

            int x, y, z;
            DirectInputNativeMethods.ReadJoystickPosition(deviceno, out x, out y, out z);

            var val = (((paddleno & 1) == 0) ? x : y) + JoystickRange;
            if (val != _lastStelladaptorPaddleInput[paddleno].Val)
            {
                _lastStelladaptorPaddleInput[paddleno].Val = val;
                StelladaptorPaddleChanged(paddleno, val);
            }
        }

        void RaiseStelladaptorPaddleButtonChangedIfNecessary(int paddleno)
        {
            var deviceno = paddleno >> 1;
            paddleno &= 3;

            var fire = DirectInputNativeMethods.ReadJoystickButtonState(deviceno, paddleno & 1);
            if (fire != _lastStelladaptorPaddleInput[paddleno].Fire)
            {
                _lastStelladaptorPaddleInput[paddleno].Fire = fire;
                StelladaptorPaddleButtonChanged(paddleno, fire);
            }
        }

        void RaiseDaptorKeypadButtonChangedIfNecessary(int deviceno)
        {
            for (var i = 0; i < _keypadToMachineInputMapping.Length; i++)
            {
                RaiseDaptorKeypadButtonChangedIfNecessary(deviceno, i);
            }
        }

        void RaiseDaptorKeypadButtonChangedIfNecessary(int deviceno, int buttonno)
        {
            var down = DirectInputNativeMethods.ReadJoystickButtonState(deviceno, buttonno);
            if (down != _lastDaptor2KeypadInput[deviceno].KeyPressed[buttonno])
            {
                _lastDaptor2KeypadInput[deviceno].KeyPressed[buttonno] = down;
                Daptor2KeypadChanged(deviceno, _keypadToMachineInputMapping[buttonno], down);
            }
        }

        void RaiseMousePaddleChangedIfNecessary()
        {
            int dx, dy;
            DirectInputNativeMethods.ReadMouseMovement(out dx, out dy);
            if (dx == 0)
                return;

            _lastMousePaddleInput.Val += dx;

            if (_lastMousePaddleInput.Val < 0)
                _lastMousePaddleInput.Val = 0;
            else if (_lastMousePaddleInput.Val > MousePaddleRange)
                _lastMousePaddleInput.Val = MousePaddleRange;

            MousePaddleChanged(_lastMousePaddleInput.Val);
        }

        void RaiseMouseChangedIfNecessary()
        {
            int dx, dy;
            DirectInputNativeMethods.ReadMouseMovement(out dx, out dy);
            if (dx == 0 && dy == 0)
                return;
            MouseChanged(dx, dy);
        }

        void RaiseMouseButtonChangedIfNecessary()
        {
            var fire = DirectInputNativeMethods.ReadMouseButtonState(0);
            if (_lastMousePaddleInput.Fire == fire)
                return;
            _lastMousePaddleInput.Fire = fire;
            MouseButtonChanged(fire);
        }

        void RaiseKeyboardChanged()
        {
            for (var i = 0; i < 0x100; i++)
            {
                var down = DirectInputNativeMethods.ReadKeyState(i);
                if (down ^ _lastKeyboardInput[i])
                    KeyboardChanged((Key)i, down);
                _lastKeyboardInput[i] = down;
            }
        }

        #endregion
    }
}
