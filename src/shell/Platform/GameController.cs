// © Mike Murphy

using EMU7800.Core;
using System;

namespace EMU7800.Shell;

public enum JoystickType { None, XInput, Usb, Stelladaptor, Daptor, Daptor2 }
public enum Daptor2Mode { Unknown, A2600, A7800, Keypad }

public sealed class GameController
{
    public static readonly MachineInput[] StelladaptorDrivingMachineInputMapping =
    [
        MachineInput.Driving0, MachineInput.Driving1, MachineInput.Driving2, MachineInput.Driving3
    ];

    public static readonly MachineInput[] Daptor2KeypadToMachineInputMapping =
    [
        MachineInput.NumPad1,    MachineInput.NumPad2, MachineInput.NumPad3,
        MachineInput.NumPad4,    MachineInput.NumPad5, MachineInput.NumPad6,
        MachineInput.NumPad7,    MachineInput.NumPad8, MachineInput.NumPad9,
        MachineInput.NumPadMult, MachineInput.NumPad0, MachineInput.NumPadHash,
        MachineInput.NumPad0,    MachineInput.NumPad0, MachineInput.NumPad0,
        MachineInput.NumPad0
    ];

    readonly Window _window;

    public int ControllerNo { get; init; }
    public Daptor2Mode Daptor2Mode { get; set; } = Daptor2Mode.Unknown;
    public string Daptor2ModeStr => Daptor2Mode switch
    {
        Daptor2Mode.A2600  => "2600",
        Daptor2Mode.A7800  => "7800",
        Daptor2Mode.Keypad => "keypad",
        _ => "unknown"
    };

    public string ProductName { get; set; } = string.Empty;
    public int InternalDeviceNumber { get; set; }
    public JoystickType JoystickType { get; set; } = JoystickType.None;
    public string Info => ProductName + (IsDaptor ? $" ({Daptor2ModeStr} mode)" : string.Empty);
    public bool IsAtariAdaptor
      => JoystickType is JoystickType.Stelladaptor or JoystickType.Daptor or JoystickType.Daptor2;
    public bool IsStelladaptor
      => JoystickType is JoystickType.Stelladaptor or JoystickType.Daptor;
    public bool IsDaptor
      => JoystickType is JoystickType.Daptor or JoystickType.Daptor2;

    public Action<int, MachineInput, bool> ButtonChanged => _window.OnButtonChanged;
    public Action<int, int, int> PaddlePositionChanged => _window.OnPaddlePositionChanged;
    public Action<int, int, bool> PaddleButtonChanged => _window.OnPaddleButtonChanged;
    public Action<int, MachineInput> DrivingPositionChanged => _window.OnDrivingPositionChanged;

    public static JoystickType JoystickTypeFrom(string name)
        => name switch
        {
            "Pixels Past Stelladaptor 2600-to-USB Interface" or
            "Stelladaptor 2600-to-USB Interface"
                => JoystickType.Stelladaptor,
            "Microchip Technology Inc. 2600-daptor" or // unverified
            "2600-daptor"
                => JoystickType.Daptor,
            "Microchip Technology Inc. 2600-daptor II" or
            "2600-daptor II"
                => JoystickType.Daptor2,
            "Controller (XBOX 360 For Windows)" or
            "Controller (Xbox 360 Wireless Receiver for Windows)"
                => JoystickType.XInput,
            _ => name.Contains("XBOX", StringComparison.OrdinalIgnoreCase) || name.Contains("XINPUT compatible", StringComparison.OrdinalIgnoreCase)
                    ? JoystickType.XInput
                    : name.Length > 0 ? JoystickType.Usb : JoystickType.XInput
        };

    #region Constructors

    public GameController(int controllerNo, Window window)
      => (ControllerNo, _window) = (controllerNo, window);

    #endregion
}
