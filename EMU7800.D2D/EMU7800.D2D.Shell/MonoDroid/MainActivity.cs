// © Mike Murphy

using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using EMU7800.D2D;
using EMU7800.D2D.Shell;

namespace EMU7800.MonoDroid
{
    [Activity(
        Label                = "EMU7800",
        MainLauncher         = true,
        Icon                 = "@drawable/appicon_128x128",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize,
        LaunchMode           = LaunchMode.SingleInstance
#if __ANDROID_11__
       ,HardwareAccelerated  = false
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

            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            Window.AddFlags(WindowManagerFlags.Fullscreen);
            Window.AddFlags(WindowManagerFlags.LayoutNoLimits);

            App = this;

            _appView = new AppView(App);

            SetContentView(_appView);
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            return OnKeyChanged(keyCode, e, true) || base.OnKeyDown(keyCode, e);
        }

        public override bool OnKeyUp(Keycode keyCode, KeyEvent e)
        {
            return OnKeyChanged(keyCode, e, false) || base.OnKeyUp(keyCode, e);
        }

        protected override void OnPause()
        {
            base.OnPause();
            _appView.Pause();
        }

        protected override void OnResume()
        {
            base.OnResume();
            _appView.Resume();
        }

        bool OnKeyChanged(Keycode keyCode, KeyEvent e, bool down)
        {
            var key = ToKeyboardKey(keyCode);
            _appView.KeyboardKeyPressed(key, down);
            return key != KeyboardKey.None;
        }

        static KeyboardKey ToKeyboardKey(Keycode keyCode)
        {
            switch (keyCode)
            {
                case Keycode.Z:                 return KeyboardKey.Z;
                case Keycode.X:                 return KeyboardKey.X;
                case Keycode.DpadLeft:          return KeyboardKey.Left;
                case Keycode.DpadUp:            return KeyboardKey.Up;
                case Keycode.DpadRight:         return KeyboardKey.Right;
                case Keycode.DpadDown:          return KeyboardKey.Down;
                case Keycode.Num0:              return KeyboardKey.Number0;
                case Keycode.Num1:              return KeyboardKey.Number1;
                case Keycode.Num2:              return KeyboardKey.Number2;
                case Keycode.Num3:              return KeyboardKey.Number3;
                case Keycode.Num4:              return KeyboardKey.Number4;
                case Keycode.Num5:              return KeyboardKey.Number5;
                case Keycode.Num6:              return KeyboardKey.Number6;
                case Keycode.Num7:              return KeyboardKey.Number7;
                case Keycode.Num8:              return KeyboardKey.Number8;
                case Keycode.Num9:              return KeyboardKey.Number9;
                case Keycode.Numpad0:           return KeyboardKey.NumberPad0;
                case Keycode.Numpad1:           return KeyboardKey.NumberPad1;
                case Keycode.Numpad2:           return KeyboardKey.NumberPad2;
                case Keycode.Numpad3:           return KeyboardKey.NumberPad3;
                case Keycode.Numpad4:           return KeyboardKey.NumberPad4;
                case Keycode.Numpad5:           return KeyboardKey.NumberPad5;
                case Keycode.Numpad6:           return KeyboardKey.NumberPad6;
                case Keycode.Numpad7:           return KeyboardKey.NumberPad7;
                case Keycode.Numpad8:           return KeyboardKey.NumberPad8;
                case Keycode.Numpad9:           return KeyboardKey.NumberPad9;
                case Keycode.NumpadMultiply:    return KeyboardKey.Multiply;
                case Keycode.NumpadDivide:      return KeyboardKey.Divide;
                case Keycode.NumpadAdd:         return KeyboardKey.Add;
                case Keycode.Q:                 return KeyboardKey.Q;
                case Keycode.W:                 return KeyboardKey.W;
                case Keycode.E:                 return KeyboardKey.E;
                case Keycode.H:                 return KeyboardKey.H;
                case Keycode.P:                 return KeyboardKey.P;
                case Keycode.R:                 return KeyboardKey.R;
                case Keycode.S:                 return KeyboardKey.S;
                case Keycode.F1:                return KeyboardKey.F1;
                case Keycode.F2:                return KeyboardKey.F2;
                case Keycode.F3:                return KeyboardKey.F3;
                case Keycode.F4:                return KeyboardKey.F4;
                case Keycode.PageUp:            return KeyboardKey.PageUp;
                case Keycode.PageDown:          return KeyboardKey.PageDown;
                default:                        return KeyboardKey.None;
            }
        }
    }
}
