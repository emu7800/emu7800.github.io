// © Mike Murphy

#include "pch.h"
#include "Direct3DContentProvider.h"
#include "Direct3DInterop.h"

using namespace EMU7800::WP8::Interop;

Direct3DContentProvider::Direct3DContentProvider(Direct3DInterop^ controller) : m_controller(controller)
{
}

// IDrawingSurfaceContentProviderNative member
HRESULT Direct3DContentProvider::Connect(_In_ IDrawingSurfaceRuntimeHostNative* host)
{
    HRESULT hr = m_controller->Connect(host);
    return hr;
}

// IDrawingSurfaceContentProviderNative member
void Direct3DContentProvider::Disconnect()
{
    m_controller->Disconnect();
}

// IDrawingSurfaceContentProviderNative member
HRESULT Direct3DContentProvider::PrepareResources(_In_ const LARGE_INTEGER* presentTargetTime, _Out_ BOOL* contentDirty)
{
    HRESULT hr = m_controller->PrepareResources(presentTargetTime, contentDirty);
    return hr;
}

// IDrawingSurfaceContentProviderNative member
HRESULT Direct3DContentProvider::GetTexture(_In_ const DrawingSurfaceSizeF* size, _Out_ IDrawingSurfaceSynchronizedTextureNative** synchronizedTexture, _Out_ DrawingSurfaceRectF* textureSubRectangle)
{
    HRESULT hr = m_controller->GetTexture(size, synchronizedTexture, textureSubRectangle);
    return hr;
}