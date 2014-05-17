#include "pch.h"
#include "D2DStructs.h"
#include "DynamicBitmap.h"
#include "StaticBitmap.h"
#include "TextFormat.h"
#include "TextLayout.h"
#include "GraphicsDevice.h"

using namespace Windows::UI::Core;
using namespace Windows::Foundation;
using namespace Microsoft::WRL;
using namespace Windows::Graphics::Display;
using namespace D2D1;

using namespace EMU7800::D2D::Interop;

void GraphicsDevice::BeginDraw()
{
    m_d2dContext->BeginDraw();
    m_d2dContext->SetTransform(D2D1::Matrix3x2F::Identity());
    m_d2dContext->Clear(D2D1::ColorF(D2D1::ColorF::Black));
}

int GraphicsDevice::EndDraw()
{
    HRESULT hr = m_d2dContext->EndDraw();
    return hr;
}

DynamicBitmap^ GraphicsDevice::CreateDynamicBitmap(EMU7800::D2D::Interop::SizeU size)
{
    auto bitmap = ref new DynamicBitmap(m_d2dContext.Get(), size);
    return bitmap;
}

StaticBitmap^ GraphicsDevice::CreateStaticBitmap(const Array<uint8>^ data)
{
    auto bitmap = ref new StaticBitmap(m_d2dContext.Get(), m_wicFactory.Get(), data);
    return bitmap;
}

TextFormat^ GraphicsDevice::CreateTextFormat(String^ fontFamilyName, FLOAT fontSize)
{
    auto textFormat = ref new TextFormat(
        m_pDWriteFactory.Get(),
        fontFamilyName,
        DWRITE_FONT_WEIGHT_NORMAL,
        DWRITE_FONT_STYLE_NORMAL,
        DWRITE_FONT_STRETCH_NORMAL,
        fontSize
        );
    return textFormat;
}

TextFormat^ GraphicsDevice::CreateTextFormat(String^ fontFamilyName, int fontWeight, int fontStyle, int fontStretch, FLOAT fontSize)
{
    auto textFormat = ref new TextFormat(
        m_pDWriteFactory.Get(),
        fontFamilyName,
        fontWeight,
        fontStyle,
        fontStretch,
        fontSize
        );
    return textFormat;
}

