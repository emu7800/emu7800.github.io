// © Mike Murphy

#pragma once

#include "TextFormat.h"
#include "TextLayout.h"

using namespace Microsoft::WRL;
using namespace Platform;

namespace EMU7800 { namespace D2D { namespace Interop {

public ref class TextLayout sealed
{
private:
    ComPtr<IDWriteTextFormat> m_pTextFormat;
    ComPtr<IDWriteTextLayout> m_pTextLayout;
    HRESULT m_hr;
    double m_width, m_height;
    int m_lineCount;

internal:
    property IDWriteTextLayout* DWriteTextLayout { IDWriteTextLayout* get() { return m_pTextLayout.Get(); } }
    TextLayout(IDWriteFactory* pDWriteFactory, String^ fontFamilyName, int fontWeight, int fontStyle, int fontStretch, FLOAT fontSize, String ^text, FLOAT width, FLOAT height);
    void RefreshMetrics();

public:
    property int HR { int get() { return m_hr; } };

    property double Width { double get() { return m_width; } }
    property double Height { double get() { return m_height; } }
    property int LineCount { int get() { return m_lineCount; } }

    int SetTextAlignment(DWriteTextAlignment textAlignment);
    int SetParagraphAlignment(DWriteParaAlignment paragraphAlignment);
    virtual ~TextLayout() {}
};

} } }