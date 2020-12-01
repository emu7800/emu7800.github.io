// © Mike Murphy

#include "pch.h"

LPDIRECTINPUT8       pDI;
LPDIRECTINPUTDEVICE8 pJoystick[2];
WCHAR                ProductName[2][MAX_PATH];

DIJOYSTATE2          JoystickState[2][2];
int                  CurrStateIndex[2];

int axisRange = 1000;
int enumJoystickNo = -1;

BOOL CALLBACK EnumJoysticksCallback(const DIDEVICEINSTANCE *pdidInstance, VOID *pContext)
{
    if (enumJoystickNo > 1 || pContext != (VOID*)1)
        return DIENUM_STOP;

    int deviceno = (++enumJoystickNo) & 1;

    HRESULT hr = pDI->CreateDevice(pdidInstance->guidInstance, &pJoystick[deviceno], NULL);

    if SUCCEEDED(hr)
    {
        wcscpy_s(ProductName[deviceno], pdidInstance->tszProductName);
    }

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
        return E_FAIL;

    pJoystick[deviceno]->SetDataFormat(&c_dfDIJoystick2);
    pJoystick[deviceno]->SetCooperativeLevel(hWnd, DISCL_NONEXCLUSIVE | DISCL_BACKGROUND);
    pJoystick[deviceno]->EnumObjects(EnumAxesCallback, (LPVOID)(UINT_PTR)(UINT)deviceno, DIDFT_AXIS);

    return S_OK;
}

HRESULT PollJoystick(int deviceno, LPVOID pJoystickState)
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

    return pJoystick[deviceno]->GetDeviceState(sizeof(DIJOYSTATE2), pJoystickState);
}

void FreeJoystick(int deviceno)
{
    if (pJoystick[deviceno]) {
        pJoystick[deviceno]->Unacquire();
        pJoystick[deviceno]->Release();
        pJoystick[deviceno] = NULL;
    }
}

extern "C" __declspec(dllexport) HRESULT __stdcall DInput8_Initialize(HWND hWnd, int axisRange, WCHAR** ppProductName1, WCHAR * *ppProductName2)
{
    axisRange = axisRange;

    enumJoystickNo = -1;

    CurrStateIndex[0] = 0;
    CurrStateIndex[1] = 0;

    JoystickState[0][0] = { 0 };
    JoystickState[0][1] = { 0 };
    JoystickState[1][0] = { 0 };
    JoystickState[1][1] = { 0 };

    for (int i = 0; i < MAX_PATH; i++)
    {
        ProductName[0][i] = 0;
        ProductName[1][i] = 0;
    }

    *ppProductName1 = ProductName[0];
    *ppProductName2 = ProductName[1];

    HINSTANCE hInstance = GetModuleHandle(NULL);

    HRESULT hr = DirectInput8Create(hInstance, DIRECTINPUT_VERSION, IID_IDirectInput8, (VOID**)&pDI, NULL);
    if FAILED(hr)
        return hr;

    pDI->EnumDevices(DI8DEVCLASS_GAMECTRL, EnumJoysticksCallback, (LPVOID)1, DIEDFL_ATTACHEDONLY);

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

extern "C" _declspec(dllexport) HRESULT __stdcall DInput8_Poll(int deviceno, DIJOYSTATE2** ppCurrState, DIJOYSTATE2** ppPrevState)
{
    if (deviceno < 0 || deviceno > enumJoystickNo || !pJoystick[deviceno])
        return E_FAIL;

    CurrStateIndex[deviceno] ^= 1;

    int index = CurrStateIndex[deviceno];

    *ppCurrState = &JoystickState[deviceno][index];
    *ppPrevState = &JoystickState[deviceno][index ^ 1];

    return PollJoystick(deviceno, *ppCurrState);
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
