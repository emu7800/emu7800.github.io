/*
 * XInputNativeMethods.cs
 *
 * .NET interface to XInput
 *
 * Copyright © 2020 Mike Murphy
 *
 */
using System.Runtime.InteropServices;
using System.Security;

namespace EMU7800.Win32.Interop
{
    // %ProgramFiles(x86)%\Windows Kits\10\Include\<version>\um\Xinput.h

    internal unsafe static class XInputNativeMethods
    {
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
        }

        [DllImport("xinput1_4.dll"), SuppressUnmanagedCodeSecurity]
        public static extern int XInputGetState(int dwUserIndex, ref XINPUT_STATE state);

        [DllImport("xinput1_4.dll"), SuppressUnmanagedCodeSecurity]
        public static extern int XInputGetCapabilities(int dwUserIndex, int dwFlags, ref XINPUT_CAPABILITIES capabilities);
    }
}