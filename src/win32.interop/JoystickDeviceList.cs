// © Mike Murphy

using System;

namespace EMU7800.Win32.Interop
{
    public static class JoystickDeviceList
    {
        public static JoystickDevice[] Joysticks { get; private set; } = Array.Empty<JoystickDevice>();

        public static void Initialize()
            => Initialize(IntPtr.Zero);

        public static void Initialize(IntPtr hWnd)
        {
            Close();
            DirectInputNativeMethods.Initialize(hWnd, out var joystickNames);
            var joysticks = new JoystickDevice[2];
            for (int i = 0, j = 0; i < joysticks.Length; i++)
            {
                if (i < joystickNames.Length)
                {
                    joysticks[i] = new JoystickDevice(joystickNames[i], i);
                }
                else
                {
                    XInputNativeMethods.Initialize(j, out var capabilities);
                    joysticks[i] = new JoystickDevice("XBox Default", j++);
                }
            }
            Joysticks = joysticks;
        }

        public static void Poll()
        {
            for (var i = 0; i < Joysticks.Length; i++)
            {
                Poll(i);
            }
        }

        public static void Poll(int deviceno)
        {
            if (Joysticks.Length == 0)
                return;

            switch (Joysticks[deviceno].JoystickType)
            {
                case JoystickType.Usb:
                case JoystickType.Stelladaptor:
                case JoystickType.Daptor:
                case JoystickType.Daptor2:
                    DirectInputNativeMethods.Poll(Joysticks[deviceno].InternalDeviceNumber, out var currDiState, out var prevDiState);
                    Joysticks[deviceno].RaiseEventsFromDirectInput(ref currDiState, ref prevDiState);
                    break;
                case JoystickType.Xbox360:
                    XInputNativeMethods.Poll(Joysticks[deviceno].InternalDeviceNumber, out var currXiState, out var prevXiState);
                    Joysticks[deviceno].RaiseEventsFromXinput(ref currXiState, ref prevXiState);
                    break;
                default:
                    break;
            }
        }

        public static void Close()
        {
            for (var i = 0; i < Joysticks.Length; i++)
            {
                Joysticks[i].ClearEventHandlers();
            }
            DirectInputNativeMethods.Shutdown();
            Joysticks = Array.Empty<JoystickDevice>();
        }
    }
}
