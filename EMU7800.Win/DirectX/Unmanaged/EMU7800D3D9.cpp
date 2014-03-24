#define WIN32_LEAN_AND_MEAN

#include <windows.h>
#include <d3d9.h>

unsigned int TargetFrameWidth, TargetFrameHeight;
unsigned int DeviceWindowWidth, DeviceWindowHeight;

HINSTANCE hInstance;
HWND hMainWindow;

int SelectedAdapter;
int EmuFrameStride;
bool FullScreen;
bool UsingWorkerThread;

bool ShowSnow;

bool IsActivated;
bool IsDeviceLost;

typedef struct {
    float x, y, z, rhw;
    DWORD color;
} Point;

int LineListSize = 0;
Point *LineList;

BYTE *FrameBuffer, *FrameBufferEnd;
int FrameBufferSize;
int *Palette;

int RUN, RIS, X0, Y0;
int LastDX, LastDY;
int FrameCount, SegmentCount;

D3DCAPS9                D3DCaps9;
D3DADAPTER_IDENTIFIER9  D3DAdapterInfo;
D3DDISPLAYMODE          D3DDisplayMode;
D3DPRESENT_PARAMETERS   D3DPP;
LPDIRECT3D9             D3DObject;
LPDIRECT3DDEVICE9       D3DDevice;
LPDIRECT3DVERTEXBUFFER9 D3DVertexBuffer;

HRESULT InitializeD3D(bool fullscreen)
{
    D3DObject = Direct3DCreate9(D3D_SDK_VERSION);
    if (!D3DObject)
        return E_FAIL;

    HRESULT hr = D3DObject->GetAdapterIdentifier(SelectedAdapter, 0, &D3DAdapterInfo);
    if FAILED(hr)
        return hr;

    hr = D3DObject->GetDeviceCaps(SelectedAdapter, D3DDEVTYPE_HAL, &D3DCaps9);
    if FAILED(hr)
        return hr;

    if (!(D3DCaps9.PresentationIntervals & D3DPRESENT_INTERVAL_IMMEDIATE))
        return E_FAIL;

    hr = D3DObject->CheckDeviceType(SelectedAdapter, D3DDEVTYPE_HAL, D3DFMT_X8R8G8B8, D3DFMT_X8R8G8B8, fullscreen ? FALSE : TRUE);
    if FAILED(hr)
        return hr;

    if (fullscreen) {
        bool foundOne = false;
        int modeCount = D3DObject->GetAdapterModeCount(SelectedAdapter, D3DFMT_X8R8G8B8);
        for (int i=0; i < modeCount; i++) {
            D3DDISPLAYMODE displayMode;
            hr = D3DObject->EnumAdapterModes(SelectedAdapter, D3DFMT_X8R8G8B8, i, &displayMode);
            if FAILED(hr)
                break;
            D3DDISPLAYMODE *p = &displayMode;
            if (p->Width < TargetFrameWidth) // look for basic requirements
                continue;
            if (foundOne && (p->Width * 3 != p->Height * 4 || p->Width > D3DDisplayMode.Width)) // look for a better one
                continue;
            memcpy(&D3DDisplayMode, &displayMode, sizeof(D3DDISPLAYMODE));
            foundOne = true;
        }
        if (foundOne)
        {
            TargetFrameWidth  = D3DDisplayMode.Width;
            TargetFrameHeight = D3DDisplayMode.Height;
        }
        else
        {
            hr = E_FAIL;
        }
    } else {
        hr = D3DObject->GetAdapterDisplayMode(SelectedAdapter, &D3DDisplayMode);
    }

    return hr;
}

