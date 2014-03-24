// © Mike Murphy

#include "stdafx.h"
#include "D2DStructs.h"
#include "StaticBitmap.h"

using namespace EMU7800::D2D::Interop;

StaticBitmap::StaticBitmap(ID2D1HwndRenderTarget* pRenderTarget, IWICImagingFactory* pWICFactory, array<byte>^ data) :
    m_pBitmap(0),
    m_hr(0)
{
    if (!pRenderTarget || !pWICFactory || !data)
    {
        m_hr = E_POINTER;
        return;
    }

    pin_ptr<byte> pData = &data[0];

    IWICStream* pStream = NULL;
    IWICBitmapDecoder* pDecoder = NULL;
    IWICBitmapFrameDecode* pSource = NULL;
    IWICFormatConverter* pConverter = NULL;
    ID2D1Bitmap* pBitmap = NULL;

    m_hr = pWICFactory->CreateStream(&pStream);
    if FAILED(m_hr)
        goto LExit;

    m_hr = pStream->InitializeFromMemory(reinterpret_cast<BYTE*>(pData), data->Length);
    if FAILED(m_hr)
        goto LExit;

    m_hr = pWICFactory->CreateDecoderFromStream(pStream, NULL, WICDecodeMetadataCacheOnLoad, &pDecoder);
    if FAILED(m_hr)
        goto LExit;

    m_hr = pDecoder->GetFrame(0, &pSource);
    if FAILED(m_hr)
        goto LExit;

    m_hr = pWICFactory->CreateFormatConverter(&pConverter);
    if FAILED(m_hr)
        goto LExit;

    m_hr = pConverter->Initialize(
            pSource,
            GUID_WICPixelFormat32bppPBGRA,
            WICBitmapDitherTypeNone,
            NULL,
            0.f,
            WICBitmapPaletteTypeMedianCut);
    if FAILED(m_hr)
        goto LExit;

    m_hr = pRenderTarget->CreateBitmapFromWicBitmap(
            pConverter,
            NULL,
            &pBitmap);
    if SUCCEEDED(m_hr)
        m_pBitmap = pBitmap;

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
}

StaticBitmap::~StaticBitmap()
{
    this->!StaticBitmap();
}

StaticBitmap::!StaticBitmap()
{
    if (m_pBitmap)
    {
        m_pBitmap->Release();
        m_pBitmap = NULL;
    }
}