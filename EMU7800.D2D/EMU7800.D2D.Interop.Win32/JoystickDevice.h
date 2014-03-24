// © Mike Murphy

#pragma once

using namespace System;

namespace EMU7800 { namespace D2D { namespace Interop {

public enum class JoystickTypeEnum { None, Stelladaptor, Daptor, Daptor2 };
public enum class JoystickDirectionalButtonEnum { Left, Right, Up, Down };

public ref class JoystickDevice
{
private:
    bool m_initialized;

    LPDIRECTINPUTDEVICE8 m_pJoystick;
    DIJOYSTATE2* m_pPrevState;
    DIJOYSTATE2* m_pCurrState;
    String^ m_ProductName;
    JoystickTypeEnum m_JoystickType;

    bool DoesProductNameMatch(String^ s) { return m_ProductName->Equals(s, StringComparison::OrdinalIgnoreCase); }

    inline bool InterpretJoyButtonDown(DIJOYSTATE2* js, int i) { return (js->rgbButtons[i] & 0x80) == 0x80; }

    inline bool InterpretJoyLeft(DIJOYSTATE2* js)     { return js->lX < -DEADZONE; }
    inline bool InterpretJoyRight(DIJOYSTATE2* js)    { return js->lX >  DEADZONE; }
    inline bool InterpretJoyUp(DIJOYSTATE2* js)       { return js->lY < -DEADZONE; }
    inline bool InterpretJoyDown(DIJOYSTATE2* js)     { return js->lY >  DEADZONE; }

    inline int InterpretStelladaptorDrivingPosition(DIJOYSTATE2 *js)
    {
        int position;
        if      (js->lY < -DEADZONE)
            position = 3;                          // up
        else if (js->lY > (AXISRANGE - DEADZONE))
            position = 1;                          // down (full)
        else if (js->lY >  DEADZONE) // down (half)
            position = 2;
        else
            position = 0;                          // center
        return position;
    }

    inline int InterpretStelladaptorPaddlePosition(DIJOYSTATE2* js, int paddleno)
    {
        int position = (((paddleno & 1) == 0) ? js->lX : js->lY) + AXISRANGE;
        return position;
    }

    inline int InterpretDaptor2Mode(DIJOYSTATE2* js)
    {
        int z = js->lZ;
        switch (z)
        {
            case -1000: return  0;  // 2600 mode
            case  -875: return  1;  // 7800 mode
            case  -750: return  2;  // keypad mode
            default:    return -1;  // unknown mode
        }
    }

internal:
    void Initialize(HWND hWnd);
    JoystickDevice(LPDIRECTINPUTDEVICE8 pJoystick, const WCHAR tszProductName[MAX_PATH]);

    ~JoystickDevice();
    !JoystickDevice();

public:
    literal int
        AXISRANGE = 1000,
        DEADZONE  = 100;

    property String^ ProductName { String^ get() { return m_ProductName; } };
    property JoystickTypeEnum JoystickType { JoystickTypeEnum get() { return m_JoystickType; } };

    delegate void JoystickButtonChangedHandler(int buttonno, bool down);
    delegate void JoystickDirectionalButtonChangedHandler(JoystickDirectionalButtonEnum button, bool down);
    delegate void StelladaptorDrivingPositionChangedHandler(int position);
    delegate void StelladaptorPaddlePositionChangedHandler(int paddleno, int position);
    delegate void Daptor2ModeChangedHandler(int mode);
    JoystickButtonChangedHandler^ JoystickButtonChanged;
    JoystickDirectionalButtonChangedHandler^ JoystickDirectionalButtonChanged;
    StelladaptorDrivingPositionChangedHandler^ StelladaptorDrivingPositionChanged;
    StelladaptorPaddlePositionChangedHandler^ StelladaptorPaddlePositionChanged;
    Daptor2ModeChangedHandler^ Daptor2ModeChanged;

    HRESULT Poll();
    void Reset();
};

} } }