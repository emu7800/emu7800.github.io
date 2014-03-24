// © Mike Murphy

#include "pch.h"
#include "D2DStructs.h"
#include "DynamicBitmap.h"

using namespace EMU7800::D2D::Interop;

DynamicBitmap::DynamicBitmap(ID2D1DeviceContext* pD2DContext, SizeU size) :
    m_hr(0),
    m_expectedDataLength(0),
    m_expectedPitch(0)
{
    if (!pD2DContext)
    {
        m_hr = E_POINTER;
        return;
    }

    m_expectedDataLength = (size.Width * size.Height) << 2;
    m_expectedPitch = size.Width << 2;

    D2D_SIZE_U bsize;
    bsize.width = size.Width;
    bsize.height = size.Height;
    D2D1_PIXEL_FORMAT pixelFormat = D2D1::PixelFormat(DXGI_FORMAT_B8G8R8A8_UNORM, D2D1_ALPHA_MODE_IGNORE);
    D2D1_BITMAP_PROPERTIES props = D2D1::BitmapProperties(pixelFormat);
    m_hr = pD2DContext->CreateBitmap(bsize, props, &m_pBitmap);
}

int DynamicBitmap::CopyFromMemory(const Array<uint8>^ data)
{
    if FAILED(m_hr)
        return m_hr;
    if (data->Length != m_expectedDataLength)
        return E_INVALIDARG;

    HRESULT hr = m_pBitmap->CopyFromMemory(NULL, data->Data, m_expectedPitch);
    return (int)hr;
}

DynamicBitmap::~DynamicBitmap()
{
}
