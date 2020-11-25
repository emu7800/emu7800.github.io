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
            DirectInputNativeMethods.Initialize(hWnd, out var joystickTypes);
            var joysticks = new JoystickDevice[joystickTypes.Length];
            for (var i = 0; i < joystickTypes.Length; i++)
            {
                joysticks[i] = new JoystickDevice(joystickTypes[i]);
            }
            Joysticks = joysticks;
        }

        public static void Poll()
        {
            DirectInputNativeMethods.Poll(out var currState, out var prevState);
            for (var i = 0; i < Joysticks.Length; i++)
            {
                Joysticks[i].RaiseEvents(ref currState, ref prevState);
            }
        }

        public static void Close()
        {
            DirectInputNativeMethods.Shutdown();
            Joysticks = Array.Empty<JoystickDevice>();
        }
    }
}
