// © Mike Murphy

using System.Collections.Generic;
using System.Linq;

namespace EMU7800.Shell;

public sealed class PageBackStackStateService
{
    #region Fields

    readonly Stack<PageBase> _pageStack = new();
    readonly HashSet<PageBase> _disposingPages = [];
    PageBase _pendingPage = PageBase.Default;

    #endregion

    public bool IsPagePending     { get; private set; }
    public bool IsQuitPending     { get; private set; }
    public bool IsDisposablePages { get; private set; }

    public void Push(PageBase newPage)
    {
        _pageStack.Push(newPage);
        _pendingPage = newPage;
        IsPagePending = !ReferenceEquals(_pendingPage, PageBase.Default);
    }

    public void Replace(PageBase newPage)
    {
        var replacedPage = _pageStack.Pop();
        _pageStack.Push(newPage);
        _pendingPage = newPage;
        _disposingPages.Add(replacedPage);
        IsPagePending = !ReferenceEquals(_pendingPage, PageBase.Default);
        IsDisposablePages = true;
    }

    public bool Pop()
    {
        if (_pageStack.Count <= 1)
        {
            IsQuitPending = true;
            return false;
        }
        var poppedPage = _pageStack.Pop();
        var newPage = _pageStack.Peek();
        _pendingPage = newPage;
        _disposingPages.Add(poppedPage);
        IsPagePending = !ReferenceEquals(_pendingPage, PageBase.Default);
        IsDisposablePages = true;
        return true;
    }

    public PageBase GetPendingPage()
    {
        var pendingPage = _pendingPage;
        _pendingPage = PageBase.Default;
        IsPagePending = false;
        return pendingPage;
    }

    public PageBase GetNextDisposablePage()
    {
        IsDisposablePages = _disposingPages.Count > 0;
        if (!IsDisposablePages)
            return PageBase.Default;
        var page = _disposingPages.First();
        _disposingPages.Remove(page);
        IsDisposablePages = _disposingPages.Count > 0;
        return page;
    }
}