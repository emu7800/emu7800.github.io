using Android.Graphics;

namespace EMU7800.D2D.Interop
{
    public class Line : DrawableShape
    {
        protected override void RefreshBitmap(Canvas canvas, Paint paint)
        {
            base.RefreshBitmap(canvas, paint);
            var rect = new Android.Graphics.RectF(BitmapMargin, BitmapMargin, DrawableWidth, DrawableHeight);
            canvas.DrawLine(rect.Left, rect.Top, rect.Right, rect.Bottom, paint);
        }
        public Line(GraphicsDevice gd, RectF rect, Paint.Style style) : base(gd, rect, style)
        {
        }
    }
}