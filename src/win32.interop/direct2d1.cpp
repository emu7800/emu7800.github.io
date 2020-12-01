// © Mike Murphy

#include "pch.h"

#define NumberOfSolidColorBrushes 8

enum class D2DSolidColorBrush { Black, Red, Orange, Yellow, Green, Blue, Gray, White };

HWND                   g_hWnd = NULL;
ID2D1Factory*          g_pD2D1Factory = NULL;
IDWriteFactory*        g_pDWriteFactory = NULL;
IWICImagingFactory*    g_pWICFactory = NULL;
ID2D1HwndRenderTarget* g_pRenderTarget = NULL;
ID2D1SolidColorBrush*  g_pSolidColorBrushes[NumberOfSolidColorBrushes] = { 0 };

HRESULT CreateSolidColorBrush(D2DSolidColorBrush brush, D2D1_COLOR_F color)
{
    ID2D1SolidColorBrush* pBrush;
    HRESULT hr = g_pRenderTarget->CreateSolidColorBrush(color, &pBrush);
    g_pSolidColorBrushes[(int)brush] = SUCCEEDED(hr) ? pBrush : NULL;
    return hr;
}

void DiscardDeviceResources()
{
    if (g_pSolidColorBrushes)
    {
        for (int i = 0; i < NumberOfSolidColorBrushes; i++)
        {
            if (g_pSolidColorBrushes[i])
            {
                g_pSolidColorBrushes[i]->Release();
                g_pSolidColorBrushes[i] = NULL;
            }
        }
    }

    if (g_pRenderTarget)
    {
        g_pRenderTarget->Release();
        g_pRenderTarget = NULL;
    }
}

HRESULT CreateDeviceResources()
{
    DiscardDeviceResources();

    if (!g_hWnd)
        return E_INVALIDARG;

    RECT rc;
    GetClientRect(g_hWnd, &rc);
    D2D1_SIZE_U pixelSize = D2D1::SizeU(rc.right - rc.left, rc.bottom - rc.top);
    D2D1_RENDER_TARGET_PROPERTIES renderTargetProperties = D2D1::RenderTargetProperties();
    D2D1_HWND_RENDER_TARGET_PROPERTIES hWndRenderTargetProperties = D2D1::HwndRenderTargetProperties(g_hWnd, pixelSize);

    HRESULT hr = g_pD2D1Factory->CreateHwndRenderTarget(renderTargetProperties, hWndRenderTargetProperties, &g_pRenderTarget);
    if FAILED(hr)
        return hr;

    CreateSolidColorBrush(D2DSolidColorBrush::Black,  D2D1::ColorF(D2D1::ColorF::Black));
    CreateSolidColorBrush(D2DSolidColorBrush::Red,    D2D1::ColorF(D2D1::ColorF::Red));
    CreateSolidColorBrush(D2DSolidColorBrush::Orange, D2D1::ColorF(0xFB9151));
    CreateSolidColorBrush(D2DSolidColorBrush::Yellow, D2D1::ColorF(D2D1::ColorF::Yellow));
    CreateSolidColorBrush(D2DSolidColorBrush::Green,  D2D1::ColorF(D2D1::ColorF::Green));
    CreateSolidColorBrush(D2DSolidColorBrush::Blue,   D2D1::ColorF(D2D1::ColorF::Blue));
    CreateSolidColorBrush(D2DSolidColorBrush::Gray,   D2D1::ColorF(D2D1::ColorF::Gray));
    CreateSolidColorBrush(D2DSolidColorBrush::White,  D2D1::ColorF(D2D1::ColorF::White));

    return S_OK;
}