void ResetD3DDevice()
{
    if (D3DVertexBuffer != NULL) {
        D3DVertexBuffer->Release();
        D3DVertexBuffer = NULL;
    }

    D3DDevice->Reset(&D3DPP);
    D3DDevice->SetRenderState(D3DRS_ZENABLE, D3DZB_FALSE);
    D3DDevice->SetRenderState(D3DRS_CULLMODE, D3DCULL_NONE);
    D3DDevice->SetRenderState(D3DRS_CLIPPING, FALSE);
    D3DDevice->SetRenderState(D3DRS_LIGHTING, FALSE);

    HRESULT hr = D3DDevice->CreateVertexBuffer(
        LineListSize * sizeof(Point),
        D3DUSAGE_DONOTCLIP | D3DUSAGE_DYNAMIC | D3DUSAGE_WRITEONLY,
        D3DFVF_XYZRHW | D3DFVF_DIFFUSE, D3DPOOL_DEFAULT,
        &D3DVertexBuffer,
        NULL);

    if SUCCEEDED(hr)
        IsDeviceLost = false;
}

HRESULT CreateD3DDevice(HWND hWnd, bool fullscreen)
{
    ZeroMemory(&D3DPP, sizeof(D3DPP));
    D3DPP.BackBufferFormat     = fullscreen ? D3DDisplayMode.Format : D3DFMT_UNKNOWN;
    D3DPP.Windowed             = fullscreen ? FALSE : TRUE;
    D3DPP.hDeviceWindow        = hWnd;
    D3DPP.SwapEffect           = D3DSWAPEFFECT_DISCARD;
    D3DPP.BackBufferCount      = 1;
    D3DPP.BackBufferWidth      = TargetFrameWidth;
    D3DPP.BackBufferHeight     = TargetFrameHeight;
    D3DPP.PresentationInterval = D3DPRESENT_INTERVAL_IMMEDIATE;
    if (fullscreen)
        D3DPP.FullScreen_RefreshRateInHz = D3DDisplayMode.RefreshRate;

    int behaviorFlags = D3DCREATE_SOFTWARE_VERTEXPROCESSING;

    HRESULT hr = D3DObject->CreateDevice(SelectedAdapter, D3DDEVTYPE_HAL, hWnd, behaviorFlags, &D3DPP, &D3DDevice);
    if FAILED(hr)
        return hr;

    RIS = TargetFrameHeight / 240;
    RUN = RIS; if (EmuFrameStride == 160) RUN <<= 1;

    X0 = (TargetFrameWidth - (RUN * EmuFrameStride)) >> 1;
    Y0 = (TargetFrameHeight - (RIS * 240)) >> 1;

    int lineSizeNeed = EmuFrameStride * 240 * 2 * RIS;
    if (LineListSize != lineSizeNeed) {
        if (LineList != NULL) delete[] LineList;
        LineListSize = lineSizeNeed;
        LineList = new Point[LineListSize];
        for (int i=0; i < LineListSize; i++) {
            LineList[i].z = 1.0f;
            LineList[i].rhw = 1.0f;
        }
    }

    ResetD3DDevice();

    return D3D_OK;
}

void ShutdownD3D()
{
    if (LineList != NULL) {
        delete LineList;
        LineList = NULL;
        LineListSize = 0;
    }
    if (D3DVertexBuffer != NULL) {
        D3DVertexBuffer->Release();
        D3DVertexBuffer = NULL;
    }
    if (D3DDevice != NULL) {
        D3DDevice->Release();
        D3DDevice = NULL;
    }
    if (D3DObject != NULL) {
        D3DObject->Release();
        D3DObject = NULL;
    }
}

HRESULT TryRestoringLostDevice()
{
    if (!IsDeviceLost)
        return D3D_OK;

    HRESULT hr = D3DDevice->TestCooperativeLevel();
    if (hr == D3DERR_DEVICENOTRESET) {
        ResetD3DDevice();
        return D3D_OK;
    }
    return hr == D3DERR_DEVICELOST ? S_FALSE : hr;
}

