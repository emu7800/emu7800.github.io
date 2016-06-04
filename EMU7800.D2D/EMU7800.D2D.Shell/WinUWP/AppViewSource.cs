// © Mike Murphy

using Windows.ApplicationModel.Core;

namespace EMU7800.D2D.Shell.WinRT
{
    public sealed class AppViewSource : IFrameworkViewSource
    {
        public IFrameworkView CreateView()
        {
            var appView = new AppView();
            return appView;
        }
    }
}
