// © Mike Murphy

#include "pch.h"
#include "DXUtils.h"
#include "D2DStructs.h"
#include "StaticBitmap.h"

using namespace EMU7800::D2D::Interop;

StaticBitmap::StaticBitmap(ID2D1DeviceContext* pD2DContext, IWICImagingFactory* pWICFactory, const Array<uint8>^ data) :
    m_hr(0)
{
    if (!pD2DContext || !pWICFactory || !data)
    {
        m_hr = E_POINTER;
        goto LExit;
    }

    IWICStream *pStream = NULL;
    IWICBitmapDecoder *pDecoder = NULL;
    IWICBitmapFrameDecode *pSource = NULL;
    IWICFormatConverter *pConverter = NULL;

    m_hr = pWICFactory->CreateStream(&pStream);
    if FAILED(m_hr)
        goto LExit;

    m_hr = pStream->InitializeFromMemory(data->Data, data->Length);
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
            WICBitmapPaletteTypeMedianCut
            );
    if FAILED(m_hr)
        goto LExit;

    m_hr = pD2DContext->CreateBitmapFromWicBitmap(pConverter, NULL, &m_pBitmap);

LExit:
    DX::SafeRelease(pStream);
    DX::SafeRelease(pDecoder);
    DX::SafeRelease(pSource);
    DX::SafeRelease(pConverter);
}

StaticBitmap::~StaticBitmap()
{
}