extern "C" _declspec(dllexport) HRESULT __stdcall Direct2D_Initialize(HWND hWnd)
{
    HRESULT hr;

    g_hWnd = hWnd;
    if (!g_hWnd)
    {
        hr = E_INVALIDARG;
        goto LExit;
    }

    hr = D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, &g_pD2D1Factory);
    if FAILED(hr)
        goto LExit;

    hr = DWriteCreateFactory(DWRITE_FACTORY_TYPE_SHARED, __uuidof(g_pDWriteFactory), reinterpret_cast<IUnknown**>(&g_pDWriteFactory));
    if FAILED(hr)
        goto LExit;

    hr = CoCreateInstance(CLSID_WICImagingFactory, NULL, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&g_pWICFactory));
    if FAILED(hr)
        goto LExit;

    return CreateDeviceResources();

LExit:
    if (g_pWICFactory)
    {
        g_pWICFactory->Release();
        g_pWICFactory = NULL;
    }
    if (g_pDWriteFactory)
    {
        g_pDWriteFactory->Release();
        g_pDWriteFactory = NULL;
    }
    if (g_pD2D1Factory)
    {
        g_pD2D1Factory->Release();
        g_pD2D1Factory = NULL;
    }
    return hr;
}

extern "C" _declspec(dllexport) void __stdcall Direct2D_BeginDraw()
{
    if (g_pRenderTarget)
    {
        g_pRenderTarget->BeginDraw();
        g_pRenderTarget->SetTransform(D2D1::Matrix3x2F::Identity());
        g_pRenderTarget->Clear(D2D1::ColorF(D2D1::ColorF::Black));
    }
}

extern "C" _declspec(dllexport) HRESULT __stdcall Direct2D_EndDraw()
{
    HRESULT hr = S_OK;
    if (g_pRenderTarget)
    {
        hr = g_pRenderTarget->EndDraw();
        if (hr == D2DERR_RECREATE_TARGET)
            CreateDeviceResources();
    }
    else
    {
        CreateDeviceResources();
    }
    return hr;
}

extern "C" _declspec(dllexport) void __stdcall Direct2D_Resize(D2D1_SIZE_U dsize)
{
    if (g_pRenderTarget)
    {
        g_pRenderTarget->Resize(dsize);
    }
}

extern "C" _declspec(dllexport) void __stdcall Direct2D_SetAntiAliasMode(D2D1_ANTIALIAS_MODE antialiasMode)
{
    if (g_pRenderTarget)
    {
        g_pRenderTarget->SetAntialiasMode(antialiasMode);
    }
}

extern "C" _declspec(dllexport) void __stdcall Direct2D_PushAxisAlignedClip(D2D_RECT_F drect, D2D1_ANTIALIAS_MODE antialiasMode)
{
    if (g_pRenderTarget)
    {
        g_pRenderTarget->PushAxisAlignedClip(drect, antialiasMode);
    }
}

extern "C" _declspec(dllexport) void __stdcall Direct2D_PopAxisAlignedClip()
{
    if (g_pRenderTarget)
    {
        g_pRenderTarget->PopAxisAlignedClip();
    }
}

extern "C" _declspec(dllexport) void __stdcall Direct2D_DrawLine(D2D_POINT_2F dp0, D2D_POINT_2F dp1, float strokeWidth, D2DSolidColorBrush brush)
{
    if (g_pRenderTarget)
    {
        g_pRenderTarget->DrawLine(dp0, dp1, g_pSolidColorBrushes[(int)brush], strokeWidth);
    }
}

extern "C" _declspec(dllexport) void __stdcall Direct2D_DrawRectangle(D2D_RECT_F drect, float strokeWidth, D2DSolidColorBrush brush)
{
    if (g_pRenderTarget)
    {
        g_pRenderTarget->DrawRectangle(drect, g_pSolidColorBrushes[(int)brush], strokeWidth);
    }
}

extern "C" _declspec(dllexport) void __stdcall Direct2D_FillRectangle(D2D_RECT_F drect, D2DSolidColorBrush brush)
{
    if (g_pRenderTarget)
    {
        g_pRenderTarget->FillRectangle(drect, g_pSolidColorBrushes[(int)brush]);
    }
}