TextLayout^ GraphicsDevice::CreateTextLayout(String^ fontFamilyName, FLOAT fontSize, String ^text, FLOAT width, FLOAT height)
{
    auto textLayout = this->CreateTextLayout(
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
    auto textLayout= ref new TextLayout(
        m_pDWriteFactory.Get(),
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
    if (!m_d2dContext)
        return;

    D2D1_POINT_2F dp0;
    dp0.x = p0.X;
    dp0.y = p0.Y;

    D2D1_POINT_2F dp1;
    dp1.x = p1.X;
    dp1.y = p1.Y;

    m_d2dContext->DrawLine(
        dp0,
        dp1,
        m_solidColorBrushes[(int)brush].Get(),
        strokeWidth
        );
}

void GraphicsDevice::DrawRectangle(EMU7800::D2D::Interop::RectF rect, float strokeWidth, D2DSolidColorBrush brush)
{
    if (!m_d2dContext)
        return;

    D2D_RECT_F drect;
    drect.left = rect.Left;
    drect.top = rect.Top;
    drect.right = rect.Right;
    drect.bottom = rect.Bottom;

    m_d2dContext->DrawRectangle(
        drect,
        m_solidColorBrushes[(int)brush].Get(),
        strokeWidth
        );
}

void GraphicsDevice::FillRectangle(EMU7800::D2D::Interop::RectF rect, D2DSolidColorBrush brush)
{
    if (!m_d2dContext)
        return;

    D2D_RECT_F drect;
    drect.left = rect.Left;
    drect.top = rect.Top;
    drect.right = rect.Right;
    drect.bottom = rect.Bottom;

    m_d2dContext->FillRectangle(
        drect,
        m_solidColorBrushes[(int)brush].Get()
        );
}

void GraphicsDevice::DrawEllipse(EMU7800::D2D::Interop::RectF rect, float strokeWidth, D2DSolidColorBrush brush)
{
    if (!m_d2dContext)
        return;

    D2D1_ELLIPSE ellipse;
    ellipse.radiusX = (rect.Right - rect.Left) / 2;
    ellipse.radiusY = (rect.Bottom - rect.Top) / 2;
    ellipse.point.x = rect.Left + ellipse.radiusX;
    ellipse.point.y = rect.Top + ellipse.radiusY;

    m_d2dContext->DrawEllipse(
        ellipse,
        m_solidColorBrushes[(int)brush].Get(),
        strokeWidth
        );
}

void GraphicsDevice::FillEllipse(EMU7800::D2D::Interop::RectF rect, D2DSolidColorBrush brush)
{
    if (!m_d2dContext)
        return;

    D2D1_ELLIPSE ellipse;
    ellipse.radiusX = (rect.Right - rect.Left) / 2;
    ellipse.radiusY = (rect.Bottom - rect.Top) / 2;
    ellipse.point.x = rect.Left + ellipse.radiusX;
    ellipse.point.y = rect.Top + ellipse.radiusY;

    m_d2dContext->FillEllipse(
        ellipse,
        m_solidColorBrushes[(int)brush].Get()
        );
}

void GraphicsDevice::DrawText(TextFormat^ textFormat, String^ text, EMU7800::D2D::Interop::RectF rect, D2DSolidColorBrush brush)
{
    if (!m_d2dContext || !textFormat || !text || !textFormat->DWriteTextFormat || textFormat->HR)
        return;

    D2D1_DRAW_TEXT_OPTIONS options = D2D1_DRAW_TEXT_OPTIONS::D2D1_DRAW_TEXT_OPTIONS_CLIP;

    D2D_RECT_F drect;
    drect.left = rect.Left;
    drect.top = rect.Top;
    drect.right = rect.Right;
    drect.bottom = rect.Bottom;

    m_d2dContext->DrawText(
        text->Data(),
        text->Length(),
        textFormat->DWriteTextFormat,
        drect,
        m_solidColorBrushes[(int)brush].Get(),
        options
        );
}

void GraphicsDevice::DrawText(TextLayout^ textLayout, PointF location, D2DSolidColorBrush brush)
{
    if (!m_d2dContext || !textLayout || !textLayout->DWriteTextLayout || textLayout->HR)
        return;

    D2D1_POINT_2F origin;
    origin.x = location.X;
    origin.y = location.Y;

    D2D1_DRAW_TEXT_OPTIONS options = D2D1_DRAW_TEXT_OPTIONS::D2D1_DRAW_TEXT_OPTIONS_CLIP;

    m_d2dContext->DrawTextLayout(
        origin,
        textLayout->DWriteTextLayout,
        m_solidColorBrushes[(int)brush].Get(),
        options
    );
}

void GraphicsDevice::DrawBitmap(DynamicBitmap^ bitmap, EMU7800::D2D::Interop::RectF rect, D2DBitmapInterpolationMode interpolationMode)
{
    if (!m_d2dContext || !bitmap || !bitmap->D2DBitmap || bitmap->HR)
        return;

    D2D_RECT_F drect;
    drect.left = rect.Left;
    drect.top = rect.Top;
    drect.right = rect.Right;
    drect.bottom = rect.Bottom;

    m_d2dContext->DrawBitmap(
        bitmap->D2DBitmap,
        drect,
        1.0f,
        (D2D1_BITMAP_INTERPOLATION_MODE)interpolationMode
        );
}

void GraphicsDevice::DrawBitmap(StaticBitmap^ bitmap, EMU7800::D2D::Interop::RectF rect)
{
    if (!m_d2dContext || !bitmap || !bitmap->D2DBitmap || bitmap->HR)
        return;

    D2D_RECT_F drect;
    drect.left = rect.Left;
    drect.top = rect.Top;
    drect.right = rect.Right;
    drect.bottom = rect.Bottom;

    m_d2dContext->DrawBitmap(
        bitmap->D2DBitmap,
        drect
        );
}

void GraphicsDevice::SetAntiAliasMode(D2DAntiAliasMode antiAliasMode)
{
    if (!m_d2dContext)
        return;

    m_d2dContext->SetAntialiasMode((D2D1_ANTIALIAS_MODE)antiAliasMode);
}

void GraphicsDevice::PushAxisAlignedClip(EMU7800::D2D::Interop::RectF rect, D2DAntiAliasMode antiAliasMode)
{
    if (!m_d2dContext)
        return;

    D2D_RECT_F drect;
    drect.left = rect.Left;
    drect.top = rect.Top;
    drect.right = rect.Right;
    drect.bottom = rect.Bottom;

    m_d2dContext->PushAxisAlignedClip(drect, (D2D1_ANTIALIAS_MODE)antiAliasMode);
}

void GraphicsDevice::PopAxisAlignedClip()
{
    if (!m_d2dContext)
        return;

    m_d2dContext->PopAxisAlignedClip();
}

void GraphicsDevice::Initialize(CoreWindow^ window, float dpi)
{
    m_window = window;
    CreateDeviceIndependentResources();
    CreateDeviceResources();
    SetDpi(dpi);
    IsDeviceResourcesRefreshed = true;
}

void GraphicsDevice::HandleDeviceLost()
{
    float dpi = m_dpi;
    m_dpi = -1.0f;
    m_windowBounds.Width = 0;
    m_windowBounds.Height = 0;
    m_swapChain = nullptr;
    for (int i = 0; i < sizeof(m_solidColorBrushes)/sizeof(m_solidColorBrushes[0]); i++)
        m_solidColorBrushes[i] = nullptr;

    CreateDeviceResources();
    SetDpi(dpi);
    IsDeviceResourcesRefreshed = true;
}

void GraphicsDevice::CreateDeviceIndependentResources()
{
    D2D1_FACTORY_OPTIONS options;
    ZeroMemory(&options, sizeof(D2D1_FACTORY_OPTIONS));

#if defined(_DEBUG)
    options.debugLevel = D2D1_DEBUG_LEVEL_INFORMATION;
#endif

    DX::ThrowIfFailed(
        D2D1CreateFactory(
            D2D1_FACTORY_TYPE_SINGLE_THREADED,
            __uuidof(ID2D1Factory1),
            &options,
            &m_d2dFactory
            )
        );

    DX::ThrowIfFailed(
        DWriteCreateFactory(
            DWRITE_FACTORY_TYPE_SHARED,
            __uuidof(IDWriteFactory),
            &m_pDWriteFactory
            )
        );

    DX::ThrowIfFailed(
        CoCreateInstance(
            CLSID_WICImagingFactory,
            nullptr,
            CLSCTX_INPROC_SERVER,
            IID_PPV_ARGS(&m_wicFactory)
            )
        );
}

void GraphicsDevice::CreateDeviceResources()
{
    // Required for compatibility with Direct2D
    UINT creationFlags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;

#if defined(_DEBUG)
    creationFlags |= D3D11_CREATE_DEVICE_DEBUG;
#endif

    D3D_FEATURE_LEVEL featureLevels[] =
    {
        D3D_FEATURE_LEVEL_11_1,
        D3D_FEATURE_LEVEL_11_0,
        D3D_FEATURE_LEVEL_10_1,
        D3D_FEATURE_LEVEL_10_0,
        D3D_FEATURE_LEVEL_9_3,
        D3D_FEATURE_LEVEL_9_2,
        D3D_FEATURE_LEVEL_9_1
    };

    ComPtr<ID3D11Device> device;
    ComPtr<ID3D11DeviceContext> context;
    DX::ThrowIfFailed(
        D3D11CreateDevice(
            nullptr,                    // Specify nullptr to use the default adapter
            D3D_DRIVER_TYPE_HARDWARE,
            0,
            creationFlags,
            featureLevels,
            ARRAYSIZE(featureLevels),
            D3D11_SDK_VERSION,          // Always set this to D3D11_SDK_VERSION for Metro style apps
            &device,
            &m_featureLevel,            // Returns feature level of device created
            &context
            )
        );

    DX::ThrowIfFailed(
        device.As(&m_d3dDevice)
        );

    DX::ThrowIfFailed(
        context.As(&m_d3dContext)
        );

    DX::ThrowIfFailed(
        m_d3dDevice.As(&m_dxgiDevice)
        );

    DX::ThrowIfFailed(
        m_d2dFactory->CreateDevice(m_dxgiDevice.Get(), &m_d2dDevice)
        );

    DX::ThrowIfFailed(
        m_d2dDevice->CreateDeviceContext(
            D2D1_DEVICE_CONTEXT_OPTIONS_NONE,
            &m_d2dContext
            )
        );

    CreateSolidColorBrush(D2DSolidColorBrush::Black, D2D1::ColorF::Black);
    CreateSolidColorBrush(D2DSolidColorBrush::Red, D2D1::ColorF(D2D1::ColorF::Red));
    CreateSolidColorBrush(D2DSolidColorBrush::Orange, D2D1::ColorF(0xFB9151));
    CreateSolidColorBrush(D2DSolidColorBrush::Yellow, D2D1::ColorF(D2D1::ColorF::Yellow));
    CreateSolidColorBrush(D2DSolidColorBrush::Green, D2D1::ColorF(D2D1::ColorF::Green));
    CreateSolidColorBrush(D2DSolidColorBrush::Blue, D2D1::ColorF(D2D1::ColorF::Blue));
    CreateSolidColorBrush(D2DSolidColorBrush::Gray, D2D1::ColorF(D2D1::ColorF::Gray));
    CreateSolidColorBrush(D2DSolidColorBrush::White, D2D1::ColorF::White);
}

HRESULT GraphicsDevice::CreateSolidColorBrush(D2DSolidColorBrush brush, D2D1::ColorF color)
{
    if (!m_d2dContext)
        return E_INVALIDARG;

    HRESULT hr = m_d2dContext->CreateSolidColorBrush(color, &m_solidColorBrushes[(int)brush]);
    return hr;
}

void GraphicsDevice::SetDpi(float dpi)
{
    if (dpi != m_dpi)
    {
        m_dpi = dpi;
        m_d2dContext->SetDpi(m_dpi, m_dpi);

        // Often a DPI change implies a window size change.
        // In some cases Windows will issue both a size changed event and a DPI changed event.
        // In this case, the resulting bounds will not change, and the window resize code will only be executed once.
        UpdateForWindowSizeChange();
    }
}

void GraphicsDevice::UpdateForWindowSizeChange()
{
    if (m_window->Bounds.Width  != m_windowBounds.Width
    ||  m_window->Bounds.Height != m_windowBounds.Height)
    {
        m_d2dContext->SetTarget(nullptr);
        m_d2dTargetBitmap = nullptr;
        m_d3dRenderTargetView = nullptr;
        m_windowSizeChangeInProgress = true;
        CreateWindowSizeDependentResources();
    }
}

void GraphicsDevice::CreateWindowSizeDependentResources()
{
    m_windowBounds = m_window->Bounds;

    if (m_swapChain != nullptr)
    {
        // Resize pre-existing swap chain.
        HRESULT hr = m_swapChain->ResizeBuffers(2, 0, 0, DXGI_FORMAT_B8G8R8A8_UNORM, 0);
        if (hr == DXGI_ERROR_DEVICE_REMOVED || hr == DXGI_ERROR_DEVICE_RESET)
        {
            HandleDeviceLost();
            return;
        }
        DX::ThrowIfFailed(hr);
    }
    else
    {
        // Otherwise, create a new one using the same adapter as the existing Direct3D device.
        DXGI_SWAP_CHAIN_DESC1 swapChainDesc = {0};
        swapChainDesc.Width              = 0;                                // Use automatic sizing.
        swapChainDesc.Height             = 0;
        swapChainDesc.Format             = DXGI_FORMAT_B8G8R8A8_UNORM;       // This is the most common swap chain format.
        swapChainDesc.Stereo             = false;
        swapChainDesc.SampleDesc.Count   = 1;                                // Don't use multi-sampling.
        swapChainDesc.SampleDesc.Quality = 0;
        swapChainDesc.BufferUsage        = DXGI_USAGE_RENDER_TARGET_OUTPUT;
        swapChainDesc.BufferCount        = 2;                                // Use double-buffering to minimize latency.
        swapChainDesc.Scaling            = DXGI_SCALING_NONE;
        swapChainDesc.SwapEffect         = DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL; // All Metro style apps must use this SwapEffect.
        swapChainDesc.Flags              = 0;

        ComPtr<IDXGIDevice1> dxgiDevice;
        DX::ThrowIfFailed(
            m_d3dDevice.As(&dxgiDevice)
            );

        ComPtr<IDXGIAdapter> dxgiAdapter;
        DX::ThrowIfFailed(
            dxgiDevice->GetAdapter(&dxgiAdapter)
            );

        ComPtr<IDXGIFactory2> dxgiFactory;
        DX::ThrowIfFailed(
            dxgiAdapter->GetParent(IID_PPV_ARGS(&dxgiFactory))
            );

        CoreWindow^ window = m_window.Get();
        DX::ThrowIfFailed(
            dxgiFactory->CreateSwapChainForCoreWindow(
                m_d3dDevice.Get(),
                reinterpret_cast<IUnknown*>(window),
                &swapChainDesc,
                nullptr,
                &m_swapChain
                )
            );

        // Ensure that DXGI does not queue more than one frame at a time.
        // This both reduces latency and ensures that the application will only render after each VSync,
        // minimizing power consumption.
        DX::ThrowIfFailed(
            dxgiDevice->SetMaximumFrameLatency(1)
            );
    }

    // Create a Direct3D render target view of the swap chain back buffer.
    ComPtr<ID3D11Texture2D> backBuffer;
    DX::ThrowIfFailed(
        m_swapChain->GetBuffer(0, IID_PPV_ARGS(&backBuffer))
        );

    DX::ThrowIfFailed(
        m_d3dDevice->CreateRenderTargetView(
            backBuffer.Get(),
            nullptr,
            &m_d3dRenderTargetView
            )
        );

    D3D11_TEXTURE2D_DESC backBufferDesc = {0};
    backBuffer->GetDesc(&backBufferDesc);
    m_renderTargetSize.Width  = static_cast<float>(backBufferDesc.Width);
    m_renderTargetSize.Height = static_cast<float>(backBufferDesc.Height);

    // Set the 3D rendering viewport to target the entire window.
    CD3D11_VIEWPORT viewport(
        0.0f,
        0.0f,
        static_cast<float>(backBufferDesc.Width),
        static_cast<float>(backBufferDesc.Height)
        );

    m_d3dContext->RSSetViewports(1, &viewport);

    // Create a Direct2D target bitmap associated with the swap chain back buffer and set it as the current target.
    D2D1_BITMAP_PROPERTIES1 bitmapProperties =
        BitmapProperties1(
            D2D1_BITMAP_OPTIONS_TARGET | D2D1_BITMAP_OPTIONS_CANNOT_DRAW,
            PixelFormat(DXGI_FORMAT_B8G8R8A8_UNORM, D2D1_ALPHA_MODE_PREMULTIPLIED),
            m_dpi,
            m_dpi
            );

    ComPtr<IDXGISurface> dxgiBackBuffer;
    DX::ThrowIfFailed(
        m_swapChain->GetBuffer(0, IID_PPV_ARGS(&dxgiBackBuffer))
        );

    DX::ThrowIfFailed(
        m_d2dContext->CreateBitmapFromDxgiSurface(
            dxgiBackBuffer.Get(),
            &bitmapProperties,
            &m_d2dTargetBitmap
            )
        );

    m_d2dContext->SetTarget(m_d2dTargetBitmap.Get());

    // Grayscale text anti-aliasing is recommended for all Metro style apps.
    m_d2dContext->SetTextAntialiasMode(D2D1_TEXT_ANTIALIAS_MODE_GRAYSCALE);
}

void GraphicsDevice::Present()
{
    // Not using dirty or scroll rects.
    DXGI_PRESENT_PARAMETERS parameters = {0};
    parameters.DirtyRectsCount = 0;
    parameters.pDirtyRects = nullptr;
    parameters.pScrollRect = nullptr;
    parameters.pScrollOffset = nullptr;

    // The first argument instructs DXGI to block until VSync, putting the application to sleep until the next VSync.
    HRESULT hr = m_swapChain->Present1(1, 0, &parameters);

    // Discard the contents of the render target (since not using dirty or scroll rects.)
    m_d3dContext->DiscardView(m_d3dRenderTargetView.Get());

    if (hr == DXGI_ERROR_DEVICE_REMOVED)
    {
        HandleDeviceLost();
    }
    else
    {
        DX::ThrowIfFailed(hr);
    }

    if (m_windowSizeChangeInProgress)
    {
        // A window size change has been initiated and the app has just completed presenting
        // the first frame with the new size. Notify the resize manager so we can short
        // circuit any resize animation and prevent unnecessary delays.
        //CoreWindowResizeManager::GetForCurrentView()->NotifyLayoutCompleted();

        m_windowSizeChangeInProgress = false;
    }
}

// Ensure the D3D Device is available for rendering.
void GraphicsDevice::ValidateDevice()
{
    // The D3D Device is no longer valid if the default adapter changes or if the device has been removed.
    // First, get the information for the adapter related to the current device.
    ComPtr<IDXGIDevice1> dxgiDevice;
    ComPtr<IDXGIAdapter> deviceAdapter;
    DXGI_ADAPTER_DESC deviceDesc;
    DX::ThrowIfFailed(m_d3dDevice.As(&dxgiDevice));
    DX::ThrowIfFailed(dxgiDevice->GetAdapter(&deviceAdapter));
    DX::ThrowIfFailed(deviceAdapter->GetDesc(&deviceDesc));

    // Next, get the information for the default adapter.
    ComPtr<IDXGIFactory2> dxgiFactory;
    ComPtr<IDXGIAdapter1> currentAdapter;
    DXGI_ADAPTER_DESC currentDesc;
    DX::ThrowIfFailed(CreateDXGIFactory1(IID_PPV_ARGS(&dxgiFactory)));
    DX::ThrowIfFailed(dxgiFactory->EnumAdapters1(0, &currentAdapter));
    DX::ThrowIfFailed(currentAdapter->GetDesc(&currentDesc));

    // If the adapter LUIDs don't match, or if the device reports that it has been removed, a new D3D device must be created.
    if ((deviceDesc.AdapterLuid.LowPart  != currentDesc.AdapterLuid.LowPart)
    ||  (deviceDesc.AdapterLuid.HighPart != currentDesc.AdapterLuid.HighPart)
    ||  FAILED(m_d3dDevice->GetDeviceRemovedReason()))
    {
        dxgiDevice = nullptr;
        deviceAdapter = nullptr;
        HandleDeviceLost();
    }
}

void GraphicsDevice::Trim()
{
    m_dxgiDevice->Trim();
}

GraphicsDevice::GraphicsDevice()
{
    m_windowSizeChangeInProgress = false;
    m_dpi = -1.0f;
    IsDeviceResourcesRefreshed = false;
}

GraphicsDevice::~GraphicsDevice()
{
}
