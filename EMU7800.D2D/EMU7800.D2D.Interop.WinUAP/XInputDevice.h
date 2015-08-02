// © Mike Murphy

#pragma once

using namespace Platform;

namespace EMU7800 { namespace D2D { namespace Interop {

public enum struct XInputButton
{
    A, B, X, Y, LThumb, RThumb, LShoulder, RShoulder, Start, Back, DUp, DDown, DLeft, DRight
};

public delegate void XInputButtonChangedHandler(XInputButton button, bool down);
public delegate void XInputThumbChangedHandler(short thumbLX, short thumbLY, short thumbRX, short thumbRY);
public delegate void XInputTriggerChangedHandler(uint8 leftTrigger, uint8 rightTrigger);

public ref class XInputDevice sealed
{
private:
    bool                  m_isControllerConnected;
    int                   m_userIndex;
    XINPUT_CAPABILITIES   m_xinputCaps;

    XINPUT_STATE*         m_pPrevState;
    XINPUT_STATE*         m_pCurrState;

    void RaiseButtonChangedIfNecessary(uint16 mask, XInputButton button);
    void RaiseThumbChangedIfNecessary();
    void RaiseTriggerChangedIfNecessary();

public:
    property bool IsControllerConnected { bool get() { return m_isControllerConnected; } }
    XInputDevice(int userIndex);
    virtual ~XInputDevice();

    property XInputButtonChangedHandler^ ButtonChanged;
    property XInputThumbChangedHandler^ ThumbChanged;
    property XInputTriggerChangedHandler^ TriggerChanged;

    int Poll();
};

} } }