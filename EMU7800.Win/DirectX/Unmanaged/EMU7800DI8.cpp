#define STRICT
#define DIRECTINPUT_VERSION 0x0800

#include <dinput.h>
#include <string.h>

LPDIRECTINPUT8       pDI;
LPDIRECTINPUTDEVICE8 pKeyboard;
LPDIRECTINPUTDEVICE8 pMouse;
LPDIRECTINPUTDEVICE8 pJoystick[2];

typedef struct
{
    BYTE          KeyboardState[0x100];
    DIMOUSESTATE2 MouseState;
    DIJOYSTATE2   JoystickState[2];
    BYTE          IsStelladaptor[2];
} InputState_t;

InputState_t *pInputState;

int AxisRange = 1000;

extern HINSTANCE hInstance;

HRESULT CreateKeyboard(HWND hWnd, bool fullScreen)
{
    HRESULT hr = pDI->CreateDevice(GUID_SysKeyboard, &pKeyboard, NULL);
    if FAILED(hr)
        return hr;

    hr = pKeyboard->SetDataFormat(&c_dfDIKeyboard);
    if FAILED(hr)
        return hr;

    DWORD dwCoopFlags = fullScreen ? (DISCL_EXCLUSIVE | DISCL_NOWINKEY | DISCL_FOREGROUND) : (DISCL_EXCLUSIVE | DISCL_FOREGROUND);
    hr = pKeyboard->SetCooperativeLevel(hWnd, dwCoopFlags);
    if (hr == DIERR_UNSUPPORTED)
        return hr;

    return pKeyboard->Acquire();
}

HRESULT PollKeyboard(bool reacquireIfNecessary)
{
    if (pKeyboard == NULL)
        return S_OK;

    HRESULT hr = pKeyboard->Poll();
    if FAILED(hr) {
        if (hr == DIERR_UNPLUGGED)
            return hr;
        if (reacquireIfNecessary && (hr == DIERR_INPUTLOST || hr == DIERR_NOTACQUIRED)) {
            hr = pKeyboard->Acquire();
            if FAILED(hr)
                return hr;
        }
    }

    return pKeyboard->GetDeviceState(0x100, &pInputState->KeyboardState);
}

void FreeKeyboard()
{
    if (pKeyboard) {
        pKeyboard->Unacquire();
        pKeyboard->Release();
        pKeyboard = NULL;
    }
}

HRESULT CreateMouse(HWND hWnd, bool fullScreen)
{
    HRESULT hr = pDI->CreateDevice(GUID_SysMouse, &pMouse, NULL);
    if FAILED(hr)
        return hr;

    hr = pMouse->SetDataFormat(&c_dfDIMouse2);
    if FAILED(hr)
        return hr;

    DWORD dwCoopFlags = fullScreen ? (DISCL_EXCLUSIVE | DISCL_FOREGROUND) : (DISCL_NONEXCLUSIVE | DISCL_FOREGROUND);
    hr = pMouse->SetCooperativeLevel(hWnd, dwCoopFlags);
    if (hr == DIERR_UNSUPPORTED)
        return hr;

    return pMouse->Acquire();
}

HRESULT PollMouse(bool reacquireIfNecessary)
{
    if (!pMouse)
        return S_OK;

    HRESULT hr = pMouse->Poll();
    if FAILED(hr) {
        if (hr == DIERR_UNPLUGGED)
            return hr;
        if (reacquireIfNecessary && (hr == DIERR_INPUTLOST || hr == DIERR_NOTACQUIRED)) {
            hr = pMouse->Acquire();
            if FAILED(hr)
                return hr;
        }
    }

    return pMouse->GetDeviceState(sizeof(DIMOUSESTATE2), &pInputState->MouseState);
}

void FreeMouse()
{
    if (pMouse) {
        pMouse->Unacquire();
        pMouse->Release();
        pMouse = NULL;
    }
}

int enumJoystickNo;

BOOL CALLBACK EnumJoysticksCallback(const DIDEVICEINSTANCE *pdidInstance, VOID *pContext)
{
    if (enumJoystickNo >= 2 || (INT_PTR)pContext != 1)
        return DIENUM_STOP;

    int deviceno = enumJoystickNo++;

    HRESULT hr = pDI->CreateDevice(pdidInstance->guidInstance, &pJoystick[deviceno], NULL);
    if FAILED(hr)
        return DIENUM_CONTINUE;

    if (wcscmp(pdidInstance->tszProductName, L"Stelladaptor 2600-to-USB Interface") == 0)
        pInputState->IsStelladaptor[deviceno] = 0x80;
    else if (wcscmp(pdidInstance->tszProductName, L"2600-daptor") == 0)
        pInputState->IsStelladaptor[deviceno] = 0x80;
    else if (wcscmp(pdidInstance->tszProductName, L"2600-daptor II") == 0)
        pInputState->IsStelladaptor[deviceno] = 0x40;
    else
        pInputState->IsStelladaptor[deviceno] = 0;

    return DIENUM_CONTINUE;
}

