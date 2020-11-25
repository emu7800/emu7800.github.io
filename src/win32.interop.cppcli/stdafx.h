// © Mike Murphy

#pragma once

// Versioning Macros
// http://msdn.microsoft.com/en-us/library/windows/desktop/dd370997%28v=vs.85%29.aspx

#ifndef WINVER
#define WINVER _WIN32_WINNT_WINTHRESHOLD
#endif

#ifndef _WIN32_WINNT
#define _WIN32_WINNT _WIN32_WINNT_WINTHRESHOLD
#endif

#ifndef UNICODE
#define UNICODE
#endif

#define WIN32_LEAN_AND_MEAN

#include <sdkddkver.h>
#include <windows.h>
#include <WindowsX.h>
#include <winuser.h>

// C RunTime Header Files
#include <stdlib.h>
#include <malloc.h>
#include <memory.h>
#include <wchar.h>
#include <math.h>

#pragma warning(push)
#pragma warning(disable:4458) // Visual C++ 2015 CTP issued this warning on this existing DX SDK header
#include <d2d1.h>
#pragma warning(pop)
#include <d2derr.h>
#include <d2d1helper.h>
#include <dwrite.h>
#include <wincodec.h>

#include <XAudio2.h>
#include <MMSystem.h>

#define DIRECTINPUT_VERSION 0x0800
#include <dinput.h>

#include <msclr/marshal.h>

#pragma comment(lib, "user32.lib")
#pragma comment(lib, "ole32.lib")
#pragma comment(lib, "d2d1.lib")
#pragma comment(lib, "dwrite.lib")
#pragma comment(lib, "windowscodecs.lib")
#pragma comment(lib, "dxgi.lib")
#pragma comment(lib, "dinput8.lib")

#pragma comment(lib, "dxguid.lib")
#pragma comment(lib, "winmm.lib")


#ifndef Assert
#if defined( DEBUG ) || defined( _DEBUG )
#define Assert(b) do {if (!(b)) {OutputDebugStringA("Assert: " #b "\n");}} while(0)
#else
#define Assert(b)
#endif //DEBUG || _DEBUG
#endif

inline double round(double x) { return (x - floor(x)) > 0.5 ? ceil(x) : floor(x); }

#include "D2DError.h"
