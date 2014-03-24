// © Mike Murphy

#include "pch.h"
#include "TextFormat.h"

using namespace EMU7800::D2D::Interop;

TextFormat::TextFormat(IDWriteFactory* pDWriteFactory, String^ fontFamilyName, int fontWeight, int fontStyle, int fontStretch, FLOAT fontSize) :
    m_hr(0)
{
    if (!pDWriteFactory)
    {
        m_hr = E_POINTER;
        return;
    }

    m_hr = pDWriteFactory->CreateTextFormat(
        fontFamilyName->Data(),
        NULL,
        (DWRITE_FONT_WEIGHT)fontWeight,
        (DWRITE_FONT_STYLE)fontStyle,
        (DWRITE_FONT_STRETCH)fontStretch,
        fontSize,
        L"",
        &m_pTextFormat);
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