HRESULT PresentFrame(int dx, int dy)
{
    if (IsDeviceLost)
        return D3DERR_DEVICELOST;

    LastDX = dx;
    LastDY = dy;

    int cCur = 0, c = 0, x, nx, y;
    BYTE *fbp = FrameBuffer + dy * EmuFrameStride + dx;
    Point *slp, *p = LineList;

    while (fbp < FrameBuffer)
        fbp += FrameBufferSize;

    y = Y0;

    for (int j=0; j < 240; j++)
    {
        slp = p;

        x = nx = X0;

        for (int i=0; i <= EmuFrameStride; i++, nx += RUN)
        {
            if (i == EmuFrameStride)
            {
            }
            else if (ShowSnow)
            {
                int r = rand() % 0x100;
                c = D3DCOLOR_XRGB(r, r, r);
            }
            else
            {
                c = Palette[*fbp];
                if (++fbp >= FrameBufferEnd)
                    fbp -= FrameBufferSize;
            }

            if (i == 0)
            {
            }
            else if (c != cCur || i == EmuFrameStride)
            {
                p->x = (float)x;
                p->y = (float)y;
                p->color = cCur;
                p++;

                x = nx;

                p->x = (float)x;
                p->y = (float)y;
                p->color = cCur;
                p++;
            }

            cCur = c;
        }

        y++;

        // copy last line as many times as necessary
        if (slp == p)
            continue;

        Point *elp = p;
        for (int r=1; r < RIS; r++) {
            for (Point *sp=slp; sp < elp; sp++) {
                p->x = sp->x;
                p->y = (float)y;
                p->color = sp->color;
                p++;
            }
            y++;
        }
    }

    int segmentCount = static_cast<int>(p - LineList) >> 1;

    VOID *pBuffer = NULL;
    HRESULT hr = D3DVertexBuffer->Lock(0, 2 * segmentCount * sizeof(Point), &pBuffer, D3DLOCK_DISCARD | D3DLOCK_NOSYSLOCK);
    if FAILED(hr)
        return hr;

    memcpy(pBuffer, LineList, 2 * segmentCount * sizeof(Point));

    D3DVertexBuffer->Unlock();

    hr = D3DDevice->BeginScene();
    if FAILED(hr)
        return hr;

    D3DDevice->SetStreamSource(0, D3DVertexBuffer, 0, sizeof(Point));
    D3DDevice->SetFVF(D3DFVF_XYZRHW | D3DFVF_DIFFUSE);
    D3DDevice->DrawPrimitive(D3DPT_LINELIST, 0, segmentCount);
    D3DDevice->EndScene();

    hr = D3DDevice->Present(NULL, NULL, NULL, NULL);
    if FAILED(hr) {
        if (hr == D3DERR_DEVICELOST) {
            IsDeviceLost = true;
        }
        return hr;
    }

    SegmentCount += segmentCount;
    FrameCount++;

    return D3D_OK;
}

LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    switch (message)
    {
    case WM_CLOSE:
        IsActivated = false;
        return 0;
    case WM_ERASEBKGND:
        if (!UsingWorkerThread && IsActivated) PresentFrame(LastDX, LastDY);
        return 0;
    default:
        return DefWindowProc(hWnd, message, wParam, lParam);
    }
 }

HWND InitWindow(HICON icon, bool fullscreen, HINSTANCE hinst)
{
    WNDCLASS wc;
    wc.style         = CS_HREDRAW | CS_VREDRAW;
    wc.lpfnWndProc   = (WNDPROC)WndProc;
    wc.cbClsExtra    = 0;
    wc.cbWndExtra    = 0;
    wc.hInstance     = hinst;
    wc.hIcon         = icon;
    wc.hCursor       = LoadCursor(NULL, IDC_ARROW);
    wc.hbrBackground = (HBRUSH)(COLOR_WINDOW+1);
    wc.lpszMenuName  = NULL;
    wc.lpszClassName = L"EMU7800.DirectX.HostingWindow";
    RegisterClass(&wc);

    HWND hWnd = CreateWindow(wc.lpszClassName, L"EMU7800",
        fullscreen ? WS_EX_TOPMOST | WS_POPUP | WS_VISIBLE : WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT, CW_USEDEFAULT,
        DeviceWindowWidth, DeviceWindowHeight, NULL, NULL,
        wc.hInstance,
        NULL);

    if (hWnd) {
        ShowWindow(hWnd, SW_SHOW);
        UpdateWindow(hWnd);
    }

    return hWnd;
}

