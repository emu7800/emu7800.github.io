// © Mike Murphy

using System.Linq;

namespace EMU7800.Shell;

public sealed class NumberControl : ControlBase
{
    readonly TextLayout[] _textlayoutDigits = [.. Enumerable.Range(0, 10).Select(_ => TextLayout.Empty)];
    TextLayout _textlayoutRadix = TextLayout.Empty;
    TextLayout _textlayoutComma = TextLayout.Empty;

    float _maxDigitWidth;

    public string TextFontFamilyName { get; set; } = Styles.NormalFontFamily;
    public int TextFontSize { get; set; } = Styles.NormalFontSize;
    public SolidColorBrush Color { get; set; } = SolidColorBrush.White;

    public int Value { get; set; }
    public int Radix { get; set; }
    public bool UseComma { get; set; } = true;

    public NumberControl() {}

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
                GraphicsDevice.Draw(_textlayoutRadix, location);
            }
            else if (UseComma && cma++ == 3)
            {
                location.X -= _textlayoutComma.Width;
                GraphicsDevice.Draw(_textlayoutComma, location);
                cma = 0;
            }
            else
            {
                location.X -= _maxDigitWidth;
                GraphicsDevice.Draw(_textlayoutDigits[val % 10], location);
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
        _textlayoutRadix = GraphicsDevice.CreateTextLayout(TextFontFamilyName, TextFontSize, ".", 100, 100, WriteParaAlignment.Near, WriteTextAlignment.Leading, Color);
        _textlayoutComma = GraphicsDevice.CreateTextLayout(TextFontFamilyName, TextFontSize, ",", 100, 100, WriteParaAlignment.Near, WriteTextAlignment.Leading, Color);
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
        _textlayoutDigits[i] = GraphicsDevice.CreateTextLayout(TextFontFamilyName, TextFontSize, i.ToString(System.Globalization.CultureInfo.InvariantCulture), 100, 100, WriteParaAlignment.Near, WriteTextAlignment.Leading, Color);
    }

    #endregion
}