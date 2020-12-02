// © Mike Murphy

using System;
using static System.Console;

namespace EMU7800.Win32.Interop
{
    public static class JoystickDeviceList
    {
        public static JoystickDevice[] Joysticks { get; private set; } = Array.Empty<JoystickDevice>();

        public static void Initialize()
            => Initialize(IntPtr.Zero);

        public static void Initialize(IntPtr hWnd)
        {
            Shutdown();
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
                    joysticks[i] = new JoystickDevice(j++);
                }
                WriteLine($"Using joystick({i}): {joysticks[i].ProductName}");
            }
            Joysticks = joysticks;
        }

        public static void Poll()
        {
            for (var i = 0; i < Joysticks.Length; i++)
            {
                Joysticks[i].Poll();
            }
        }

        public static void Shutdown()
        {
            DirectInputNativeMethods.Shutdown();
            for (var i = 0; i < Joysticks.Length; i++)
            {
                Joysticks[i].ClearEventHandlers();
                WriteLine($"Freed joystick({i}): {Joysticks[i].ProductName}");
            }
            Joysticks = Array.Empty<JoystickDevice>();
        }
    }
}
