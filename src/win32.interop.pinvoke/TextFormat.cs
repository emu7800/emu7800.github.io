using System;

using static EMU7800.Win32.Interop.Direct2DNativeMethods;

namespace EMU7800.Win32.Interop;

public class TextFormat : IDisposable
{
    #region Fields

    readonly string _fontFamilyName;
    readonly int _fontWeight, _fontStyle, _fontStretch;
    readonly float _fontSize;

    public IntPtr TextFormatPtr { get; private set; }

    public int HR { get; private set; }

    #endregion

    public void SetTextAlignment(DWriteTextAlignment textAlignment)
        => Direct2D_SetTextAlignmentForTextFormat(TextFormatPtr, textAlignment);

    public void SetParagraphAlignment(DWriteParaAlignment paragraphAlignment)
        => Direct2D_SetParagraphAlignmentForTextFormat(TextFormatPtr, paragraphAlignment);

    internal void Draw(string text, D2D_RECT_F drect, D2DSolidColorBrush brush)
        => Direct2D_DrawTextFormat(TextFormatPtr, text, drect, brush);

    public void Initialize()
    {
        if (TextFormatPtr != IntPtr.Zero)
            return;
        var ptr = TextFormatPtr;
        HR = Direct2D_CreateTextFormat(_fontFamilyName, _fontWeight, _fontStyle, _fontStretch, _fontSize, ref ptr);
        TextFormatPtr = ptr;
    }

    #region IDispose Members

    public void Dispose()
    {
        if (TextFormatPtr == IntPtr.Zero)
            return;
        Direct2D_ReleaseTextFormat(TextFormatPtr);
        TextFormatPtr = IntPtr.Zero;
        HR = 0;
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Constructors

    public TextFormat(string fontFamilyName, float fontSize)
        : this(fontFamilyName,
            DWRITE_FONT_WEIGHT_NORMAL,
            DWRITE_FONT_STYLE_NORMAL,
            DWRITE_FONT_STRETCH_NORMAL,
            fontSize) {}

    public TextFormat(string fontFamilyName, int fontWeight, int fontStyle, int fontStretch, float fontSize)
    {
        _fontFamilyName = fontFamilyName;
        _fontWeight     = fontWeight;
        _fontStyle      = fontStyle;
        _fontStretch    = fontStretch;
        _fontSize       = fontSize;
        Initialize();
    }

    #endregion
}