BOOL CALLBACK EnumAxesCallback(const DIDEVICEOBJECTINSTANCE *pdidoi, VOID *pContext)
{
    INT_PTR deviceno = (INT_PTR)pContext;

    if (pdidoi->dwType & DIDFT_AXIS)
    {
        DIPROPRANGE diprg;
        diprg.diph.dwSize       = sizeof(DIPROPRANGE);
        diprg.diph.dwHeaderSize = sizeof(DIPROPHEADER);
        diprg.diph.dwHow        = DIPH_BYID;
        diprg.diph.dwObj        = pdidoi->dwType;
        diprg.lMin              = -AxisRange;
        diprg.lMax              = AxisRange;
        pJoystick[deviceno]->SetProperty(DIPROP_RANGE, &diprg.diph);
    }

    return DIENUM_CONTINUE;
}

HRESULT CreateJoystick(int deviceno, HWND hWnd)
{
    if (!pJoystick[deviceno])
        return S_OK;

    pJoystick[deviceno]->SetDataFormat(&c_dfDIJoystick2);
    pJoystick[deviceno]->SetCooperativeLevel(hWnd, DISCL_NONEXCLUSIVE | DISCL_BACKGROUND);
    pJoystick[deviceno]->EnumObjects(EnumAxesCallback, (LPVOID)(INT_PTR)deviceno, DIDFT_AXIS);

    return S_OK;
}

HRESULT PollJoystick(int deviceno)
{
    if (!pJoystick[deviceno])
        return S_OK;

    HRESULT hr = pJoystick[deviceno]->Poll();
    if FAILED(hr) {
        if (hr == DIERR_UNPLUGGED)
            return hr;
        if (hr == DIERR_INPUTLOST || hr == DIERR_NOTACQUIRED) {
            pJoystick[deviceno]->Acquire();
            return S_OK;
        }
    }

    return pJoystick[deviceno]->GetDeviceState(sizeof(DIJOYSTATE2), &pInputState->JoystickState[deviceno]);
}

void FreeJoystick(int deviceno)
{
    if (pJoystick[deviceno]) {
        pJoystick[deviceno]->Unacquire();
        pJoystick[deviceno]->Release();
        pJoystick[deviceno] = NULL;
    }
}

extern "C" __declspec(dllexport) HRESULT __stdcall EMU7800DirectInput8_Initialize(HWND hWnd, bool fullScreen, int axisRange, InputState_t *inputState)
{
    if (!inputState)
        return S_FALSE;

    AxisRange = axisRange;
    pInputState = inputState;

    HRESULT hr = DirectInput8Create(hInstance, DIRECTINPUT_VERSION, IID_IDirectInput8, (VOID**)&pDI, NULL);
    if FAILED(hr)
        return hr;

    hr = CreateKeyboard(hWnd, fullScreen);
    if FAILED(hr)
        return hr;

    hr = CreateMouse(hWnd, fullScreen);
    if FAILED(hr)
        return hr;

    enumJoystickNo = 0;
    pDI->EnumDevices(DI8DEVCLASS_GAMECTRL, EnumJoysticksCallback, (LPVOID)1, DIEDFL_ATTACHEDONLY);

    hr = CreateJoystick(0, hWnd);
    if FAILED(hr)
        return hr;

    hr = CreateJoystick(1, hWnd);
    if FAILED(hr)
        return hr;

    return hr;
}

extern "C" _declspec(dllexport) HRESULT __stdcall EMU7800DirectInput8_Poll(bool reacquireIfNecessary)
{
    if (!pInputState) return S_FALSE;

    HRESULT hr = PollKeyboard(reacquireIfNecessary);
    if FAILED(hr)
        return hr;

    hr = PollMouse(reacquireIfNecessary);
    if FAILED(hr)
        return hr;

    hr = PollJoystick(0);
    if FAILED(hr)
        return hr;

    hr = PollJoystick(1);
    if FAILED(hr)
        return hr;

    return hr;
}

extern "C" _declspec(dllexport) void __stdcall EMU7800DirectInput8_Shutdown()
{
    FreeKeyboard();
    FreeMouse();
    FreeJoystick(0);
    FreeJoystick(1);

    if (pDI) {
        pDI->Release();
        pDI = NULL;
    }
}
