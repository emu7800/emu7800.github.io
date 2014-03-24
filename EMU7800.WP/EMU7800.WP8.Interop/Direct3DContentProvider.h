// © Mike Murphy

#pragma once

#include "pch.h"
#include "Direct3DInterop.h"

class Direct3DContentProvider : public Microsoft::WRL::RuntimeClass<
        Microsoft::WRL::RuntimeClassFlags<Microsoft::WRL::WinRtClassicComMix>,
        ABI::Windows::Phone::Graphics::Interop::IDrawingSurfaceContentProvider,
        IDrawingSurfaceContentProviderNative>
{
public:
    Direct3DContentProvider(EMU7800::WP8::Interop::Direct3DInterop^ controller);

    // IDrawingSurfaceContentProviderNative methods
    HRESULT STDMETHODCALLTYPE Connect(_In_ IDrawingSurfaceRuntimeHostNative* host);
    void STDMETHODCALLTYPE Disconnect();
    HRESULT STDMETHODCALLTYPE PrepareResources(_In_ const LARGE_INTEGER* presentTargetTime, _Out_ BOOL* contentDirty);
    HRESULT STDMETHODCALLTYPE GetTexture(_In_ const DrawingSurfaceSizeF* size, _Out_ IDrawingSurfaceSynchronizedTextureNative** synchronizedTexture, _Out_ DrawingSurfaceRectF* textureSubRectangle);

private:
    EMU7800::WP8::Interop::Direct3DInterop^ m_controller;
};