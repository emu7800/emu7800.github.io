// © Mike Murphy

#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;

namespace EMU7800 { namespace D2D { namespace Interop {

public ref class JoystickDeviceList
{
private:
    LPDIRECTINPUT8 m_pDI;
    array<JoystickDevice^>^ m_Joysticks;

    delegate int EnumJoysticksCallbackDelegate(const DIDEVICEINSTANCE* pdidInstance, VOID* pContext);
    int EnumJoysticksCallback(const DIDEVICEINSTANCE* pdidInstance, VOID* pContext);
    void Initialize(IntPtr^ hWnd);

public:
    property array<JoystickDevice^>^ Joysticks { array<JoystickDevice^>^ get() { return m_Joysticks; } }

    JoystickDeviceList();
    JoystickDeviceList(IntPtr^ hWnd);
    ~JoystickDeviceList();
    !JoystickDeviceList();
};

} } }