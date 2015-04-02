// © Mike Murphy

#pragma once

#include "pch.h"
#include "SpriteBatch.h"
#include "SpriteFont.h"

namespace EMU7800 { namespace WP8 { namespace Interop {

[Windows::Foundation::Metadata::WebHostHidden]
public delegate void PointerEventHandler(Windows::UI::Core::PointerEventArgs^);

#define FRAME_BUFFER_WIDTH  320
#define FRAME_BUFFER_HEIGHT 240

[Windows::Foundation::Metadata::WebHostHidden]
public ref class Direct3DInterop sealed : public Windows::Phone::Input::Interop::IDrawingSurfaceManipulationHandler
{
public:
    Direct3DInterop();

    // Provide to DrawingSurface Xaml: IDrawingSurfaceContentProvider, IDrawingSurfaceContentProviderNative interfaces
    Windows::Phone::Graphics::Interop::IDrawingSurfaceContentProvider^ CreateContentProvider();

    // IDrawingSurfaceManipulationHandler members
    virtual void SetManipulationHost(Windows::Phone::Input::Interop::DrawingSurfaceManipulationHost^ manipulationHost);

    property PointerEventHandler^ PointerPressed;
    property PointerEventHandler^ PointerMoved;
    property PointerEventHandler^ PointerReleased;

    property bool IsDPadLeft;
    property bool IsDPadUp;
    property bool IsDPadRight;
    property bool IsDPadDown;
    property bool IsFire1;
    property bool IsFire2;

    property double ActualWidth;
    property double ActualHeight;
    property int ScaleFactor;
    property int RenderWidth;
    property int RenderHeight;
    property int DestRectLeft;
    property int DestRectTop;
    property int DestRectRight;
    property int DestRectBottom;

    void SubmitFrameBuffer(const Platform::Array<uint32>^ data);

internal:
    HRESULT STDMETHODCALLTYPE Connect(_In_ IDrawingSurfaceRuntimeHostNative* host);
    void STDMETHODCALLTYPE Disconnect();
    HRESULT STDMETHODCALLTYPE PrepareResources(_In_ const LARGE_INTEGER* presentTargetTime, _Out_ BOOL* contentDirty);
    HRESULT STDMETHODCALLTYPE GetTexture(_In_ const DrawingSurfaceSizeF* size, _Inout_ IDrawingSurfaceSynchronizedTextureNative** synchronizedTexture, _Inout_ DrawingSurfaceRectF* textureSubRectangle);

private:
    void OnPointerPressed(Windows::Phone::Input::Interop::DrawingSurfaceManipulationHost^ sender, Windows::UI::Core::PointerEventArgs^ args);
    void OnPointerMoved(Windows::Phone::Input::Interop::DrawingSurfaceManipulationHost^ sender, Windows::UI::Core::PointerEventArgs^ args);
    void OnPointerReleased(Windows::Phone::Input::Interop::DrawingSurfaceManipulationHost^ sender, Windows::UI::Core::PointerEventArgs^ args);

    void CreateDeviceResources();
    void CreateWindowSizeDependentResources();
    void UpdateForRenderResolutionChange();
    void Render();

    Windows::Foundation::Point m_dpadStartPt, m_dpadCurrentPt;
    uint32 m_dpadStartPtId;

    uint32 m_dynamicTextureData[FRAME_BUFFER_WIDTH * FRAME_BUFFER_HEIGHT];

    Microsoft::WRL::ComPtr<IDrawingSurfaceRuntimeHostNative> m_host;
    Microsoft::WRL::ComPtr<IDrawingSurfaceSynchronizedTextureNative> m_synchronizedTexture;

    D3D_FEATURE_LEVEL m_featureLevel;

    Microsoft::WRL::ComPtr<ID3D11Device1> m_d3dDevice;
    Microsoft::WRL::ComPtr<ID3D11DeviceContext1> m_d3dContext;
    Microsoft::WRL::ComPtr<ID3D11Texture2D> m_renderTarget;
    Microsoft::WRL::ComPtr<ID3D11RenderTargetView> m_renderTargetView;

    std::unique_ptr<DirectX::SpriteBatch> m_spriteBatch;
    Microsoft::WRL::ComPtr<ID3D11ShaderResourceView> m_dynamicTextureView;
    Microsoft::WRL::ComPtr<ID3D11Texture2D> m_dynamicTexture;
};

}}}
