// © Mike Murphy

#include "stdafx.h"
#include "TextFormat.h"

using namespace msclr::interop;
using namespace EMU7800::D2D::Interop;

TextFormat::TextFormat(IDWriteFactory* pDWriteFactory, String^ fontFamilyName, int fontWeight, int fontStyle, int fontStretch, FLOAT fontSize) :
    m_pTextFormat(0),
    m_hr(0)
{
    if (!pDWriteFactory)
    {
        m_hr = E_POINTER;
        return;
    }

    marshal_context context;
    const WCHAR *pMarshaledText  = context.marshal_as<const WCHAR*>(fontFamilyName);

    IDWriteTextFormat* pTextFormat = NULL;
    m_hr = pDWriteFactory->CreateTextFormat(
        pMarshaledText,
        NULL,
        (DWRITE_FONT_WEIGHT)fontWeight,
        (DWRITE_FONT_STYLE)fontStyle,
        (DWRITE_FONT_STRETCH)fontStretch,
        fontSize,
        L"",
        &pTextFormat);
    if SUCCEEDED(m_hr)
        m_pTextFormat = pTextFormat;
}

int TextFormat::SetTextAlignment(DWriteTextAlignment textAlignment)
{
    if FAILED(m_hr)
        return m_hr;

    HRESULT hr = m_pTextFormat->SetTextAlignment((DWRITE_TEXT_ALIGNMENT)textAlignment);
    return hr;
}

int TextFormat::SetParagraphAlignment(DWriteParaAlignment paragraphAlignment)
{
    if FAILED(m_hr)
        return m_hr;

    HRESULT hr = m_pTextFormat->SetParagraphAlignment((DWRITE_PARAGRAPH_ALIGNMENT)paragraphAlignment);
    return hr;
}

TextFormat::~TextFormat()
{
    this->!TextFormat();
}

TextFormat::!TextFormat()
{
    if (m_pTextFormat)
    {
        m_pTextFormat->Release();
        m_pTextFormat = NULL;
    }
}