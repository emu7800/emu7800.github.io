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
        D2DSolidColorBrush _brush;
        bool _isPaintInitialized;

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

        public void Draw(PointF location, D2DSolidColorBrush brush)
        {
            if (brush != _brush)
            {
                _brush = brush;
                RequestBitmapRefresh = true;
            }
            Draw(location);
        }

        protected override void RefreshBitmap(Canvas canvas, Paint paint)
        {
            if (!_isPaintInitialized)
            {
                paint.SetTypeface(_tf);
                paint.TextSize = _fontSize;
                paint.AntiAlias = true;
                _isPaintInitialized = true;
            }

            paint.Color = ToColor(_brush);
            paint.TextAlign = ToPaintAlign(_textAlignment);

            float tx = 0f, ty = 0f;

            if (paint.TextAlign == Paint.Align.Right)
                tx = BitmapWidth + BitmapMargin;
            else if (paint.TextAlign == Paint.Align.Center)
                tx = (BitmapWidth + BitmapMargin) / 2.0f;

            var bounds = new Rect();
            paint.GetTextBounds(_text, 0, _text.Length, bounds);

            Width  = Math.Abs(bounds.Left - bounds.Right);
            Height = Math.Abs(bounds.Top - bounds.Bottom) + 2.0f;

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

            canvas.DrawText(_text, tx, ty, paint);
        }

        #region Constructors

        public TextLayout(GraphicsDevice gd, string fontFamilyName, float fontSize, string text, float width, float height) : base(gd, width, height)
        {
            _fontFamilyName = fontFamilyName;
            _fontSize = fontSize;
            _text = text;
            _textAlignment = DWriteTextAlignment.Leading;
            _paragraphAlignment = fontSize >= 50 ? DWriteParaAlignment.Center : DWriteParaAlignment.Near;
            _brush = D2DSolidColorBrush.White;

            RequestBitmapRefresh = true;
        }

        #endregion

        #region Helpers

        static Paint.Align ToPaintAlign(DWriteTextAlignment textAlignment)
        {
            switch (textAlignment)
            {
                case DWriteTextAlignment.Center:
                    return Paint.Align.Center;
                case DWriteTextAlignment.Trailing:
                    return Paint.Align.Right;
                default:
                case DWriteTextAlignment.Leading:
                    return Paint.Align.Left;
            }
        }

        #endregion
    }
}