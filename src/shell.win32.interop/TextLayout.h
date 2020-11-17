// © Mike Murphy

#pragma once

#include "TextFormat.h"

using namespace System;

namespace EMU7800 { namespace D2D { namespace Interop {

public ref class TextLayout
{
private:
    IDWriteTextFormat* m_pTextFormat;
    IDWriteTextLayout* m_pTextLayout;
    HRESULT m_hr;
    double m_width, m_height;
    int m_lineCount;

internal:
    property IDWriteTextLayout* DWriteTextLayout { IDWriteTextLayout* get() { return m_pTextLayout; } }
    TextLayout(IDWriteFactory* pDWriteFactory, String^ fontFamilyName, int fontWeight, int fontStyle, int fontStretch, FLOAT fontSize, String ^text, FLOAT width, FLOAT height);
    void RefreshMetrics();

public:
    property int HR { int get() { return m_hr; } };

    property double Width { double get() { return m_width; } }
    property double Height { double get() { return m_height; } }
    property int LineCount { int get() { return m_lineCount; } }

    int SetTextAlignment(DWriteTextAlignment textAlignment);
    int SetParagraphAlignment(DWriteParaAlignment paragraphAlignment);
    TextLayout() : TextLayout(NULL, "", 0, 0, 0, 0.0, "", 0.0, 0.0) {};
    ~TextLayout();
    !TextLayout();
};

} } }