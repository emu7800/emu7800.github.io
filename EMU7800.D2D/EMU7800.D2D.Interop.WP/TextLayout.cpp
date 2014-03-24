// © Mike Murphy

#include "pch.h"
#include "TextLayout.h"

using namespace EMU7800::D2D::Interop;

TextLayout::TextLayout(IDWriteFactory* pDWriteFactory, String^ fontFamilyName, int fontWeight, int fontStyle, int fontStretch, FLOAT fontSize, String ^text, FLOAT width, FLOAT height) :
    m_pTextLayout(0),
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
        &m_pTextFormat
        );
    if FAILED(m_hr)
        return;

    m_hr = pDWriteFactory->CreateTextLayout(
        text->Data(),
        text->Length(),
        m_pTextFormat.Get(),
        width,
        height,
        &m_pTextLayout
        );
    if SUCCEEDED(m_hr)
        RefreshMetrics();
}

void TextLayout::RefreshMetrics()
{
    DWRITE_TEXT_METRICS m = {0};
    m_pTextLayout->GetMetrics(&m);
    m_width = m.width;
    m_height = m.height;
    m_lineCount = m.lineCount;
}

int TextLayout::SetTextAlignment(DWriteTextAlignment textAlignment)
{
    if FAILED(m_hr)
        return m_hr;

    HRESULT hr = m_pTextLayout->SetTextAlignment((DWRITE_TEXT_ALIGNMENT)textAlignment);
    if SUCCEEDED(hr)
        RefreshMetrics();
    return hr;
}

int TextLayout::SetParagraphAlignment(DWriteParaAlignment paragraphAlignment)
{
    if FAILED(m_hr)
        return m_hr;

    HRESULT hr = m_pTextLayout->SetParagraphAlignment((DWRITE_PARAGRAPH_ALIGNMENT)paragraphAlignment);
    if SUCCEEDED(hr)
        RefreshMetrics();
    return hr;
}
