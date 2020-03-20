// © Mike Murphy

#include "stdafx.h"
#include "D2DStructs.h"
#include "DynamicBitmap.h"
#include "StaticBitmap.h"
#include "TextFormat.h"
#include "TextLayout.h"
#include "GraphicsDevice.h"

using namespace msclr::interop;
using namespace EMU7800::D2D::Interop;

HRESULT GraphicsDevice::CreateDeviceResources()
{
    if (!m_hWnd)
        return E_INVALIDARG;

    DiscardDeviceResources();

    RECT rc;
    GetClientRect(m_hWnd, &rc);
    D2D1_SIZE_U pixelSize = D2D1::SizeU(rc.right - rc.left, rc.bottom - rc.top);
    D2D1_RENDER_TARGET_PROPERTIES renderTargetProperties = D2D1::RenderTargetProperties();
    D2D1_HWND_RENDER_TARGET_PROPERTIES hWndRenderTargetProperties = D2D1::HwndRenderTargetProperties(m_hWnd, pixelSize);

    ID2D1HwndRenderTarget *pRenderTarget = NULL;
    HRESULT hr = m_pD2D1Factory->CreateHwndRenderTarget(renderTargetProperties, hWndRenderTargetProperties, &pRenderTarget);
    if SUCCEEDED(hr)
        m_pRenderTarget = pRenderTarget;
    else
        return hr;

    CreateSolidColorBrush(D2DSolidColorBrush::Black, D2D1::ColorF::Black);
    CreateSolidColorBrush(D2DSolidColorBrush::Red, D2D1::ColorF(D2D1::ColorF::Red));
    CreateSolidColorBrush(D2DSolidColorBrush::Orange, D2D1::ColorF(0xFB9151));
    CreateSolidColorBrush(D2DSolidColorBrush::Yellow, D2D1::ColorF(D2D1::ColorF::Yellow));
    CreateSolidColorBrush(D2DSolidColorBrush::Green, D2D1::ColorF(D2D1::ColorF::Green));
    CreateSolidColorBrush(D2DSolidColorBrush::Blue, D2D1::ColorF(D2D1::ColorF::Blue));
    CreateSolidColorBrush(D2DSolidColorBrush::Gray, D2D1::ColorF(D2D1::ColorF::Gray));
    CreateSolidColorBrush(D2DSolidColorBrush::White, D2D1::ColorF::White);

    IsDeviceResourcesRefreshed = true;

    return S_OK;
}

HRESULT GraphicsDevice::CreateSolidColorBrush(D2DSolidColorBrush brush, D2D1::ColorF color)
{
    if (!m_pRenderTarget)
        return E_INVALIDARG;

    ID2D1SolidColorBrush *pBrush;
    HRESULT hr = m_pRenderTarget->CreateSolidColorBrush(color, &pBrush);
    if SUCCEEDED(hr)
        m_ppSolidColorBrushes[(int)brush] = pBrush;
    return hr;
}

void GraphicsDevice::DiscardDeviceResources()
{
    if (m_ppSolidColorBrushes)
    {
        for (int i = 0; i < nSolidColorBrushes; i++)
        {
            if (m_ppSolidColorBrushes[i])
            {
                m_ppSolidColorBrushes[i]->Release();
                m_ppSolidColorBrushes[i] = NULL;
            }
        }
    }

    if (m_pRenderTarget)
    {
        m_pRenderTarget->Release();
        m_pRenderTarget = NULL;
    }
}

void GraphicsDevice::BeginDraw()
{
    if (!m_pRenderTarget)
        return;

    m_pRenderTarget->BeginDraw();
    m_pRenderTarget->SetTransform(D2D1::Matrix3x2F::Identity());
    m_pRenderTarget->Clear(D2D1::ColorF(D2D1::ColorF::Black));
}

