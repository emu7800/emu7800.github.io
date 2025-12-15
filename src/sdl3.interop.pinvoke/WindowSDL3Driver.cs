// © Mike Murphy

using EMU7800.Shell;
using System;

using static EMU7800.SDL3.Interop.SDL3;

namespace EMU7800.SDL3.Interop;

public static class WindowSDL3Driver
{
    static float ScaleFactor = 1f;
    static bool StartMaximized;

    public static unsafe void StartWindowAndProcessEvents(bool startMaximized)
    {
        StartMaximized = startMaximized;

        AudioDevice.DriverFactory     = AudioDeviceSDL3Driver.Factory;
        GameControllers.DriverFactory = GameControllersSDL3InputDriver.Factory;

        SDL_EnterAppMainCallbacks(0, IntPtr.Zero, AppInit, AppIterate, AppEvent, AppQuit);
    }

    static unsafe SDL_AppResult AppInit(IntPtr _, int argc, IntPtr argv)
    {
        Console.WriteLine($"Using SDL3: Version: {SDL_GetVersion()} Revision: {SDL_GetRevision()}");

        SDL_SetAppMetadata(VersionInfo.EMU7800, VersionInfo.AssemblyVersion, "https//emu7800.net");

        var driver = new GraphicsDeviceSDL3Driver(StartMaximized);
        ScaleFactor = driver.ScaleFactor;

        GraphicsDevice.Initialize(driver);
        GameControllers.Initialize();

        return GraphicsDevice.EC == 0 ? SDL_AppResult.SDL_APP_CONTINUE : SDL_AppResult.SDL_APP_FAILURE;
    }

    static unsafe SDL_AppResult AppIterate(IntPtr _)
    {
        Window.OnIterate();
        return SDL_AppResult.SDL_APP_CONTINUE;
    }

    static unsafe SDL_AppResult AppEvent(IntPtr _, SDL_Event* pEvt)
    {

        switch ((SDL_EventType)pEvt->type)
        {
            case SDL_EventType.SDL_EVENT_QUIT:
                return SDL_AppResult.SDL_APP_SUCCESS;
            case SDL_EventType.SDL_EVENT_WINDOW_RESIZED:
                {
                    var w = (int)(pEvt->window.data1 / ScaleFactor);
                    var h = (int)(pEvt->window.data2 / ScaleFactor);
                    Window.OnResized(w, h);
                }
                break;
            case SDL_EventType.SDL_EVENT_MOUSE_MOTION:
                {
                    var x = (int)(pEvt->motion.x / ScaleFactor);
                    var y = (int)(pEvt->motion.y / ScaleFactor);
                    Window.OnMouseMoved(x, y, (int)pEvt->motion.xrel, (int)pEvt->motion.yrel);
                }
                break;
            case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN:
            case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP:
                {
                    var x = (int)(pEvt->button.x / ScaleFactor);
                    var y = (int)(pEvt->button.y / ScaleFactor);
                    Window.OnMouseButtonChanged(x, y, pEvt->button.down);
                }
                break;
            case SDL_EventType.SDL_EVENT_MOUSE_WHEEL:
                {
                    var x = (int)(pEvt->wheel.mouse_x / ScaleFactor);
                    var y = (int)(pEvt->wheel.mouse_y / ScaleFactor);
                    var delta = (int)(120 * pEvt->wheel.y);
                    Window.OnMouseWheelChanged(x, y, delta);
                }
                break;
            case SDL_EventType.SDL_EVENT_KEY_DOWN:
            case SDL_EventType.SDL_EVENT_KEY_UP:
                Window.OnKeyboardKeyPressed((ushort)ToKeyboardKey(pEvt->key.scancode), pEvt->key.down);
                break;
        }
        return SDL_AppResult.SDL_APP_CONTINUE;
    }

    static KeyboardKey ToKeyboardKey(SDL_Scancode scancode)
      => scancode switch
      {
          SDL_Scancode.SDL_SCANCODE_UP          => KeyboardKey.Up,
          SDL_Scancode.SDL_SCANCODE_DOWN        => KeyboardKey.Down,
          SDL_Scancode.SDL_SCANCODE_LEFT        => KeyboardKey.Left,
          SDL_Scancode.SDL_SCANCODE_RIGHT       => KeyboardKey.Right,
          SDL_Scancode.SDL_SCANCODE_RETURN      => KeyboardKey.Enter,
          SDL_Scancode.SDL_SCANCODE_RETURN2     => KeyboardKey.Enter,
          SDL_Scancode.SDL_SCANCODE_ESCAPE      => KeyboardKey.Escape,

          SDL_Scancode.SDL_SCANCODE_E           => KeyboardKey.E,
          SDL_Scancode.SDL_SCANCODE_H           => KeyboardKey.H,
          SDL_Scancode.SDL_SCANCODE_P           => KeyboardKey.P,
          SDL_Scancode.SDL_SCANCODE_Q           => KeyboardKey.Q,
          SDL_Scancode.SDL_SCANCODE_R           => KeyboardKey.R,
          SDL_Scancode.SDL_SCANCODE_S           => KeyboardKey.S,
          SDL_Scancode.SDL_SCANCODE_W           => KeyboardKey.W,
          SDL_Scancode.SDL_SCANCODE_X           => KeyboardKey.X,
          SDL_Scancode.SDL_SCANCODE_Z           => KeyboardKey.Z,

          SDL_Scancode.SDL_SCANCODE_F1          => KeyboardKey.F1,
          SDL_Scancode.SDL_SCANCODE_F2          => KeyboardKey.F2,
          SDL_Scancode.SDL_SCANCODE_F3          => KeyboardKey.F3,
          SDL_Scancode.SDL_SCANCODE_F4          => KeyboardKey.F4,

          SDL_Scancode.SDL_SCANCODE_KP_0        => KeyboardKey.NumberPad0,
          SDL_Scancode.SDL_SCANCODE_KP_1        => KeyboardKey.NumberPad1,
          SDL_Scancode.SDL_SCANCODE_KP_2        => KeyboardKey.NumberPad2,
          SDL_Scancode.SDL_SCANCODE_KP_3        => KeyboardKey.NumberPad3,
          SDL_Scancode.SDL_SCANCODE_KP_4        => KeyboardKey.NumberPad4,
          SDL_Scancode.SDL_SCANCODE_KP_5        => KeyboardKey.NumberPad5,
          SDL_Scancode.SDL_SCANCODE_KP_6        => KeyboardKey.NumberPad6,
          SDL_Scancode.SDL_SCANCODE_KP_7        => KeyboardKey.NumberPad7,
          SDL_Scancode.SDL_SCANCODE_KP_8        => KeyboardKey.NumberPad8,
          SDL_Scancode.SDL_SCANCODE_KP_9        => KeyboardKey.NumberPad9,
          SDL_Scancode.SDL_SCANCODE_KP_MULTIPLY => KeyboardKey.Multiply,
          SDL_Scancode.SDL_SCANCODE_KP_PLUS     => KeyboardKey.Add,

          SDL_Scancode.SDL_SCANCODE_PAGEDOWN    => KeyboardKey.PageDown,
          SDL_Scancode.SDL_SCANCODE_PAGEUP      => KeyboardKey.PageUp,

          SDL_Scancode.SDL_SCANCODE_UNKNOWN     => KeyboardKey.None,
          _                                     => KeyboardKey.None
      };

    static unsafe void AppQuit(IntPtr _, SDL_AppResult result)
    {
        GameControllers.Shutdown();
        GraphicsDevice.Shutdown();
    }
}