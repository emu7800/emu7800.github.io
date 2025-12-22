// © Mike Murphy

using EMU7800.Core;
using EMU7800.Shell;
using System;
using System.Collections.Concurrent;
using System.Threading;
using static EMU7800.SDL3.Interop.SDL3;

namespace EMU7800.SDL3.Interop;

public record SDLWindowDevices(Window Window,
    GraphicsDeviceSDL3Driver SDL3GraphicsDevice,
    IAudioDeviceDriver AudioDevice,
    GameControllersSDL3InputDriver SDL3GameControllers,
    ILogger Logger)
    : WindowDevices(Window, SDL3GraphicsDevice, AudioDevice, SDL3GameControllers);

public sealed class WindowSDL3Driver : IWindowDriver
{
    static readonly ConcurrentDictionary<IntPtr, SDLWindowDevices> RegisteredWindows = [];
    static readonly ConcurrentQueue<IntPtr> PendingIds = [];
    static int _nextId;

    readonly ILogger _logger;

    #region IWindowDriver Members

    public unsafe void Start(Window window, bool startMaximized)
    {
        SDLWindowDevices devices = new(
            window,
            new GraphicsDeviceSDL3Driver(_logger, startMaximized),
            new AudioDeviceSDL3Driver(),
            new GameControllersSDL3InputDriver(window, _logger),
            _logger);

        window.OnAudioChanged(devices.AudioDevice);
        window.OnControllersChanged(devices.GameControllers);

        devices.SDL3GameControllers.Initialize();

        IntPtr id = Interlocked.Increment(ref _nextId);

        PendingIds.Enqueue(id);
        RegisteredWindows.TryAdd(id, devices);

        SDL_EnterAppMainCallbacks(0, IntPtr.Zero, AppInit, AppIterate, AppEvent, AppQuit);
    }

    #endregion

    #region Constructors

    public WindowSDL3Driver(ILogger logger)
      => _logger = logger;

    #endregion

    #region Helpers

    static unsafe SDL_AppResult AppInit(IntPtr ppAppState, int argc, IntPtr argv)
    {
        if (!PendingIds.TryDequeue(out var id) || !RegisteredWindows.TryGetValue(id, out var wd))
        {
            return SDL_AppResult.SDL_APP_FAILURE;
        }

        *(void**)ppAppState = (void*)id;

        SDL_SetAppMetadata(VersionInfo.EMU7800, VersionInfo.AssemblyVersion, "https//emu7800.net");

        wd.Logger.Log(3, $"Using SDL3: Version: {SDL_GetVersion()} Revision: {SDL_GetRevision()}");

        // Windows:

        // Version  Revision
        // 3002026  release-3.2.26-0-gbadbf8da4 (libsdl.org)

        wd.SDL3GameControllers.Initialize();

        return wd.GraphicsDevice.HR == 0 ? SDL_AppResult.SDL_APP_CONTINUE : SDL_AppResult.SDL_APP_FAILURE;
    }

    static unsafe SDL_AppResult AppIterate(IntPtr pAppState)
    {
        if (!RegisteredWindows.TryGetValue(pAppState, out var wd))
        {
            return SDL_AppResult.SDL_APP_FAILURE;
        }

        if (!wd.Window.OnIterate(wd.GraphicsDevice, wd.GameControllers))
        {
            return SDL_AppResult.SDL_APP_FAILURE;
        }

        return SDL_AppResult.SDL_APP_CONTINUE;
    }