HRESULT GraphicsDevice::EndDraw()
{
    if (!m_pRenderTarget)
    {
        CreateDeviceResources();
        return S_OK;
    }

    HRESULT hr = m_pRenderTarget->EndDraw();
    if (hr == D2DERR_RECREATE_TARGET)
        CreateDeviceResources();

    return hr;
}

DynamicBitmap^ GraphicsDevice::CreateDynamicBitmap(SizeU size)
{
    DynamicBitmap ^bitmap = gcnew DynamicBitmap(m_pRenderTarget, size);
    return bitmap;
}

StaticBitmap^ GraphicsDevice::CreateStaticBitmap(array<byte> ^data)
{
    StaticBitmap ^bitmap = gcnew StaticBitmap(m_pRenderTarget, m_pWICFactory, data);
    return bitmap;
}

TextFormat^ GraphicsDevice::CreateTextFormat(String ^fontFamilyName, FLOAT fontSize)
{
    TextFormat^ textFormat = this->CreateTextFormat(
        fontFamilyName,
        DWRITE_FONT_WEIGHT_NORMAL,
        DWRITE_FONT_STYLE_NORMAL,
        DWRITE_FONT_STRETCH_NORMAL,
        fontSize);
    return textFormat;
}

TextFormat^ GraphicsDevice::CreateTextFormat(String ^fontFamilyName, int fontWeight, int fontStyle, int fontStretch, FLOAT fontSize)
{
    TextFormat^ textFormat = gcnew TextFormat(
        m_pDWriteFactory,
        fontFamilyName,
        fontWeight,
        fontStyle,
        fontStretch,
        fontSize);
    return textFormat;
}

TextLayout^ GraphicsDevice::CreateTextLayout(String^ fontFamilyName, FLOAT fontSize, String ^text, FLOAT width, FLOAT height)
{
    TextLayout^ textLayout = this->CreateTextLayout(
        fontFamilyName,
        DWRITE_FONT_WEIGHT_NORMAL,
        DWRITE_FONT_STYLE_NORMAL,
        DWRITE_FONT_STRETCH_NORMAL,
        fontSize,
        text,
        width,
        height
        );
    return textLayout;
}

TextLayout^ GraphicsDevice::CreateTextLayout(String^ fontFamilyName, int fontWeight, int fontStyle, int fontStretch, FLOAT fontSize, String ^text, FLOAT width, FLOAT height)
{
    TextLayout^ textLayout= gcnew TextLayout(
        m_pDWriteFactory,
        fontFamilyName,
        fontWeight,
        fontStyle,
        fontStretch,
        fontSize,
        text,
        width,
        height
        );
    return textLayout;
}

void GraphicsDevice::DrawLine(PointF p0, PointF p1, float strokeWidth, D2DSolidColorBrush brush)
{
    if (!m_pRenderTarget)
        return;

    D2D1_POINT_2F dp0;
    D2D1_POINT_2F dp1;
    p0.CopyTo(&dp0);
    p1.CopyTo(&dp1);

    m_pRenderTarget->DrawLine(
        dp0,
        dp1,
        m_ppSolidColorBrushes[(int)brush],
        strokeWidth
        );
}

void GraphicsDevice::DrawRectangle(RectF rect, float strokeWidth, D2DSolidColorBrush brush)
{
    if (!m_pRenderTarget)
        return;

    D2D_RECT_F drect;
    rect.CopyTo(&drect);

    m_pRenderTarget->DrawRectangle(
        drect,
        m_ppSolidColorBrushes[(int)brush],
        strokeWidth
        );
}

void GraphicsDevice::FillRectangle(RectF rect, D2DSolidColorBrush brush)
{
    if (!m_pRenderTarget)
        return;

    D2D_RECT_F drect;
    rect.CopyTo(&drect);

    m_pRenderTarget->FillRectangle(
        drect,
        m_ppSolidColorBrushes[(int)brush]
        );
}

