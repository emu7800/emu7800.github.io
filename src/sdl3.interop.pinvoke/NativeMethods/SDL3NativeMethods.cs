// Adapted from https://github.com/flibitijibibo/SDL3-CS

namespace EMU7800.SDL3.Interop;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

internal static unsafe partial class SDL3
{
    // Custom marshaller for SDL-owned strings returned by SDL.
    [CustomMarshaller(typeof(string), MarshalMode.ManagedToUnmanagedOut, typeof(SDLOwnedStringMarshaller))]
    internal static unsafe class SDLOwnedStringMarshaller
    {
        /// <summary>
        /// Converts an unmanaged string to a managed version.
        /// </summary>
        /// <returns>A managed string.</returns>
        internal static string ConvertToManaged(byte* unmanaged)
          => Marshal.PtrToStringUTF8((IntPtr)unmanaged) ?? string.Empty;
    }

    // Custom marshaller for caller-owned strings returned by SDL.
    [CustomMarshaller(typeof(string), MarshalMode.ManagedToUnmanagedOut, typeof(CallerOwnedStringMarshaller))]
    internal static unsafe class CallerOwnedStringMarshaller
    {
        /// <summary>
        /// Converts an unmanaged string to a managed version.
        /// </summary>
        /// <returns>A managed string.</returns>
        internal static string ConvertToManaged(byte* unmanaged)
          => Marshal.PtrToStringUTF8((IntPtr)unmanaged) ?? string.Empty;

        /// <summary>
        /// Free the memory for a specified unmanaged string.
        /// </summary>
        internal static void Free(byte* unmanaged)
          => SDL_free((IntPtr) unmanaged);
    }

    // Taken from https://github.com/ppy/SDL3-CS
    // C# bools are not blittable, so we need this workaround
    public readonly record struct SDLBool
    {
        readonly byte _value;

        const byte
            FALSE_VALUE = 0,
            TRUE_VALUE  = 1;

        internal SDLBool(byte value) => _value = value;

        public static implicit operator bool(SDLBool b) => b._value != FALSE_VALUE;

        public static implicit operator SDLBool(bool b) => new(b ? TRUE_VALUE : FALSE_VALUE);

        public bool Equals(SDLBool other) => other._value == _value;

        public override int GetHashCode() => _value.GetHashCode();
    }

    const string
        SDL3DllName      = "SDL3",
        SDL3DllimageName = "SDL3_image",
        SDL3DllttfName   = "SDL3_ttf";

    // /usr/local/include/SDL3/SDL_stdinc.h

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_malloc(UIntPtr size);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_free(IntPtr mem);

    // /usr/local/include/SDL3/SDL_error.h

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetError();

    // /usr/local/include/SDL3/SDL_audio.h

    public enum SDL_AudioFormat
    {
        SDL_AUDIO_UNKNOWN = 0,
        SDL_AUDIO_U8      = 8,
        SDL_AUDIO_S8      = 32776,
        SDL_AUDIO_S16LE   = 32784,
        SDL_AUDIO_S16BE   = 36880,
        SDL_AUDIO_S32LE   = 32800,
        SDL_AUDIO_S32BE   = 36896,
        SDL_AUDIO_F32LE   = 33056,
        SDL_AUDIO_F32BE   = 37152,
        SDL_AUDIO_S16     = 32784,
        SDL_AUDIO_S32     = 32800,
        SDL_AUDIO_F32     = 33056,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_AudioSpec
    {
        public SDL_AudioFormat format;
        public int channels;
        public int freq;
    }

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetAudioStreamGain(IntPtr stream, float gain);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_PutAudioStreamData(IntPtr stream, ReadOnlySpan<byte> buffer, int len);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_GetAudioStreamQueued(IntPtr stream);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_ResumeAudioStreamDevice(IntPtr stream);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_DestroyAudioStream(IntPtr stream);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_OpenAudioDeviceStream(uint devid, ref SDL_AudioSpec spec, IntPtr callback, IntPtr userdata);

    // /usr/local/include/SDL3/SDL_pixels.h

    public enum SDL_PixelFormat
    {
        SDL_PIXELFORMAT_UNKNOWN       = 0,
        SDL_PIXELFORMAT_INDEX1LSB     = 286261504,
        SDL_PIXELFORMAT_INDEX1MSB     = 287310080,
        SDL_PIXELFORMAT_INDEX2LSB     = 470811136,
        SDL_PIXELFORMAT_INDEX2MSB     = 471859712,
        SDL_PIXELFORMAT_INDEX4LSB     = 303039488,
        SDL_PIXELFORMAT_INDEX4MSB     = 304088064,
        SDL_PIXELFORMAT_INDEX8        = 318769153,
        SDL_PIXELFORMAT_RGB332        = 336660481,
        SDL_PIXELFORMAT_XRGB4444      = 353504258,
        SDL_PIXELFORMAT_XBGR4444      = 357698562,
        SDL_PIXELFORMAT_XRGB1555      = 353570562,
        SDL_PIXELFORMAT_XBGR1555      = 357764866,
        SDL_PIXELFORMAT_ARGB4444      = 355602434,
        SDL_PIXELFORMAT_RGBA4444      = 356651010,
        SDL_PIXELFORMAT_ABGR4444      = 359796738,
        SDL_PIXELFORMAT_BGRA4444      = 360845314,
        SDL_PIXELFORMAT_ARGB1555      = 355667970,
        SDL_PIXELFORMAT_RGBA5551      = 356782082,
        SDL_PIXELFORMAT_ABGR1555      = 359862274,
        SDL_PIXELFORMAT_BGRA5551      = 360976386,
        SDL_PIXELFORMAT_RGB565        = 353701890,
        SDL_PIXELFORMAT_BGR565        = 357896194,
        SDL_PIXELFORMAT_RGB24         = 386930691,
        SDL_PIXELFORMAT_BGR24         = 390076419,
        SDL_PIXELFORMAT_XRGB8888      = 370546692,
        SDL_PIXELFORMAT_RGBX8888      = 371595268,
        SDL_PIXELFORMAT_XBGR8888      = 374740996,
        SDL_PIXELFORMAT_BGRX8888      = 375789572,
        SDL_PIXELFORMAT_ARGB8888      = 372645892,
        SDL_PIXELFORMAT_RGBA8888      = 373694468,
        SDL_PIXELFORMAT_ABGR8888      = 376840196,
        SDL_PIXELFORMAT_BGRA8888      = 377888772,
        SDL_PIXELFORMAT_XRGB2101010   = 370614276,
        SDL_PIXELFORMAT_XBGR2101010   = 374808580,
        SDL_PIXELFORMAT_ARGB2101010   = 372711428,
        SDL_PIXELFORMAT_ABGR2101010   = 376905732,
        SDL_PIXELFORMAT_RGB48         = 403714054,
        SDL_PIXELFORMAT_BGR48         = 406859782,
        SDL_PIXELFORMAT_RGBA64        = 404766728,
        SDL_PIXELFORMAT_ARGB64        = 405815304,
        SDL_PIXELFORMAT_BGRA64        = 407912456,
        SDL_PIXELFORMAT_ABGR64        = 408961032,
        SDL_PIXELFORMAT_RGB48_FLOAT   = 437268486,
        SDL_PIXELFORMAT_BGR48_FLOAT   = 440414214,
        SDL_PIXELFORMAT_RGBA64_FLOAT  = 438321160,
        SDL_PIXELFORMAT_ARGB64_FLOAT  = 439369736,
        SDL_PIXELFORMAT_BGRA64_FLOAT  = 441466888,
        SDL_PIXELFORMAT_ABGR64_FLOAT  = 442515464,
        SDL_PIXELFORMAT_RGB96_FLOAT   = 454057996,
        SDL_PIXELFORMAT_BGR96_FLOAT   = 457203724,
        SDL_PIXELFORMAT_RGBA128_FLOAT = 455114768,
        SDL_PIXELFORMAT_ARGB128_FLOAT = 456163344,
        SDL_PIXELFORMAT_BGRA128_FLOAT = 458260496,
        SDL_PIXELFORMAT_ABGR128_FLOAT = 459309072,
        SDL_PIXELFORMAT_YV12          = 842094169,
        SDL_PIXELFORMAT_IYUV          = 1448433993,
        SDL_PIXELFORMAT_YUY2          = 844715353,
        SDL_PIXELFORMAT_UYVY          = 1498831189,
        SDL_PIXELFORMAT_YVYU          = 1431918169,
        SDL_PIXELFORMAT_NV12          = 842094158,
        SDL_PIXELFORMAT_NV21          = 825382478,
        SDL_PIXELFORMAT_P010          = 808530000,
        SDL_PIXELFORMAT_EXTERNAL_OES  = 542328143,
        SDL_PIXELFORMAT_RGBA32        = 376840196,
        SDL_PIXELFORMAT_ARGB32        = 377888772,
        SDL_PIXELFORMAT_BGRA32        = 372645892,
        SDL_PIXELFORMAT_ABGR32        = 373694468,
        SDL_PIXELFORMAT_RGBX32        = 374740996,
        SDL_PIXELFORMAT_XRGB32        = 375789572,
        SDL_PIXELFORMAT_BGRX32        = 370546692,
        SDL_PIXELFORMAT_XBGR32        = 371595268,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_Color
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_FColor
    {
        public float r;
        public float g;
        public float b;
        public float a;
    }

