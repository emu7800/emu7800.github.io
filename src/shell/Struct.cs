// © Mike Murphy

using EMU7800.D2D.Interop;

namespace EMU7800.D2D.Shell
{
    public static class Struct
    {
        public static PointF ToPointF(float x, float y)
            => new() { X = x, Y = y };

        public static SizeF ToSizeF(float width, float height)
            => new() { Width = width, Height = height };

        public static SizeU ToSizeU(uint width, uint height)
            => new() { Width = width, Height = height };

        public static RectF ToRectF(PointF point, SizeF size)
            => new() { Left = point.X, Top = point.Y, Right = point.X + size.Width, Bottom = point.Y + size.Height };

        public static PointF ToLocation(RectF rect)
            => new() { X = rect.Left, Y = rect.Top };

        public static SizeF ToSize(RectF rect)
            => new() { Width = rect.Right - rect.Left, Height = rect.Bottom - rect.Top };

        public static PointF ToRightOf(ControlBase control, int dx, int dy)
            => ToPointF(control.Location.X + control.Size.Width + dx, control.Location.Y + dy);

        public static PointF ToBottomOf(ControlBase control, int dx, int dy)
            => ToPointF(control.Location.X + dx, control.Location.Y + control.Size.Height + dy);
    }
}