extern "C" _declspec(dllexport) void __stdcall Direct2D_DrawEllipse(D2D_RECT_F drect, float strokeWidth, D2DSolidColorBrush brush)
{
    if (g_pRenderTarget)
    {
        D2D1_ELLIPSE ellipse = { 0 };
        ellipse.radiusX = (drect.right - drect.left) / 2;
        ellipse.radiusY = (drect.bottom - drect.top) / 2;
        ellipse.point.x = drect.left + ellipse.radiusX;
        ellipse.point.y = drect.top + ellipse.radiusY;
        g_pRenderTarget->DrawEllipse(ellipse, g_pSolidColorBrushes[(int)brush], strokeWidth);
    }
}

extern "C" _declspec(dllexport) void __stdcall Direct2D_FillEllipse(D2D_RECT_F drect, D2DSolidColorBrush brush)
{
    if (g_pRenderTarget)
    {
        D2D1_ELLIPSE ellipse = { 0 };
        ellipse.radiusX = (drect.right - drect.left) / 2;
        ellipse.radiusY = (drect.bottom - drect.top) / 2;
        ellipse.point.x = drect.left + ellipse.radiusX;
        ellipse.point.y = drect.top + ellipse.radiusY;
        g_pRenderTarget->FillEllipse(ellipse, g_pSolidColorBrushes[(int)brush]);
    }
}

extern "C" _declspec(dllexport) HRESULT __stdcall Direct2D_CreateTextFormat(WCHAR* fontFamilyName, int fontWeight, int fontStyle, int fontStretch, FLOAT fontSize, IDWriteTextFormat** ppTextFormat)
{
    if (g_pDWriteFactory)
    {
        return g_pDWriteFactory->CreateTextFormat(
            fontFamilyName,
            NULL,
            (DWRITE_FONT_WEIGHT)fontWeight,
            (DWRITE_FONT_STYLE)fontStyle,
            (DWRITE_FONT_STRETCH)fontStretch,
            fontSize,
            L"",
            ppTextFormat);
    }
    else
    {
        return E_INVALIDARG;
    }
}

extern "C" _declspec(dllexport) void __stdcall Direct2D_DrawTextFormat(IDWriteTextFormat* pTextFormat, WCHAR* text, D2D_RECT_F drect, D2DSolidColorBrush brush)
{
    if (g_pRenderTarget && pTextFormat)
    {
        D2D1_DRAW_TEXT_OPTIONS options = D2D1_DRAW_TEXT_OPTIONS::D2D1_DRAW_TEXT_OPTIONS_CLIP;
        g_pRenderTarget->DrawText(text, (UINT32)wcslen(text), pTextFormat, drect, g_pSolidColorBrushes[(int)brush], options);
    }
}

extern "C" _declspec(dllexport) void __stdcall Direct2D_SetTextAlignmentForTextFormat(IDWriteTextFormat* pTextFormat, DWRITE_TEXT_ALIGNMENT textAlignment)
{
    if (pTextFormat)
    {
        pTextFormat->SetTextAlignment(textAlignment);
    }
}

extern "C" _declspec(dllexport) void __stdcall Direct2D_SetParagraphAlignmentForTextFormat(IDWriteTextFormat* pTextFormat, DWRITE_PARAGRAPH_ALIGNMENT paragraphAlignment)
{
    if (pTextFormat)
    {
        pTextFormat->SetParagraphAlignment(paragraphAlignment);
    }
}

extern "C" _declspec(dllexport) void __stdcall Direct2D_ReleaseTextFormat(IDWriteTextFormat* pTextFormat)
{
    if (pTextFormat)
    {
        pTextFormat->Release();
    }
}

