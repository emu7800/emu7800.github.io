// © Mike Murphy

using Android.Graphics;
using System;

namespace EMU7800.D2D.Interop
{
    public abstract class DrawableShape : Drawable
    {
        public void Draw(PointF location, float strokeWidth, D2DSolidColorBrush brush)
        {
            if (Math.Abs(strokeWidth - StrokeWidth) > 1e-6)
            {
                RequestBitmapRefresh = true;
                StrokeWidth = strokeWidth;
            }
            Draw(location, brush);
        }

        public void Draw(PointF location, D2DSolidColorBrush brush)
        {
            if (brush != Brush)
            {
                RequestBitmapRefresh = true;
                Brush = brush;
            }
            Draw(location);
        }

        protected D2DSolidColorBrush Brush { get; private set; }
        protected float StrokeWidth { get; private set; }
        protected Paint.Style Style { get; private set; }

        #region Constructors

        protected DrawableShape(GraphicsDevice gd, float width, float height) : base(gd, width, height, 0)
        {
            Style = Paint.Style.Fill;
            Brush = D2DSolidColorBrush.White;
        }

        protected DrawableShape(GraphicsDevice gd, RectF rect, Paint.Style style) : base(gd, ToWidth(rect), ToHeight(rect), 4)
        {
            Style = style;
            Brush = D2DSolidColorBrush.White;
        }

        #endregion
    }
}