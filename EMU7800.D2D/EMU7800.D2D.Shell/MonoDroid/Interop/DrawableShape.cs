using Android.Graphics;
using System;

namespace EMU7800.D2D.Interop
{
    public abstract class DrawableShape : Drawable
    {
        #region Fields

        readonly Paint _paint = new Paint();
        D2DSolidColorBrush _brush = D2DSolidColorBrush.White;
        float _strokeWidth;

        #endregion

        public void Draw(PointF location, float strokeWidth, D2DSolidColorBrush brush)
        {
            if (Math.Abs(strokeWidth - _strokeWidth) > 1e-6)
            {
                RequestBitmapRefresh = true;
                _strokeWidth = strokeWidth;
            }
            Draw(location, brush);
        }

        public void Draw(PointF location, D2DSolidColorBrush brush)
        {
            if (brush != _brush)
            {
                RequestBitmapRefresh = true;
                _brush = brush;
            }
            Draw(location);
        }

        protected Paint Paint { get { return _paint; } }

        protected override void RefreshBitmap()
        {
            Paint.StrokeWidth = _strokeWidth;
            Paint.Color = ToColor(_brush);
        }

        #region IDisposable Members

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                using (_paint) { }
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Constructors

        protected DrawableShape(GraphicsDevice gd, float width, float height) : base(gd, width, height, 0)
        {
            Paint.SetStyle(Paint.Style.Fill);
        }

        protected DrawableShape(GraphicsDevice gd, RectF rect, Paint.Style style) : base(gd, ToWidth(rect), ToHeight(rect), 4)
        {
            Paint.SetStyle(style);
        }

        #endregion
    }
}