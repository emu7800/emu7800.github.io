﻿using System;

using static EMU7800.Win32.Interop.Direct2DNativeMethods;

namespace EMU7800.Win32.Interop;

public class TextLayout : IDisposable
{
    public static readonly TextLayout Default = new();

    #region Fields

    readonly string _fontFamilyName, _text;
    readonly int _fontWeight, _fontStyle, _fontStretch;
    readonly float _fontSize, _width, _height;

    public IntPtr TextFormatPtr { get; private set; }
    public IntPtr TextLayoutPtr { get; private set; }

    public int HR { get; private set; }

    public float Width { get; private set; }
    public float Height { get; private set; }
    public int LineCount { get; private set; }

    #endregion

    public void SetTextAlignment(DWriteTextAlignment textAlignment)
        => Direct2D_SetTextAlignmentForTextLayout(TextLayoutPtr, textAlignment);

    public void SetParagraphAlignment(DWriteParaAlignment paragraphAlignment)
        => Direct2D_SetParagraphAlignmentForTextLayout(TextLayoutPtr, paragraphAlignment);

    internal void Draw(D2D_POINT_2F location, D2DSolidColorBrush brush)
        => Direct2D_DrawTextLayout(TextLayoutPtr, location, brush);

    public void Initialize()
    {
        if (TextLayoutPtr != IntPtr.Zero || _fontSize < 0)
            return;
        var fptr = TextFormatPtr;
        var lptr = TextLayoutPtr;
        HR = Direct2D_CreateTextLayout(_fontFamilyName, _fontWeight, _fontStyle, _fontStretch, _fontSize, _text, _width, _height, ref fptr, ref lptr);
        TextFormatPtr = fptr;
        TextLayoutPtr = lptr;

        DWRITE_TEXT_METRICS metrics = new();
        Direct2D_GetMetrics(TextLayoutPtr, ref metrics);
        Width = metrics.width;
        Height = metrics.height;
        LineCount = (int)metrics.lineCount;
    }

    #region IDispose Members

    public void Dispose()
    {
        if (TextLayoutPtr == IntPtr.Zero)
            return;
        Direct2D_ReleaseTextLayout(TextFormatPtr, TextLayoutPtr);
        TextFormatPtr = TextLayoutPtr = IntPtr.Zero;
        HR = 0;
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Constructors

    TextLayout()
        : this(string.Empty, -1, string.Empty, 0, 0) {}

    public TextLayout(string fontFamilyName, float fontSize, string text, float width, float height)
        : this(fontFamilyName,
            DWRITE_FONT_WEIGHT_NORMAL,
            DWRITE_FONT_STYLE_NORMAL,
            DWRITE_FONT_STRETCH_NORMAL,
            fontSize,
            text,
            width,
            height) {}

    public TextLayout(string fontFamilyName, int fontWeight, int fontStyle, int fontStretch, float fontSize, string text, float width, float height)
    {
        _fontFamilyName = fontFamilyName;
        _fontWeight     = fontWeight;
        _fontStyle      = fontStyle;
        _fontStretch    = fontStretch;
        _fontSize       = fontSize;
        _text           = text;
        _width          = width;
        _height         = height;
        Initialize();
    }

    #endregion
}