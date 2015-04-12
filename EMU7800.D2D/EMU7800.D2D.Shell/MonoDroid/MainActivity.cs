using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;

namespace EMU7800.MonoDroid
{
    [Activity(Label = "EMU7800.MonoDroid",
        MainLauncher = true,
        Icon = "@drawable/icon",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden
#if __ANDROID_11__
        ,HardwareAccelerated=false
#endif
        )]
    public class MainActivity : Activity
    {
        GLView1 view;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);


            // Create our OpenGL view, and display it
            view = new GLView1(this);
            SetContentView(view);
        }

        public override bool OnKeyDown([GeneratedEnum]Keycode keyCode, KeyEvent e)
        {
            return view.OnKeyDown(keyCode, e);
        }

        protected override void OnPause()
        {
            base.OnPause();
            view.Pause();
        }

        protected override void OnResume()
        {
            base.OnResume();
            view.Resume();
        }
    }
}

