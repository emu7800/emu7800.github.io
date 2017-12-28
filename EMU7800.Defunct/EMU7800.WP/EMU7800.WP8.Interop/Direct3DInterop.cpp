// © Mike Murphy

#include "pch.h"
#include "Direct3DInterop.h"
#include "Direct3DContentProvider.h"
#include "DirectXHelper.h"

using namespace DirectX;
using namespace Microsoft::WRL;
using namespace Platform;
using namespace Windows::Foundation;
using namespace Windows::UI::Core;
using namespace Windows::Phone::Graphics::Interop;
using namespace Windows::Phone::Input::Interop;

namespace EMU7800 { namespace WP8 { namespace Interop {

Direct3DInterop::Direct3DInterop()
{
    RenderWidth = 0;
    RenderHeight = 0;
    ZeroMemory(m_dynamicTextureData, sizeof(m_dynamicTextureData));
}

IDrawingSurfaceContentProvider^ Direct3DInterop::CreateContentProvider()
{
    ComPtr<Direct3DContentProvider> provider = Make<Direct3DContentProvider>(this);
    return reinterpret_cast<IDrawingSurfaceContentProvider^>(provider.Get());
}

void Direct3DInterop::SetManipulationHost(DrawingSurfaceManipulationHost^ manipulationHost)
{
    manipulationHost->PointerPressed +=
        ref new TypedEventHandler<DrawingSurfaceManipulationHost^, PointerEventArgs^>(this, &Direct3DInterop::OnPointerPressed);

    manipulationHost->PointerMoved +=
        ref new TypedEventHandler<DrawingSurfaceManipulationHost^, PointerEventArgs^>(this, &Direct3DInterop::OnPointerMoved);

    manipulationHost->PointerReleased +=
        ref new TypedEventHandler<DrawingSurfaceManipulationHost^, PointerEventArgs^>(this, &Direct3DInterop::OnPointerReleased);
}

void Direct3DInterop::OnPointerPressed(DrawingSurfaceManipulationHost^ sender, PointerEventArgs^ args)
{
    auto evt = PointerPressed;
    if (evt)
        PointerPressed(args);

    auto cpt = args->CurrentPoint;
    if (cpt->Position.X < (RenderWidth >> 1))
    {
        m_dpadStartPtId = cpt->PointerId;
        m_dpadStartPt   = cpt->Position;
        m_dpadCurrentPt = cpt->Position;
        IsDPadLeft  = false;
        IsDPadUp    = false;
        IsDPadRight = false;
        IsDPadDown  = false;
    }
    else
    {
        if (cpt->Position.X > (ActualWidth - (ActualWidth / 10)))
            IsFire1 = true;
        else
            IsFire2 = true;
    }
}

void Direct3DInterop::OnPointerMoved(DrawingSurfaceManipulationHost^ sender, PointerEventArgs^ args)
{
    auto evt = PointerMoved;
    if (evt)
    {
        evt(args);
    }

    auto cpt = args->CurrentPoint;
    if (cpt->PointerId == m_dpadStartPtId)
    {
        m_dpadCurrentPt = cpt->Position;
        float x = m_dpadCurrentPt.X - m_dpadStartPt.X;
        float y = m_dpadCurrentPt.Y - m_dpadStartPt.Y;
        float d = x * x + y * y;
        if (d > 128.0)
        {
            IsDPadUp    = (y < 0.0f) && abs(-y/ x) > 0.414213562373f;
            IsDPadDown  = (y > 0.0f) && abs( y/ x) > 0.414213562373f;
            IsDPadLeft  = (x < 0.0f) && abs( y/-x) < 2.414213562373f;
            IsDPadRight = (x > 0.0f) && abs( y/ x) < 2.414213562373f;
            m_dpadStartPt = m_dpadCurrentPt;
        }
    }
}

void Direct3DInterop::OnPointerReleased(DrawingSurfaceManipulationHost^ sender, PointerEventArgs^ args)
{
    auto evt = PointerReleased;
    if (evt)
    {
        evt(args);
    }

    auto cpt = args->CurrentPoint;
    if (cpt->PointerId == m_dpadStartPtId || cpt->Position.X < (ActualWidth / 2))
    {
        IsDPadLeft = false;
        IsDPadUp = false;
        IsDPadRight = false;
        IsDPadDown = false;
    }
    else
    {
        IsFire1 = false;
        IsFire2 = false;
    }
}

void Direct3DInterop::SubmitFrameBuffer(const Platform::Array<uint32>^ data)
{
    if (!data || data->Length != FRAME_BUFFER_WIDTH * FRAME_BUFFER_HEIGHT)
        return;
    CopyMemory(m_dynamicTextureData, data->Data, sizeof(m_dynamicTextureData));
}

void Direct3DInterop::CreateDeviceResources()
{
    // This flag adds support for surfaces with a different color channel ordering
    // than the API default. It is required for compatibility with Direct2D.
    UINT creationFlags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;

#if defined(_DEBUG)
    // If the project is in a debug build, enable debugging via SDK Layers with this flag.
    creationFlags |= D3D11_CREATE_DEVICE_DEBUG;
#endif

    // This array defines the set of DirectX hardware feature levels this app will support.
    // Note the ordering should be preserved.
    // Don't forget to declare your application's minimum required feature level in its
    // description.  All applications are assumed to support 9.3 unless otherwise stated.
    D3D_FEATURE_LEVEL featureLevels[] =
    {
        D3D_FEATURE_LEVEL_9_3
    };

    // Create the Direct3D 11 API device object and a corresponding context.
    ComPtr<ID3D11Device> device;
    ComPtr<ID3D11DeviceContext> context;
    DX::ThrowIfFailed(
        D3D11CreateDevice(
            nullptr,                // Specify nullptr to use the default adapter.
            D3D_DRIVER_TYPE_HARDWARE,
            nullptr,
            creationFlags,          // Set set debug and Direct2D compatibility flags.
            featureLevels,          // List of feature levels this app can support.
            ARRAYSIZE(featureLevels),
            D3D11_SDK_VERSION,      // Always set this to D3D11_SDK_VERSION.
            &device,                // Returns the Direct3D device created.
            &m_featureLevel,        // Returns feature level of device created.
            &context                // Returns the device immediate context.
            )
        );

    // Get the Direct3D 11.1 API device and context interfaces.
    DX::ThrowIfFailed(
        device.As(&m_d3dDevice)
        );

    DX::ThrowIfFailed(
        context.As(&m_d3dContext)
        );

    m_spriteBatch.reset(new SpriteBatch(m_d3dContext.Get()));
}

void Direct3DInterop::CreateWindowSizeDependentResources()
{
    // Create a descriptor for the render target buffer.
    CD3D11_TEXTURE2D_DESC renderTargetDesc(
        DXGI_FORMAT_B8G8R8A8_UNORM,
        RenderWidth,
        RenderHeight,
        1,
        1,
        D3D11_BIND_RENDER_TARGET | D3D11_BIND_SHADER_RESOURCE
        );
    renderTargetDesc.MiscFlags = D3D11_RESOURCE_MISC_SHARED_KEYEDMUTEX | D3D11_RESOURCE_MISC_SHARED_NTHANDLE;

    // Allocate a 2-D surface as the render target buffer.
    DX::ThrowIfFailed(
        m_d3dDevice->CreateTexture2D(
            &renderTargetDesc,
            nullptr,
            &m_renderTarget
            )
        );

    DX::ThrowIfFailed(
        m_d3dDevice->CreateRenderTargetView(
            m_renderTarget.Get(),
            nullptr,
            &m_renderTargetView
            )
        );

    CD3D11_VIEWPORT viewport(
        0.0f,
        0.0f,
        (FLOAT)RenderWidth,
        (FLOAT)RenderHeight
        );

    m_d3dContext->RSSetViewports(1, &viewport);

    D3D11_TEXTURE2D_DESC desc;
    ZeroMemory(&desc, sizeof(desc));
    desc.Width            = FRAME_BUFFER_WIDTH;
    desc.Height           = FRAME_BUFFER_HEIGHT;
    desc.MipLevels        = desc.ArraySize = 1;
    desc.Format           = DXGI_FORMAT_R8G8B8A8_UNORM;
    desc.SampleDesc.Count = 1;
    desc.Usage            = D3D11_USAGE_DYNAMIC;
    desc.BindFlags        = D3D11_BIND_SHADER_RESOURCE;
    desc.CPUAccessFlags   = D3D11_CPU_ACCESS_WRITE;
    DX::ThrowIfFailed(
        m_d3dDevice->CreateTexture2D(
            &desc,
            NULL,
            &m_dynamicTexture
            )
        );

    D3D11_SHADER_RESOURCE_VIEW_DESC srvDesc;
    ZeroMemory(&srvDesc, sizeof(srvDesc));
    srvDesc.Format                    = desc.Format;
    srvDesc.ViewDimension             = D3D11_SRV_DIMENSION_TEXTURE2D;
    srvDesc.Texture2D.MostDetailedMip = 0;
    srvDesc.Texture2D.MipLevels       = 1;
    DX::ThrowIfFailed(
        m_d3dDevice->CreateShaderResourceView(
            m_dynamicTexture.Get(),
            &srvDesc,
            &m_dynamicTextureView
            )
        );
}

void Direct3DInterop::UpdateForRenderResolutionChange()
{
    ID3D11RenderTargetView* nullViews[] = {nullptr};
    m_d3dContext->OMSetRenderTargets(ARRAYSIZE(nullViews), nullViews, nullptr);
    m_renderTarget = nullptr;
    m_renderTargetView = nullptr;
    m_dynamicTextureView = nullptr;
    m_dynamicTexture = nullptr;
    m_d3dContext->Flush();

    CreateWindowSizeDependentResources();
}

void Direct3DInterop::Render()
{
    m_d3dContext->OMSetRenderTargets(
        1,
        m_renderTargetView.GetAddressOf(),
        NULL
        );

    D3D11_MAPPED_SUBRESOURCE mappedResource;
    HRESULT hr = m_d3dContext->Map(m_dynamicTexture.Get(), 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);

    if SUCCEEDED(hr)
    {
        UCHAR* src   = (UCHAR*)m_dynamicTextureData;
        UCHAR* dst   = (UCHAR*)mappedResource.pData;
        int srcWidth = FRAME_BUFFER_WIDTH * sizeof(uint32);

        // NOTE: this copy is not synchronized with the copy in SubmitBuffer() - there could be some visual tearing as a result

        for (int i = 0; i < FRAME_BUFFER_HEIGHT; i++)
        {
            CopyMemory(dst, src, srcWidth);
            src += srcWidth;
            dst += mappedResource.RowPitch;
        }

        m_d3dContext->Unmap(m_dynamicTexture.Get(), 0);
    }

    RECT destRect;
    destRect.left   = DestRectLeft;
    destRect.top    = DestRectTop;
    destRect.right  = DestRectRight;
    destRect.bottom = DestRectBottom;

    m_spriteBatch->Begin();
    m_spriteBatch->Draw(m_dynamicTextureView.Get(), destRect, Colors::White);
    m_spriteBatch->End();
}

// Interface With Direct3DContentProvider
HRESULT Direct3DInterop::Connect(_In_ IDrawingSurfaceRuntimeHostNative* host)
{
    m_host = host;

    CreateDeviceResources();
    UpdateForRenderResolutionChange();

    return S_OK;
}

void Direct3DInterop::Disconnect()
{
    m_host = nullptr;
    m_synchronizedTexture = nullptr;
}

HRESULT Direct3DInterop::PrepareResources(_In_ const LARGE_INTEGER* presentTargetTime, _Out_ BOOL* contentDirty)
{
    *contentDirty = true;
    return S_OK;
}

HRESULT Direct3DInterop::GetTexture(_In_ const DrawingSurfaceSizeF* size, _Inout_ IDrawingSurfaceSynchronizedTextureNative** synchronizedTexture, _Inout_ DrawingSurfaceRectF* textureSubRectangle)
{
    HRESULT hr = S_OK;

    if (!m_synchronizedTexture)
    {
        hr = m_host->CreateSynchronizedTexture(m_renderTarget.Get(), &m_synchronizedTexture);
        if FAILED(hr)
            return hr;
    }

    hr = m_synchronizedTexture->BeginDraw();
    if FAILED(hr)
        return hr;

    Render();

    m_host->RequestAdditionalFrame();

    m_synchronizedTexture->EndDraw();

    if (synchronizedTexture)
    {
        m_synchronizedTexture.CopyTo(synchronizedTexture);
    }
    if (textureSubRectangle && size)
    {
        textureSubRectangle->left   = 0.0f;
        textureSubRectangle->top    = 0.0f;
        textureSubRectangle->right  = static_cast<FLOAT>(size->width);
        textureSubRectangle->bottom = static_cast<FLOAT>(size->height);
    }

    return hr;
}

}}}