void GraphicsDevice::DrawEllipse(RectF rect, float strokeWidth, D2DSolidColorBrush brush)
{
    if (!m_pRenderTarget)
        return;

    D2D1_ELLIPSE ellipse;
    ellipse.radiusX = (rect.Right - rect.Left) / 2;
    ellipse.radiusY = (rect.Bottom - rect.Top) / 2;
    ellipse.point.x = rect.Left + ellipse.radiusX;
    ellipse.point.y = rect.Top + ellipse.radiusY;

    m_pRenderTarget->DrawEllipse(
        ellipse,
        m_ppSolidColorBrushes[(int)brush],
        strokeWidth
        );
}

void GraphicsDevice::FillEllipse(RectF rect, D2DSolidColorBrush brush)
{
    if (!m_pRenderTarget)
        return;

    D2D1_ELLIPSE ellipse;
    ellipse.radiusX = (rect.Right - rect.Left) / 2;
    ellipse.radiusY = (rect.Bottom - rect.Top) / 2;
    ellipse.point.x = rect.Left + ellipse.radiusX;
    ellipse.point.y = rect.Top + ellipse.radiusY;

    m_pRenderTarget->FillEllipse(
        ellipse,
        m_ppSolidColorBrushes[(int)brush]
        );
}

void GraphicsDevice::DrawText(TextFormat ^textFormat, String ^text, RectF rect, D2DSolidColorBrush brush)
{
    if (!m_pRenderTarget || !textFormat || !text || !textFormat->DWriteTextFormat || textFormat->HR)
        return;

    marshal_context context;
    const WCHAR *pMarshaledText  = context.marshal_as<const WCHAR*>(text);

    D2D1_DRAW_TEXT_OPTIONS options = D2D1_DRAW_TEXT_OPTIONS::D2D1_DRAW_TEXT_OPTIONS_CLIP;

    D2D_RECT_F drect;
    rect.CopyTo(&drect);
    m_pRenderTarget->DrawText(
        pMarshaledText,
        text->Length,
        textFormat->DWriteTextFormat,
        drect,
        m_ppSolidColorBrushes[(int)brush],
        options
        );
}

void GraphicsDevice::DrawText(TextLayout^ textLayout, PointF location, D2DSolidColorBrush brush)
{
    if (!m_pRenderTarget || !textLayout || !textLayout->DWriteTextLayout || textLayout->HR)
        return;

    D2D1_POINT_2F origin;
    origin.x = location.X;
    origin.y = location.Y;

    D2D1_DRAW_TEXT_OPTIONS options = D2D1_DRAW_TEXT_OPTIONS::D2D1_DRAW_TEXT_OPTIONS_CLIP;

    m_pRenderTarget->DrawTextLayout(
        origin,
        textLayout->DWriteTextLayout,
        m_ppSolidColorBrushes[(int)brush],
        options
    );
}

void GraphicsDevice::DrawBitmap(DynamicBitmap ^bitmap, RectF rect, D2DBitmapInterpolationMode interpolationMode)
{
    if (!m_pRenderTarget || !bitmap || !bitmap->D2DBitmap || bitmap->HR)
        return;

    D2D_RECT_F drect;
    rect.CopyTo(&drect);
    m_pRenderTarget->DrawBitmap(bitmap->D2DBitmap, drect, 1.0f, (D2D1_BITMAP_INTERPOLATION_MODE)interpolationMode);
}

void GraphicsDevice::DrawBitmap(StaticBitmap ^bitmap, RectF rect)
{
    if (!m_pRenderTarget || !bitmap || !bitmap->D2DBitmap || bitmap->HR)
        return;

    D2D_RECT_F drect;
    rect.CopyTo(&drect);
    m_pRenderTarget->DrawBitmap(bitmap->D2DBitmap, drect);
}

void GraphicsDevice::SetAntiAliasMode(D2DAntiAliasMode antialiasMode)
{
    if (!m_pRenderTarget)
        return;

    m_pRenderTarget->SetAntialiasMode((D2D1_ANTIALIAS_MODE)antialiasMode);
}

