// © Mike Murphy

using System.Collections.Generic;
using System.Linq;

namespace EMU7800.D2D.Shell
{
    public sealed class PageBackStackStateService
    {
        #region Fields

        static readonly Stack<PageBase> _pageStack = new Stack<PageBase>();
        static readonly HashSet<PageBase> _disposingPages = new HashSet<PageBase>();
        static PageBase _pendingPage;
        static bool _isPagePending, _isDisposablePages;

        #endregion

        public bool IsPagePending     { get { return _isPagePending; }     set { _isPagePending = value; } }      // cached _pendingPage != null for perf
        public bool IsDisposablePages { get { return _isDisposablePages; } set { _isDisposablePages = value; } }  // cached _disposingPages.Count > 0 for perf

        public void Push(PageBase newPage)
        {
            _pageStack.Push(newPage);
            _pendingPage = newPage;
            IsPagePending = _pendingPage != null;
        }

        public void Replace(PageBase newPage)
        {
            var replacedPage = _pageStack.Pop();
            _pageStack.Push(newPage);
            _pendingPage = newPage;
            _disposingPages.Add(replacedPage);
            IsPagePending = _pendingPage != null;
            IsDisposablePages = true;
        }

        public bool Pop()
        {
            if (_pageStack.Count <= 1)
                return false;
            var poppedPage = _pageStack.Pop();
            var newPage = _pageStack.Peek();
            _pendingPage = newPage;
            _disposingPages.Add(poppedPage);
            IsPagePending = _pendingPage != null;
            IsDisposablePages = true;
            return true;
        }

        public PageBase GetPendingPage()
        {
            var pendingPage = _pendingPage;
            _pendingPage = null;
            IsPagePending = false;
            return pendingPage;
        }

        public PageBase GetNextDisposablePage()
        {
            IsDisposablePages = _disposingPages.Count > 0;
            if (!IsDisposablePages)
                return null;
            var page = _disposingPages.First();
            _disposingPages.Remove(page);
            IsDisposablePages = _disposingPages.Count > 0;
            return page;
        }
    }
}
