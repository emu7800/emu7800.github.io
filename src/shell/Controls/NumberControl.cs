// © Mike Murphy

using System.Linq;

namespace EMU7800.Shell;

public sealed class NumberControl : ControlBase
{
    readonly TextLayout[] _textlayoutDigits;
    TextLayout _textlayoutRadix = TextLayout.Default;
    TextLayout _textlayoutComma = TextLayout.Default;

    float _maxDigitWidth;

    public string TextFontFamilyName { get; set; }
    public int TextFontSize { get; set; }
    public SolidColorBrush Color { get; set; }

    public int Value { get; set; }
    public int Radix { get; set; }
    public bool UseComma { get; set; }

    public NumberControl()
    {
        _textlayoutDigits = [.. Enumerable.Range(0, 10).Select(_ => TextLayout.Default)];
        TextFontFamilyName = Styles.NormalFontFamily;
        TextFontSize = Styles.NormalFontSize;
        Color = SolidColorBrush.White;
        UseComma = true;
    }

    #region ControlBase Overrides

    public override void Render()
    {
        base.Render();

        PointF location = new(Location.X + Size.Width, Location.Y);

        var val = Value;
        var rad = Radix;
        var cma = 0;

        while (true)
        {
            if (rad == 0 && rad != Radix)
            {
                location.X -= _textlayoutRadix.Width;
                GraphicsDevice.Draw(_textlayoutRadix, location, Color);
            }
            else if (UseComma && cma++ == 3)
            {
                location.X -= _textlayoutComma.Width;
                GraphicsDevice.Draw(_textlayoutComma, location, Color);
                cma = 0;
            }
            else
            {
                location.X -= _maxDigitWidth;
                GraphicsDevice.Draw(_textlayoutDigits[val % 10], location, Color);
                val /= 10;
                if (val == 0)
                    break;
            }
            rad--;
        }
    }

    protected override void CreateResources()
    {
        for (var i = 0; i < _textlayoutDigits.Length; i++)
        {
            CreateDigitTextLayout(i);
            if (_textlayoutDigits[i].Width > _maxDigitWidth)
                _maxDigitWidth = _textlayoutDigits[i].Width;
        }
        _textlayoutRadix = new TextLayout(TextFontFamilyName, TextFontSize, ".", 100, 100);
        _textlayoutComma = new TextLayout(TextFontFamilyName, TextFontSize, ",", 100, 100);
    }

    protected override void DisposeResources()
    {
        for (var i = 0; i < _textlayoutDigits.Length; i++)
        {
            SafeDispose(ref _textlayoutDigits[i]);
        }
        SafeDispose(ref _textlayoutRadix);
        SafeDispose(ref _textlayoutComma);
        base.DisposeResources();
    }

    #endregion

    #region Helpers

    void CreateDigitTextLayout(int i)
    {
        _textlayoutDigits[i] = new TextLayout(TextFontFamilyName, TextFontSize, i.ToString(System.Globalization.CultureInfo.InvariantCulture), 100, 100);
    }

    #endregion
}