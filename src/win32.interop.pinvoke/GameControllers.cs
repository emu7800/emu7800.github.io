// © Mike Murphy

using System;
using static EMU7800.Win32.Interop.XInputNativeMethods;
using static System.Console;

namespace EMU7800.Win32.Interop;

public static class GameControllers
{
    static IntPtr HWnd;

    public static readonly GameController[] Controllers = [new(0), new(1)];

    public static void Initialize()
        => Initialize(HWnd);

    public static void Initialize(IntPtr hWnd)
    {
        WriteLine("Initializing game controllers...");
        HWnd = hWnd;

        Shutdown();

        DirectInputNativeMethods.Initialize(hWnd, out var joystickNames);

        for (var i = 0; i < Controllers.Length; i++)
        {
            if (i < joystickNames.Length)
            {
                Controllers[i].ProductName = joystickNames[i];
                Controllers[i].JoystickType = JoystickTypeFrom(joystickNames[i]);
                Controllers[i].InternalDeviceNumber = i;
            }
            else
            {
                Controllers[i].ProductName = "(None)";
                Controllers[i].JoystickType = JoystickType.None;
            }
            WriteLine($"Using {Controllers[i].ProductName} for P{i + 1}");
        }
        for (int i = 0, j = 0; i < Controllers.Length; i++)
        {
            if (Controllers[i].JoystickType == JoystickType.XInput)
            {
                var caps = new XINPUT_CAPABILITIES();
                do
                {
                    XInputNativeMethods.Initialize(j++, ref caps);
                }
                while (caps.Type == 0 && j < 4);
                if (j < 4)
                {
                    Controllers[i].InternalDeviceNumber = j - 1;
                    WriteLine($"Using XBox controller {Controllers[i].InternalDeviceNumber} for P{i + 1}");
                }
            }
        }
    }

    public static void Poll()
    {
        for (var i = 0; i < Controllers.Length; i++)
        {
            var controller = Controllers[i];
            switch (controller.JoystickType)
            {
                case JoystickType.XInput:
                    controller.RaiseEventsFromXinput();
                    break;
                case JoystickType.None:
                    break;
                default:
                    controller.RaiseEventsFromDirectInput();
                    break;
            }
        }
    }

    public static void Shutdown()
    {
        DirectInputNativeMethods.Shutdown();
    }

    #region Helpers

    static JoystickType JoystickTypeFrom(string name)
    {
        if (name == "Stelladaptor 2600-to-USB Interface")
        {
            return JoystickType.Stelladaptor;
        }
        else if (name == "2600-daptor")
        {
            return JoystickType.Daptor;
        }
        else if (name == "2600-daptor II")
        {
            return JoystickType.Daptor2;
        }
        else if (name == "Controller (XBOX 360 For Windows)"
             ||  name == "Controller (Xbox 360 Wireless Receiver for Windows)")
        {
            return JoystickType.XInput;
        }
        else if (name.Contains("XBOX", StringComparison.OrdinalIgnoreCase))
        {
            return JoystickType.XInput;
        }
        else if (name.Length > 0)
        {
            return JoystickType.Usb;
        }
        else
        {
            return JoystickType.XInput;
        }
    }

    #endregion
}
