#pragma once

#include <agile.h>
#include <wchar.h>
#include <wrl.h>
#include <wrl/client.h>
#if (NTDDI_VERSION == NTDDI_WIN8)
#include <d3d11_1.h>
#include <d2d1_1.h>
#include <dwrite_1.h>
#include <d2d1effects.h>
#else
#include <d3d11_2.h>
#include <d2d1_2.h>
#include <dwrite_2.h>
#include <d2d1effects_1.h>
#endif
#include <wincodec.h>
#include <DirectXMath.h>
#include <XAudio2.h>
#include <shcore.h>
#if !(WINAPI_FAMILY == WINAPI_FAMILY_PHONE_APP)
#include <Xinput.h>
#endif

#pragma comment(lib, "d2d1.lib")
#pragma comment(lib, "d3d11.lib")
#pragma comment(lib, "dxgi.lib")
#pragma comment(lib, "dwrite.lib")
#pragma comment(lib, "dxguid.lib")
#if !(NTDDI_VERSION == NTDDI_WIN10)
#pragma comment(lib, "ole32.lib")
#endif
#pragma comment(lib, "runtimeobject.lib")
#pragma comment(lib, "shcore.lib")
#pragma comment(lib, "windowscodecs.lib")
#pragma comment(lib, "xaudio2.lib")
#if !(WINAPI_FAMILY == WINAPI_FAMILY_PHONE_APP)
#pragma comment(lib, "xinput.lib")
#endif