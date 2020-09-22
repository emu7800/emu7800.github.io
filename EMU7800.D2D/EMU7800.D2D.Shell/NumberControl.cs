// © Mike Murphy

using EMU7800.D2D.Interop;
using System.Linq;

namespace EMU7800.D2D.Shell
{
    public sealed class NumberControl : ControlBase
    {
        readonly TextLayout[] _textlayoutDigits;
        TextLayout _textlayoutRadix = TextLayoutDefault;
        TextLayout _textlayoutComma = TextLayoutDefault;

        float _maxDigitWidth;

        public string TextFontFamilyName { get; set; }
        public int TextFontSize { get; set; }
        public D2DSolidColorBrush Color { get; set; }

        public int Value { get; set; }
        public int Radix { get; set; }
        public bool UseComma { get; set; }

        public NumberControl()
        {
            _textlayoutDigits = Enumerable.Range(0, 10).Select(i => TextLayoutDefault).ToArray();
            TextFontFamilyName = Styles.NormalFontFamily;
            TextFontSize = Styles.NormalFontSize;
            Color = D2DSolidColorBrush.White;
            UseComma = true;
        }

        #region ControlBase Overrides

        public override void Render(GraphicsDevice gd)
        {
            base.Render(gd);

            var location = Struct.ToPointF(Location.X + Size.Width, Location.Y);

            var val = Value;
            var rad = Radix;
            var cma = 0;

            while (true)
            {
                if (rad == 0 && rad != Radix)
                {
                    location.X -= (float)_textlayoutRadix.Width;
                    gd.DrawText(_textlayoutRadix, location, Color);
                }
                else if (UseComma && cma++ == 3)
                {
                    location.X -= (float)_textlayoutComma.Width;
                    gd.DrawText(_textlayoutComma, location, Color);
                    cma = 0;
                }
                else
                {
                    location.X -= _maxDigitWidth;
                    gd.DrawText(_textlayoutDigits[val % 10], location, Color);
                    val /= 10;
                    if (val == 0)
                        break;
                }
                rad--;
            }
        }

        protected override void CreateResources(GraphicsDevice gd)
        {
            for (var i = 0; i < _textlayoutDigits.Length; i++)
            {
                CreateDigitTextLayout(gd, i);
                if (_textlayoutDigits[i].Width > _maxDigitWidth)
                    _maxDigitWidth = (float)_textlayoutDigits[i].Width;
            }
            _textlayoutRadix = gd.CreateTextLayout(
                TextFontFamilyName,
                TextFontSize,
                ".",
                100, 100
                );
            _textlayoutComma = gd.CreateTextLayout(
                TextFontFamilyName,
                TextFontSize,
                ",",
                100, 100
                );
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

        void CreateDigitTextLayout(GraphicsDevice gd, int i)
        {
            _textlayoutDigits[i] = gd.CreateTextLayout(
                TextFontFamilyName,
                TextFontSize,
                i.ToString(System.Globalization.CultureInfo.InvariantCulture),
                100, 100
                );
        }

        #endregion
    }
}