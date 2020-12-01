using System;
using System.Runtime.InteropServices;
using System.Security;

namespace EMU7800.Win32.Interop
{
    internal unsafe static class XInputNativeMethods
    {
        static readonly GCHandle[,] XInputStates = AllocateXInputStateStructures();
        static readonly int[] CurrXInputStateIndices = new int[2];

        public const int
            XINPUT_FLAG_GAMEPAD             = 0x00000001,

            XINPUT_GAMEPAD_DPAD_UP          = 0x0001,
            XINPUT_GAMEPAD_DPAD_DOWN        = 0x0002,
            XINPUT_GAMEPAD_DPAD_LEFT        = 0x0004,
            XINPUT_GAMEPAD_DPAD_RIGHT       = 0x0008,
            XINPUT_GAMEPAD_START            = 0x0010,
            XINPUT_GAMEPAD_BACK             = 0x0020,
            XINPUT_GAMEPAD_LEFT_THUMB       = 0x0040,
            XINPUT_GAMEPAD_RIGHT_THUMB      = 0x0080,
            XINPUT_GAMEPAD_LEFT_SHOULDER    = 0x0100,
            XINPUT_GAMEPAD_RIGHT_SHOULDER   = 0x0200,
            XINPUT_GAMEPAD_A                = 0x1000,
            XINPUT_GAMEPAD_B                = 0x2000,
            XINPUT_GAMEPAD_X                = 0x4000,
            XINPUT_GAMEPAD_Y                = 0x8000
            ;

        [StructLayout(LayoutKind.Sequential)]
        public struct XINPUT_GAMEPAD
        {
            public ushort wButtons;
            public byte bLeftTrigger;
            public byte bRightTrigger;
            public short sThumbLX;
            public short sThumbLY;
            public short sThumbRX;
            public short sThumbRY;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XINPUT_VIBRATION
        {
            public ushort wLeftMotorSpeed;
            public ushort wRightMotorSpeed;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XINPUT_CAPABILITIES
        {
            public byte Type;
            public byte SubType;
            public ushort Flags;
            public XINPUT_GAMEPAD Gamepad;
            public XINPUT_VIBRATION Vibration;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XINPUT_STATE
        {
            public uint dwPacketNumber;
            public XINPUT_GAMEPAD Gamepad;

            public bool InterpretButtonDown(int buttonno)
                => (Gamepad.wButtons & (0x1000 << buttonno)) != 0;

            public bool InterpretJoyLeft()
                => (Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_LEFT) != 0;
            public bool InterpretJoyRight()
                => (Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_RIGHT) != 0;
            public bool InterpretJoyUp()
                => (Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_UP) != 0;
            public bool InterpretJoyDown()
                => (Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_DOWN) != 0;

            public bool InterpretButtonBack()
                => (Gamepad.wButtons & XINPUT_GAMEPAD_BACK) != 0;
            public bool InterpretButtonStart()
                => (Gamepad.wButtons & XINPUT_GAMEPAD_START) != 0;
        }

        public static int Initialize(int deviceno, out XINPUT_CAPABILITIES capabilities)
            => XInputGetCapabilities(deviceno, XINPUT_FLAG_GAMEPAD, out capabilities);

        public static int Poll(int deviceno, out XINPUT_STATE currState, out XINPUT_STATE prevState)
        {
            CurrXInputStateIndices[deviceno] ^= 1;
            var index = CurrXInputStateIndices[deviceno];
            var currStateHandle = XInputStates[deviceno, index];
            var hr = XInputGetState(deviceno, currStateHandle.AddrOfPinnedObject());
#pragma warning disable CS8605 // Unboxing a possibly null value.
            currState = (XINPUT_STATE)currStateHandle.Target;
            prevState = (XINPUT_STATE)XInputStates[deviceno, index ^ 1].Target;
#pragma warning restore CS8605 // Unboxing a possibly null value.
            return hr;
        }

        static GCHandle[,] AllocateXInputStateStructures()
            => new[,]
            {
                {
                    GCHandle.Alloc(new XINPUT_STATE(), GCHandleType.Pinned),
                    GCHandle.Alloc(new XINPUT_STATE(), GCHandleType.Pinned)
                },
                {
                    GCHandle.Alloc(new XINPUT_STATE(), GCHandleType.Pinned),
                    GCHandle.Alloc(new XINPUT_STATE(), GCHandleType.Pinned)
                }
            };

        [DllImport("xinput1_4.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        static extern int XInputGetState(int dwUserIndex, IntPtr xinputState);

        [DllImport("xinput1_4.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        static extern int XInputGetCapabilities(int dwUserIndex, int dwFlags, out XINPUT_CAPABILITIES capabilities);
    }
}