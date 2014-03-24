// © Mike Murphy

#include "pch.h"
#include "XInputDevice.h"

using namespace EMU7800::D2D::Interop;

inline void XInputDevice::RaiseButtonChangedIfNecessary(uint16 mask, XInputButton button)
{
    uint16 prevDown = m_pPrevState->Gamepad.wButtons & mask;
    uint16 currDown = m_pCurrState->Gamepad.wButtons & mask;
    if (prevDown != currDown)
        ButtonChanged(button, currDown != 0);
}

inline void XInputDevice::RaiseThumbChangedIfNecessary()
{
    short pLX = m_pPrevState->Gamepad.sThumbLX;
    short cLX = m_pCurrState->Gamepad.sThumbLX;

    short pLY = m_pPrevState->Gamepad.sThumbLY;
    short cLY = m_pCurrState->Gamepad.sThumbLY;

    short pRX = m_pPrevState->Gamepad.sThumbRX;
    short cRX = m_pCurrState->Gamepad.sThumbRX;

    short pRY = m_pPrevState->Gamepad.sThumbRY;
    short cRY = m_pCurrState->Gamepad.sThumbRY;

    if (pLX != cLX || pLY != cLY || pRX != cRX || pRY != cRY)
        ThumbChanged(cLX, cLY, cRX, cRY);
}

inline void XInputDevice::RaiseTriggerChangedIfNecessary()
{
    uint8 prevLTrigger = m_pPrevState->Gamepad.bLeftTrigger;
    uint8 currLTrigger = m_pCurrState->Gamepad.bLeftTrigger;

    uint8 prevRTrigger = m_pPrevState->Gamepad.bRightTrigger;
    uint8 currRTrigger = m_pCurrState->Gamepad.bRightTrigger;

    if (prevLTrigger != currLTrigger || prevRTrigger != currRTrigger)
        TriggerChanged(currLTrigger, currRTrigger);
}

int XInputDevice::Poll()
{
    if (m_userIndex < 0 || m_userIndex > 3 || !m_pPrevState || !m_pCurrState)
        return -4;

    uint32 stateResult = XInputGetState(m_userIndex, m_pCurrState);
    if (stateResult)
    {
        m_isControllerConnected = false;
        return stateResult;
    }

    m_isControllerConnected = true;

    if (ButtonChanged != nullptr)
    {
        RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_DPAD_UP, XInputButton::DUp);
        RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_DPAD_DOWN, XInputButton::DDown);
        RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_DPAD_LEFT, XInputButton::DLeft);
        RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_DPAD_RIGHT, XInputButton::DRight);

        RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_A, XInputButton::A);
        RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_B, XInputButton::B);
        RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_X, XInputButton::X);
        RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_Y, XInputButton::Y);

        RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_LEFT_THUMB, XInputButton::LThumb);
        RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_RIGHT_THUMB, XInputButton::RThumb);
        RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_LEFT_SHOULDER, XInputButton::LShoulder);
        RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_RIGHT_SHOULDER, XInputButton::RShoulder);

        RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_BACK, XInputButton::Back);
        RaiseButtonChangedIfNecessary(XINPUT_GAMEPAD_START, XInputButton::Start);
    }

    if (ThumbChanged != nullptr)
    {
        RaiseThumbChangedIfNecessary();
    }

    if (TriggerChanged != nullptr)
    {
        RaiseTriggerChangedIfNecessary();
    }

    XINPUT_STATE* pTmp = m_pPrevState;
    m_pPrevState = m_pCurrState;
    m_pCurrState = pTmp;

    return S_OK;
}

XInputDevice::XInputDevice(int userIndex)
{
    m_userIndex = userIndex;
    m_isControllerConnected = false;

    HANDLE hHeap = GetProcessHeap();
    m_pPrevState = static_cast<XINPUT_STATE*>(HeapAlloc(hHeap, 0, sizeof(XINPUT_STATE)));
    if (m_pPrevState)
        ZeroMemory(m_pPrevState, sizeof(XINPUT_STATE));
    m_pCurrState = static_cast<XINPUT_STATE*>(HeapAlloc(hHeap, 0, sizeof(XINPUT_STATE)));
    if (m_pCurrState)
        ZeroMemory(m_pCurrState, sizeof(XINPUT_STATE));

    uint32 capsResult = XInputGetCapabilities(m_userIndex, XINPUT_FLAG_GAMEPAD, &m_xinputCaps);
    m_isControllerConnected = !capsResult;
}

XInputDevice::~XInputDevice()
{
    HANDLE hHeap = GetProcessHeap();
    if (m_pPrevState)
    {
        HeapFree(hHeap, 0, m_pPrevState);
        m_pPrevState = NULL;
    }
    if (m_pCurrState)
    {
        HeapFree(hHeap, 0, m_pCurrState);
        m_pCurrState = NULL;
    }
    ButtonChanged = nullptr;
    ThumbChanged = nullptr;
    TriggerChanged = nullptr;
}
