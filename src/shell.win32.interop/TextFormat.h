// © Mike Murphy

#pragma once

using namespace System;

namespace EMU7800 { namespace D2D { namespace Interop {

public enum struct DWriteTextAlignment { Leading, Trailing, Center };
public enum struct DWriteParaAlignment { Near, Far, Center };
public enum struct DWriteFontStyle { Normal, Oblique, Italic };
public enum struct DWriteFontStretch { Undefined, UltraCondensed, ExtraCondensed, StretchCondensed, SemiCondensed, Normal, SemiExpanded, Expanded, ExtraExpanded, UltraExpanded };
public enum struct DWriteFontWeight
{
    Thin = 100,
    ExtraLight = 200,
    UltraLight = 200,
    Light = 300,
    SemiLight = 350,
    Normal = 400,
    Medium = 500,
    SemiBold = 600,
    Bold = 700,
    ExtraBold = 800,
    UltraBold = 800,
    Heavy = 900,
    ExtraHeavy = 950,
};

public ref class TextFormat
{
private:
    IDWriteTextFormat* m_pTextFormat;
    HRESULT m_hr;

internal:
    property IDWriteTextFormat* DWriteTextFormat { IDWriteTextFormat* get() { return m_pTextFormat; } }
    TextFormat(IDWriteFactory* pDWriteFactory, String^ fontFamilyName, int fontWeight, int fontStyle, int fontStretch, FLOAT fontSize);

public:
    property int HR { int get() { return m_hr; } };
    int SetTextAlignment(DWriteTextAlignment textAlignment);
    int SetParagraphAlignment(DWriteParaAlignment paragraphAlignment);
    ~TextFormat();
    !TextFormat();
};

} } }