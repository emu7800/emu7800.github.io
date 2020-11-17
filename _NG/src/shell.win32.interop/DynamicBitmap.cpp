// © Mike Murphy

#include "stdafx.h"
#include "D2DStructs.h"
#include "DynamicBitmap.h"

using namespace EMU7800::D2D::Interop;

DynamicBitmap::DynamicBitmap(ID2D1HwndRenderTarget *pRenderTarget, SizeU size) :
    m_pBitmap(0),
    m_hr(0),
    m_expectedDataLength(0),
    m_expectedPitch(0)
{
    if (!pRenderTarget)
    {
        m_hr = E_POINTER;
        return;
    }

    m_expectedDataLength = (size.Width * size.Height) << 2;
    m_expectedPitch = size.Width << 2;

    D2D_SIZE_U bsize;
    size.CopyTo(&bsize);
    D2D1_PIXEL_FORMAT pixelFormat = D2D1::PixelFormat(DXGI_FORMAT_B8G8R8A8_UNORM, D2D1_ALPHA_MODE_IGNORE);
    D2D1_BITMAP_PROPERTIES props = D2D1::BitmapProperties(pixelFormat);
    ID2D1Bitmap *pBitmap = NULL;
    m_hr = pRenderTarget->CreateBitmap(bsize, props, &pBitmap);
    if SUCCEEDED(m_hr)
        m_pBitmap = pBitmap;
}

int DynamicBitmap::CopyFromMemory(array<byte> ^data)
{
    if FAILED(m_hr)
        return m_hr;
    if (data->Length != m_expectedDataLength)
        return E_INVALIDARG;

    pin_ptr<byte> pData = &data[0];
    HRESULT hr = m_pBitmap->CopyFromMemory(NULL, pData, m_expectedPitch);
    return hr;
}

DynamicBitmap::~DynamicBitmap()
{
    this->!DynamicBitmap();
}

DynamicBitmap::!DynamicBitmap()
{
    if (m_pBitmap)
    {
        m_pBitmap->Release();
        m_pBitmap = NULL;
    }
}