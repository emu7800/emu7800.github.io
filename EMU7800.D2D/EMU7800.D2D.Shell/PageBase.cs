// © Mike Murphy

using System;
using EMU7800.D2D.Interop;

namespace EMU7800.D2D.Shell
{
    public abstract class PageBase : IDisposable
    {
        #region Fields

        readonly PageBackStackStateService _pageStateService = new PageBackStackStateService();

        static int _nextIdToProvision;
        readonly int _id = _nextIdToProvision++;

        #endregion

        protected ControlCollection Controls = new ControlCollection();

        public virtual void OnNavigatingHere()
        {
        }

        public virtual void OnNavigatingAway()
        {
        }

        public virtual void Resized(SizeF size)
        {
        }

        public virtual void KeyboardKeyPressed(KeyboardKey key, bool down)
        {
            Controls.KeyboardKeyPressed(key, down);
        }

        public virtual void MouseMoved(uint pointerId, int x, int y, int dx, int dy)
        {
            Controls.MouseMoved(pointerId, x, y, dx, dy);
        }

        public virtual void MouseButtonChanged(uint pointerId, int x, int y, bool down)
        {
            Controls.MouseButtonChanged(pointerId, x, y, down);
        }

        public virtual void MouseWheelChanged(uint pointerId, int x, int y, int delta)
        {
            Controls.MouseWheelChanged(pointerId, x, y, delta);
        }

        public virtual void LoadResources(GraphicsDevice gd)
        {
            Controls.LoadResources(gd);
        }

        public virtual void Update(TimerDevice td)
        {
            Controls.Update(td);
        }

        public virtual void Render(GraphicsDevice gd)
        {
            Controls.Render(gd);
        }

        #region PageStateService Accessors

        protected void PushPage(PageBase pageToPush)
        {
            _pageStateService.Push(pageToPush);
        }

        protected void ReplacePage(PageBase replacePage)
        {
            _pageStateService.Replace(replacePage);
        }

        protected bool PopPage()
        {
            return _pageStateService.Pop();
        }

        #endregion

        #region Object Overrides

        public override bool Equals(object obj)
        {
            var them = (PageBase)obj;
            return _id == them._id;
        }

        public override int GetHashCode()
        {
            return _id;
        }

        public override string ToString()
        {
            return string.Format("{0}: ID={1} Type={1}", GetType().FullName, _id);
        }

        #endregion

        #region Disposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Controls != null)
                {
                    Controls.Dispose();
                    Controls = null;
                }
            }
        }

        #endregion
    }
}
