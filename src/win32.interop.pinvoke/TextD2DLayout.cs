// © Mike Murphy

using EMU7800.Shell;
using System;

namespace EMU7800.Win32.Interop;

public sealed class TextD2DLayout : TextLayout
{
    #region Fields

    readonly IntPtr _textFormatPtr, _textLayoutPtr;
    readonly D2DSolidColorBrush _brush;

    #endregion

    public override void Draw(PointF location)
      => Direct2DNativeMethods.Direct2D_DrawTextLayout(_textLayoutPtr, location, _brush);

    #region IDispose Members

    protected override void Dispose(bool disposing)
    {
        if (!_resourceDisposed)
        {
            Direct2DNativeMethods.Direct2D_ReleaseTextLayout(_textFormatPtr, _textLayoutPtr);
            _resourceDisposed = true;
        }
        base.Dispose(disposing);
    }

    #endregion

    #region Constructors

    public TextD2DLayout(string fontFamilyName, float fontSize, string text, float width, float height, WriteParaAlignment paragraphAlignment, WriteTextAlignment textAlignment, SolidColorBrush brush)
        : this(fontFamilyName, -1, -1, -1, fontSize, text, width, height, paragraphAlignment, textAlignment, brush) {}

    public TextD2DLayout(string fontFamilyName, int fontWeight, int fontStyle, int fontStretch, float fontSize, string text, float width, float height, WriteParaAlignment paragraphAlignment, WriteTextAlignment textAlignment, SolidColorBrush brush)
    {
        if (fontWeight < 0)
            fontWeight = Direct2DNativeMethods.DWRITE_FONT_WEIGHT_NORMAL;
        if (fontStyle < 0)
            fontStyle = Direct2DNativeMethods.DWRITE_FONT_STYLE_NORMAL;
        if (fontStretch < 0)
            fontStretch = Direct2DNativeMethods.DWRITE_FONT_STRETCH_NORMAL;

        _brush = brush;

        HR = Direct2DNativeMethods.Direct2D_CreateTextLayout(fontFamilyName, fontWeight, fontStyle, fontStretch, fontSize, text, width, height, ref _textFormatPtr, ref _textLayoutPtr);

        if (HR == 0)
        {
            Direct2DNativeMethods.Direct2D_SetParagraphAlignmentForTextLayout(_textLayoutPtr, paragraphAlignment);
            Direct2DNativeMethods.Direct2D_SetTextAlignmentForTextLayout(_textLayoutPtr, textAlignment);

            DWRITE_TEXT_METRICS metrics = new();
            Direct2DNativeMethods.Direct2D_GetMetrics(_textLayoutPtr, ref metrics);
            Size = new SizeF(metrics.width, metrics.height);
        }
    }

    #endregion
}