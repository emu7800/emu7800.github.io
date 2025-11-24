// © Mike Murphy

using EMU7800.Shell;
using System;

namespace EMU7800.Win32.Interop;

public sealed class TextLayoutD2DDriver : ITextLayoutDriver
{
    public static TextLayoutD2DDriver Factory() => new();
    TextLayoutD2DDriver() {}

    #region Fields

    IntPtr _textFormatPtr, _textLayoutPtr;

    #endregion

    #region ITextLayoutDriver Members

    public int Create(string fontFamilyName, int fontWeight, int fontStyle, int fontStretch, float fontSize, string text, float width, float height)
    {
        if (fontWeight < 0)
            fontWeight = Direct2DNativeMethods.DWRITE_FONT_WEIGHT_NORMAL;
        if (fontStyle < 0)
            fontStyle = Direct2DNativeMethods.DWRITE_FONT_STYLE_NORMAL;
        if (fontStretch < 0)
            fontStretch = Direct2DNativeMethods.DWRITE_FONT_STRETCH_NORMAL;
        Release();
        return Direct2DNativeMethods.Direct2D_CreateTextLayout(fontFamilyName, fontWeight, fontStyle, fontStretch, fontSize, text, width, height, ref _textFormatPtr, ref _textLayoutPtr);
    }

    public void Draw(PointF location, SolidColorBrush brush)
      => Direct2DNativeMethods.Direct2D_DrawTextLayout(_textLayoutPtr, location, brush);

    public (SizeF, int) GetMetrics()
    {
        if (_textLayoutPtr == IntPtr.Zero)
            return (new(0, 0), 0);
        DWRITE_TEXT_METRICS metrics = new();
        Direct2DNativeMethods.Direct2D_GetMetrics(_textLayoutPtr, ref metrics);
        return (new(metrics.width, metrics.height), (int)metrics.lineCount);
    }

    public void Release()
    {
        if (_textLayoutPtr == IntPtr.Zero)
            return;
        Direct2DNativeMethods.Direct2D_ReleaseTextLayout(_textFormatPtr, _textLayoutPtr);
        _textFormatPtr = _textLayoutPtr = IntPtr.Zero;
    }

    public void SetParagraphAlignment(WriteParaAlignment paragraphAlignment)
    {
        if (_textLayoutPtr == IntPtr.Zero)
            return;
        Direct2DNativeMethods.Direct2D_SetParagraphAlignmentForTextLayout(_textLayoutPtr, paragraphAlignment);
    }

    public void SetTextAlignment(WriteTextAlignment textAlignment)
    {
        if (_textLayoutPtr == IntPtr.Zero)
            return;
        Direct2DNativeMethods.Direct2D_SetTextAlignmentForTextLayout(_textLayoutPtr, textAlignment);
    }

    #endregion
}