    static unsafe SDL_AppResult AppEvent(IntPtr pAppState, SDL_Event* pEvt)
    {
        if (!RegisteredWindows.TryGetValue(pAppState, out var wd))
        {
            return SDL_AppResult.SDL_APP_FAILURE;
        }

        switch ((SDL_EventType)pEvt->type)
        {
            case SDL_EventType.SDL_EVENT_QUIT:
                return SDL_AppResult.SDL_APP_SUCCESS;
            case SDL_EventType.SDL_EVENT_WINDOW_RESIZED:
                {
                    var w = (int)(pEvt->window.data1 / wd.SDL3GraphicsDevice.ScaleFactor);
                    var h = (int)(pEvt->window.data2 / wd.SDL3GraphicsDevice.ScaleFactor);
                    wd.Window.OnResized(wd.GraphicsDevice, w, h);
                }
                break;
            case SDL_EventType.SDL_EVENT_MOUSE_MOTION:
                {
                    var x = (int)(pEvt->motion.x / wd.SDL3GraphicsDevice.ScaleFactor);
                    var y = (int)(pEvt->motion.y / wd.SDL3GraphicsDevice.ScaleFactor);
                    wd.Window.OnMouseMoved(x, y, (int)pEvt->motion.xrel, (int)pEvt->motion.yrel);
                }
                break;
            case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN:
            case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP:
                {
                    var x = (int)(pEvt->button.x / wd.SDL3GraphicsDevice.ScaleFactor);
                    var y = (int)(pEvt->button.y / wd.SDL3GraphicsDevice.ScaleFactor);
                    wd.Window.OnMouseButtonChanged(x, y, pEvt->button.down);
                }
                break;
            case SDL_EventType.SDL_EVENT_MOUSE_WHEEL:
                {
                    var x = (int)(pEvt->wheel.mouse_x / wd.SDL3GraphicsDevice.ScaleFactor);
                    var y = (int)(pEvt->wheel.mouse_y / wd.SDL3GraphicsDevice.ScaleFactor);
                    var delta = (int)(120 * pEvt->wheel.y);
                    wd.Window.OnMouseWheelChanged(x, y, delta);
                }
                break;
            case SDL_EventType.SDL_EVENT_KEY_DOWN:
            case SDL_EventType.SDL_EVENT_KEY_UP:
                {
                    wd.Window.OnKeyboardKeyPressed((ushort)ToKeyboardKey(pEvt->key.scancode), pEvt->key.down);
                }
                break;
            case SDL_EventType.SDL_EVENT_GAMEPAD_ADDED:
                {
                    wd.SDL3GameControllers.AddController(pEvt->gdevice.which);
                }
                break;
            case SDL_EventType.SDL_EVENT_GAMEPAD_REMOVED:
                {
                    wd.SDL3GameControllers.RemoveController(pEvt->gdevice.which);
                }
                break;
            case SDL_EventType.SDL_EVENT_GAMEPAD_AXIS_MOTION:
                break;
            case SDL_EventType.SDL_EVENT_GAMEPAD_BUTTON_DOWN:
            case SDL_EventType.SDL_EVENT_GAMEPAD_BUTTON_UP:
                {
                    switch (pEvt->gbutton.button)
                    {
                        case (byte)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_UP:
                            wd.SDL3GameControllers.ButtonChanged(pEvt->gbutton.which, MachineInput.Up, pEvt->gbutton.down);
                            break;
                        case (byte)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_DOWN:
                            wd.SDL3GameControllers.ButtonChanged(pEvt->gbutton.which, MachineInput.Down, pEvt->gbutton.down);
                            break;
                        case (byte)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_LEFT:
                            wd.SDL3GameControllers.ButtonChanged(pEvt->gbutton.which, MachineInput.Left, pEvt->gbutton.down);
                            break;
                        case (byte)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_DPAD_RIGHT:
                            wd.SDL3GameControllers.ButtonChanged(pEvt->gbutton.which, MachineInput.Right, pEvt->gbutton.down);
                            break;
                        case (byte)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_SOUTH:
                            wd.SDL3GameControllers.ButtonChanged(pEvt->gbutton.which, MachineInput.Fire, pEvt->gbutton.down);
                            break;
                        case (byte)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_EAST:
                            wd.SDL3GameControllers.ButtonChanged(pEvt->gbutton.which, MachineInput.Fire2, pEvt->gbutton.down);
                            break;
                        case (byte)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_NORTH:
                            wd.SDL3GameControllers.ButtonChanged(pEvt->gbutton.which, MachineInput.Fire2, pEvt->gbutton.down);
                            break;
                        case (byte)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_WEST:
                            wd.SDL3GameControllers.ButtonChanged(pEvt->gbutton.which, MachineInput.Fire, pEvt->gbutton.down);
                            break;
                        case (byte)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_START:
                            wd.SDL3GameControllers.ButtonChanged(pEvt->gbutton.which, MachineInput.Start, pEvt->gbutton.down);
                            break;
                        case (byte)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_LEFT_SHOULDER:
                            wd.SDL3GameControllers.ButtonChanged(pEvt->gbutton.which, MachineInput.Select, pEvt->gbutton.down);
                            break;
                        case (byte)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_RIGHT_SHOULDER:
                            wd.SDL3GameControllers.ButtonChanged(pEvt->gbutton.which, MachineInput.Reset, pEvt->gbutton.down);
                            break;
                        case (byte)SDL_GamepadButton.SDL_GAMEPAD_BUTTON_BACK:
                            wd.SDL3GameControllers.ButtonChanged(pEvt->gbutton.which, MachineInput.End, pEvt->gbutton.down);
                            break;
                    }
                }
                break;
        }

        return SDL_AppResult.SDL_APP_CONTINUE;
    }

    static unsafe void AppQuit(IntPtr pAppState, SDL_AppResult result)
    {
        if (RegisteredWindows.TryRemove(pAppState, out var wd))
        {
            wd.GraphicsDevice.Shutdown();
            wd.SDL3GameControllers.Shutdown();
            wd.AudioDevice.Close();
        }
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
          SDL_Scancode.SDL_SCANCODE_SLASH       => KeyboardKey.Help,

          SDL_Scancode.SDL_SCANCODE_A           => KeyboardKey.A,
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

    static void Info(SDLWindowDevices wd, string message)
      => wd.Logger.Log(3, message);

    #endregion
}
