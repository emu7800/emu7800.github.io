using Android.App;
using Android.Runtime;
using Android.Views;
using Android.OS;
using Android.Content.PM;
using EMU7800.D2D;

namespace EMU7800.MonoDroid
{
    [Activity(Label = "EMU7800.MonoDroid",
        MainLauncher = true,
        Icon = "@drawable/appicon_128x128",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden
#if __ANDROID_11__
        ,HardwareAccelerated = false
#endif
        )]
    public class MainActivity : Activity
    {
        AppView _appView;

        public static MainActivity App { get; private set; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

            App = this;

            _appView = new AppView(App);
            SetContentView(_appView);
        }

        //public override bool OnKeyDown([GeneratedEnum]Keycode keyCode, KeyEvent e)
        //{
        //    return view.OnKeyDown(keyCode, e);
        //}

        protected override void OnPause()
        {
            base.OnPause();
            //view.Pause();
        }

        protected override void OnResume()
        {
            base.OnResume();
            //view.Resume();
        }
    }
}

