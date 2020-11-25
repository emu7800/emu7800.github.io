// © Mike Murphy

#include "pch.h"

LPDIRECTINPUT8       pDI;
LPDIRECTINPUTDEVICE8 pJoystick[2];
BYTE                 StelladaptorType[2];

DIJOYSTATE2 JoystickState[4];

int currStateIndex, prevStateIndex;

int axisRange = 1000;
int enumJoystickNo = -1;

BOOL CALLBACK EnumJoysticksCallback(const DIDEVICEINSTANCE *pdidInstance, VOID *pContext)
{
    if (enumJoystickNo > 1 || pContext != (VOID*)1)
        return DIENUM_STOP;

    int deviceno = (++enumJoystickNo) & 1;

    HRESULT hr = pDI->CreateDevice(pdidInstance->guidInstance, &pJoystick[deviceno], NULL);
    if FAILED(hr)
        return DIENUM_CONTINUE;

    if (wcscmp(pdidInstance->tszProductName, L"Stelladaptor 2600-to-USB Interface") == 0)
        StelladaptorType[deviceno] = 1;
    else if (wcscmp(pdidInstance->tszProductName, L"2600-daptor") == 0)
        StelladaptorType[deviceno] = 2;
    else if (wcscmp(pdidInstance->tszProductName, L"2600-daptor II") == 0)
        StelladaptorType[deviceno] = 3;

    return DIENUM_CONTINUE;
}

BOOL CALLBACK EnumAxesCallback(const DIDEVICEOBJECTINSTANCE *pdidoi, VOID *pContext)
{
    int deviceno = (UINT64)pContext & 1;

    if (pdidoi->dwType & DIDFT_AXIS)
    {
        DIPROPRANGE diprg       = { 0 };
        diprg.diph.dwSize       = sizeof(DIPROPRANGE);
        diprg.diph.dwHeaderSize = sizeof(DIPROPHEADER);
        diprg.diph.dwHow        = DIPH_BYID;
        diprg.diph.dwObj        = pdidoi->dwType;
        diprg.lMin              = -axisRange;
        diprg.lMax              = axisRange;
        pJoystick[deviceno]->SetProperty(DIPROP_RANGE, &diprg.diph);
    }

    return DIENUM_CONTINUE;
}

HRESULT CreateJoystick(int deviceno, HWND hWnd)
{
    if (deviceno < 0 || deviceno > enumJoystickNo || !pJoystick[deviceno])
        return S_OK;

    pJoystick[deviceno]->SetDataFormat(&c_dfDIJoystick2);
    pJoystick[deviceno]->SetCooperativeLevel(hWnd, DISCL_NONEXCLUSIVE | DISCL_BACKGROUND);
    pJoystick[deviceno]->EnumObjects(EnumAxesCallback, (LPVOID)(UINT_PTR)(UINT)deviceno, DIDFT_AXIS);

    return S_OK;
}

HRESULT PollJoystick(int deviceno)
{
    if (deviceno < 0 || deviceno > enumJoystickNo || !pJoystick[deviceno])
        return E_FAIL;

    HRESULT hr = pJoystick[deviceno]->Poll();
    if FAILED(hr) {
        if (hr == DIERR_UNPLUGGED)
            return hr;
        if (hr == DIERR_INPUTLOST || hr == DIERR_NOTACQUIRED) {
            pJoystick[deviceno]->Acquire();
            return S_OK;
        }
    }

    return pJoystick[deviceno]->GetDeviceState(sizeof(DIJOYSTATE2), &JoystickState[currStateIndex + deviceno]);
}

void FreeJoystick(int deviceno)
{
    if (pJoystick[deviceno]) {
        pJoystick[deviceno]->Unacquire();
        pJoystick[deviceno]->Release();
        pJoystick[deviceno] = NULL;
    }
}

extern "C" __declspec(dllexport) HRESULT __stdcall DInput8_Initialize(HWND hWnd, int axisRange, BYTE** ppStelladaptorType, int* pJoysticksFound)
{
    axisRange = axisRange;

    enumJoystickNo = -1;

    currStateIndex = 0;
    prevStateIndex = 2;

    JoystickState[0] = { 0 };
    JoystickState[1] = { 0 };
    JoystickState[2] = { 0 };
    JoystickState[3] = { 0 };

    StelladaptorType[0] = 0;
    StelladaptorType[1] = 0;

    HINSTANCE hInstance = GetModuleHandle(NULL);

    HRESULT hr = DirectInput8Create(hInstance, DIRECTINPUT_VERSION, IID_IDirectInput8, (VOID**)&pDI, NULL);
    if FAILED(hr)
        return hr;

    pDI->EnumDevices(DI8DEVCLASS_GAMECTRL, EnumJoysticksCallback, (LPVOID)1, DIEDFL_ATTACHEDONLY);

    *ppStelladaptorType = StelladaptorType;
    *pJoysticksFound = enumJoystickNo + 1;

    if (enumJoystickNo >= 0)
    {
        hr = CreateJoystick(0, hWnd);
        if FAILED(hr)
            return hr;

        if (enumJoystickNo >= 1)
        {
            hr = CreateJoystick(1, hWnd);
            if FAILED(hr)
                return hr;
        }
    }

    return S_OK;
}

extern "C" _declspec(dllexport) HRESULT __stdcall DInput8_Poll(DIJOYSTATE2** ppCurrState, DIJOYSTATE2** ppPrevState)
{
    currStateIndex += 2;
    prevStateIndex += 2;

    currStateIndex &= 3;
    prevStateIndex &= 3;

    *ppCurrState = JoystickState + currStateIndex;
    *ppPrevState = JoystickState + prevStateIndex;

    if (enumJoystickNo >= 0)
    {
        HRESULT hr = PollJoystick(0);
        if FAILED(hr)
            return hr;

        if (enumJoystickNo >= 1)
        {
            hr = PollJoystick(1);
            if FAILED(hr)
                return hr;
        }
    }

    return S_OK;
}

extern "C" _declspec(dllexport) void __stdcall DInput8_Shutdown()
{
    FreeJoystick(0);
    FreeJoystick(1);

    if (pDI) {
        pDI->Release();
        pDI = NULL;
    }
}
