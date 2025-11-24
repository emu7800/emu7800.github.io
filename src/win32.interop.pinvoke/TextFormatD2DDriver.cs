// © Mike Murphy

using EMU7800.Shell;
using System;

namespace EMU7800.Win32.Interop;

public sealed class TextFormatD2DDriver : ITextFormatDriver
{
    public static TextFormatD2DDriver Factory() => new();
    TextFormatD2DDriver() {}

    #region Fields

    IntPtr _textFormatPtr;

    #endregion

    #region ITextFormatDriver Members

    public int Create(string fontFamilyName, int fontWeight, int fontStyle, int fontStretch, float fontSize)
    {
        if (fontWeight < 0)
            fontWeight = Direct2DNativeMethods.DWRITE_FONT_WEIGHT_NORMAL;
        if (fontStyle < 0)
            fontStyle = Direct2DNativeMethods.DWRITE_FONT_STYLE_NORMAL;
        if (fontStretch < 0)
            fontStretch = Direct2DNativeMethods.DWRITE_FONT_STRETCH_NORMAL;

        Release();
        return Direct2DNativeMethods.Direct2D_CreateTextFormat(fontFamilyName, fontWeight, fontStyle, fontStretch, fontSize, ref _textFormatPtr);
    }

    public void Draw(string text, RectF rect, SolidColorBrush brush)
      => Direct2DNativeMethods.Direct2D_DrawTextFormat(_textFormatPtr, text, rect, brush);

    public void Release()
    {
        if (_textFormatPtr == IntPtr.Zero)
            return;
        Direct2DNativeMethods.Direct2D_ReleaseTextFormat(_textFormatPtr);
        _textFormatPtr = IntPtr.Zero;
    }

    public void SetParagraphAlignment(WriteParaAlignment paragraphAlignment)
    {
        if (_textFormatPtr == IntPtr.Zero)
            return;
        Direct2DNativeMethods.Direct2D_SetParagraphAlignmentForTextFormat(_textFormatPtr, paragraphAlignment);
    }

    public void SetTextAlignment(WriteTextAlignment textAlignment)
    {
        if (_textFormatPtr == IntPtr.Zero)
            return;
        Direct2DNativeMethods.Direct2D_SetTextAlignmentForTextFormat(_textFormatPtr, textAlignment);
    }

    #endregion
}
