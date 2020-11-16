// © Mike Murphy

#include "stdafx.h"
#include "JoystickDevice.h"
#include "JoystickDeviceList.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;
using namespace EMU7800::D2D::Interop;

int JoystickDeviceList::EnumJoysticksCallback(const DIDEVICEINSTANCE* pdidInstance, VOID* pContext)
{
    LPDIRECTINPUTDEVICE8 pJoystick;
    HRESULT hr = m_pDI->CreateDevice(pdidInstance->guidInstance, &pJoystick, NULL);
    if SUCCEEDED(hr)
    {
        JoystickDevice^ ji = gcnew JoystickDevice(pJoystick, pdidInstance->tszProductName);
        List<JoystickDevice^> list = gcnew List<JoystickDevice^>(m_Joysticks);
        list.Add(ji);
        m_Joysticks = list.ToArray();
    }
    return DIENUM_CONTINUE;
}

void JoystickDeviceList::Initialize(IntPtr^ hWnd)
{
    m_Joysticks = gcnew array<JoystickDevice^>(0);

    HINSTANCE hInstance = GetModuleHandle(NULL);
    LPDIRECTINPUT8 pDI;
    HRESULT hr = DirectInput8Create(hInstance, DIRECTINPUT_VERSION, IID_IDirectInput8, (VOID**)&pDI, NULL);
    m_pDI = SUCCEEDED(hr) ? pDI : NULL;
    if (m_pDI)
    {
        EnumJoysticksCallbackDelegate^ cbd = gcnew EnumJoysticksCallbackDelegate(this, &JoystickDeviceList::EnumJoysticksCallback);
        LPDIENUMDEVICESCALLBACKW cb = (LPDIENUMDEVICESCALLBACKW)Marshal::GetFunctionPointerForDelegate(cbd).ToPointer();
        m_pDI->EnumDevices(DI8DEVCLASS_GAMECTRL, cb, NULL, DIEDFL_ATTACHEDONLY);
        for each (JoystickDevice^ ji in m_Joysticks)
        {
            ji->Initialize((HWND)hWnd->ToPointer());
        }
    }
}

JoystickDeviceList::JoystickDeviceList()
{
    Initialize(IntPtr::Zero);
}

JoystickDeviceList::JoystickDeviceList(IntPtr ^hWnd)
{
    Initialize(hWnd);
}

JoystickDeviceList::~JoystickDeviceList()
{
    for each (JoystickDevice^ ji in m_Joysticks)
    {
        delete ji;
    }
    m_Joysticks = gcnew array<JoystickDevice^>(0);
    this->!JoystickDeviceList();
}

JoystickDeviceList::!JoystickDeviceList()
{
    if (m_pDI)
    {
        m_pDI->Release();
        m_pDI = NULL;
    }
}