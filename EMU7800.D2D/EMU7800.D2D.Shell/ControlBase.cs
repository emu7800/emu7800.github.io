// © Mike Murphy

using System;
using EMU7800.D2D.Interop;

namespace EMU7800.D2D.Shell
{
    public abstract class ControlBase : IDisposable
    {
        #region Fields

        static int _nextIdToProvision;
        readonly int _id = _nextIdToProvision++;

        PointF _location;
        SizeF _size;

        #endregion

        public PointF Location
        {
            get { return _location; }
            set
            {
                _location = value;
                LocationChanged();
            }
        }

        public SizeF Size
        {
            get { return _size; }
            set
            {
                _size = value;
                SizeChanged();
            }
        }

        public bool IsMouseOver
        {
            get { return IsMouseOverPointerId.HasValue; }
        }

        public bool IsVisible { get; set; }
        public bool IsEnabled { get; set; }

        protected ControlBase()
        {
            IsVisible = true;
            IsEnabled = true;
        }

        #region ControlBase Virtuals

        public virtual void KeyboardKeyPressed(KeyboardKey key, bool down)
        {
        }

        public virtual void MouseMoved(uint pointerId, int x, int y, int dx, int dy)
        {
        }

        public virtual void MouseButtonChanged(uint pointerId, int x, int y, bool down)
        {
        }

        public virtual void MouseWheelChanged(uint pointerId, int x, int y, int delta)
        {
        }

        public virtual void LocationChanged()
        {
        }

        public virtual void SizeChanged()
        {
        }

        public virtual void LoadResources(GraphicsDevice gd)
        {
            DisposeResources();
            CreateResources(gd);
        }

        public virtual void Update(TimerDevice td)
        {
        }

        public virtual void Render(GraphicsDevice gd)
        {
        }

        protected virtual void CreateResources(GraphicsDevice gd)
        {
        }

        protected virtual void DisposeResources()
        {
        }

        #endregion

        #region Object Overrides

        public override bool Equals(object obj)
        {
            var them = (ControlBase)obj;
            return _id == them._id;
        }

        public override int GetHashCode()
        {
            return _id;
        }

        public override string ToString()
        {
            return $"EMU7800.D2D.Shell.ControlBase: ID={_id}";
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeResources();
            }
        }

        #endregion

        protected uint? IsMouseOverPointerId { get; set; }

        protected void SafeDispose(ref StaticBitmap bitmap)
        {
            if (bitmap == null)
                return;
            bitmap.Dispose();
            bitmap = null;
        }

        protected void SafeDispose(ref DynamicBitmap bitmap)
        {
            if (bitmap == null)
                return;
            bitmap.Dispose();
            bitmap = null;
        }

        protected void SafeDispose(ref TextLayout textLayout)
        {
            if (textLayout == null)
                return;
            textLayout.Dispose();
            textLayout = null;
        }

        protected bool IsInBounds(int x, int y, RectF bounds)
        {
            var outOfBounds = x < bounds.Left
                || x > bounds.Right
                    || y < bounds.Top
                        || y > bounds.Bottom;
            return !outOfBounds;
        }
    }
}
