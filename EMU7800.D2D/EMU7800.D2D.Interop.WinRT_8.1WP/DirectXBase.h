#pragma once

#include "DXUtils.h"

using namespace Microsoft::WRL;
using namespace Platform;
using namespace Windows::Foundation;
using namespace Windows::UI::Core;

namespace EMU7800 { namespace D2D { namespace Interop {

public ref class DirectXBase sealed
{
public:
    DirectXBase();

    void Initialize(CoreWindow^ window, float dpi);
    void HandleDeviceLost();
    void CreateDeviceIndependentResources();
    void CreateDeviceResources();
    void SetDpi(float dpi);
    void UpdateForWindowSizeChange();
    void CreateWindowSizeDependentResources();
    void Render();
    void Present();
    void ValidateDevice();

protected private:
    Agile<CoreWindow>               m_window;

    // DirectWrite & Windows Imaging Component Objects.
    ComPtr<IDWriteFactory1>         m_dwriteFactory;
    ComPtr<IWICImagingFactory2>     m_wicFactory;

    // DirectX Core Objects. Required for 2D and 3D.
    ComPtr<ID3D11Device1>           m_d3dDevice;
    ComPtr<ID3D11DeviceContext1>    m_d3dContext;
    ComPtr<IDXGISwapChain1>         m_swapChain;
    ComPtr<ID3D11RenderTargetView>  m_d3dRenderTargetView;

    // Direct2D Rendering Objects. Required for 2D.
    ComPtr<ID2D1Factory1>           m_d2dFactory;
    ComPtr<ID2D1Device>             m_d2dDevice;
    ComPtr<ID2D1DeviceContext>      m_d2dContext;
    ComPtr<ID2D1Bitmap1>            m_d2dTargetBitmap;

    // Direct3D Rendering Objects. Required for 3D.
    ComPtr<ID3D11DepthStencilView>  m_d3dDepthStencilView;

    // Cached renderer properties.
    D3D_FEATURE_LEVEL               m_featureLevel;
    Windows::Foundation::Size       m_renderTargetSize;
    Windows::Foundation::Rect       m_windowBounds;
    float                           m_dpi;
    bool                            m_windowSizeChangeInProgress;
};

}}}
