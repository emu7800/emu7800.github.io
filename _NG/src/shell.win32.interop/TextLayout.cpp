// © Mike Murphy

#include "stdafx.h"
#include "TextLayout.h"

using namespace msclr::interop;
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

    marshal_context context;
    const WCHAR *pMarshaledFontFamilyName  = context.marshal_as<const WCHAR*>(fontFamilyName);
    const WCHAR *pMarshaledText  = context.marshal_as<const WCHAR*>(text);

    IDWriteTextFormat* pTextFormat = NULL;
    m_hr = pDWriteFactory->CreateTextFormat(
        pMarshaledText,
        NULL,
        (DWRITE_FONT_WEIGHT)fontWeight,
        (DWRITE_FONT_STYLE)fontStyle,
        (DWRITE_FONT_STRETCH)fontStretch,
        fontSize,
        L"",
        &pTextFormat
        );
    if SUCCEEDED(m_hr)
        m_pTextFormat = pTextFormat;
    else
        return;

    IDWriteTextLayout* pTextLayout = NULL;
    m_hr = pDWriteFactory->CreateTextLayout(
        pMarshaledText,
        text->Length,
        m_pTextFormat,
        width,
        height,
        &pTextLayout
        );
    if SUCCEEDED(m_hr)
    {
        m_pTextLayout = pTextLayout;
        RefreshMetrics();
    }
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

TextLayout::~TextLayout()
{
    this->!TextLayout();
}

TextLayout::!TextLayout()
{
    if (m_pTextLayout)
    {
        m_pTextLayout->Release();
        m_pTextLayout = NULL;
    }
    if (m_pTextFormat)
    {
        m_pTextFormat->Release();
        m_pTextFormat = NULL;
    }
}