    // /usr/local/include/SDL3/SDL_rect.h

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_Point
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_FPoint
    {
        public float x;
        public float y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_Rect
    {
        public int x;
        public int y;
        public int w;
        public int h;
        public static implicit operator SDL_Rect(Shell.RectF r) => new() { x = (int)r.Left, y = (int)r.Top, w = (int)r.Right - (int)r.Left, h = (int)r.Bottom - (int)r.Top };
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_FRect
    {
        public float x;
        public float y;
        public float w;
        public float h;
        public static implicit operator SDL_FRect(Shell.RectF r) => new() { x = r.Left, y = r.Top, w = r.Right - r.Left, h = r.Bottom - r.Top };
    }

    // /usr/local/include/SDL3/SDL_surface.h

    [Flags]
    public enum SDL_SurfaceFlags : uint
    {
        SDL_SURFACE_PREALLOCATED = 0x1,
        SDL_SURFACE_LOCK_NEEDED  = 0x2,
        SDL_SURFACE_LOCKED       = 0x4,
        SDL_SURFACE_SIMD_ALIGNED = 0x08,
    }

    public enum SDL_ScaleMode
    {
        SDL_SCALEMODE_NEAREST = 0,
        SDL_SCALEMODE_LINEAR  = 1,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_Surface
    {
        public SDL_SurfaceFlags flags;
        public SDL_PixelFormat format;
        public int w;
        public int h;
        public int pitch;
        public IntPtr pixels;
        public int refcount;
        public IntPtr reserved;
    }

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_DestroySurface(IntPtr surface);

    [LibraryImport(SDL3DllimageName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Surface* IMG_LoadPNG_IO(IntPtr src, SDLBool closeio);

    // /usr/local/include/SDL3/SDL_video.h

    [Flags]
    public enum SDL_WindowFlags : ulong
    {
        SDL_WINDOW_FULLSCREEN           = 0x1,
        SDL_WINDOW_OPENGL               = 0x2,
        SDL_WINDOW_OCCLUDED             = 0x4,
        SDL_WINDOW_HIDDEN               = 0x08,
        SDL_WINDOW_BORDERLESS           = 0x10,
        SDL_WINDOW_RESIZABLE            = 0x20,
        SDL_WINDOW_MINIMIZED            = 0x40,
        SDL_WINDOW_MAXIMIZED            = 0x080,
        SDL_WINDOW_MOUSE_GRABBED        = 0x100,
        SDL_WINDOW_INPUT_FOCUS          = 0x200,
        SDL_WINDOW_MOUSE_FOCUS          = 0x400,
        SDL_WINDOW_EXTERNAL             = 0x0800,
        SDL_WINDOW_MODAL                = 0x1000,
        SDL_WINDOW_HIGH_PIXEL_DENSITY   = 0x2000,
        SDL_WINDOW_MOUSE_CAPTURE        = 0x4000,
        SDL_WINDOW_MOUSE_RELATIVE_MODE  = 0x08000,
        SDL_WINDOW_ALWAYS_ON_TOP        = 0x10000,
        SDL_WINDOW_UTILITY              = 0x20000,
        SDL_WINDOW_TOOLTIP              = 0x40000,
        SDL_WINDOW_POPUP_MENU           = 0x080000,
        SDL_WINDOW_KEYBOARD_GRABBED     = 0x100000,
        SDL_WINDOW_VULKAN               = 0x10000000,
        SDL_WINDOW_METAL                = 0x20000000,
        SDL_WINDOW_TRANSPARENT          = 0x40000000,
        SDL_WINDOW_NOT_FOCUSABLE        = 0x080000000,
    }

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetDisplayBounds(uint displayID, out SDL_Rect rect);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetFullscreenDisplayModes(uint displayID, out int count);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_GetDisplayForWindow(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial float SDL_GetWindowDisplayScale(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetWindows(out int count);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetWindowPosition(IntPtr window, int x, int y);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetWindowSize(IntPtr window, int w, int h);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_DestroyWindow(IntPtr window);

    // /usr/local/include/SDL3/SDL_guid.h

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GUID
    {
        public fixed byte data[16];
    }

    // /usr/local/include/SDL3/SDL_joystick.h

    public const string
        SDL_PROP_JOYSTICK_CAP_MONO_LED_BOOLEAN       = "SDL.joystick.cap.mono_led",
        SDL_PROP_JOYSTICK_CAP_RGB_LED_BOOLEAN        = "SDL.joystick.cap.rgb_led",
        SDL_PROP_JOYSTICK_CAP_PLAYER_LED_BOOLEAN     = "SDL.joystick.cap.player_led",
        SDL_PROP_JOYSTICK_CAP_RUMBLE_BOOLEAN         = "SDL.joystick.cap.rumble",
        SDL_PROP_JOYSTICK_CAP_TRIGGER_RUMBLE_BOOLEAN = "SDL.joystick.cap.trigger_rumble"
        ;

    public enum SDL_JoystickType
    {
        SDL_JOYSTICK_TYPE_UNKNOWN        = 0,
        SDL_JOYSTICK_TYPE_GAMEPAD        = 1,
        SDL_JOYSTICK_TYPE_WHEEL          = 2,
        SDL_JOYSTICK_TYPE_ARCADE_STICK   = 3,
        SDL_JOYSTICK_TYPE_FLIGHT_STICK   = 4,
        SDL_JOYSTICK_TYPE_DANCE_PAD      = 5,
        SDL_JOYSTICK_TYPE_GUITAR         = 6,
        SDL_JOYSTICK_TYPE_DRUM_KIT       = 7,
        SDL_JOYSTICK_TYPE_ARCADE_PAD     = 8,
        SDL_JOYSTICK_TYPE_THROTTLE       = 9,
        SDL_JOYSTICK_TYPE_COUNT          = 10,
    }

    public enum SDL_JoystickConnectionState
    {
        SDL_JOYSTICK_CONNECTION_INVALID  = -1,
        SDL_JOYSTICK_CONNECTION_UNKNOWN  = 0,
        SDL_JOYSTICK_CONNECTION_WIRED    = 1,
        SDL_JOYSTICK_CONNECTION_WIRELESS = 2,
    }

    public enum SDL_Hat
    {
        SDL_HAT_CENTERED                 = 0,
        SDL_HAT_UP                       = 1,
        SDL_HAT_RIGHT                    = 2,
        SDL_HAT_DOWN                     = 4,
        SDL_HAT_LEFT                     = 8,
        SDL_HAT_RIGHTUP                  = 2+1,
        SDL_HAT_RIGHTDOWN                = 2+4,
        SDL_HAT_LEFTUP                   = 8+1,
        SDL_HAT_LEFTDOWN                 = 8+4,
    }

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_LockJoysticks();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_UnlockJoysticks();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_HasJoystick();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetJoysticks(out int count);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetJoystickNameForID(uint instance_id);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetJoystickPathForID(uint instance_id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_GetJoystickPlayerIndexForID(uint instance_id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_GUID SDL_GetJoystickGUIDForID(uint instance_id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ushort SDL_GetJoystickVendorForID(uint instance_id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ushort SDL_GetJoystickProductForID(uint instance_id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ushort SDL_GetJoystickProductVersionForID(uint instance_id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_JoystickType SDL_GetJoystickTypeForID(uint instance_id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_OpenJoystick(uint instance_id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetJoystickFromID(uint instance_id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetJoystickFromPlayerIndex(int player_index);

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_VirtualJoystickTouchpadDesc
    {
        public ushort nfingers;
        public fixed ushort padding[3];
    }

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_DetachVirtualJoystick(uint instance_id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_IsJoystickVirtual(uint instance_id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetJoystickVirtualAxis(IntPtr joystick, int axis, short value);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetJoystickVirtualBall(IntPtr joystick, int ball, short xrel, short yrel);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetJoystickVirtualButton(IntPtr joystick, int button, SDLBool down);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetJoystickVirtualHat(IntPtr joystick, int hat, byte value);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetJoystickVirtualTouchpad(IntPtr joystick, int touchpad, int finger, SDLBool down, float x, float y, float pressure);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_GetJoystickProperties(IntPtr joystick);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetJoystickName(IntPtr joystick);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetJoystickPath(IntPtr joystick);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_GetJoystickPlayerIndex(IntPtr joystick);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetJoystickPlayerIndex(IntPtr joystick, int player_index);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_GUID SDL_GetJoystickGUID(IntPtr joystick);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ushort SDL_GetJoystickVendor(IntPtr joystick);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ushort SDL_GetJoystickProduct(IntPtr joystick);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ushort SDL_GetJoystickProductVersion(IntPtr joystick);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ushort SDL_GetJoystickFirmwareVersion(IntPtr joystick);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetJoystickSerial(IntPtr joystick);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_JoystickType SDL_GetJoystickType(IntPtr joystick);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_GetJoystickGUIDInfo(SDL_GUID guid, out ushort vendor, out ushort product, out ushort version, out ushort crc16);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_JoystickConnected(IntPtr joystick);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_GetJoystickID(IntPtr joystick);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_GetNumJoystickAxes(IntPtr joystick);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_GetNumJoystickBalls(IntPtr joystick);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_GetNumJoystickHats(IntPtr joystick);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_GetNumJoystickButtons(IntPtr joystick);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_SetJoystickEventsEnabled(SDLBool enabled);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_JoystickEventsEnabled();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_UpdateJoysticks();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial short SDL_GetJoystickAxis(IntPtr joystick, int axis);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetJoystickAxisInitialState(IntPtr joystick, int axis, out short state);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetJoystickBall(IntPtr joystick, int ball, out int dx, out int dy);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial byte SDL_GetJoystickHat(IntPtr joystick, int hat);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetJoystickButton(IntPtr joystick, int button);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RumbleJoystick(IntPtr joystick, ushort low_frequency_rumble, ushort high_frequency_rumble, uint duration_ms);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RumbleJoystickTriggers(IntPtr joystick, ushort left_rumble, ushort right_rumble, uint duration_ms);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetJoystickLED(IntPtr joystick, byte red, byte green, byte blue);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SendJoystickEffect(IntPtr joystick, IntPtr data, int size);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_CloseJoystick(IntPtr joystick);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_JoystickConnectionState SDL_GetJoystickConnectionState(IntPtr joystick);

    // /usr/local/include/SDL3/SDL_gamepad.h

    public enum SDL_GamepadType
    {
        SDL_GAMEPAD_TYPE_UNKNOWN                      = 0,
        SDL_GAMEPAD_TYPE_STANDARD                     = 1,
        SDL_GAMEPAD_TYPE_XBOX360                      = 2,
        SDL_GAMEPAD_TYPE_XBOXONE                      = 3,
        SDL_GAMEPAD_TYPE_PS3                          = 4,
        SDL_GAMEPAD_TYPE_PS4                          = 5,
        SDL_GAMEPAD_TYPE_PS5                          = 6,
        SDL_GAMEPAD_TYPE_NINTENDO_SWITCH_PRO          = 7,
        SDL_GAMEPAD_TYPE_NINTENDO_SWITCH_JOYCON_LEFT  = 8,
        SDL_GAMEPAD_TYPE_NINTENDO_SWITCH_JOYCON_RIGHT = 9,
        SDL_GAMEPAD_TYPE_NINTENDO_SWITCH_JOYCON_PAIR  = 10,
        SDL_GAMEPAD_TYPE_COUNT                        = 11,
    }

    public enum SDL_GamepadButton
    {
        SDL_GAMEPAD_BUTTON_INVALID        = -1,
        SDL_GAMEPAD_BUTTON_SOUTH          = 0,
        SDL_GAMEPAD_BUTTON_EAST           = 1,
        SDL_GAMEPAD_BUTTON_WEST           = 2,
        SDL_GAMEPAD_BUTTON_NORTH          = 3,
        SDL_GAMEPAD_BUTTON_BACK           = 4,
        SDL_GAMEPAD_BUTTON_GUIDE          = 5,
        SDL_GAMEPAD_BUTTON_START          = 6,
        SDL_GAMEPAD_BUTTON_LEFT_STICK     = 7,
        SDL_GAMEPAD_BUTTON_RIGHT_STICK    = 8,
        SDL_GAMEPAD_BUTTON_LEFT_SHOULDER  = 9,
        SDL_GAMEPAD_BUTTON_RIGHT_SHOULDER = 10,
        SDL_GAMEPAD_BUTTON_DPAD_UP        = 11,
        SDL_GAMEPAD_BUTTON_DPAD_DOWN      = 12,
        SDL_GAMEPAD_BUTTON_DPAD_LEFT      = 13,
        SDL_GAMEPAD_BUTTON_DPAD_RIGHT     = 14,
        SDL_GAMEPAD_BUTTON_MISC1          = 15,
        SDL_GAMEPAD_BUTTON_RIGHT_PADDLE1  = 16,
        SDL_GAMEPAD_BUTTON_LEFT_PADDLE1   = 17,
        SDL_GAMEPAD_BUTTON_RIGHT_PADDLE2  = 18,
        SDL_GAMEPAD_BUTTON_LEFT_PADDLE2   = 19,
        SDL_GAMEPAD_BUTTON_TOUCHPAD       = 20,
        SDL_GAMEPAD_BUTTON_MISC2          = 21,
        SDL_GAMEPAD_BUTTON_MISC3          = 22,
        SDL_GAMEPAD_BUTTON_MISC4          = 23,
        SDL_GAMEPAD_BUTTON_MISC5          = 24,
        SDL_GAMEPAD_BUTTON_MISC6          = 25,
        SDL_GAMEPAD_BUTTON_COUNT          = 26,
    }

    public enum SDL_GamepadButtonLabel
    {
        SDL_GAMEPAD_BUTTON_LABEL_UNKNOWN  = 0,
        SDL_GAMEPAD_BUTTON_LABEL_A        = 1,
        SDL_GAMEPAD_BUTTON_LABEL_B        = 2,
        SDL_GAMEPAD_BUTTON_LABEL_X        = 3,
        SDL_GAMEPAD_BUTTON_LABEL_Y        = 4,
        SDL_GAMEPAD_BUTTON_LABEL_CROSS    = 5,
        SDL_GAMEPAD_BUTTON_LABEL_CIRCLE   = 6,
        SDL_GAMEPAD_BUTTON_LABEL_SQUARE   = 7,
        SDL_GAMEPAD_BUTTON_LABEL_TRIANGLE = 8,
    }

    public enum SDL_GamepadAxis
    {
        SDL_GAMEPAD_AXIS_INVALID       = -1,
        SDL_GAMEPAD_AXIS_LEFTX         = 0,
        SDL_GAMEPAD_AXIS_LEFTY         = 1,
        SDL_GAMEPAD_AXIS_RIGHTX        = 2,
        SDL_GAMEPAD_AXIS_RIGHTY        = 3,
        SDL_GAMEPAD_AXIS_LEFT_TRIGGER  = 4,
        SDL_GAMEPAD_AXIS_RIGHT_TRIGGER = 5,
        SDL_GAMEPAD_AXIS_COUNT         = 6,
    }

    public enum SDL_GamepadBindingType
    {
        SDL_GAMEPAD_BINDTYPE_NONE   = 0,
        SDL_GAMEPAD_BINDTYPE_BUTTON = 1,
        SDL_GAMEPAD_BINDTYPE_AXIS   = 2,
        SDL_GAMEPAD_BINDTYPE_HAT    = 3,
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct SDL_GamepadBinding
    {
        [FieldOffset(0)]
        public SDL_GamepadBindingType input_type;
        [FieldOffset(4)]
        public int input_button;
        [FieldOffset(4)]
        public INTERNAL_SDL_GamepadBinding_input_axis input_axis;
        [FieldOffset(4)]
        public INTERNAL_SDL_GamepadBinding_input_hat input_hat;
        [FieldOffset(16)]
        public SDL_GamepadBindingType output_type;
        [FieldOffset(20)]
        public SDL_GamepadButton output_button;
        [FieldOffset(20)]
        public INTERNAL_SDL_GamepadBinding_output_axis output_axis;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct INTERNAL_SDL_GamepadBinding_input_axis
    {
        public int axis;
        public int axis_min;
        public int axis_max;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct INTERNAL_SDL_GamepadBinding_input_hat
    {
        public int hat;
        public int hat_mask;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct INTERNAL_SDL_GamepadBinding_output_axis
    {
        public SDL_GamepadAxis axis;
        public int axis_min;
        public int axis_max;
    }

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_AddGamepadMapping(string mapping);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_AddGamepadMappingsFromIO(IntPtr src, SDLBool closeio);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_AddGamepadMappingsFromFile(string file);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_ReloadGamepadMappings();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetGamepadMappings(out int count);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(CallerOwnedStringMarshaller))]
    internal static partial string SDL_GetGamepadMappingForGUID(SDL_GUID guid);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(CallerOwnedStringMarshaller))]
    internal static partial string SDL_GetGamepadMapping(IntPtr gamepad);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetGamepadMapping(uint instance_id, string mapping);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_HasGamepad();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetGamepads(out int count);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_IsGamepad(uint instance_id);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetGamepadNameForID(uint instance_id);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetGamepadPathForID(uint instance_id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_GetGamepadPlayerIndexForID(uint instance_id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_GUID SDL_GetGamepadGUIDForID(uint instance_id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ushort SDL_GetGamepadVendorForID(uint instance_id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ushort SDL_GetGamepadProductForID(uint instance_id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ushort SDL_GetGamepadProductVersionForID(uint instance_id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_GamepadType SDL_GetGamepadTypeForID(uint instance_id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_GamepadType SDL_GetRealGamepadTypeForID(uint instance_id);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(CallerOwnedStringMarshaller))]
    internal static partial string SDL_GetGamepadMappingForID(uint instance_id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_OpenGamepad(uint instance_id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetGamepadFromID(uint instance_id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetGamepadFromPlayerIndex(int player_index);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_GetGamepadProperties(IntPtr gamepad);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_GetGamepadID(IntPtr gamepad);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetGamepadName(IntPtr gamepad);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetGamepadPath(IntPtr gamepad);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_GamepadType SDL_GetGamepadType(IntPtr gamepad);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_GamepadType SDL_GetRealGamepadType(IntPtr gamepad);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_GetGamepadPlayerIndex(IntPtr gamepad);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetGamepadPlayerIndex(IntPtr gamepad, int player_index);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ushort SDL_GetGamepadVendor(IntPtr gamepad);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ushort SDL_GetGamepadProduct(IntPtr gamepad);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ushort SDL_GetGamepadProductVersion(IntPtr gamepad);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ushort SDL_GetGamepadFirmwareVersion(IntPtr gamepad);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetGamepadSerial(IntPtr gamepad);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ulong SDL_GetGamepadSteamHandle(IntPtr gamepad);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_JoystickConnectionState SDL_GetGamepadConnectionState(IntPtr gamepad);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GamepadConnected(IntPtr gamepad);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetGamepadJoystick(IntPtr gamepad);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_SetGamepadEventsEnabled(SDLBool enabled);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GamepadEventsEnabled();

    internal static Span<IntPtr> SDL_GetGamepadBindings(IntPtr gamepad)
    {
        var result = SDL_GetGamepadBindings(gamepad, out var count);
        return new Span<IntPtr>((void*) result, count);
    }

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetGamepadBindings(IntPtr gamepad, out int count);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_UpdateGamepads();

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_GamepadType SDL_GetGamepadTypeFromString(string str);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetGamepadStringForType(SDL_GamepadType type);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_GamepadAxis SDL_GetGamepadAxisFromString(string str);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetGamepadStringForAxis(SDL_GamepadAxis axis);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GamepadHasAxis(IntPtr gamepad, SDL_GamepadAxis axis);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial short SDL_GetGamepadAxis(IntPtr gamepad, SDL_GamepadAxis axis);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_GamepadButton SDL_GetGamepadButtonFromString(string str);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetGamepadStringForButton(SDL_GamepadButton button);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GamepadHasButton(IntPtr gamepad, SDL_GamepadButton button);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetGamepadButton(IntPtr gamepad, SDL_GamepadButton button);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_GamepadButtonLabel SDL_GetGamepadButtonLabelForType(SDL_GamepadType type, SDL_GamepadButton button);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_GamepadButtonLabel SDL_GetGamepadButtonLabel(IntPtr gamepad, SDL_GamepadButton button);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_GetNumGamepadTouchpads(IntPtr gamepad);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_GetNumGamepadTouchpadFingers(IntPtr gamepad, int touchpad);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetGamepadTouchpadFinger(IntPtr gamepad, int touchpad, int finger, out SDLBool down, out float x, out float y, out float pressure);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RumbleGamepad(IntPtr gamepad, ushort low_frequency_rumble, ushort high_frequency_rumble, uint duration_ms);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RumbleGamepadTriggers(IntPtr gamepad, ushort left_rumble, ushort right_rumble, uint duration_ms);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetGamepadLED(IntPtr gamepad, byte red, byte green, byte blue);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SendGamepadEffect(IntPtr gamepad, IntPtr data, int size);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_CloseGamepad(IntPtr gamepad);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetGamepadAppleSFSymbolsNameForButton(IntPtr gamepad, SDL_GamepadButton button);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetGamepadAppleSFSymbolsNameForAxis(IntPtr gamepad, SDL_GamepadAxis axis);

    // /usr/local/include/SDL3/SDL_scancode.h

    public enum SDL_Scancode
    {
        SDL_SCANCODE_UNKNOWN = 0,
        SDL_SCANCODE_A = 4,
        SDL_SCANCODE_B = 5,
        SDL_SCANCODE_C = 6,
        SDL_SCANCODE_D = 7,
        SDL_SCANCODE_E = 8,
        SDL_SCANCODE_F = 9,
        SDL_SCANCODE_G = 10,
        SDL_SCANCODE_H = 11,
        SDL_SCANCODE_I = 12,
        SDL_SCANCODE_J = 13,
        SDL_SCANCODE_K = 14,
        SDL_SCANCODE_L = 15,
        SDL_SCANCODE_M = 16,
        SDL_SCANCODE_N = 17,
        SDL_SCANCODE_O = 18,
        SDL_SCANCODE_P = 19,
        SDL_SCANCODE_Q = 20,
        SDL_SCANCODE_R = 21,
        SDL_SCANCODE_S = 22,
        SDL_SCANCODE_T = 23,
        SDL_SCANCODE_U = 24,
        SDL_SCANCODE_V = 25,
        SDL_SCANCODE_W = 26,
        SDL_SCANCODE_X = 27,
        SDL_SCANCODE_Y = 28,
        SDL_SCANCODE_Z = 29,
        SDL_SCANCODE_1 = 30,
        SDL_SCANCODE_2 = 31,
        SDL_SCANCODE_3 = 32,
        SDL_SCANCODE_4 = 33,
        SDL_SCANCODE_5 = 34,
        SDL_SCANCODE_6 = 35,
        SDL_SCANCODE_7 = 36,
        SDL_SCANCODE_8 = 37,
        SDL_SCANCODE_9 = 38,
        SDL_SCANCODE_0 = 39,
        SDL_SCANCODE_RETURN = 40,
        SDL_SCANCODE_ESCAPE = 41,
        SDL_SCANCODE_BACKSPACE = 42,
        SDL_SCANCODE_TAB = 43,
        SDL_SCANCODE_SPACE = 44,
        SDL_SCANCODE_MINUS = 45,
        SDL_SCANCODE_EQUALS = 46,
        SDL_SCANCODE_LEFTBRACKET = 47,
        SDL_SCANCODE_RIGHTBRACKET = 48,
        SDL_SCANCODE_BACKSLASH = 49,
        SDL_SCANCODE_NONUSHASH = 50,
        SDL_SCANCODE_SEMICOLON = 51,
        SDL_SCANCODE_APOSTROPHE = 52,
        SDL_SCANCODE_GRAVE = 53,
        SDL_SCANCODE_COMMA = 54,
        SDL_SCANCODE_PERIOD = 55,
        SDL_SCANCODE_SLASH = 56,
        SDL_SCANCODE_CAPSLOCK = 57,
        SDL_SCANCODE_F1 = 58,
        SDL_SCANCODE_F2 = 59,
        SDL_SCANCODE_F3 = 60,
        SDL_SCANCODE_F4 = 61,
        SDL_SCANCODE_F5 = 62,
        SDL_SCANCODE_F6 = 63,
        SDL_SCANCODE_F7 = 64,
        SDL_SCANCODE_F8 = 65,
        SDL_SCANCODE_F9 = 66,
        SDL_SCANCODE_F10 = 67,
        SDL_SCANCODE_F11 = 68,
        SDL_SCANCODE_F12 = 69,
        SDL_SCANCODE_PRINTSCREEN = 70,
        SDL_SCANCODE_SCROLLLOCK = 71,
        SDL_SCANCODE_PAUSE = 72,
        SDL_SCANCODE_INSERT = 73,
        SDL_SCANCODE_HOME = 74,
        SDL_SCANCODE_PAGEUP = 75,
        SDL_SCANCODE_DELETE = 76,
        SDL_SCANCODE_END = 77,
        SDL_SCANCODE_PAGEDOWN = 78,
        SDL_SCANCODE_RIGHT = 79,
        SDL_SCANCODE_LEFT = 80,
        SDL_SCANCODE_DOWN = 81,
        SDL_SCANCODE_UP = 82,
        SDL_SCANCODE_NUMLOCKCLEAR = 83,
        SDL_SCANCODE_KP_DIVIDE = 84,
        SDL_SCANCODE_KP_MULTIPLY = 85,
        SDL_SCANCODE_KP_MINUS = 86,
        SDL_SCANCODE_KP_PLUS = 87,
        SDL_SCANCODE_KP_ENTER = 88,
        SDL_SCANCODE_KP_1 = 89,
        SDL_SCANCODE_KP_2 = 90,
        SDL_SCANCODE_KP_3 = 91,
        SDL_SCANCODE_KP_4 = 92,
        SDL_SCANCODE_KP_5 = 93,
        SDL_SCANCODE_KP_6 = 94,
        SDL_SCANCODE_KP_7 = 95,
        SDL_SCANCODE_KP_8 = 96,
        SDL_SCANCODE_KP_9 = 97,
        SDL_SCANCODE_KP_0 = 98,
        SDL_SCANCODE_KP_PERIOD = 99,
        SDL_SCANCODE_NONUSBACKSLASH = 100,
        SDL_SCANCODE_APPLICATION = 101,
        SDL_SCANCODE_POWER = 102,
        SDL_SCANCODE_KP_EQUALS = 103,
        SDL_SCANCODE_F13 = 104,
        SDL_SCANCODE_F14 = 105,
        SDL_SCANCODE_F15 = 106,
        SDL_SCANCODE_F16 = 107,
        SDL_SCANCODE_F17 = 108,
        SDL_SCANCODE_F18 = 109,
        SDL_SCANCODE_F19 = 110,
        SDL_SCANCODE_F20 = 111,
        SDL_SCANCODE_F21 = 112,
        SDL_SCANCODE_F22 = 113,
        SDL_SCANCODE_F23 = 114,
        SDL_SCANCODE_F24 = 115,
        SDL_SCANCODE_EXECUTE = 116,
        SDL_SCANCODE_HELP = 117,
        SDL_SCANCODE_MENU = 118,
        SDL_SCANCODE_SELECT = 119,
        SDL_SCANCODE_STOP = 120,
        SDL_SCANCODE_AGAIN = 121,
        SDL_SCANCODE_UNDO = 122,
        SDL_SCANCODE_CUT = 123,
        SDL_SCANCODE_COPY = 124,
        SDL_SCANCODE_PASTE = 125,
        SDL_SCANCODE_FIND = 126,
        SDL_SCANCODE_MUTE = 127,
        SDL_SCANCODE_VOLUMEUP = 128,
        SDL_SCANCODE_VOLUMEDOWN = 129,
        SDL_SCANCODE_KP_COMMA = 133,
        SDL_SCANCODE_KP_EQUALSAS400 = 134,
        SDL_SCANCODE_INTERNATIONAL1 = 135,
        SDL_SCANCODE_INTERNATIONAL2 = 136,
        SDL_SCANCODE_INTERNATIONAL3 = 137,
        SDL_SCANCODE_INTERNATIONAL4 = 138,
        SDL_SCANCODE_INTERNATIONAL5 = 139,
        SDL_SCANCODE_INTERNATIONAL6 = 140,
        SDL_SCANCODE_INTERNATIONAL7 = 141,
        SDL_SCANCODE_INTERNATIONAL8 = 142,
        SDL_SCANCODE_INTERNATIONAL9 = 143,
        SDL_SCANCODE_LANG1 = 144,
        SDL_SCANCODE_LANG2 = 145,
        SDL_SCANCODE_LANG3 = 146,
        SDL_SCANCODE_LANG4 = 147,
        SDL_SCANCODE_LANG5 = 148,
        SDL_SCANCODE_LANG6 = 149,
        SDL_SCANCODE_LANG7 = 150,
        SDL_SCANCODE_LANG8 = 151,
        SDL_SCANCODE_LANG9 = 152,
        SDL_SCANCODE_ALTERASE = 153,
        SDL_SCANCODE_SYSREQ = 154,
        SDL_SCANCODE_CANCEL = 155,
        SDL_SCANCODE_CLEAR = 156,
        SDL_SCANCODE_PRIOR = 157,
        SDL_SCANCODE_RETURN2 = 158,
        SDL_SCANCODE_SEPARATOR = 159,
        SDL_SCANCODE_OUT = 160,
        SDL_SCANCODE_OPER = 161,
        SDL_SCANCODE_CLEARAGAIN = 162,
        SDL_SCANCODE_CRSEL = 163,
        SDL_SCANCODE_EXSEL = 164,
        SDL_SCANCODE_KP_00 = 176,
        SDL_SCANCODE_KP_000 = 177,
        SDL_SCANCODE_THOUSANDSSEPARATOR = 178,
        SDL_SCANCODE_DECIMALSEPARATOR = 179,
        SDL_SCANCODE_CURRENCYUNIT = 180,
        SDL_SCANCODE_CURRENCYSUBUNIT = 181,
        SDL_SCANCODE_KP_LEFTPAREN = 182,
        SDL_SCANCODE_KP_RIGHTPAREN = 183,
        SDL_SCANCODE_KP_LEFTBRACE = 184,
        SDL_SCANCODE_KP_RIGHTBRACE = 185,
        SDL_SCANCODE_KP_TAB = 186,
        SDL_SCANCODE_KP_BACKSPACE = 187,
        SDL_SCANCODE_KP_A = 188,
        SDL_SCANCODE_KP_B = 189,
        SDL_SCANCODE_KP_C = 190,
        SDL_SCANCODE_KP_D = 191,
        SDL_SCANCODE_KP_E = 192,
        SDL_SCANCODE_KP_F = 193,
        SDL_SCANCODE_KP_XOR = 194,
        SDL_SCANCODE_KP_POWER = 195,
        SDL_SCANCODE_KP_PERCENT = 196,
        SDL_SCANCODE_KP_LESS = 197,
        SDL_SCANCODE_KP_GREATER = 198,
        SDL_SCANCODE_KP_AMPERSAND = 199,
        SDL_SCANCODE_KP_DBLAMPERSAND = 200,
        SDL_SCANCODE_KP_VERTICALBAR = 201,
        SDL_SCANCODE_KP_DBLVERTICALBAR = 202,
        SDL_SCANCODE_KP_COLON = 203,
        SDL_SCANCODE_KP_HASH = 204,
        SDL_SCANCODE_KP_SPACE = 205,
        SDL_SCANCODE_KP_AT = 206,
        SDL_SCANCODE_KP_EXCLAM = 207,
        SDL_SCANCODE_KP_MEMSTORE = 208,
        SDL_SCANCODE_KP_MEMRECALL = 209,
        SDL_SCANCODE_KP_MEMCLEAR = 210,
        SDL_SCANCODE_KP_MEMADD = 211,
        SDL_SCANCODE_KP_MEMSUBTRACT = 212,
        SDL_SCANCODE_KP_MEMMULTIPLY = 213,
        SDL_SCANCODE_KP_MEMDIVIDE = 214,
        SDL_SCANCODE_KP_PLUSMINUS = 215,
        SDL_SCANCODE_KP_CLEAR = 216,
        SDL_SCANCODE_KP_CLEARENTRY = 217,
        SDL_SCANCODE_KP_BINARY = 218,
        SDL_SCANCODE_KP_OCTAL = 219,
        SDL_SCANCODE_KP_DECIMAL = 220,
        SDL_SCANCODE_KP_HEXADECIMAL = 221,
        SDL_SCANCODE_LCTRL = 224,
        SDL_SCANCODE_LSHIFT = 225,
        SDL_SCANCODE_LALT = 226,
        SDL_SCANCODE_LGUI = 227,
        SDL_SCANCODE_RCTRL = 228,
        SDL_SCANCODE_RSHIFT = 229,
        SDL_SCANCODE_RALT = 230,
        SDL_SCANCODE_RGUI = 231,
        SDL_SCANCODE_MODE = 257,
        SDL_SCANCODE_SLEEP = 258,
        SDL_SCANCODE_WAKE = 259,
        SDL_SCANCODE_CHANNEL_INCREMENT = 260,
        SDL_SCANCODE_CHANNEL_DECREMENT = 261,
        SDL_SCANCODE_MEDIA_PLAY = 262,
        SDL_SCANCODE_MEDIA_PAUSE = 263,
        SDL_SCANCODE_MEDIA_RECORD = 264,
        SDL_SCANCODE_MEDIA_FAST_FORWARD = 265,
        SDL_SCANCODE_MEDIA_REWIND = 266,
        SDL_SCANCODE_MEDIA_NEXT_TRACK = 267,
        SDL_SCANCODE_MEDIA_PREVIOUS_TRACK = 268,
        SDL_SCANCODE_MEDIA_STOP = 269,
        SDL_SCANCODE_MEDIA_EJECT = 270,
        SDL_SCANCODE_MEDIA_PLAY_PAUSE = 271,
        SDL_SCANCODE_MEDIA_SELECT = 272,
        SDL_SCANCODE_AC_NEW = 273,
        SDL_SCANCODE_AC_OPEN = 274,
        SDL_SCANCODE_AC_CLOSE = 275,
        SDL_SCANCODE_AC_EXIT = 276,
        SDL_SCANCODE_AC_SAVE = 277,
        SDL_SCANCODE_AC_PRINT = 278,
        SDL_SCANCODE_AC_PROPERTIES = 279,
        SDL_SCANCODE_AC_SEARCH = 280,
        SDL_SCANCODE_AC_HOME = 281,
        SDL_SCANCODE_AC_BACK = 282,
        SDL_SCANCODE_AC_FORWARD = 283,
        SDL_SCANCODE_AC_STOP = 284,
        SDL_SCANCODE_AC_REFRESH = 285,
        SDL_SCANCODE_AC_BOOKMARKS = 286,
        SDL_SCANCODE_SOFTLEFT = 287,
        SDL_SCANCODE_SOFTRIGHT = 288,
        SDL_SCANCODE_CALL = 289,
        SDL_SCANCODE_ENDCALL = 290,
        SDL_SCANCODE_RESERVED = 400,
        SDL_SCANCODE_COUNT = 512,
    }

    // /usr/local/include/SDL3/SDL_keycode.h

    public enum SDL_Keycode : uint
    {
        SDLK_SCANCODE_MASK = 0x40000000,
        SDLK_UNKNOWN = 0x00000000u,
        SDLK_RETURN = 0x0000000du,
        SDLK_ESCAPE = 0x0000001bu,
        SDLK_BACKSPACE = 0x00000008u,
        SDLK_TAB = 0x00000009u,
        SDLK_SPACE = 0x00000020u,
        SDLK_EXCLAIM = 0x00000021u,
        SDLK_DBLAPOSTROPHE = 0x00000022u,
        SDLK_HASH = 0x00000023u,
        SDLK_DOLLAR = 0x00000024u,
        SDLK_PERCENT = 0x00000025u,
        SDLK_AMPERSAND = 0x00000026u,
        SDLK_APOSTROPHE = 0x00000027u,
        SDLK_LEFTPAREN = 0x00000028u,
        SDLK_RIGHTPAREN = 0x00000029u,
        SDLK_ASTERISK = 0x0000002au,
        SDLK_PLUS = 0x0000002bu,
        SDLK_COMMA = 0x0000002cu,
        SDLK_MINUS = 0x0000002du,
        SDLK_PERIOD = 0x0000002eu,
        SDLK_SLASH = 0x0000002fu,
        SDLK_0 = 0x00000030u,
        SDLK_1 = 0x00000031u,
        SDLK_2 = 0x00000032u,
        SDLK_3 = 0x00000033u,
        SDLK_4 = 0x00000034u,
        SDLK_5 = 0x00000035u,
        SDLK_6 = 0x00000036u,
        SDLK_7 = 0x00000037u,
        SDLK_8 = 0x00000038u,
        SDLK_9 = 0x00000039u,
        SDLK_COLON = 0x0000003au,
        SDLK_SEMICOLON = 0x0000003bu,
        SDLK_LESS = 0x0000003cu,
        SDLK_EQUALS = 0x0000003du,
        SDLK_GREATER = 0x0000003eu,
        SDLK_QUESTION = 0x0000003fu,
        SDLK_AT = 0x00000040u,
        SDLK_LEFTBRACKET = 0x0000005bu,
        SDLK_BACKSLASH = 0x0000005cu,
        SDLK_RIGHTBRACKET = 0x0000005du,
        SDLK_CARET = 0x0000005eu,
        SDLK_UNDERSCORE = 0x0000005fu,
        SDLK_GRAVE = 0x00000060u,
        SDLK_A = 0x00000061u,
        SDLK_B = 0x00000062u,
        SDLK_C = 0x00000063u,
        SDLK_D = 0x00000064u,
        SDLK_E = 0x00000065u,
        SDLK_F = 0x00000066u,
        SDLK_G = 0x00000067u,
        SDLK_H = 0x00000068u,
        SDLK_I = 0x00000069u,
        SDLK_J = 0x0000006au,
        SDLK_K = 0x0000006bu,
        SDLK_L = 0x0000006cu,
        SDLK_M = 0x0000006du,
        SDLK_N = 0x0000006eu,
        SDLK_O = 0x0000006fu,
        SDLK_P = 0x00000070u,
        SDLK_Q = 0x00000071u,
        SDLK_R = 0x00000072u,
        SDLK_S = 0x00000073u,
        SDLK_T = 0x00000074u,
        SDLK_U = 0x00000075u,
        SDLK_V = 0x00000076u,
        SDLK_W = 0x00000077u,
        SDLK_X = 0x00000078u,
        SDLK_Y = 0x00000079u,
        SDLK_Z = 0x0000007au,
        SDLK_LEFTBRACE = 0x0000007bu,
        SDLK_PIPE = 0x0000007cu,
        SDLK_RIGHTBRACE = 0x0000007du,
        SDLK_TILDE = 0x0000007eu,
        SDLK_DELETE = 0x0000007fu,
        SDLK_PLUSMINUS = 0x000000b1u,
        SDLK_CAPSLOCK = 0x40000039u,
        SDLK_F1 = 0x4000003au,
        SDLK_F2 = 0x4000003bu,
        SDLK_F3 = 0x4000003cu,
        SDLK_F4 = 0x4000003du,
        SDLK_F5 = 0x4000003eu,
        SDLK_F6 = 0x4000003fu,
        SDLK_F7 = 0x40000040u,
        SDLK_F8 = 0x40000041u,
        SDLK_F9 = 0x40000042u,
        SDLK_F10 = 0x40000043u,
        SDLK_F11 = 0x40000044u,
        SDLK_F12 = 0x40000045u,
        SDLK_PRINTSCREEN = 0x40000046u,
        SDLK_SCROLLLOCK = 0x40000047u,
        SDLK_PAUSE = 0x40000048u,
        SDLK_INSERT = 0x40000049u,
        SDLK_HOME = 0x4000004au,
        SDLK_PAGEUP = 0x4000004bu,
        SDLK_END = 0x4000004du,
        SDLK_PAGEDOWN = 0x4000004eu,
        SDLK_RIGHT = 0x4000004fu,
        SDLK_LEFT = 0x40000050u,
        SDLK_DOWN = 0x40000051u,
        SDLK_UP = 0x40000052u,
        SDLK_NUMLOCKCLEAR = 0x40000053u,
        SDLK_KP_DIVIDE = 0x40000054u,
        SDLK_KP_MULTIPLY = 0x40000055u,
        SDLK_KP_MINUS = 0x40000056u,
        SDLK_KP_PLUS = 0x40000057u,
        SDLK_KP_ENTER = 0x40000058u,
        SDLK_KP_1 = 0x40000059u,
        SDLK_KP_2 = 0x4000005au,
        SDLK_KP_3 = 0x4000005bu,
        SDLK_KP_4 = 0x4000005cu,
        SDLK_KP_5 = 0x4000005du,
        SDLK_KP_6 = 0x4000005eu,
        SDLK_KP_7 = 0x4000005fu,
        SDLK_KP_8 = 0x40000060u,
        SDLK_KP_9 = 0x40000061u,
        SDLK_KP_0 = 0x40000062u,
        SDLK_KP_PERIOD = 0x40000063u,
        SDLK_APPLICATION = 0x40000065u,
        SDLK_POWER = 0x40000066u,
        SDLK_KP_EQUALS = 0x40000067u,
        SDLK_F13 = 0x40000068u,
        SDLK_F14 = 0x40000069u,
        SDLK_F15 = 0x4000006au,
        SDLK_F16 = 0x4000006bu,
        SDLK_F17 = 0x4000006cu,
        SDLK_F18 = 0x4000006du,
        SDLK_F19 = 0x4000006eu,
        SDLK_F20 = 0x4000006fu,
        SDLK_F21 = 0x40000070u,
        SDLK_F22 = 0x40000071u,
        SDLK_F23 = 0x40000072u,
        SDLK_F24 = 0x40000073u,
        SDLK_EXECUTE = 0x40000074u,
        SDLK_HELP = 0x40000075u,
        SDLK_MENU = 0x40000076u,
        SDLK_SELECT = 0x40000077u,
        SDLK_STOP = 0x40000078u,
        SDLK_AGAIN = 0x40000079u,
        SDLK_UNDO = 0x4000007au,
        SDLK_CUT = 0x4000007bu,
        SDLK_COPY = 0x4000007cu,
        SDLK_PASTE = 0x4000007du,
        SDLK_FIND = 0x4000007eu,
        SDLK_MUTE = 0x4000007fu,
        SDLK_VOLUMEUP = 0x40000080u,
        SDLK_VOLUMEDOWN = 0x40000081u,
        SDLK_KP_COMMA = 0x40000085u,
        SDLK_KP_EQUALSAS400 = 0x40000086u,
        SDLK_ALTERASE = 0x40000099u,
        SDLK_SYSREQ = 0x4000009au,
        SDLK_CANCEL = 0x4000009bu,
        SDLK_CLEAR = 0x4000009cu,
        SDLK_PRIOR = 0x4000009du,
        SDLK_RETURN2 = 0x4000009eu,
        SDLK_SEPARATOR = 0x4000009fu,
        SDLK_OUT = 0x400000a0u,
        SDLK_OPER = 0x400000a1u,
        SDLK_CLEARAGAIN = 0x400000a2u,
        SDLK_CRSEL = 0x400000a3u,
        SDLK_EXSEL = 0x400000a4u,
        SDLK_KP_00 = 0x400000b0u,
        SDLK_KP_000 = 0x400000b1u,
        SDLK_THOUSANDSSEPARATOR = 0x400000b2u,
        SDLK_DECIMALSEPARATOR = 0x400000b3u,
        SDLK_CURRENCYUNIT = 0x400000b4u,
        SDLK_CURRENCYSUBUNIT = 0x400000b5u,
        SDLK_KP_LEFTPAREN = 0x400000b6u,
        SDLK_KP_RIGHTPAREN = 0x400000b7u,
        SDLK_KP_LEFTBRACE = 0x400000b8u,
        SDLK_KP_RIGHTBRACE = 0x400000b9u,
        SDLK_KP_TAB = 0x400000bau,
        SDLK_KP_BACKSPACE = 0x400000bbu,
        SDLK_KP_A = 0x400000bcu,
        SDLK_KP_B = 0x400000bdu,
        SDLK_KP_C = 0x400000beu,
        SDLK_KP_D = 0x400000bfu,
        SDLK_KP_E = 0x400000c0u,
        SDLK_KP_F = 0x400000c1u,
        SDLK_KP_XOR = 0x400000c2u,
        SDLK_KP_POWER = 0x400000c3u,
        SDLK_KP_PERCENT = 0x400000c4u,
        SDLK_KP_LESS = 0x400000c5u,
        SDLK_KP_GREATER = 0x400000c6u,
        SDLK_KP_AMPERSAND = 0x400000c7u,
        SDLK_KP_DBLAMPERSAND = 0x400000c8u,
        SDLK_KP_VERTICALBAR = 0x400000c9u,
        SDLK_KP_DBLVERTICALBAR = 0x400000cau,
        SDLK_KP_COLON = 0x400000cbu,
        SDLK_KP_HASH = 0x400000ccu,
        SDLK_KP_SPACE = 0x400000cdu,
        SDLK_KP_AT = 0x400000ceu,
        SDLK_KP_EXCLAM = 0x400000cfu,
        SDLK_KP_MEMSTORE = 0x400000d0u,
        SDLK_KP_MEMRECALL = 0x400000d1u,
        SDLK_KP_MEMCLEAR = 0x400000d2u,
        SDLK_KP_MEMADD = 0x400000d3u,
        SDLK_KP_MEMSUBTRACT = 0x400000d4u,
        SDLK_KP_MEMMULTIPLY = 0x400000d5u,
        SDLK_KP_MEMDIVIDE = 0x400000d6u,
        SDLK_KP_PLUSMINUS = 0x400000d7u,
        SDLK_KP_CLEAR = 0x400000d8u,
        SDLK_KP_CLEARENTRY = 0x400000d9u,
        SDLK_KP_BINARY = 0x400000dau,
        SDLK_KP_OCTAL = 0x400000dbu,
        SDLK_KP_DECIMAL = 0x400000dcu,
        SDLK_KP_HEXADECIMAL = 0x400000ddu,
        SDLK_LCTRL = 0x400000e0u,
        SDLK_LSHIFT = 0x400000e1u,
        SDLK_LALT = 0x400000e2u,
        SDLK_LGUI = 0x400000e3u,
        SDLK_RCTRL = 0x400000e4u,
        SDLK_RSHIFT = 0x400000e5u,
        SDLK_RALT = 0x400000e6u,
        SDLK_RGUI = 0x400000e7u,
        SDLK_MODE = 0x40000101u,
        SDLK_SLEEP = 0x40000102u,
        SDLK_WAKE = 0x40000103u,
        SDLK_CHANNEL_INCREMENT = 0x40000104u,
        SDLK_CHANNEL_DECREMENT = 0x40000105u,
        SDLK_MEDIA_PLAY = 0x40000106u,
        SDLK_MEDIA_PAUSE = 0x40000107u,
        SDLK_MEDIA_RECORD = 0x40000108u,
        SDLK_MEDIA_FAST_FORWARD = 0x40000109u,
        SDLK_MEDIA_REWIND = 0x4000010au,
        SDLK_MEDIA_NEXT_TRACK = 0x4000010bu,
        SDLK_MEDIA_PREVIOUS_TRACK = 0x4000010cu,
        SDLK_MEDIA_STOP = 0x4000010du,
        SDLK_MEDIA_EJECT = 0x4000010eu,
        SDLK_MEDIA_PLAY_PAUSE = 0x4000010fu,
        SDLK_MEDIA_SELECT = 0x40000110u,
        SDLK_AC_NEW = 0x40000111u,
        SDLK_AC_OPEN = 0x40000112u,
        SDLK_AC_CLOSE = 0x40000113u,
        SDLK_AC_EXIT = 0x40000114u,
        SDLK_AC_SAVE = 0x40000115u,
        SDLK_AC_PRINT = 0x40000116u,
        SDLK_AC_PROPERTIES = 0x40000117u,
        SDLK_AC_SEARCH = 0x40000118u,
        SDLK_AC_HOME = 0x40000119u,
        SDLK_AC_BACK = 0x4000011au,
        SDLK_AC_FORWARD = 0x4000011bu,
        SDLK_AC_STOP = 0x4000011cu,
        SDLK_AC_REFRESH = 0x4000011du,
        SDLK_AC_BOOKMARKS = 0x4000011eu,
        SDLK_SOFTLEFT = 0x4000011fu,
        SDLK_SOFTRIGHT = 0x40000120u,
        SDLK_CALL = 0x40000121u,
        SDLK_ENDCALL = 0x40000122u,
        SDLK_LEFT_TAB = 0x20000001u,
        SDLK_LEVEL5_SHIFT = 0x20000002u,
        SDLK_MULTI_KEY_COMPOSE = 0x20000003u,
        SDLK_LMETA = 0x20000004u,
        SDLK_RMETA = 0x20000005u,
        SDLK_LHYPER = 0x20000006u,
        SDLK_RHYPER = 0x20000007u,
    }

    [Flags]
    public enum SDL_Keymod : ushort
    {
        SDL_KMOD_NONE   = 0x0000,
        SDL_KMOD_LSHIFT = 0x0001,
        SDL_KMOD_RSHIFT = 0x0002,
        SDL_KMOD_LCTRL  = 0x0040,
        SDL_KMOD_RCTRL  = 0x0080,
        SDL_KMOD_LALT   = 0x0100,
        SDL_KMOD_RALT   = 0x0200,
        SDL_KMOD_LGUI   = 0x0400,
        SDL_KMOD_RGUI   = 0x0800,
        SDL_KMOD_NUM    = 0x1000,
        SDL_KMOD_CAPS   = 0x2000,
        SDL_KMOD_MODE   = 0x4000,
        SDL_KMOD_SCROLL = 0x8000,
        SDL_KMOD_CTRL   = SDL_KMOD_LCTRL | SDL_KMOD_RCTRL,
        SDL_KMOD_SHIFT  = SDL_KMOD_LSHIFT | SDL_KMOD_RSHIFT,
        SDL_KMOD_ALT    = SDL_KMOD_RALT | SDL_KMOD_LALT,
        SDL_KMOD_GUI    = SDL_KMOD_RGUI | SDL_KMOD_LGUI,
    }

    // /usr/local/include/SDL3/SDL_keyboard.h

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_HasKeyboard();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetKeyboards(out int count);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetKeyboardNameForID(uint instance_id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetKeyboardFocus();

    internal static Span<SDLBool> SDL_GetKeyboardState()
    {
        var result = SDL_GetKeyboardState(out var numkeys);
        return new Span<SDLBool>((void*) result, numkeys);
    }

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetKeyboardState(out int numkeys);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_ResetKeyboard();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Keymod SDL_GetModState();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_SetModState(SDL_Keymod modstate);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_GetKeyFromScancode(SDL_Scancode scancode, SDL_Keymod modstate, SDLBool key_event);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Scancode SDL_GetScancodeFromKey(uint key, IntPtr modstate);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetScancodeName(SDL_Scancode scancode, string name);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetScancodeName(SDL_Scancode scancode);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Scancode SDL_GetScancodeFromName(string name);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetKeyName(uint key);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_GetKeyFromName(string name);

    // /usr/local/include/SDL3/SDL_mouse.h

    public enum SDL_SystemCursor
    {
        SDL_SYSTEM_CURSOR_DEFAULT     = 0,
        SDL_SYSTEM_CURSOR_TEXT        = 1,
        SDL_SYSTEM_CURSOR_WAIT        = 2,
        SDL_SYSTEM_CURSOR_CROSSHAIR   = 3,
        SDL_SYSTEM_CURSOR_PROGRESS    = 4,
        SDL_SYSTEM_CURSOR_NWSE_RESIZE = 5,
        SDL_SYSTEM_CURSOR_NESW_RESIZE = 6,
        SDL_SYSTEM_CURSOR_EW_RESIZE   = 7,
        SDL_SYSTEM_CURSOR_NS_RESIZE   = 8,
        SDL_SYSTEM_CURSOR_MOVE        = 9,
        SDL_SYSTEM_CURSOR_NOT_ALLOWED = 10,
        SDL_SYSTEM_CURSOR_POINTER     = 11,
        SDL_SYSTEM_CURSOR_NW_RESIZE   = 12,
        SDL_SYSTEM_CURSOR_N_RESIZE    = 13,
        SDL_SYSTEM_CURSOR_NE_RESIZE   = 14,
        SDL_SYSTEM_CURSOR_E_RESIZE    = 15,
        SDL_SYSTEM_CURSOR_SE_RESIZE   = 16,
        SDL_SYSTEM_CURSOR_S_RESIZE    = 17,
        SDL_SYSTEM_CURSOR_SW_RESIZE   = 18,
        SDL_SYSTEM_CURSOR_W_RESIZE    = 19,
        SDL_SYSTEM_CURSOR_COUNT       = 20,
    }

    public enum SDL_MouseWheelDirection
    {
        SDL_MOUSEWHEEL_NORMAL  = 0,
        SDL_MOUSEWHEEL_FLIPPED = 1,
    }

    [Flags]
    public enum SDL_MouseButtonFlags : uint
    {
        SDL_BUTTON_LMASK  = 0x1,
        SDL_BUTTON_MMASK  = 0x2,
        SDL_BUTTON_RMASK  = 0x4,
        SDL_BUTTON_X1MASK = 0x08,
        SDL_BUTTON_X2MASK = 0x10,
    }

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_HasMouse();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetMice(out int count);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetMouseNameForID(uint instance_id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetMouseFocus();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_MouseButtonFlags SDL_GetMouseState(out float x, out float y);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_MouseButtonFlags SDL_GetGlobalMouseState(out float x, out float y);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_MouseButtonFlags SDL_GetRelativeMouseState(out float x, out float y);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_WarpMouseInWindow(IntPtr window, float x, float y);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_WarpMouseGlobal(float x, float y);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetWindowRelativeMouseMode(IntPtr window, SDLBool enabled);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetWindowRelativeMouseMode(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_CaptureMouse(SDLBool enabled);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_CreateCursor(IntPtr data, IntPtr mask, int w, int h, int hot_x, int hot_y);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_CreateColorCursor(IntPtr surface, int hot_x, int hot_y);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_CreateSystemCursor(SDL_SystemCursor id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetCursor(IntPtr cursor);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetCursor();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetDefaultCursor();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_DestroyCursor(IntPtr cursor);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_ShowCursor();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_HideCursor();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_CursorVisible();

    // /usr/local/include/SDL3/SDL_events.h

    public enum SDL_EventType
    {
        SDL_EVENT_FIRST = 0,
        SDL_EVENT_QUIT = 256,
        SDL_EVENT_TERMINATING = 257,
        SDL_EVENT_LOW_MEMORY = 258,
        SDL_EVENT_WILL_ENTER_BACKGROUND = 259,
        SDL_EVENT_DID_ENTER_BACKGROUND = 260,
        SDL_EVENT_WILL_ENTER_FOREGROUND = 261,
        SDL_EVENT_DID_ENTER_FOREGROUND = 262,
        SDL_EVENT_LOCALE_CHANGED = 263,
        SDL_EVENT_SYSTEM_THEME_CHANGED = 264,
        SDL_EVENT_DISPLAY_ORIENTATION = 337,
        SDL_EVENT_DISPLAY_ADDED = 338,
        SDL_EVENT_DISPLAY_REMOVED = 339,
        SDL_EVENT_DISPLAY_MOVED = 340,
        SDL_EVENT_DISPLAY_DESKTOP_MODE_CHANGED = 341,
        SDL_EVENT_DISPLAY_CURRENT_MODE_CHANGED = 342,
        SDL_EVENT_DISPLAY_CONTENT_SCALE_CHANGED = 343,
        SDL_EVENT_DISPLAY_FIRST = 337,
        SDL_EVENT_DISPLAY_LAST = 343,
        SDL_EVENT_WINDOW_SHOWN = 514,
        SDL_EVENT_WINDOW_HIDDEN = 515,
        SDL_EVENT_WINDOW_EXPOSED = 516,
        SDL_EVENT_WINDOW_MOVED = 517,
        SDL_EVENT_WINDOW_RESIZED = 518,
        SDL_EVENT_WINDOW_PIXEL_SIZE_CHANGED = 519,
        SDL_EVENT_WINDOW_METAL_VIEW_RESIZED = 520,
        SDL_EVENT_WINDOW_MINIMIZED = 521,
        SDL_EVENT_WINDOW_MAXIMIZED = 522,
        SDL_EVENT_WINDOW_RESTORED = 523,
        SDL_EVENT_WINDOW_MOUSE_ENTER = 524,
        SDL_EVENT_WINDOW_MOUSE_LEAVE = 525,
        SDL_EVENT_WINDOW_FOCUS_GAINED = 526,
        SDL_EVENT_WINDOW_FOCUS_LOST = 527,
        SDL_EVENT_WINDOW_CLOSE_REQUESTED = 528,
        SDL_EVENT_WINDOW_HIT_TEST = 529,
        SDL_EVENT_WINDOW_ICCPROF_CHANGED = 530,
        SDL_EVENT_WINDOW_DISPLAY_CHANGED = 531,
        SDL_EVENT_WINDOW_DISPLAY_SCALE_CHANGED = 532,
        SDL_EVENT_WINDOW_SAFE_AREA_CHANGED = 533,
        SDL_EVENT_WINDOW_OCCLUDED = 534,
        SDL_EVENT_WINDOW_ENTER_FULLSCREEN = 535,
        SDL_EVENT_WINDOW_LEAVE_FULLSCREEN = 536,
        SDL_EVENT_WINDOW_DESTROYED = 537,
        SDL_EVENT_WINDOW_HDR_STATE_CHANGED = 538,
        SDL_EVENT_WINDOW_FIRST = 514,
        SDL_EVENT_WINDOW_LAST = 538,
        SDL_EVENT_KEY_DOWN = 768,
        SDL_EVENT_KEY_UP = 769,
        SDL_EVENT_TEXT_EDITING = 770,
        SDL_EVENT_TEXT_INPUT = 771,
        SDL_EVENT_KEYMAP_CHANGED = 772,
        SDL_EVENT_KEYBOARD_ADDED = 773,
        SDL_EVENT_KEYBOARD_REMOVED = 774,
        SDL_EVENT_TEXT_EDITING_CANDIDATES = 775,
        SDL_EVENT_MOUSE_MOTION = 1024,
        SDL_EVENT_MOUSE_BUTTON_DOWN = 1025,
        SDL_EVENT_MOUSE_BUTTON_UP = 1026,
        SDL_EVENT_MOUSE_WHEEL = 1027,
        SDL_EVENT_MOUSE_ADDED = 1028,
        SDL_EVENT_MOUSE_REMOVED = 1029,
        SDL_EVENT_JOYSTICK_AXIS_MOTION = 1536,
        SDL_EVENT_JOYSTICK_BALL_MOTION = 1537,
        SDL_EVENT_JOYSTICK_HAT_MOTION = 1538,
        SDL_EVENT_JOYSTICK_BUTTON_DOWN = 1539,
        SDL_EVENT_JOYSTICK_BUTTON_UP = 1540,
        SDL_EVENT_JOYSTICK_ADDED = 1541,
        SDL_EVENT_JOYSTICK_REMOVED = 1542,
        SDL_EVENT_JOYSTICK_BATTERY_UPDATED = 1543,
        SDL_EVENT_JOYSTICK_UPDATE_COMPLETE = 1544,
        SDL_EVENT_GAMEPAD_AXIS_MOTION = 1616,
        SDL_EVENT_GAMEPAD_BUTTON_DOWN = 1617,
        SDL_EVENT_GAMEPAD_BUTTON_UP = 1618,
        SDL_EVENT_GAMEPAD_ADDED = 1619,
        SDL_EVENT_GAMEPAD_REMOVED = 1620,
        SDL_EVENT_GAMEPAD_REMAPPED = 1621,
        SDL_EVENT_GAMEPAD_TOUCHPAD_DOWN = 1622,
        SDL_EVENT_GAMEPAD_TOUCHPAD_MOTION = 1623,
        SDL_EVENT_GAMEPAD_TOUCHPAD_UP = 1624,
        SDL_EVENT_GAMEPAD_SENSOR_UPDATE = 1625,
        SDL_EVENT_GAMEPAD_UPDATE_COMPLETE = 1626,
        SDL_EVENT_GAMEPAD_STEAM_HANDLE_UPDATED = 1627,
        SDL_EVENT_FINGER_DOWN = 1792,
        SDL_EVENT_FINGER_UP = 1793,
        SDL_EVENT_FINGER_MOTION = 1794,
        SDL_EVENT_FINGER_CANCELED = 1795,
        SDL_EVENT_CLIPBOARD_UPDATE = 2304,
        SDL_EVENT_DROP_FILE = 4096,
        SDL_EVENT_DROP_TEXT = 4097,
        SDL_EVENT_DROP_BEGIN = 4098,
        SDL_EVENT_DROP_COMPLETE = 4099,
        SDL_EVENT_DROP_POSITION = 4100,
        SDL_EVENT_AUDIO_DEVICE_ADDED = 4352,
        SDL_EVENT_AUDIO_DEVICE_REMOVED = 4353,
        SDL_EVENT_AUDIO_DEVICE_FORMAT_CHANGED = 4354,
        SDL_EVENT_SENSOR_UPDATE = 4608,
        SDL_EVENT_PEN_PROXIMITY_IN = 4864,
        SDL_EVENT_PEN_PROXIMITY_OUT = 4865,
        SDL_EVENT_PEN_DOWN = 4866,
        SDL_EVENT_PEN_UP = 4867,
        SDL_EVENT_PEN_BUTTON_DOWN = 4868,
        SDL_EVENT_PEN_BUTTON_UP = 4869,
        SDL_EVENT_PEN_MOTION = 4870,
        SDL_EVENT_PEN_AXIS = 4871,
        SDL_EVENT_CAMERA_DEVICE_ADDED = 5120,
        SDL_EVENT_CAMERA_DEVICE_REMOVED = 5121,
        SDL_EVENT_CAMERA_DEVICE_APPROVED = 5122,
        SDL_EVENT_CAMERA_DEVICE_DENIED = 5123,
        SDL_EVENT_RENDER_TARGETS_RESET = 8192,
        SDL_EVENT_RENDER_DEVICE_RESET = 8193,
        SDL_EVENT_RENDER_DEVICE_LOST = 8194,
        SDL_EVENT_PRIVATE0 = 16384,
        SDL_EVENT_PRIVATE1 = 16385,
        SDL_EVENT_PRIVATE2 = 16386,
        SDL_EVENT_PRIVATE3 = 16387,
        SDL_EVENT_POLL_SENTINEL = 32512,
        SDL_EVENT_USER = 32768,
        SDL_EVENT_LAST = 65535,
        SDL_EVENT_ENUM_PADDING = 2147483647,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_CommonEvent
    {
        public uint type;
        public uint reserved;
        public ulong timestamp;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_DisplayEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint displayID;
        public int data1;
        public int data2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_WindowEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint windowID;
        public int data1;
        public int data2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_KeyboardDeviceEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint which;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_KeyboardEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint windowID;
        public uint which;
        public SDL_Scancode scancode;
        public uint key;
        public SDL_Keymod mod;
        public ushort raw;
        public SDLBool down;
        public SDLBool repeat;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_MouseDeviceEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint which;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_MouseMotionEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint windowID;
        public uint which;
        public SDL_MouseButtonFlags state;
        public float x;
        public float y;
        public float xrel;
        public float yrel;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_MouseButtonEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint windowID;
        public uint which;
        public byte button;
        public SDLBool down;
        public byte clicks;
        public byte padding;
        public float x;
        public float y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_MouseWheelEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint windowID;
        public uint which;
        public float x;
        public float y;
        public SDL_MouseWheelDirection direction;
        public float mouse_x;
        public float mouse_y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_JoyAxisEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint which;
        public byte axis;
        public byte padding1;
        public byte padding2;
        public byte padding3;
        public short value;
        public ushort padding4;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_JoyBallEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint which;
        public byte ball;
        public byte padding1;
        public byte padding2;
        public byte padding3;
        public short xrel;
        public short yrel;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_JoyHatEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint which;
        public byte hat;
        public SDL_Hat value;
        public byte padding1;
        public byte padding2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_JoyButtonEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint which;
        public byte button;
        public SDLBool down;
        public byte padding1;
        public byte padding2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_JoyDeviceEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint which;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GamepadAxisEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint which;
        public byte axis;
        public byte padding1;
        public byte padding2;
        public byte padding3;
        public short value;
        public ushort padding4;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GamepadButtonEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint which;
        public byte button;
        public SDLBool down;
        public byte padding1;
        public byte padding2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GamepadDeviceEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint which;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GamepadTouchpadEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint which;
        public int touchpad;
        public int finger;
        public float x;
        public float y;
        public float pressure;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GamepadSensorEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint which;
        public int sensor;
        public fixed float data[3];
        public ulong sensor_timestamp;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_TouchFingerEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public ulong touchID;
        public ulong fingerID;
        public float x;
        public float y;
        public float dx;
        public float dy;
        public float pressure;
        public uint windowID;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_DropEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint windowID;
        public float x;
        public float y;
        public byte* source;
        public byte* data;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_QuitEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_UserEvent
    {
        public uint type;
        public uint reserved;
        public ulong timestamp;
        public uint windowID;
        public int code;
        public IntPtr data1;
        public IntPtr data2;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct SDL_Event
    {
        [FieldOffset(0)]
        public uint type;
        [FieldOffset(0)]
        public SDL_CommonEvent common;
        [FieldOffset(0)]
        public SDL_DisplayEvent display;
        [FieldOffset(0)]
        public SDL_WindowEvent window;
        [FieldOffset(0)]
        public SDL_KeyboardDeviceEvent kdevice;
        [FieldOffset(0)]
        public SDL_KeyboardEvent key;
      //[FieldOffset(0)]
      //public SDL_TextEditingEvent edit;
      //[FieldOffset(0)]
      //public SDL_TextEditingCandidatesEvent edit_candidates;
      //[FieldOffset(0)]
      //public SDL_TextInputEvent text;
        [FieldOffset(0)]
        public SDL_MouseDeviceEvent mdevice;
        [FieldOffset(0)]
        public SDL_MouseMotionEvent motion;
        [FieldOffset(0)]
        public SDL_MouseButtonEvent button;
        [FieldOffset(0)]
        public SDL_MouseWheelEvent wheel;
        [FieldOffset(0)]
        public SDL_JoyDeviceEvent jdevice;
        [FieldOffset(0)]
        public SDL_JoyAxisEvent jaxis;
        [FieldOffset(0)]
        public SDL_JoyBallEvent jball;
        [FieldOffset(0)]
        public SDL_JoyHatEvent jhat;
        [FieldOffset(0)]
        public SDL_JoyButtonEvent jbutton;
        [FieldOffset(0)]
        public SDL_GamepadDeviceEvent gdevice;
        [FieldOffset(0)]
        public SDL_GamepadAxisEvent gaxis;
        [FieldOffset(0)]
        public SDL_GamepadButtonEvent gbutton;
        [FieldOffset(0)]
        public SDL_GamepadTouchpadEvent gtouchpad;
        [FieldOffset(0)]
        public SDL_GamepadSensorEvent gsensor;
       //FieldOffset(0)]
       //ublic SDL_AudioDeviceEvent adevice;
       //FieldOffset(0)]
       //ublic SDL_CameraDeviceEvent cdevice;
       //FieldOffset(0)]
       //public SDL_SensorEvent sensor;
        [FieldOffset(0)]
        public SDL_QuitEvent quit;
        [FieldOffset(0)]
        public SDL_UserEvent user;
        [FieldOffset(0)]
        public SDL_TouchFingerEvent tfinger;
      //[FieldOffset(0)]
      //public SDL_PenProximityEvent pproximity;
      //[FieldOffset(0)]
      //public SDL_RenderEvent render;
        [FieldOffset(0)]
        public SDL_DropEvent drop;
      //[FieldOffset(0)]
      //public SDL_ClipboardEvent clipboard;
        [FieldOffset(0)]
        public fixed byte padding[128];
    }

    // /usr/local/include/SDL3/SDL_init.h

    [Flags]
    public enum SDL_InitFlags : uint
    {
        SDL_INIT_TIMER    = 0x1,
        SDL_INIT_AUDIO    = 0x10,
        SDL_INIT_VIDEO    = 0x20,
        SDL_INIT_JOYSTICK = 0x200,
        SDL_INIT_HAPTIC   = 0x1000,
        SDL_INIT_GAMEPAD  = 0x2000,
        SDL_INIT_EVENTS   = 0x4000,
        SDL_INIT_SENSOR   = 0x08000,
        SDL_INIT_CAMERA   = 0x10000,
    }

    public enum SDL_AppResult
    {
        SDL_APP_CONTINUE = 0,
        SDL_APP_SUCCESS  = 1,
        SDL_APP_FAILURE  = 2,
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate SDL_AppResult SDL_AppInit_func(IntPtr appstate, int argc, IntPtr argv);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate SDL_AppResult SDL_AppIterate_func(IntPtr appstate);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate SDL_AppResult SDL_AppEvent_func(IntPtr appstate, SDL_Event* evt);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SDL_AppQuit_func(IntPtr appstate, SDL_AppResult result);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_Init(SDL_InitFlags flags);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_Quit();

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetAppMetadata(string appname, string appversion, string appidentifier);

    // /usr/local/include/SDL3/SDL_loadso.h

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_LoadObject(string sofile);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_LoadFunction(IntPtr handle, string name);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_UnloadObject(IntPtr handle);

    // /usr/local/include/SDL3/SDL_log.h

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_Log(string fmt);

    // /usr/local/include/SDL3/SDL_render.h

    public enum SDL_TextureAccess
    {
        SDL_TEXTUREACCESS_STATIC    = 0,
        SDL_TEXTUREACCESS_STREAMING = 1,
        SDL_TEXTUREACCESS_TARGET    = 2,
    }

    public enum SDL_RendererLogicalPresentation
    {
        SDL_LOGICAL_PRESENTATION_DISABLED      = 0,
        SDL_LOGICAL_PRESENTATION_STRETCH       = 1,
        SDL_LOGICAL_PRESENTATION_LETTERBOX     = 2,
        SDL_LOGICAL_PRESENTATION_OVERSCAN      = 3,
        SDL_LOGICAL_PRESENTATION_INTEGER_SCALE = 4,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_Texture
    {
        public SDL_PixelFormat format;
        public int w;
        public int h;
        public int refcount;
    }

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_CreateWindowAndRenderer(string title, int width, int height, SDL_WindowFlags window_flags, out IntPtr window, out IntPtr renderer);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Texture* SDL_CreateTexture(IntPtr renderer, SDL_PixelFormat format, SDL_TextureAccess access, int w, int h);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Texture* SDL_CreateTextureFromSurface(IntPtr renderer, IntPtr surface);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetTextureScaleMode(IntPtr texture, SDL_ScaleMode scaleMode);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_UpdateTexture(IntPtr texture, IntPtr rect, ReadOnlySpan<byte> pixels, int pitch);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetRenderLogicalPresentation(IntPtr renderer, int w, int h, SDL_RendererLogicalPresentation mode);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetRenderClipRect(IntPtr renderer, SDL_Rect* rect);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetRenderScale(IntPtr renderer, float scaleX, float scaleY);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetRenderDrawColor(IntPtr renderer, byte r, byte g, byte b, byte a);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderClear(IntPtr renderer);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderLine(IntPtr renderer, float x1, float y1, float x2, float y2);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderRect(IntPtr renderer, ref SDL_FRect rect);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderFillRect(IntPtr renderer, ref SDL_FRect rect);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderTexture(IntPtr renderer, IntPtr texture, IntPtr srcrect, ref SDL_FRect dstrect);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderPresent(IntPtr renderer);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_DestroyTexture(IntPtr texture);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_DestroyRenderer(IntPtr renderer);

    // /usr/local/include/SDL3/SDL_version.h

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_GetVersion();

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetRevision();

    // /usr/local/include/SDL3/SDL_timer.h

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ulong SDL_GetTicks();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ulong SDL_GetTicksNS();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ulong SDL_GetPerformanceCounter();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ulong SDL_GetPerformanceFrequency();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_Delay(uint ms);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_DelayNS(ulong ns);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_DelayPrecise(ulong ns);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate uint SDL_TimerCallback(IntPtr userdata, uint timerID, uint interval);

    // /usr/local/include/SDL3/SDL_main.h

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_EnterAppMainCallbacks(int argc, IntPtr argv, SDL_AppInit_func appinit, SDL_AppIterate_func appiter, SDL_AppEvent_func appevent, SDL_AppQuit_func appquit);

    // /usr/local/include/SDL3/SDL_iostream.h

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_IOFromConstMem(ReadOnlySpan<byte> mem, nint size);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_CloseIO(IntPtr context);

    // ttf

    [LibraryImport(SDL3DllttfName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool TTF_Init();

    [LibraryImport(SDL3DllttfName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool TTF_Quit();

    [LibraryImport(SDL3DllttfName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr TTF_OpenFont(string file, float ptsize);

    [LibraryImport(SDL3DllttfName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void TTF_CloseFont(IntPtr font);

    [LibraryImport(SDL3DllttfName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Surface* TTF_RenderText_Blended(IntPtr pFont, string text, UIntPtr length, SDL_Color fg);

    [LibraryImport(SDL3DllttfName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Surface* TTF_RenderText_Blended_Wrapped(IntPtr pFont, string text, UIntPtr length, SDL_Color fg, int wrap_width);
}