void GraphicsDevice::PushAxisAlignedClip(RectF rect, D2DAntiAliasMode antialiasMode)
{
    if (!m_pRenderTarget)
        return;

    D2D_RECT_F drect;
    rect.CopyTo(&drect);
    m_pRenderTarget->PushAxisAlignedClip(drect, (D2D1_ANTIALIAS_MODE)antialiasMode);
}

void GraphicsDevice::PopAxisAlignedClip()
{
    if (!m_pRenderTarget)
        return;

    m_pRenderTarget->PopAxisAlignedClip();
}

HRESULT GraphicsDevice::Initialize()
{
    HRESULT hr;
    ID2D1Factory *pD2D1Factory = NULL;
    hr = D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, &pD2D1Factory);
    if SUCCEEDED(hr)
        m_pD2D1Factory = pD2D1Factory;
    else
        return hr;

    IDWriteFactory *pDWriteFactory = NULL;
    hr = DWriteCreateFactory(
        DWRITE_FACTORY_TYPE_SHARED,
        __uuidof(pDWriteFactory),
        reinterpret_cast<IUnknown**>(&pDWriteFactory)
        );
    if SUCCEEDED(hr)
        m_pDWriteFactory = pDWriteFactory;
    else
        return hr;

    IWICImagingFactory *pWICFactory = NULL;
    hr = CoCreateInstance(
        CLSID_WICImagingFactory,
        NULL,
        CLSCTX_INPROC_SERVER,
        IID_PPV_ARGS(&pWICFactory)
        );
    if SUCCEEDED(hr)
        m_pWICFactory = pWICFactory;
    else
        return hr;

    return S_OK;
}

void GraphicsDevice::AttachHwnd(HWND hWnd)
{
    if (!hWnd)
        return;

    m_hWnd = hWnd;
    CreateDeviceResources();
}

void GraphicsDevice::Resize(SizeU size)
{
    if (!m_pRenderTarget)
        return;

    // Note: This method can fail, but it's okay to ignore the error here -- it will be repeated on the next call to EndDraw.
    D2D1_SIZE_U dsize;
    size.CopyTo(&dsize);
    m_pRenderTarget->Resize(dsize);
}

GraphicsDevice::GraphicsDevice() : m_hWnd(0),
                                   m_pD2D1Factory(0),
                                   m_pDWriteFactory(0),
                                   m_pWICFactory(0),
                                   m_pRenderTarget(0),
                                   m_ppSolidColorBrushes(0)
{
    IsDeviceResourcesRefreshed = false;

    HANDLE hHeap = GetProcessHeap();
    m_ppSolidColorBrushes = reinterpret_cast<ID2D1SolidColorBrush**>(
        HeapAlloc(hHeap, 0, nSolidColorBrushes * sizeof(&m_ppSolidColorBrushes))
        );

    if (m_ppSolidColorBrushes)
        for (int i=0; i < nSolidColorBrushes; i++)
            m_ppSolidColorBrushes[i] = NULL;
}

GraphicsDevice::~GraphicsDevice()
{
    this->!GraphicsDevice();
}

GraphicsDevice::!GraphicsDevice()
{
    DiscardDeviceResources();

    if (m_ppSolidColorBrushes)
    {
        HANDLE hHeap = GetProcessHeap();
        HeapFree(hHeap, 0, m_ppSolidColorBrushes);
        m_ppSolidColorBrushes = NULL;
    }
    if (m_pWICFactory)
    {
        m_pWICFactory->Release();
        m_pWICFactory = NULL;
    }
    if (m_pDWriteFactory)
    {
        m_pDWriteFactory->Release();
        m_pDWriteFactory = NULL;
    }
    if (m_pD2D1Factory)
    {
        m_pD2D1Factory->Release();
        m_pD2D1Factory = NULL;
    }
}
