using Android.App;
using Android.Graphics;
using System;

namespace EMU7800.D2D.Interop
{
    public class TextLayout : DrawableShape
    {
        #region Fields

        static Typeface _tf = Typeface.CreateFromAsset(Application.Context.Assets, "fonts/segoeui.ttf");

        readonly string _fontFamilyName;
        readonly float _fontSize;
        readonly string _text;

        DWriteTextAlignment _textAlignment;
        DWriteParaAlignment _paragraphAlignment;

        #endregion

        public int HR { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }

        public int SetTextAlignment(DWriteTextAlignment textAlignment)
        {
            if (textAlignment != _textAlignment)
            {
                _textAlignment = textAlignment;
                RequestBitmapRefresh = true;
            }
            return 0;
        }

        public int SetParagraphAlignment(DWriteParaAlignment paragraphAlignment)
        {
            if (paragraphAlignment != _paragraphAlignment)
            {
                _paragraphAlignment = paragraphAlignment;
                RequestBitmapRefresh = true;
            }
            return 0;
        }

        protected override void RefreshBitmap()
        {
            base.RefreshBitmap();

            float ty = 0f;

            switch (_paragraphAlignment)
            {
                case DWriteParaAlignment.Near:      // top of the text flow is aligned to the top edge of the layout box
                    ty = (float)Height;
                    break;
                case DWriteParaAlignment.Center:    // center of the flow is aligned to the center of the layout box
                    ty = (BitmapHeight + BitmapMargin) / 2.0f + (float)Height / 2.0f;
                    break;
                case DWriteParaAlignment.Far:       // bottom of the flow is aligned to the bottom edge of the layout box
                    ty = BitmapHeight + BitmapMargin;
                    break;
            }

            using (var textPaint = new Android.Text.TextPaint())
            {
                textPaint.AntiAlias = true;
                textPaint.SetTypeface(_tf);
                textPaint.TextSize = _fontSize;
                textPaint.Color = ToColor(Brush);
                Android.Text.Layout.Alignment alignment;
                switch (_textAlignment)
                {
                    case DWriteTextAlignment.Center:
                        alignment = Android.Text.Layout.Alignment.AlignCenter;
                        break;
                    case DWriteTextAlignment.Trailing:
                        alignment = Android.Text.Layout.Alignment.AlignOpposite;
                        break;
                    default:
                    case DWriteTextAlignment.Leading:
                        alignment = Android.Text.Layout.Alignment.AlignNormal;
                        break;
                }
                switch (_paragraphAlignment)
                {
                    default:
                    case DWriteParaAlignment.Near:
                        ty = 0f;
                        break;
                    case DWriteParaAlignment.Center:
                        ty = (BitmapHeight + BitmapMargin) / 2.0f - (float)Height / 2.0f;
                        break;
                    case DWriteParaAlignment.Far:
                        ty = (BitmapHeight + BitmapMargin) - (float)Height;
                        break;
                }
                using (var sl = new Android.Text.StaticLayout(_text, textPaint, (int)DrawableWidth, alignment, 1, 1, false))
                {
                    Canvas.Save();
                    Canvas.Translate(0f, ty);
                    sl.Draw(Canvas);
                    Canvas.Restore();
                }
            }
        }

        #region Constructors

        public TextLayout(GraphicsDevice gd, string fontFamilyName, float fontSize, string text, float width, float height) : base(gd, width, height)
        {
            _fontFamilyName = fontFamilyName;
            _fontSize = fontSize;
            _text = text;
            _textAlignment = DWriteTextAlignment.Leading;
            _paragraphAlignment = DWriteParaAlignment.Near;

            var bounds = new Rect();
            using (var paint = new Android.Text.TextPaint())
            {
                paint.AntiAlias = true;
                paint.SetTypeface(_tf);
                paint.TextSize = _fontSize;
                paint.GetTextBounds(_text, 0, _text.Length, bounds);
            }

            Width  = Math.Abs(bounds.Left - bounds.Right);
            Height = Math.Abs(bounds.Top - bounds.Bottom) + 2.0f;
        }

        #endregion
    }
}