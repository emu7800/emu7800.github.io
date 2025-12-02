using EMU7800.Shell;
using System;

using static EMU7800.SDL3.Interop.SDL3;

namespace EMU7800.SDL3.Interop;

public sealed class WindowSDL3Driver : IWindowDriver
{
    public static WindowSDL3Driver Factory() => new();
    WindowSDL3Driver() {}

    #region IWindowDriver Members

    public unsafe void StartWindowAndProcessEvents(bool startMaximized)
    {
        AudioDevice.DriverFactory     = AudioDeviceSDL3Driver.Factory;
        GraphicsDevice.DriverFactory  = GraphicsDeviceSDL3Driver.Factory;
        GameControllers.DriverFactory = GameControllersSDL3InputDriver.Factory;

        SDL_EnterAppMainCallbacks(0, IntPtr.Zero, AppInit, AppIterate, AppEvent, AppQuit);
    }

    #endregion

    static unsafe SDL_AppResult AppInit(IntPtr _, int argc, IntPtr argv)
    {
        Console.WriteLine($"Using SDL3: Version: {SDL_GetVersion()} Revision: {SDL_GetRevision()}");

        SDL_SetAppMetadata(VersionInfo.EMU7800, VersionInfo.AssemblyVersion, "https//emu7800.net");

        if (!SDL_Init(SDL_InitFlags.SDL_INIT_TIMER | SDL_InitFlags.SDL_INIT_VIDEO | SDL_InitFlags.SDL_INIT_AUDIO | SDL_InitFlags.SDL_INIT_GAMEPAD))
        {
            SDL_Log($"Couldn't initialize SDL: Init: {SDL_GetError()}");
            return SDL_AppResult.SDL_APP_FAILURE;
        }

        GraphicsDevice.Initialize();
        GameControllers.Initialize();

        return SDL_AppResult.SDL_APP_CONTINUE;
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
            case SDL_EventType.SDL_EVENT_KEY_DOWN:
                switch (pEvt->key.scancode)
                {
                    case SDL_Scancode.SDL_SCANCODE_ESCAPE:
                        return SDL_AppResult.SDL_APP_SUCCESS;
                }
                break;
            case SDL_EventType.SDL_EVENT_WINDOW_RESIZED:
                var w = pEvt->window.data1;
                var h = pEvt->window.data2;
                Console.WriteLine($"SDL_EVENT_WINDOW_RESIZED: {w}x{h}");
                Window.OnResized(w, h);
                break;
        }
        return SDL_AppResult.SDL_APP_CONTINUE;
    }

    static unsafe void AppQuit(IntPtr _, SDL_AppResult result)
    {
        GameControllers.Shutdown();
        GraphicsDevice.Shutdown();
    }
}