#pragma warning(push)
#pragma warning(disable: 4100)
BOOL WINAPI DllMain(__in HINSTANCE hinstDLL, __in DWORD fdwReason, __in LPVOID lpvReserved)
{
    if (fdwReason != DLL_PROCESS_ATTACH)
        return TRUE;

    hInstance = hinstDLL;
    IsActivated = false;
    return TRUE;
}
#pragma warning(pop)

extern "C" __declspec(dllexport) void __stdcall EMU7800DirectX_Shutdown()
{
    IsActivated = false;
    ShutdownD3D();
    if (hMainWindow) {
        DestroyWindow(hMainWindow);
        hMainWindow = 0;
    }
}

extern "C" __declspec(dllexport) int __stdcall EMU7800DirectX_Initialize(
    int adapter,
    HICON icon,
    bool fullscreen,
    BYTE *frameBuffer,
    int frameBufferSize,
    int *palette,
    int stride,
    int targetFrameWidth,
    int targetFrameHeight,
    int deviceWindowWidth,
    int deviceWindowHeight,
    bool usingWorkerThread)
{
    if (IsActivated)
        return 2;

    SelectedAdapter = adapter;
    FullScreen = fullscreen;
    FrameBuffer = frameBuffer;
    FrameBufferSize = frameBufferSize;
    Palette = palette;
    EmuFrameStride = stride;
    TargetFrameWidth = targetFrameWidth;
    TargetFrameHeight = targetFrameHeight;
    DeviceWindowWidth = deviceWindowWidth;
    DeviceWindowHeight = deviceWindowHeight;
    UsingWorkerThread = usingWorkerThread;

    FrameCount = 0;
    SegmentCount = 0;

    FrameBufferEnd = FrameBuffer + FrameBufferSize;

    HRESULT hr;

    if (EmuFrameStride != 160 && EmuFrameStride != 320)
    {
        hr = 2;
        EMU7800DirectX_Shutdown();
        return hr;
    }

    hMainWindow = InitWindow(icon, fullscreen, hInstance);
    if (!hMainWindow) {
        hr = 2;
        EMU7800DirectX_Shutdown();
        return hr;
    }

    hr = InitializeD3D(fullscreen);
    if FAILED(hr) {
        EMU7800DirectX_Shutdown();
        return hr;
    }

    hr = CreateD3DDevice(hMainWindow, FullScreen);
    if FAILED(hr) {
        EMU7800DirectX_Shutdown();
        return hr;
    }

    IsActivated = true;
    return D3D_OK;
}

extern "C" __declspec(dllexport) int __stdcall EMU7800DirectX_TryRestoringLostDevice()
{
    if (!IsActivated)
        return 2;

    return TryRestoringLostDevice();
}

extern "C" __declspec(dllexport) int __stdcall EMU7800DirectX_PresentFrame(bool showSnow, int dx, int dy)
{
    if (!IsActivated)
        return 2;

    ShowSnow = showSnow;
    HRESULT hr = PresentFrame(dx, dy);
    return IsDeviceLost ? S_FALSE : hr;
}

typedef struct {
    int FrameCount;
    int SegmentCount;
    int Hz;
    int FrameWidth;
    int FrameHeight;
    HWND DeviceWindow;
} stat_s;

extern "C" __declspec(dllexport) stat_s *__stdcall EMU7800DirectX_GetStatistics()
{
    static stat_s s;
    s.FrameCount = FrameCount;
    s.SegmentCount = SegmentCount;
    s.Hz = D3DDisplayMode.RefreshRate;
    s.FrameWidth = D3DDisplayMode.Width;
    s.FrameHeight = D3DDisplayMode.Height;
    s.DeviceWindow = hMainWindow;
    return &s;
}
