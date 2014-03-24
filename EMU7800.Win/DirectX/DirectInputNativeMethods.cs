/*
 * DirectInput
 *
 * Utility class for acquiring input using DirectInput
 *
 * Copyright © 2008 Mike Murphy
 *
 */
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace EMU7800.Win.DirectX
{
    public enum DirectInputStatus
    {
        Ok, Unplugged, InputLost, NotAcquired, OtherAppHasPriority, Unknown
    }

    public enum Key
    {
        A            = 30,
        AbntC1       = 0x73,
        AbntC2       = 0x7e,
        Add          = 0x4e,
        Apostrophe   = 40,
        Apps         = 0xdd,
        At           = 0x91,
        AX           = 150,
        B            = 0x30,
        Back         = 14,
        BackSlash    = 0x2b,
        BackSpace    = 14,
        C            = 0x2e,
        Calculator   = 0xa1,
        Capital      = 0x3a,
        CapsLock     = 0x3a,
        Circumflex   = 0x90,
        Colon        = 0x92,
        Comma        = 0x33,
        Convert      = 0x79,
        D            = 0x20,
        D0           = 11,
        D1           = 2,
        D2           = 3,
        D3           = 4,
        D4           = 5,
        D5           = 6,
        D6           = 7,
        D7           = 8,
        D8           = 9,
        D9           = 10,
        Decimal      = 0x53,
        Delete       = 0xd3,
        Divide       = 0xb5,
        Down         = 0xd0,
        E            = 0x12,
        End          = 0xcf,
        Equals       = 13,
        Escape       = 1,
        F            = 0x21,
        F1           = 0x3b,
        F10          = 0x44,
        F11          = 0x57,
        F12          = 0x58,
        F13          = 100,
        F14          = 0x65,
        F15          = 0x66,
        F2           = 60,
        F3           = 0x3d,
        F4           = 0x3e,
        F5           = 0x3f,
        F6           = 0x40,
        F7           = 0x41,
        F8           = 0x42,
        F9           = 0x43,
        G            = 0x22,
        Grave        = 0x29,
        H            = 0x23,
        Home         = 0xc7,
        I            = 0x17,
        Insert       = 210,
        J            = 0x24,
        K            = 0x25,
        Kana         = 0x70,
        Kanji        = 0x94,
        L            = 0x26,
        Left         = 0xcb,
        LeftAlt      = 0x38,
        LeftBracket  = 0x1a,
        LeftControl  = 0x1d,
        LeftMenu     = 0x38,
        LeftShift    = 0x2a,
        LeftWindows  = 0xdb,
        M            = 50,
        Mail         = 0xec,
        MediaSelect  = 0xed,
        MediaStop    = 0xa4,
        Minus        = 12,
        Multiply     = 0x37,
        Mute         = 160,
        MyComputer   = 0xeb,
        N            = 0x31,
        Next         = 0xd1,
        NextTrack    = 0x99,
        NoConvert    = 0x7b,
        Numlock      = 0x45,
        NumPad0      = 0x52,
        NumPad1      = 0x4f,
        NumPad2      = 80,
        NumPad3      = 0x51,
        NumPad4      = 0x4b,
        NumPad5      = 0x4c,
        NumPad6      = 0x4d,
        NumPad7      = 0x47,
        NumPad8      = 0x48,
        NumPad9      = 0x49,
        NumPadComma  = 0xb3,
        NumPadEnter  = 0x9c,
        NumPadEquals = 0x8d,
        NumPadMinus  = 0x4a,
        NumPadPeriod = 0x53,
        NumPadPlus   = 0x4e,
        NumPadSlash  = 0xb5,
        NumPadStar   = 0x37,
        O            = 0x18,
        OEM102       = 0x56,
        P            = 0x19,
        PageDown     = 0xd1,
        PageUp       = 0xc9,
        Pause        = 0xc5,
        Period       = 0x34,
        PlayPause    = 0xa2,
        Power        = 0xde,
        PrevTrack    = 0x90,
        Prior        = 0xc9,
        Q            = 0x10,
        R            = 0x13,
        Return       = 0x1c,
        Right        = 0xcd,
        RightAlt     = 0xb8,
        RightBracket = 0x1b,
        RightControl = 0x9d,
        RightMenu    = 0xb8,
        RightShift   = 0x36,
        RightWindows = 220,
        S            = 0x1f,
        Scroll       = 70,
        SemiColon    = 0x27,
        Slash        = 0x35,
        Sleep        = 0xdf,
        Space        = 0x39,
        Stop         = 0x95,
        Subtract     = 0x4a,
        SysRq        = 0xb7,
        T            = 20,
        Tab          = 15,
        U            = 0x16,
        Underline    = 0x93,
        Unlabeled    = 0x97,
        Up           = 200,
        V            = 0x2f,
        VolumeDown   = 0xae,
        VolumeUp     = 0xb0,
        W            = 0x11,
        Wake         = 0xe3,
        WebBack      = 0xea,
        WebFavorites = 230,
        WebForward   = 0xe9,
        WebHome      = 0xb2,
        WebRefresh   = 0xe7,
        WebSearch    = 0xe5,
        WebStop      = 0xe8,
        X            = 0x2d,
        Y            = 0x15,
        Yen          = 0x7d,
        Z            = 0x2c
    }

    public static class DirectInputNativeMethods
    {
        #region Fields

        const uint
            BYTE_SIZE              =   1,
            LONG_SIZE              =   4,
            KEYBOARDSTATE_SIZE     = 256 * BYTE_SIZE,
            MOUSESTATE_SIZE        =   3 * LONG_SIZE +   8 * BYTE_SIZE,
            JOYSTICKSTATE_SIZE     =  36 * LONG_SIZE + 128 * BYTE_SIZE,
            STELLADAPTOR_SIZE      =   1,
            HR_OK                  = 0,
            HR_UNPLUGGED           = 0x80040209,
            HR_INPUTLOST           = 0x8007001E,
            HR_NOTACQUIRED         = 0x8007000C,
            HR_OTHERAPPHASPRIORITY = 0x80070005;

        static readonly byte[] _InputState = new byte[KEYBOARDSTATE_SIZE + MOUSESTATE_SIZE + 2 * JOYSTICKSTATE_SIZE + 2 * STELLADAPTOR_SIZE];
        static GCHandle _InputStateHandle;

        #endregion

        #region Public Members

        public static int HResult { get; private set; }

        public static DirectInputStatus Status
        {
            get
            {
                switch ((uint)HResult)
                {
                    case HR_OK:                  return DirectInputStatus.Ok;
                    case HR_UNPLUGGED:           return DirectInputStatus.Unplugged;
                    case HR_INPUTLOST:           return DirectInputStatus.InputLost;
                    case HR_NOTACQUIRED:         return DirectInputStatus.NotAcquired;
                    case HR_OTHERAPPHASPRIORITY: return DirectInputStatus.OtherAppHasPriority;
                    default:                     return DirectInputStatus.Unknown;
                }
            }
        }

        public static bool ReadKeyState(Key key)
        {
            return ReadKeyState((int)key);
        }

        public static bool ReadKeyState(int key)
        {
            return (_InputState[key & 0xff] & 0x80) == 0x80;
        }

        public static void ReadMouseMovement(out int dx, out int dy)
        {
            dx = ReadInt32(KEYBOARDSTATE_SIZE);
            dy = ReadInt32(KEYBOARDSTATE_SIZE + 1 * LONG_SIZE);
        }

        public static bool ReadMouseButtonState(int buttonno)
        {
            return (_InputState[KEYBOARDSTATE_SIZE + 3 * LONG_SIZE + (buttonno & 0x7)] & 0x80) == 0x80;
        }

        public static void ReadJoystickPosition(int deviceno, out int x, out int y, out int z)
        {
            var baseOffset = KEYBOARDSTATE_SIZE + MOUSESTATE_SIZE + (deviceno & 1)*JOYSTICKSTATE_SIZE;
            x = ReadInt32(baseOffset);
            y = ReadInt32(baseOffset + 1*LONG_SIZE);
            z = ReadInt32(baseOffset + 2*LONG_SIZE);
        }

        public static bool ReadJoystickButtonState(int deviceno, int buttonno)
        {
            return (_InputState[KEYBOARDSTATE_SIZE + MOUSESTATE_SIZE + (deviceno & 1) * JOYSTICKSTATE_SIZE + (12*LONG_SIZE + (buttonno & 0x7f))] & 0x80) == 0x80;
        }

        public static bool IsStelladaptor(int deviceno)
        {
            return (_InputState[KEYBOARDSTATE_SIZE + MOUSESTATE_SIZE + 2 * JOYSTICKSTATE_SIZE + (deviceno & 1) * STELLADAPTOR_SIZE] & 0x80) == 0x80;
        }

        public static bool Is2600daptorII(int deviceno)
        {
            return (_InputState[KEYBOARDSTATE_SIZE + MOUSESTATE_SIZE + 2 * JOYSTICKSTATE_SIZE + (deviceno & 1) * STELLADAPTOR_SIZE] & 0x40) == 0x40;
        }

        /// <summary>
        /// Initialize input devices.
        /// </summary>
        /// <param name="hWnd">handle to focus window</param>
        /// <param name="fullScreen">fullscreen mode</param>
        /// <param name="axisRange">joystick axis range</param>
        /// <returns>check HResult and Status property when false</returns>
        public static bool Initialize(IntPtr hWnd, bool fullScreen, int axisRange)
        {
            _InputStateHandle = GCHandle.Alloc(_InputState, GCHandleType.Pinned);
            HResult = EMU7800DirectInput8_Initialize(hWnd, fullScreen, axisRange, _InputStateHandle.AddrOfPinnedObject());
            return Status.Equals(DirectInputStatus.Ok);
        }

        /// <summary>
        /// Polls all input devices.
        /// </summary>
        /// <param name="reacquireIfNecessary">attempt reacquire when input device(s) are lost</param>
        /// <returns>check HResult and Status property when false</returns>
        public static bool Poll(bool reacquireIfNecessary)
        {
            HResult = EMU7800DirectInput8_Poll(reacquireIfNecessary);
            return Status.Equals(DirectInputStatus.Ok);
        }

        /// <summary>
        /// Shutdown input devices
        /// </summary>
        public static void Shutdown()
        {
            for (var i = 0; i < _InputState.Length; i++)
                _InputState[i] = 0;
            EMU7800DirectInput8_Shutdown();
            if (_InputStateHandle.IsAllocated)
                _InputStateHandle.Free();
        }

        #endregion

        #region Helpers and DllImports

        private static int ReadInt32(long offset)
        {
            return ReadInt32((uint)offset);
        }

        private static int ReadInt32(uint offset)
        {
            return _InputState[offset] | (_InputState[offset + 1] << 8) | (_InputState[offset + 2] << 16) | (_InputState[offset + 3] << 24);
        }

        [DllImport("EMU7800.DirectX.dll"), SuppressUnmanagedCodeSecurity]
        private static extern int EMU7800DirectInput8_Initialize(IntPtr hWnd, bool fullScreen, int axisRange, IntPtr inputState);

        [DllImport("EMU7800.DirectX.dll"), SuppressUnmanagedCodeSecurity]
        private static extern int EMU7800DirectInput8_Poll(bool reacquireIfNecessary);

        [DllImport("EMU7800.DirectX.dll"), SuppressUnmanagedCodeSecurity]
        private static extern void EMU7800DirectInput8_Shutdown();

        #endregion
    }
}
