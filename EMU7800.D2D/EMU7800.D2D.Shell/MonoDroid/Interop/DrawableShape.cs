using Android.Graphics;
using System;

namespace EMU7800.D2D.Interop
{
    public abstract class DrawableShape : Drawable
    {
        #region Fields

        readonly Paint _paint;
        readonly Paint.Style _style;
        D2DSolidColorBrush _brush;
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

        protected override void RefreshBitmap(Canvas canvas)
        {
            RefreshBitmap(canvas, _paint);
        }

        protected virtual void RefreshBitmap(Canvas canvas, Paint paint)
        {
            paint.StrokeWidth = _strokeWidth;
            paint.Color = ToColor(_brush);
            paint.SetStyle(_style);
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

        public DrawableShape(GraphicsDevice gd, float width, float height) : base(gd, width, height, 0)
        {
            _paint = new Paint();
        }

        public DrawableShape(GraphicsDevice gd, RectF rect, Paint.Style style) : base(gd, ToWidth(rect), ToHeight(rect), 4)
        {
            _paint = new Paint();
            _style = style;
        }

        #endregion
    }
}