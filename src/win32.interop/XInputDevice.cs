namespace EMU7800.Win32.Interop
{
    using static EMU7800.Win32.Interop.XInputNativeMethods;

    public enum XInputButton
    {
        A, B, X, Y, LThumb, RThumb, LShoulder, RShoulder, Start, Back, DUp, DDown, DLeft, DRight
    };

    public delegate void XInputButtonChangedHandler(XInputButton button, bool down);
    public delegate void XInputThumbChangedHandler(short thumbLX, short thumbLY, short thumbRX, short thumbRY);
    public delegate void XInputTriggerChangedHandler(byte leftTrigger, byte rightTrigger);

    public class XInputDevice
    {
        public readonly static XInputButtonChangedHandler  DefaultButtonChangedHandler = (b, d) => {};
        public readonly static XInputThumbChangedHandler DefaultThumbChangedHandler = (lx, ly, rx, ry) => {};
        public readonly static XInputTriggerChangedHandler DefaultTriggerChangedHandler = (l, r) => {};

        #region Fields

        readonly int _userIndex;

        XINPUT_CAPABILITIES _xinputCaps;
        XINPUT_STATE _pPrevState, _pCurrState;

        #endregion

        public bool IsControllerConnected { get; private set; }

        public XInputButtonChangedHandler ButtonChanged { get; set; } = DefaultButtonChangedHandler;
        public XInputThumbChangedHandler ThumbChanged { get; set; } = DefaultThumbChangedHandler;
        public XInputTriggerChangedHandler TriggerChanged { get; set; } = DefaultTriggerChangedHandler;

        public int Poll()
        {
            if (_userIndex < 0)
                return -4;

            var stateResult = XInputGetState(_userIndex, ref _pCurrState);
            if (stateResult != 0)
            {
                IsControllerConnected = false;
                return stateResult;
            }

            IsControllerConnected = true;

            if (ButtonChanged != DefaultButtonChangedHandler)
            {
                RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_DPAD_UP, XInputButton.DUp);
                RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_DPAD_DOWN, XInputButton.DDown);
                RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_DPAD_LEFT, XInputButton.DLeft);
                RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_DPAD_RIGHT, XInputButton.DRight);

                RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_A, XInputButton.A);
                RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_B, XInputButton.B);
                RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_X, XInputButton.X);
                RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_Y, XInputButton.Y);

                RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_LEFT_THUMB, XInputButton.LThumb);
                RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_RIGHT_THUMB, XInputButton.RThumb);
                RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_LEFT_SHOULDER, XInputButton.LShoulder);
                RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_RIGHT_SHOULDER, XInputButton.RShoulder);

                RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_BACK, XInputButton.Back);
                RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_START, XInputButton.Start);
            }

            if (ThumbChanged != DefaultThumbChangedHandler)
            {
                RaiseThumbChangedIfNecessary();
            }

            if (TriggerChanged != DefaultTriggerChangedHandler)
            {
                RaiseTriggerChangedIfNecessary();
            }

            _pPrevState = _pCurrState;

            return 0;
        }

        #region Constructors

        public XInputDevice(int userIndex)
        {
            _userIndex = userIndex;
            var capsResult = XInputGetCapabilities(_userIndex, XINPUT_FLAG_GAMEPAD, ref _xinputCaps);
            IsControllerConnected = capsResult == 0;
        }

        #endregion

        #region Helpers

        void RaiseButtonChangedIfNecessary(ushort mask, XInputButton button)
        {
            var prevDown = _pPrevState.Gamepad.wButtons & mask;
            var currDown = _pCurrState.Gamepad.wButtons & mask;
            if (prevDown != currDown)
            {
                ButtonChanged(button, currDown != 0);
            }
        }

        void RaiseThumbChangedIfNecessary()
        {
            var pLX = _pPrevState.Gamepad.sThumbLX;
            var cLX = _pCurrState.Gamepad.sThumbLX;

            var pLY = _pPrevState.Gamepad.sThumbLY;
            var cLY = _pCurrState.Gamepad.sThumbLY;

            var pRX = _pPrevState.Gamepad.sThumbRX;
            var cRX = _pCurrState.Gamepad.sThumbRX;

            var pRY = _pPrevState.Gamepad.sThumbRY;
            var cRY = _pCurrState.Gamepad.sThumbRY;

            if (pLX != cLX || pLY != cLY || pRX != cRX || pRY != cRY)
            {
                ThumbChanged(cLX, cLY, cRX, cRY);
            }
        }

        void RaiseTriggerChangedIfNecessary()
        {
            var prevLTrigger = _pPrevState.Gamepad.bLeftTrigger;
            var currLTrigger = _pCurrState.Gamepad.bLeftTrigger;

            var prevRTrigger = _pPrevState.Gamepad.bRightTrigger;
            var currRTrigger = _pCurrState.Gamepad.bRightTrigger;

            if (prevLTrigger != currLTrigger || prevRTrigger != currRTrigger)
            {
                TriggerChanged(currLTrigger, currRTrigger);
            }
        }

        #endregion
    }
}
