// © Mike Murphy

using EMU7800.Core;
using System;

namespace EMU7800.Shell;

public enum JoystickType { None, XInput, Usb, Stelladaptor, Daptor, Daptor2 }

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

    public static readonly Action<int, MachineInput, bool> ButtonChangedHandlerDefault = (cn, mi, d) => {};
    public static readonly Action<int, int, int> PaddlePositionChangedHandlerDefault = (cn, pn, o) => {};
    public static readonly Action<int, int, bool> PaddleButtonChangedHandlerDefault = (cn, pn, d) => {};
    public static readonly Action<int, MachineInput> DrivingPositionChangedHandlerDefault = (cn, mi) => {};

    public int ControllerNo { get; init; }
    public int DaptorMode { get; set; }
    public string DaptorModeStr => DaptorMode switch
    {
        0 => "2600",
        1 => "7800",
        2 => "keypad",
        _ => "unknown"
    };

    public string ProductName { get; set; } = string.Empty;
    public int InternalDeviceNumber { get; set; }
    public JoystickType JoystickType { get; set; } = JoystickType.None;
    public string Info => ProductName + (IsDaptor ? $" ({DaptorModeStr} mode)" : string.Empty);
    public bool IsAtariAdaptor
      => JoystickType is JoystickType.Stelladaptor or JoystickType.Daptor or JoystickType.Daptor2;
    public bool IsDaptor
      => JoystickType is JoystickType.Daptor or JoystickType.Daptor2;

    public Action<int, MachineInput, bool> ButtonChanged { get; set; } = ButtonChangedHandlerDefault;
    public Action<int, int, int> PaddlePositionChanged { get; set; } = PaddlePositionChangedHandlerDefault;
    public Action<int, int, bool> PaddleButtonChanged { get; set; } = PaddleButtonChangedHandlerDefault;
    public Action<int, MachineInput> DrivingPositionChanged { get; set; } = DrivingPositionChangedHandlerDefault;

    #region Constructors

    GameController() {}
    public GameController(int controllerNo)
        => ControllerNo = controllerNo;

    #endregion
}