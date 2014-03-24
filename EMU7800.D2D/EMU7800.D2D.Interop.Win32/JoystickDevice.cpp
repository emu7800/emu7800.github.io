// © Mike Murphy

#include "stdafx.h"
#include "JoystickDevice.h"

using namespace EMU7800::D2D::Interop;

BOOL CALLBACK EnumAxesCallback(const DIDEVICEOBJECTINSTANCE *pdidoi, VOID *pContext)
{
    if (pdidoi->dwType & DIDFT_AXIS)
    {
        DIPROPRANGE diprg = {0};
        diprg.diph.dwSize       = sizeof(DIPROPRANGE);
        diprg.diph.dwHeaderSize = sizeof(DIPROPHEADER);
        diprg.diph.dwHow        = DIPH_BYID;
        diprg.diph.dwObj        = pdidoi->dwType;
        diprg.lMin              = -JoystickDevice::AXISRANGE;
        diprg.lMax              =  JoystickDevice::AXISRANGE;
        ((LPDIRECTINPUTDEVICE8)pContext)->SetProperty(DIPROP_RANGE, &diprg.diph);
    }
    return DIENUM_CONTINUE;
}

void JoystickDevice::Initialize(HWND hWnd)
{
    if (!m_pJoystick)
        return;

    m_pJoystick->SetDataFormat(&c_dfDIJoystick2);
    m_pJoystick->SetCooperativeLevel(hWnd, DISCL_NONEXCLUSIVE | DISCL_BACKGROUND);
    m_pJoystick->EnumObjects(EnumAxesCallback, m_pJoystick, DIDFT_AXIS);

    m_initialized = true;
}

HRESULT JoystickDevice::Poll()
{
    if (!m_pJoystick || !m_initialized || !m_pPrevState || !m_pCurrState)
        return E_FAIL;

    HRESULT hr = m_pJoystick->Poll();
    if FAILED(hr)
    {
        if (hr == DIERR_UNPLUGGED)
        {
            return hr;
        }
        if (hr == DIERR_INPUTLOST || hr == DIERR_NOTACQUIRED)
        {
            m_pJoystick->Acquire();
            return S_OK;
        }
    }

    hr = m_pJoystick->GetDeviceState(sizeof(DIJOYSTATE2), m_pCurrState);
    if FAILED(hr)
        return hr;

    if (JoystickButtonChanged != nullptr)
    {
        for (int i = 0; i < 16; i++)
        {
            bool prevDown = InterpretJoyButtonDown(m_pPrevState, i);
            bool currDown = InterpretJoyButtonDown(m_pCurrState, i);
            if (prevDown != currDown)
            {
                JoystickButtonChanged(i, currDown);
            }
        }
    }

    if (JoystickDirectionalButtonChanged != nullptr)
    {
        bool prevLeft  = InterpretJoyLeft(m_pPrevState);
        bool currLeft  = InterpretJoyLeft(m_pCurrState);
        if (prevLeft != currLeft)
        {
            JoystickDirectionalButtonChanged(JoystickDirectionalButtonEnum::Left, currLeft);
        }

        bool prevRight = InterpretJoyRight(m_pPrevState);
        bool currRight = InterpretJoyRight(m_pCurrState);
        if (prevRight != currRight)
        {
            JoystickDirectionalButtonChanged(JoystickDirectionalButtonEnum::Right, currRight);
        }

        bool prevUp    = InterpretJoyUp(m_pPrevState);
        bool currUp    = InterpretJoyUp(m_pCurrState);
        if (prevUp != currUp)
        {
            JoystickDirectionalButtonChanged(JoystickDirectionalButtonEnum::Up, currUp);
        }

        bool prevDown  = InterpretJoyDown(m_pPrevState);
        bool currDown  = InterpretJoyDown(m_pCurrState);
        if (prevDown != currDown)
        {
            JoystickDirectionalButtonChanged(JoystickDirectionalButtonEnum::Down, currDown);
        }
    }

    if (StelladaptorDrivingPositionChanged != nullptr)
    {
        int prevPos = InterpretStelladaptorDrivingPosition(m_pPrevState);
        int currPos = InterpretStelladaptorDrivingPosition(m_pCurrState);
        if (prevPos != currPos)
        {
            StelladaptorDrivingPositionChanged(currPos);
        }
    }

    if (StelladaptorPaddlePositionChanged != nullptr)
    {
        int prevPos = InterpretStelladaptorPaddlePosition(m_pPrevState, 0);
        int currPos = InterpretStelladaptorPaddlePosition(m_pCurrState, 0);
        if (prevPos != currPos)
        {
            StelladaptorPaddlePositionChanged(0, currPos);
        }

        prevPos = InterpretStelladaptorPaddlePosition(m_pPrevState, 1);
        currPos = InterpretStelladaptorPaddlePosition(m_pCurrState, 1);
        if (prevPos != currPos)
        {
            StelladaptorPaddlePositionChanged(1, currPos);
        }
    }

    if (Daptor2ModeChanged != nullptr)
    {
        int prevMode = InterpretDaptor2Mode(m_pPrevState);
        int currMode = InterpretDaptor2Mode(m_pCurrState);
        if (prevMode != currMode)
        {
            Daptor2ModeChanged(currMode);
        }
    }

    DIJOYSTATE2 *pTmp = m_pPrevState;
    m_pPrevState = m_pCurrState;
    m_pCurrState = pTmp;

    return S_OK;
}

void JoystickDevice::Reset()
{
    if (m_pPrevState)
        ZeroMemory(m_pPrevState, sizeof(DIJOYSTATE2));
    if (m_pCurrState)
        ZeroMemory(m_pCurrState, sizeof(DIJOYSTATE2));
}

JoystickDevice::JoystickDevice(LPDIRECTINPUTDEVICE8 pJoystick, const WCHAR tszProductName[MAX_PATH]) : m_initialized(false)
{
    m_pJoystick = pJoystick;
    m_ProductName = gcnew String(tszProductName);

    if      (DoesProductNameMatch("Stelladaptor 2600-to-USB Interface"))
        m_JoystickType = JoystickTypeEnum::Stelladaptor;
    else if (DoesProductNameMatch("2600-daptor"))
        m_JoystickType = JoystickTypeEnum::Daptor;
    else if (DoesProductNameMatch("2600-daptor II"))
        m_JoystickType = JoystickTypeEnum::Daptor2;
    else
        m_JoystickType = JoystickTypeEnum::None;

    HANDLE hHeap = GetProcessHeap();
    m_pPrevState = (DIJOYSTATE2*)HeapAlloc(hHeap, 0, sizeof(DIJOYSTATE2));
    if (m_pPrevState)
        ZeroMemory(m_pPrevState, sizeof(DIJOYSTATE2));
    m_pCurrState = (DIJOYSTATE2*)HeapAlloc(hHeap, 0, sizeof(DIJOYSTATE2));
    if (m_pCurrState)
        ZeroMemory(m_pCurrState, sizeof(DIJOYSTATE2));

    Reset();
}

JoystickDevice::~JoystickDevice()
{
    JoystickButtonChanged = nullptr;
    JoystickDirectionalButtonChanged = nullptr;
    StelladaptorDrivingPositionChanged = nullptr;
    StelladaptorPaddlePositionChanged = nullptr;
    Daptor2ModeChanged = nullptr;
    this->!JoystickDevice();
}

JoystickDevice::!JoystickDevice()
{
    if (m_pJoystick)
    {
        m_pJoystick->Unacquire();
        m_pJoystick = NULL;
    }
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
}