extern "C" _declspec(dllexport) HRESULT __stdcall Direct2D_CreateTextLayout(WCHAR* fontFamilyName, int fontWeight, int fontStyle, int fontStretch, FLOAT fontSize, WCHAR* text, FLOAT width, FLOAT height, IDWriteTextFormat** ppTextFormat, IDWriteTextLayout** ppTextLayout)
{
    if (g_pDWriteFactory)
    {
        HRESULT hr = g_pDWriteFactory->CreateTextFormat(
            fontFamilyName,
            NULL,
            (DWRITE_FONT_WEIGHT)fontWeight,
            (DWRITE_FONT_STYLE)fontStyle,
            (DWRITE_FONT_STRETCH)fontStretch,
            fontSize,
            L"",
            ppTextFormat
            );
        if SUCCEEDED(hr)
        {
            hr = g_pDWriteFactory->CreateTextLayout(
                text,
                (UINT32)wcslen(text),
                *ppTextFormat,
                width,
                height,
                ppTextLayout
                );
        }
        if FAILED(hr)
        {
            if (ppTextLayout && *ppTextLayout)
            {
                (*ppTextLayout)->Release();
            }
            if (ppTextFormat && *ppTextFormat)
            {
                (*ppTextFormat)->Release();
            }
        }
        return hr;
    }
    else
    {
        return E_INVALIDARG;
    }
}

extern "C" _declspec(dllexport) void __stdcall Direct2D_DrawTextLayout(IDWriteTextLayout* pTextLayout, D2D_POINT_2F location, D2DSolidColorBrush brush)
{
    if (g_pRenderTarget && pTextLayout)
    {
        D2D1_DRAW_TEXT_OPTIONS options = D2D1_DRAW_TEXT_OPTIONS::D2D1_DRAW_TEXT_OPTIONS_CLIP;
        g_pRenderTarget->DrawTextLayout(location, pTextLayout, g_pSolidColorBrushes[(int)brush], options);
    }
}

extern "C" _declspec(dllexport) void __stdcall Direct2D_SetTextAlignmentForTextLayout(IDWriteTextLayout* pTextLayout, DWRITE_TEXT_ALIGNMENT textAlignment)
{
    if (pTextLayout)
    {
        pTextLayout->SetTextAlignment(textAlignment);
    }
}

extern "C" _declspec(dllexport) void __stdcall Direct2D_SetParagraphAlignmentForTextLayout(IDWriteTextLayout* pTextLayout, DWRITE_PARAGRAPH_ALIGNMENT paragraphAlignment)
{
    if (pTextLayout)
    {
        pTextLayout->SetParagraphAlignment(paragraphAlignment);
    }
}

extern "C" _declspec(dllexport) void __stdcall Direct2D_GetMetrics(IDWriteTextLayout* pTextLayout, DWRITE_TEXT_METRICS* pMetrics)
{
    if (pTextLayout && pMetrics)
    {
        pTextLayout->GetMetrics(pMetrics);
    }
}

extern "C" _declspec(dllexport) void __stdcall Direct2D_ReleaseTextLayout(IDWriteTextFormat* pTextFormat, IDWriteTextLayout* pTextLayout)
{
    if (pTextLayout)
    {
        pTextLayout->Release();
    }
    if (pTextFormat)
    {
        pTextFormat->Release();
    }
}

