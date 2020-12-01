#ifndef PCH_H
#define PCH_H

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
#include <WinUser.h>

#include <string.h>
#include <math.h>

#include <d2d1.h>
#include <d2derr.h>
#include <d2d1helper.h>
#include <dwrite.h>
#include <wincodec.h>

#define DIRECTINPUT_VERSION 0x0800
#include <dinput.h>

#include "resource.h"

#endif //PCH_H
