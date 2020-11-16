namespace EMU7800.Core
{
    public enum ControllerAction
    {
        Up,
        Down,
        Left,
        Right,
        Trigger,   // Interpretation: ProLineJoystick RFire; Joystick Fire, BoosterGrip Top
        Trigger2,  // Interpretation: ProLineJoystick LFire, BoosterGrip Trigger
        Keypad1, Keypad2, Keypad3,
        Keypad4, Keypad5, Keypad6,
        Keypad7, Keypad8, Keypad9,
        KeypadA, Keypad0, KeypadP,
        Driving0, Driving1, Driving2, Driving3,
    }
}