extern "C" _declspec(dllexport) HRESULT __stdcall Direct2D_CreateStaticBitmap(byte* data, int len, ID2D1Bitmap** ppBitmap)
{
    if (!g_pRenderTarget || !g_pWICFactory || !data)
    {
        return E_INVALIDARG;
    }

    IWICStream* pStream = NULL;
    IWICBitmapDecoder* pDecoder = NULL;
    IWICBitmapFrameDecode* pSource = NULL;
    IWICFormatConverter* pConverter = NULL;
    ID2D1Bitmap* pBitmap = NULL;

    HRESULT hr = g_pWICFactory->CreateStream(&pStream);
    if FAILED(hr)
        goto LExit;

    hr = pStream->InitializeFromMemory(data, len);
    if FAILED(hr)
        goto LExit;

    hr = g_pWICFactory->CreateDecoderFromStream(pStream, NULL, WICDecodeMetadataCacheOnLoad, &pDecoder);
    if FAILED(hr)
        goto LExit;

    hr = pDecoder->GetFrame(0, &pSource);
    if FAILED(hr)
        goto LExit;

    hr = g_pWICFactory->CreateFormatConverter(&pConverter);
    if FAILED(hr)
        goto LExit;

    hr = pConverter->Initialize(
        pSource,
        GUID_WICPixelFormat32bppPBGRA,
        WICBitmapDitherTypeNone,
        NULL,
        0.f,
        WICBitmapPaletteTypeMedianCut);
    if FAILED(hr)
        goto LExit;

    hr = g_pRenderTarget->CreateBitmapFromWicBitmap(
        pConverter,
        NULL,
        ppBitmap);

LExit:
    if (pConverter)
    {
        pConverter->Release();
        pConverter = NULL;
    }
    if (pSource)
    {
        pSource->Release();
        pSource = NULL;
    }
    if (pDecoder)
    {
        pDecoder->Release();
        pDecoder = NULL;
    }
    if (pStream)
    {
        pStream->Release();
        pStream = NULL;
    }

    return hr;
}

extern "C" _declspec(dllexport) void __stdcall Direct2D_DrawStaticBitmap(ID2D1Bitmap* pBitmap, D2D_RECT_F drect)
{
    if (g_pRenderTarget && pBitmap)
    {
        g_pRenderTarget->DrawBitmap(pBitmap, drect);
    }
}

extern "C" _declspec(dllexport) void __stdcall Direct2D_ReleaseStaticBitmap(ID2D1Bitmap* pBitmap)
{
    if (pBitmap)
    {
        pBitmap->Release();
    }
}

extern "C" _declspec(dllexport) HRESULT __stdcall Direct2D_CreateDynamicBitmap(D2D_SIZE_U bsize, ID2D1Bitmap** ppBitmap)
{
    if (g_pRenderTarget)
    {
        D2D1_PIXEL_FORMAT pixelFormat = D2D1::PixelFormat(DXGI_FORMAT_B8G8R8A8_UNORM, D2D1_ALPHA_MODE_IGNORE);
        D2D1_BITMAP_PROPERTIES props = D2D1::BitmapProperties(pixelFormat);
        return g_pRenderTarget->CreateBitmap(bsize, props, ppBitmap);
    }
    else
    {
        return E_INVALIDARG;
    }
}

extern "C" _declspec(dllexport) void __stdcall Direct2D_LoadDynamicBitmapFromMemory(ID2D1Bitmap* pBitmap, byte* data, int expectedPitch)
{
    if (pBitmap && data)
    {
        pBitmap->CopyFromMemory(NULL, data, expectedPitch);
    }
}

extern "C" _declspec(dllexport) void __stdcall Direct2D_DrawDynamicBitmap(ID2D1Bitmap* pBitmap, D2D_RECT_F drect, D2D1_BITMAP_INTERPOLATION_MODE interpolationMode)
{
    if (g_pRenderTarget && pBitmap)
    {
        g_pRenderTarget->DrawBitmap(pBitmap, drect, 1.0f, interpolationMode);
    }
}

extern "C" _declspec(dllexport) void __stdcall Direct2D_ReleaseDynamicBitmap(ID2D1Bitmap* pBitmap)
{
    if (pBitmap)
    {
        pBitmap->Release();
    }
}

extern "C" _declspec(dllexport) void __stdcall Direct2D_Shutdown()
{
    DiscardDeviceResources();

    if (g_pWICFactory)
    {
        g_pWICFactory->Release();
        g_pWICFactory = NULL;
    }
    if (g_pDWriteFactory)
    {
        g_pDWriteFactory->Release();
        g_pDWriteFactory = NULL;
    }
    if (g_pD2D1Factory)
    {
        g_pD2D1Factory->Release();
        g_pD2D1Factory = NULL;
    }
    g_hWnd = NULL;
}