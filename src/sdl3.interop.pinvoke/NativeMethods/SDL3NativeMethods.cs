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

    const string SDL3DllName = "SDL3";

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
    internal static partial SDLBool SDL_SetError(string fmt);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_OutOfMemory();

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetError();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_ClearError();

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
    internal static partial int SDL_GetNumAudioDrivers();

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetAudioDriver(int index);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetCurrentAudioDriver();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetAudioPlaybackDevices(out int count);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetAudioRecordingDevices(out int count);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetAudioDeviceName(uint devid);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetAudioDeviceFormat(uint devid, out SDL_AudioSpec spec, out int sample_frames);

    internal static Span<int> SDL_GetAudioDeviceChannelMap(uint devid)
    {
        var result = SDL_GetAudioDeviceChannelMap(devid, out var count);
        return new Span<int>((void*) result, count);
    }

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetAudioDeviceChannelMap(uint devid, out int count);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_OpenAudioDevice(uint devid, ref SDL_AudioSpec spec);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_IsAudioDevicePhysical(uint devid);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_IsAudioDevicePlayback(uint devid);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_PauseAudioDevice(uint dev);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_ResumeAudioDevice(uint dev);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_AudioDevicePaused(uint dev);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial float SDL_GetAudioDeviceGain(uint devid);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetAudioDeviceGain(uint devid, float gain);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_CloseAudioDevice(uint devid);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_BindAudioStreams(uint devid, Span<IntPtr> streams, int num_streams);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_BindAudioStream(uint devid, IntPtr stream);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_UnbindAudioStreams(Span<IntPtr> streams, int num_streams);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_UnbindAudioStream(IntPtr stream);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_GetAudioStreamDevice(IntPtr stream);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_CreateAudioStream(ref SDL_AudioSpec src_spec, ref SDL_AudioSpec dst_spec);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_GetAudioStreamProperties(IntPtr stream);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetAudioStreamFormat(IntPtr stream, out SDL_AudioSpec src_spec, out SDL_AudioSpec dst_spec);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetAudioStreamFormat(IntPtr stream, ref SDL_AudioSpec src_spec, ref SDL_AudioSpec dst_spec);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial float SDL_GetAudioStreamFrequencyRatio(IntPtr stream);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetAudioStreamFrequencyRatio(IntPtr stream, float ratio);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial float SDL_GetAudioStreamGain(IntPtr stream);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetAudioStreamGain(IntPtr stream, float gain);

    internal static Span<int> SDL_GetAudioStreamInputChannelMap(IntPtr stream)
    {
        var result = SDL_GetAudioStreamInputChannelMap(stream, out var count);
        return new Span<int>((void*) result, count);
    }

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetAudioStreamInputChannelMap(IntPtr stream, out int count);

    internal static Span<int> SDL_GetAudioStreamOutputChannelMap(IntPtr stream)
    {
        var result = SDL_GetAudioStreamOutputChannelMap(stream, out var count);
        return new Span<int>((void*) result, count);
    }

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetAudioStreamOutputChannelMap(IntPtr stream, out int count);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetAudioStreamInputChannelMap(IntPtr stream, Span<int> chmap, int count);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetAudioStreamOutputChannelMap(IntPtr stream, Span<int> chmap, int count);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_PutAudioStreamData(IntPtr stream, IntPtr buf, int len);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_GetAudioStreamData(IntPtr stream, IntPtr buf, int len);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_GetAudioStreamAvailable(IntPtr stream);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_GetAudioStreamQueued(IntPtr stream);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_FlushAudioStream(IntPtr stream);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_ClearAudioStream(IntPtr stream);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_PauseAudioStreamDevice(IntPtr stream);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_ResumeAudioStreamDevice(IntPtr stream);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_AudioStreamDevicePaused(IntPtr stream);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_LockAudioStream(IntPtr stream);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_UnlockAudioStream(IntPtr stream);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SDL_AudioStreamCallback(IntPtr userdata, IntPtr stream, int additional_amount, int total_amount);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetAudioStreamGetCallback(IntPtr stream, SDL_AudioStreamCallback callback, IntPtr userdata);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetAudioStreamPutCallback(IntPtr stream, SDL_AudioStreamCallback callback, IntPtr userdata);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_DestroyAudioStream(IntPtr stream);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_OpenAudioDeviceStream(uint devid, ref SDL_AudioSpec spec, SDL_AudioStreamCallback callback, IntPtr userdata);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SDL_AudioPostmixCallback(IntPtr userdata, SDL_AudioSpec* spec, float* buffer, int buflen);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetAudioPostmixCallback(uint devid, SDL_AudioPostmixCallback callback, IntPtr userdata);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_LoadWAV_IO(IntPtr src, SDLBool closeio, out SDL_AudioSpec spec, out IntPtr audio_buf, out uint audio_len);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_LoadWAV(string path, out SDL_AudioSpec spec, out IntPtr audio_buf, out uint audio_len);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_MixAudio(IntPtr dst, IntPtr src, SDL_AudioFormat format, uint len, float volume);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_ConvertAudioSamples(ref SDL_AudioSpec src_spec, IntPtr src_data, int src_len, ref SDL_AudioSpec dst_spec, IntPtr dst_data, out int dst_len);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetAudioFormatName(SDL_AudioFormat format);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_GetSilenceValueForFormat(SDL_AudioFormat format);

    // /usr/local/include/SDL3/SDL_pixels.h

    public enum SDL_PixelType
    {
        SDL_PIXELTYPE_UNKNOWN  = 0,
        SDL_PIXELTYPE_INDEX1   = 1,
        SDL_PIXELTYPE_INDEX4   = 2,
        SDL_PIXELTYPE_INDEX8   = 3,
        SDL_PIXELTYPE_PACKED8  = 4,
        SDL_PIXELTYPE_PACKED16 = 5,
        SDL_PIXELTYPE_PACKED32 = 6,
        SDL_PIXELTYPE_ARRAYU8  = 7,
        SDL_PIXELTYPE_ARRAYU16 = 8,
        SDL_PIXELTYPE_ARRAYU32 = 9,
        SDL_PIXELTYPE_ARRAYF16 = 10,
        SDL_PIXELTYPE_ARRAYF32 = 11,
        SDL_PIXELTYPE_INDEX2   = 12,
    }

    public enum SDL_BitmapOrder
    {
        SDL_BITMAPORDER_NONE = 0,
        SDL_BITMAPORDER_4321 = 1,
        SDL_BITMAPORDER_1234 = 2,
    }

    public enum SDL_PackedOrder
    {
        SDL_PACKEDORDER_NONE = 0,
        SDL_PACKEDORDER_XRGB = 1,
        SDL_PACKEDORDER_RGBX = 2,
        SDL_PACKEDORDER_ARGB = 3,
        SDL_PACKEDORDER_RGBA = 4,
        SDL_PACKEDORDER_XBGR = 5,
        SDL_PACKEDORDER_BGRX = 6,
        SDL_PACKEDORDER_ABGR = 7,
        SDL_PACKEDORDER_BGRA = 8,
    }

    public enum SDL_ArrayOrder
    {
        SDL_ARRAYORDER_NONE = 0,
        SDL_ARRAYORDER_RGB  = 1,
        SDL_ARRAYORDER_RGBA = 2,
        SDL_ARRAYORDER_ARGB = 3,
        SDL_ARRAYORDER_BGR  = 4,
        SDL_ARRAYORDER_BGRA = 5,
        SDL_ARRAYORDER_ABGR = 6,
    }

    public enum SDL_PackedLayout
    {
        SDL_PACKEDLAYOUT_NONE    = 0,
        SDL_PACKEDLAYOUT_332     = 1,
        SDL_PACKEDLAYOUT_4444    = 2,
        SDL_PACKEDLAYOUT_1555    = 3,
        SDL_PACKEDLAYOUT_5551    = 4,
        SDL_PACKEDLAYOUT_565     = 5,
        SDL_PACKEDLAYOUT_8888    = 6,
        SDL_PACKEDLAYOUT_2101010 = 7,
        SDL_PACKEDLAYOUT_1010102 = 8,
    }

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

    public enum SDL_ColorType
    {
        SDL_COLOR_TYPE_UNKNOWN = 0,
        SDL_COLOR_TYPE_RGB     = 1,
        SDL_COLOR_TYPE_YCBCR   = 2,
    }

    public enum SDL_ColorRange
    {
        SDL_COLOR_RANGE_UNKNOWN = 0,
        SDL_COLOR_RANGE_LIMITED = 1,
        SDL_COLOR_RANGE_FULL    = 2,
    }

    public enum SDL_ColorPrimaries
    {
        SDL_COLOR_PRIMARIES_UNKNOWN      = 0,
        SDL_COLOR_PRIMARIES_BT709        = 1,
        SDL_COLOR_PRIMARIES_UNSPECIFIED  = 2,
        SDL_COLOR_PRIMARIES_BT470M       = 4,
        SDL_COLOR_PRIMARIES_BT470BG      = 5,
        SDL_COLOR_PRIMARIES_BT601        = 6,
        SDL_COLOR_PRIMARIES_SMPTE240     = 7,
        SDL_COLOR_PRIMARIES_GENERIC_FILM = 8,
        SDL_COLOR_PRIMARIES_BT2020       = 9,
        SDL_COLOR_PRIMARIES_XYZ          = 10,
        SDL_COLOR_PRIMARIES_SMPTE431     = 11,
        SDL_COLOR_PRIMARIES_SMPTE432     = 12,
        SDL_COLOR_PRIMARIES_EBU3213      = 22,
        SDL_COLOR_PRIMARIES_CUSTOM       = 31,
    }

    public enum SDL_TransferCharacteristics
    {
        SDL_TRANSFER_CHARACTERISTICS_UNKNOWN       = 0,
        SDL_TRANSFER_CHARACTERISTICS_BT709         = 1,
        SDL_TRANSFER_CHARACTERISTICS_UNSPECIFIED   = 2,
        SDL_TRANSFER_CHARACTERISTICS_GAMMA22       = 4,
        SDL_TRANSFER_CHARACTERISTICS_GAMMA28       = 5,
        SDL_TRANSFER_CHARACTERISTICS_BT601         = 6,
        SDL_TRANSFER_CHARACTERISTICS_SMPTE240      = 7,
        SDL_TRANSFER_CHARACTERISTICS_LINEAR        = 8,
        SDL_TRANSFER_CHARACTERISTICS_LOG100        = 9,
        SDL_TRANSFER_CHARACTERISTICS_LOG100_SQRT10 = 10,
        SDL_TRANSFER_CHARACTERISTICS_IEC61966      = 11,
        SDL_TRANSFER_CHARACTERISTICS_BT1361        = 12,
        SDL_TRANSFER_CHARACTERISTICS_SRGB          = 13,
        SDL_TRANSFER_CHARACTERISTICS_BT2020_10BIT  = 14,
        SDL_TRANSFER_CHARACTERISTICS_BT2020_12BIT  = 15,
        SDL_TRANSFER_CHARACTERISTICS_PQ            = 16,
        SDL_TRANSFER_CHARACTERISTICS_SMPTE428      = 17,
        SDL_TRANSFER_CHARACTERISTICS_HLG           = 18,
        SDL_TRANSFER_CHARACTERISTICS_CUSTOM        = 31,
    }

    public enum SDL_MatrixCoefficients
    {
        SDL_MATRIX_COEFFICIENTS_IDENTITY            = 0,
        SDL_MATRIX_COEFFICIENTS_BT709               = 1,
        SDL_MATRIX_COEFFICIENTS_UNSPECIFIED         = 2,
        SDL_MATRIX_COEFFICIENTS_FCC                 = 4,
        SDL_MATRIX_COEFFICIENTS_BT470BG             = 5,
        SDL_MATRIX_COEFFICIENTS_BT601               = 6,
        SDL_MATRIX_COEFFICIENTS_SMPTE240            = 7,
        SDL_MATRIX_COEFFICIENTS_YCGCO               = 8,
        SDL_MATRIX_COEFFICIENTS_BT2020_NCL          = 9,
        SDL_MATRIX_COEFFICIENTS_BT2020_CL           = 10,
        SDL_MATRIX_COEFFICIENTS_SMPTE2085           = 11,
        SDL_MATRIX_COEFFICIENTS_CHROMA_DERIVED_NCL  = 12,
        SDL_MATRIX_COEFFICIENTS_CHROMA_DERIVED_CL   = 13,
        SDL_MATRIX_COEFFICIENTS_ICTCP               = 14,
        SDL_MATRIX_COEFFICIENTS_CUSTOM              = 31,
    }

    public enum SDL_ChromaLocation
    {
        SDL_CHROMA_LOCATION_NONE    = 0,
        SDL_CHROMA_LOCATION_LEFT    = 1,
        SDL_CHROMA_LOCATION_CENTER  = 2,
        SDL_CHROMA_LOCATION_TOPLEFT = 3,
    }

    public enum SDL_Colorspace
    {
        SDL_COLORSPACE_UNKNOWN        = 0,
        SDL_COLORSPACE_SRGB           = 301991328,
        SDL_COLORSPACE_SRGB_LINEAR    = 301991168,
        SDL_COLORSPACE_HDR10          = 301999616,
        SDL_COLORSPACE_JPEG           = 570426566,
        SDL_COLORSPACE_BT601_LIMITED  = 554703046,
        SDL_COLORSPACE_BT601_FULL     = 571480262,
        SDL_COLORSPACE_BT709_LIMITED  = 554697761,
        SDL_COLORSPACE_BT709_FULL     = 571474977,
        SDL_COLORSPACE_BT2020_LIMITED = 554706441,
        SDL_COLORSPACE_BT2020_FULL    = 571483657,
        SDL_COLORSPACE_RGB_DEFAULT    = 301991328,
        SDL_COLORSPACE_YUV_DEFAULT    = 570426566,
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

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_Palette
    {
        public int ncolors;
        public SDL_Color* colors;
        public uint version;
        public int refcount;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_PixelFormatDetails
    {
        public SDL_PixelFormat format;
        public byte bits_per_pixel;
        public byte bytes_per_pixel;
        public fixed byte padding[2];
        public uint Rmask;
        public uint Gmask;
        public uint Bmask;
        public uint Amask;
        public byte Rbits;
        public byte Gbits;
        public byte Bbits;
        public byte Abits;
        public byte Rshift;
        public byte Gshift;
        public byte Bshift;
        public byte Ashift;
    }

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetPixelFormatName(SDL_PixelFormat format);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetMasksForPixelFormat(SDL_PixelFormat format, out int bpp, out uint Rmask, out uint Gmask, out uint Bmask, out uint Amask);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_PixelFormat SDL_GetPixelFormatForMasks(int bpp, uint Rmask, uint Gmask, uint Bmask, uint Amask);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_PixelFormatDetails* SDL_GetPixelFormatDetails(SDL_PixelFormat format);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Palette* SDL_CreatePalette(int ncolors);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetPaletteColors(IntPtr palette, Span<SDL_Color> colors, int firstcolor, int ncolors);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_DestroyPalette(IntPtr palette);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_MapRGB(IntPtr format, IntPtr palette, byte r, byte g, byte b);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_MapRGBA(IntPtr format, IntPtr palette, byte r, byte g, byte b, byte a);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_GetRGB(uint pixel, IntPtr format, IntPtr palette, out byte r, out byte g, out byte b);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_GetRGBA(uint pixel, IntPtr format, IntPtr palette, out byte r, out byte g, out byte b, out byte a);

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
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_FRect
    {
        public float x;
        public float y;
        public float w;
        public float h;
    }

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_HasRectIntersection(ref SDL_Rect A, ref SDL_Rect B);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetRectIntersection(ref SDL_Rect A, ref SDL_Rect B, out SDL_Rect result);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetRectUnion(ref SDL_Rect A, ref SDL_Rect B, out SDL_Rect result);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetRectEnclosingPoints(Span<SDL_Point> points, int count, ref SDL_Rect clip, out SDL_Rect result);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetRectAndLineIntersection(ref SDL_Rect rect, ref int X1, ref int Y1, ref int X2, ref int Y2);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_HasRectIntersectionFloat(ref SDL_FRect A, ref SDL_FRect B);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetRectIntersectionFloat(ref SDL_FRect A, ref SDL_FRect B, out SDL_FRect result);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetRectUnionFloat(ref SDL_FRect A, ref SDL_FRect B, out SDL_FRect result);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetRectEnclosingPointsFloat(Span<SDL_FPoint> points, int count, ref SDL_FRect clip, out SDL_FRect result);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetRectAndLineIntersectionFloat(ref SDL_FRect rect, ref float X1, ref float Y1, ref float X2, ref float Y2);

    // /usr/local/include/SDL3/SDL_surface.h

    public const string
        SDL_PROP_SURFACE_SDR_WHITE_POINT_FLOAT   = "SDL.surface.SDR_white_point",
        SDL_PROP_SURFACE_HDR_HEADROOM_FLOAT      = "SDL.surface.HDR_headroom",
        SDL_PROP_SURFACE_TONEMAP_OPERATOR_STRING = "SDL.surface.tonemap",
        SDL_PROP_SURFACE_HOTSPOT_X_NUMBER        = "SDL.surface.hotspot.x",
        SDL_PROP_SURFACE_HOTSPOT_Y_NUMBER        = "SDL.surface.hotspot.y";

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

    public enum SDL_FlipMode
    {
        SDL_FLIP_NONE       = 0,
        SDL_FLIP_HORIZONTAL = 1,
        SDL_FLIP_VERTICAL   = 2,
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
    internal static partial SDL_Surface* SDL_CreateSurface(int width, int height, SDL_PixelFormat format);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Surface* SDL_CreateSurfaceFrom(int width, int height, SDL_PixelFormat format, IntPtr pixels, int pitch);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_DestroySurface(IntPtr surface);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_GetSurfaceProperties(IntPtr surface);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetSurfaceColorspace(IntPtr surface, SDL_Colorspace colorspace);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Colorspace SDL_GetSurfaceColorspace(IntPtr surface);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Palette* SDL_CreateSurfacePalette(IntPtr surface);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetSurfacePalette(IntPtr surface, IntPtr palette);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Palette* SDL_GetSurfacePalette(IntPtr surface);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_AddSurfaceAlternateImage(IntPtr surface, IntPtr image);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SurfaceHasAlternateImages(IntPtr surface);

    internal static Span<IntPtr> SDL_GetSurfaceImages(IntPtr surface)
    {
        var result = SDL_GetSurfaceImages(surface, out var count);
        return new Span<IntPtr>((void*) result, count);
    }

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetSurfaceImages(IntPtr surface, out int count);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_RemoveSurfaceAlternateImages(IntPtr surface);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_LockSurface(IntPtr surface);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_UnlockSurface(IntPtr surface);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Surface* SDL_LoadBMP_IO(IntPtr src, SDLBool closeio);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Surface* SDL_LoadBMP(string file);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SaveBMP_IO(IntPtr surface, IntPtr dst, SDLBool closeio);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SaveBMP(IntPtr surface, string file);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetSurfaceRLE(IntPtr surface, SDLBool enabled);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SurfaceHasRLE(IntPtr surface);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetSurfaceColorKey(IntPtr surface, SDLBool enabled, uint key);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SurfaceHasColorKey(IntPtr surface);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetSurfaceColorKey(IntPtr surface, out uint key);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetSurfaceColorMod(IntPtr surface, byte r, byte g, byte b);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetSurfaceColorMod(IntPtr surface, out byte r, out byte g, out byte b);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetSurfaceAlphaMod(IntPtr surface, byte alpha);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetSurfaceAlphaMod(IntPtr surface, out byte alpha);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetSurfaceBlendMode(IntPtr surface, uint blendMode);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetSurfaceBlendMode(IntPtr surface, IntPtr blendMode);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetSurfaceClipRect(IntPtr surface, ref SDL_Rect rect);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetSurfaceClipRect(IntPtr surface, out SDL_Rect rect);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_FlipSurface(IntPtr surface, SDL_FlipMode flip);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Surface* SDL_DuplicateSurface(IntPtr surface);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Surface* SDL_ScaleSurface(IntPtr surface, int width, int height, SDL_ScaleMode scaleMode);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Surface* SDL_ConvertSurface(IntPtr surface, SDL_PixelFormat format);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Surface* SDL_ConvertSurfaceAndColorspace(IntPtr surface, SDL_PixelFormat format, IntPtr palette, SDL_Colorspace colorspace, uint props);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_ConvertPixels(int width, int height, SDL_PixelFormat src_format, IntPtr src, int src_pitch, SDL_PixelFormat dst_format, IntPtr dst, int dst_pitch);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_ConvertPixelsAndColorspace(int width, int height, SDL_PixelFormat src_format, SDL_Colorspace src_colorspace, uint src_properties, IntPtr src, int src_pitch, SDL_PixelFormat dst_format, SDL_Colorspace dst_colorspace, uint dst_properties, IntPtr dst, int dst_pitch);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_PremultiplyAlpha(int width, int height, SDL_PixelFormat src_format, IntPtr src, int src_pitch, SDL_PixelFormat dst_format, IntPtr dst, int dst_pitch, SDLBool linear);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_PremultiplySurfaceAlpha(IntPtr surface, SDLBool linear);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_ClearSurface(IntPtr surface, float r, float g, float b, float a);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_FillSurfaceRect(IntPtr dst, IntPtr rect, uint color); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_FillSurfaceRects(IntPtr dst, Span<SDL_Rect> rects, int count, uint color);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_BlitSurface(IntPtr src, IntPtr srcrect, IntPtr dst, IntPtr dstrect); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_BlitSurfaceUnchecked(IntPtr src, IntPtr srcrect, IntPtr dst, IntPtr dstrect); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_BlitSurfaceScaled(IntPtr src, IntPtr srcrect, IntPtr dst, IntPtr dstrect, SDL_ScaleMode scaleMode); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_BlitSurfaceUncheckedScaled(IntPtr src, IntPtr srcrect, IntPtr dst, IntPtr dstrect, SDL_ScaleMode scaleMode); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_BlitSurfaceTiled(IntPtr src, IntPtr srcrect, IntPtr dst, IntPtr dstrect); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_BlitSurfaceTiledWithScale(IntPtr src, IntPtr srcrect, float scale, SDL_ScaleMode scaleMode, IntPtr dst, IntPtr dstrect); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_BlitSurface9Grid(IntPtr src, IntPtr srcrect, int left_width, int right_width, int top_height, int bottom_height, float scale, SDL_ScaleMode scaleMode, IntPtr dst, IntPtr dstrect); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_MapSurfaceRGB(IntPtr surface, byte r, byte g, byte b);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_MapSurfaceRGBA(IntPtr surface, byte r, byte g, byte b, byte a);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_ReadSurfacePixel(IntPtr surface, int x, int y, out byte r, out byte g, out byte b, out byte a);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_ReadSurfacePixelFloat(IntPtr surface, int x, int y, out float r, out float g, out float b, out float a);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_WriteSurfacePixel(IntPtr surface, int x, int y, byte r, byte g, byte b, byte a);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_WriteSurfacePixelFloat(IntPtr surface, int x, int y, float r, float g, float b, float a);

    // /usr/local/include/SDL3/SDL_video.h

    public const string
        SDL_PROP_GLOBAL_VIDEO_WAYLAND_WL_DISPLAY_POINTER           = "SDL.video.wayland.wl_display",
        SDL_PROP_DISPLAY_HDR_ENABLED_BOOLEAN                       = "SDL.display.HDR_enabled",
        SDL_PROP_DISPLAY_KMSDRM_PANEL_ORIENTATION_NUMBER           = "SDL.display.KMSDRM.panel_orientation",
        SDL_PROP_WINDOW_CREATE_ALWAYS_ON_TOP_BOOLEAN               = "SDL.window.create.always_on_top",
        SDL_PROP_WINDOW_CREATE_BORDERLESS_BOOLEAN                  = "SDL.window.create.borderless",
        SDL_PROP_WINDOW_CREATE_FOCUSABLE_BOOLEAN                   = "SDL.window.create.focusable",
        SDL_PROP_WINDOW_CREATE_EXTERNAL_GRAPHICS_CONTEXT_BOOLEAN   = "SDL.window.create.external_graphics_context",
        SDL_PROP_WINDOW_CREATE_FLAGS_NUMBER                        = "SDL.window.create.flags",
        SDL_PROP_WINDOW_CREATE_FULLSCREEN_BOOLEAN                  = "SDL.window.create.fullscreen",
        SDL_PROP_WINDOW_CREATE_HEIGHT_NUMBER                       = "SDL.window.create.height",
        SDL_PROP_WINDOW_CREATE_HIDDEN_BOOLEAN                      = "SDL.window.create.hidden",
        SDL_PROP_WINDOW_CREATE_HIGH_PIXEL_DENSITY_BOOLEAN          = "SDL.window.create.high_pixel_density",
        SDL_PROP_WINDOW_CREATE_MAXIMIZED_BOOLEAN                   = "SDL.window.create.maximized",
        SDL_PROP_WINDOW_CREATE_MENU_BOOLEAN                        = "SDL.window.create.menu",
        SDL_PROP_WINDOW_CREATE_METAL_BOOLEAN                       = "SDL.window.create.metal",
        SDL_PROP_WINDOW_CREATE_MINIMIZED_BOOLEAN                   = "SDL.window.create.minimized",
        SDL_PROP_WINDOW_CREATE_MODAL_BOOLEAN                       = "SDL.window.create.modal",
        SDL_PROP_WINDOW_CREATE_MOUSE_GRABBED_BOOLEAN               = "SDL.window.create.mouse_grabbed",
        SDL_PROP_WINDOW_CREATE_OPENGL_BOOLEAN                      = "SDL.window.create.opengl",
        SDL_PROP_WINDOW_CREATE_PARENT_POINTER                      = "SDL.window.create.parent",
        SDL_PROP_WINDOW_CREATE_RESIZABLE_BOOLEAN                   = "SDL.window.create.resizable",
        SDL_PROP_WINDOW_CREATE_TITLE_STRING                        = "SDL.window.create.title",
        SDL_PROP_WINDOW_CREATE_TRANSPARENT_BOOLEAN                 = "SDL.window.create.transparent",
        SDL_PROP_WINDOW_CREATE_TOOLTIP_BOOLEAN                     = "SDL.window.create.tooltip",
        SDL_PROP_WINDOW_CREATE_UTILITY_BOOLEAN                     = "SDL.window.create.utility",
        SDL_PROP_WINDOW_CREATE_VULKAN_BOOLEAN                      = "SDL.window.create.vulkan",
        SDL_PROP_WINDOW_CREATE_WIDTH_NUMBER                        = "SDL.window.create.width",
        SDL_PROP_WINDOW_CREATE_X_NUMBER                            = "SDL.window.create.x",
        SDL_PROP_WINDOW_CREATE_Y_NUMBER                            = "SDL.window.create.y",
        SDL_PROP_WINDOW_CREATE_COCOA_WINDOW_POINTER                = "SDL.window.create.cocoa.window",
        SDL_PROP_WINDOW_CREATE_COCOA_VIEW_POINTER                  = "SDL.window.create.cocoa.view",
        SDL_PROP_WINDOW_CREATE_WAYLAND_SURFACE_ROLE_CUSTOM_BOOLEAN = "SDL.window.create.wayland.surface_role_custom",
        SDL_PROP_WINDOW_CREATE_WAYLAND_CREATE_EGL_WINDOW_BOOLEAN   = "SDL.window.create.wayland.create_egl_window",
        SDL_PROP_WINDOW_CREATE_WAYLAND_WL_SURFACE_POINTER          = "SDL.window.create.wayland.wl_surface",
        SDL_PROP_WINDOW_CREATE_WIN32_HWND_POINTER                  = "SDL.window.create.win32.hwnd",
        SDL_PROP_WINDOW_CREATE_WIN32_PIXEL_FORMAT_HWND_POINTER     = "SDL.window.create.win32.pixel_format_hwnd",
        SDL_PROP_WINDOW_CREATE_X11_WINDOW_NUMBER                   = "SDL.window.create.x11.window",
        SDL_PROP_WINDOW_SHAPE_POINTER                              = "SDL.window.shape",
        SDL_PROP_WINDOW_HDR_ENABLED_BOOLEAN                        = "SDL.window.HDR_enabled",
        SDL_PROP_WINDOW_SDR_WHITE_LEVEL_FLOAT                      = "SDL.window.SDR_white_level",
        SDL_PROP_WINDOW_HDR_HEADROOM_FLOAT                         = "SDL.window.HDR_headroom",
        SDL_PROP_WINDOW_ANDROID_WINDOW_POINTER                     = "SDL.window.android.window",
        SDL_PROP_WINDOW_ANDROID_SURFACE_POINTER                    = "SDL.window.android.surface",
        SDL_PROP_WINDOW_UIKIT_WINDOW_POINTER                       = "SDL.window.uikit.window",
        SDL_PROP_WINDOW_UIKIT_METAL_VIEW_TAG_NUMBER                = "SDL.window.uikit.metal_view_tag",
        SDL_PROP_WINDOW_UIKIT_OPENGL_FRAMEBUFFER_NUMBER            = "SDL.window.uikit.opengl.framebuffer",
        SDL_PROP_WINDOW_UIKIT_OPENGL_RENDERBUFFER_NUMBER           = "SDL.window.uikit.opengl.renderbuffer",
        SDL_PROP_WINDOW_UIKIT_OPENGL_RESOLVE_FRAMEBUFFER_NUMBER    = "SDL.window.uikit.opengl.resolve_framebuffer",
        SDL_PROP_WINDOW_KMSDRM_DEVICE_INDEX_NUMBER                 = "SDL.window.kmsdrm.dev_index",
        SDL_PROP_WINDOW_KMSDRM_DRM_FD_NUMBER                       = "SDL.window.kmsdrm.drm_fd",
        SDL_PROP_WINDOW_KMSDRM_GBM_DEVICE_POINTER                  = "SDL.window.kmsdrm.gbm_dev",
        SDL_PROP_WINDOW_COCOA_WINDOW_POINTER                       = "SDL.window.cocoa.window",
        SDL_PROP_WINDOW_COCOA_METAL_VIEW_TAG_NUMBER                = "SDL.window.cocoa.metal_view_tag",
        SDL_PROP_WINDOW_OPENVR_OVERLAY_ID                          = "SDL.window.openvr.overlay_id",
        SDL_PROP_WINDOW_VIVANTE_DISPLAY_POINTER                    = "SDL.window.vivante.display",
        SDL_PROP_WINDOW_VIVANTE_WINDOW_POINTER                     = "SDL.window.vivante.window",
        SDL_PROP_WINDOW_VIVANTE_SURFACE_POINTER                    = "SDL.window.vivante.surface",
        SDL_PROP_WINDOW_WIN32_HWND_POINTER                         = "SDL.window.win32.hwnd",
        SDL_PROP_WINDOW_WIN32_HDC_POINTER                          = "SDL.window.win32.hdc",
        SDL_PROP_WINDOW_WIN32_INSTANCE_POINTER                     = "SDL.window.win32.instance",
        SDL_PROP_WINDOW_WAYLAND_DISPLAY_POINTER                    = "SDL.window.wayland.display",
        SDL_PROP_WINDOW_WAYLAND_SURFACE_POINTER                    = "SDL.window.wayland.surface",
        SDL_PROP_WINDOW_WAYLAND_VIEWPORT_POINTER                   = "SDL.window.wayland.viewport",
        SDL_PROP_WINDOW_WAYLAND_EGL_WINDOW_POINTER                 = "SDL.window.wayland.egl_window",
        SDL_PROP_WINDOW_WAYLAND_XDG_SURFACE_POINTER                = "SDL.window.wayland.xdg_surface",
        SDL_PROP_WINDOW_WAYLAND_XDG_TOPLEVEL_POINTER               = "SDL.window.wayland.xdg_toplevel",
        SDL_PROP_WINDOW_WAYLAND_XDG_TOPLEVEL_EXPORT_HANDLE_STRING  = "SDL.window.wayland.xdg_toplevel_export_handle",
        SDL_PROP_WINDOW_WAYLAND_XDG_POPUP_POINTER                  = "SDL.window.wayland.xdg_popup",
        SDL_PROP_WINDOW_WAYLAND_XDG_POSITIONER_POINTER             = "SDL.window.wayland.xdg_positioner",
        SDL_PROP_WINDOW_X11_DISPLAY_POINTER                        = "SDL.window.x11.display",
        SDL_PROP_WINDOW_X11_SCREEN_NUMBER                          = "SDL.window.x11.screen",
        SDL_PROP_WINDOW_X11_WINDOW_NUMBER                          = "SDL.window.x11.window";

    public enum SDL_SystemTheme
    {
        SDL_SYSTEM_THEME_UNKNOWN = 0,
        SDL_SYSTEM_THEME_LIGHT   = 1,
        SDL_SYSTEM_THEME_DARK    = 2,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_DisplayMode
    {
        public uint displayID;
        public SDL_PixelFormat format;
        public int w;
        public int h;
        public float pixel_density;
        public float refresh_rate;
        public int refresh_rate_numerator;
        public int refresh_rate_denominator;
        public IntPtr @internal;
    }

    public enum SDL_DisplayOrientation
    {
        SDL_ORIENTATION_UNKNOWN           = 0,
        SDL_ORIENTATION_LANDSCAPE         = 1,
        SDL_ORIENTATION_LANDSCAPE_FLIPPED = 2,
        SDL_ORIENTATION_PORTRAIT          = 3,
        SDL_ORIENTATION_PORTRAIT_FLIPPED  = 4,
    }

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

    public enum SDL_FlashOperation
    {
        SDL_FLASH_CANCEL        = 0,
        SDL_FLASH_BRIEFLY       = 1,
        SDL_FLASH_UNTIL_FOCUSED = 2,
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr SDL_EGLAttribArrayCallback();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr SDL_EGLIntArrayCallback();

    public enum SDL_GLAttr
    {
        SDL_GL_RED_SIZE                     = 0,
        SDL_GL_GREEN_SIZE                   = 1,
        SDL_GL_BLUE_SIZE                    = 2,
        SDL_GL_ALPHA_SIZE                   = 3,
        SDL_GL_BUFFER_SIZE                  = 4,
        SDL_GL_DOUBLEBUFFER                 = 5,
        SDL_GL_DEPTH_SIZE                   = 6,
        SDL_GL_STENCIL_SIZE                 = 7,
        SDL_GL_ACCUM_RED_SIZE               = 8,
        SDL_GL_ACCUM_GREEN_SIZE             = 9,
        SDL_GL_ACCUM_BLUE_SIZE              = 10,
        SDL_GL_ACCUM_ALPHA_SIZE             = 11,
        SDL_GL_STEREO                       = 12,
        SDL_GL_MULTISAMPLEBUFFERS           = 13,
        SDL_GL_MULTISAMPLESAMPLES           = 14,
        SDL_GL_ACCELERATED_VISUAL           = 15,
        SDL_GL_RETAINED_BACKING             = 16,
        SDL_GL_CONTEXT_MAJOR_VERSION        = 17,
        SDL_GL_CONTEXT_MINOR_VERSION        = 18,
        SDL_GL_CONTEXT_FLAGS                = 19,
        SDL_GL_CONTEXT_PROFILE_MASK         = 20,
        SDL_GL_SHARE_WITH_CURRENT_CONTEXT   = 21,
        SDL_GL_FRAMEBUFFER_SRGB_CAPABLE     = 22,
        SDL_GL_CONTEXT_RELEASE_BEHAVIOR     = 23,
        SDL_GL_CONTEXT_RESET_NOTIFICATION   = 24,
        SDL_GL_CONTEXT_NO_ERROR             = 25,
        SDL_GL_FLOATBUFFERS                 = 26,
        SDL_GL_EGL_PLATFORM                 = 27,
    }

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_GetNumVideoDrivers();

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetVideoDriver(int index);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetCurrentVideoDriver();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_SystemTheme SDL_GetSystemTheme();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetDisplays(out int count);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_GetPrimaryDisplay();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_GetDisplayProperties(uint displayID);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetDisplayName(uint displayID);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetDisplayBounds(uint displayID, out SDL_Rect rect);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetDisplayUsableBounds(uint displayID, out SDL_Rect rect);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_DisplayOrientation SDL_GetNaturalDisplayOrientation(uint displayID);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_DisplayOrientation SDL_GetCurrentDisplayOrientation(uint displayID);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial float SDL_GetDisplayContentScale(uint displayID);

    internal static Span<IntPtr> SDL_GetFullscreenDisplayModes(uint displayID)
    {
        var result = SDL_GetFullscreenDisplayModes(displayID, out var count);
        return new Span<IntPtr>((void*) result, count);
    }

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetFullscreenDisplayModes(uint displayID, out int count);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetClosestFullscreenDisplayMode(uint displayID, int w, int h, float refresh_rate, SDLBool include_high_density_modes, out SDL_DisplayMode closest);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_DisplayMode* SDL_GetDesktopDisplayMode(uint displayID);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_DisplayMode* SDL_GetCurrentDisplayMode(uint displayID);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_GetDisplayForPoint(ref SDL_Point point);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_GetDisplayForRect(ref SDL_Rect rect);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_GetDisplayForWindow(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial float SDL_GetWindowPixelDensity(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial float SDL_GetWindowDisplayScale(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetWindowFullscreenMode(IntPtr window, ref SDL_DisplayMode mode);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_DisplayMode* SDL_GetWindowFullscreenMode(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetWindowICCProfile(IntPtr window, out UIntPtr size);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_PixelFormat SDL_GetWindowPixelFormat(IntPtr window);

    internal static Span<IntPtr> SDL_GetWindows()
    {
        var result = SDL_GetWindows(out var count);
        return new Span<IntPtr>((void*) result, count);
    }

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetWindows(out int count);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_CreateWindow(string title, int w, int h, SDL_WindowFlags flags);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_CreatePopupWindow(IntPtr parent, int offset_x, int offset_y, int w, int h, SDL_WindowFlags flags);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_CreateWindowWithProperties(uint props);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_GetWindowID(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetWindowFromID(uint id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetWindowParent(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_GetWindowProperties(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_WindowFlags SDL_GetWindowFlags(IntPtr window);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetWindowTitle(IntPtr window, string title);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetWindowTitle(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetWindowIcon(IntPtr window, IntPtr icon);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetWindowPosition(IntPtr window, int x, int y);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetWindowPosition(IntPtr window, out int x, out int y);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetWindowSize(IntPtr window, int w, int h);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetWindowSize(IntPtr window, out int w, out int h);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetWindowSafeArea(IntPtr window, out SDL_Rect rect);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetWindowAspectRatio(IntPtr window, float min_aspect, float max_aspect);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetWindowAspectRatio(IntPtr window, out float min_aspect, out float max_aspect);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetWindowBordersSize(IntPtr window, out int top, out int left, out int bottom, out int right);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetWindowSizeInPixels(IntPtr window, out int w, out int h);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetWindowMinimumSize(IntPtr window, int min_w, int min_h);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetWindowMinimumSize(IntPtr window, out int w, out int h);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetWindowMaximumSize(IntPtr window, int max_w, int max_h);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetWindowMaximumSize(IntPtr window, out int w, out int h);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetWindowBordered(IntPtr window, SDLBool bordered);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetWindowResizable(IntPtr window, SDLBool resizable);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetWindowAlwaysOnTop(IntPtr window, SDLBool on_top);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_ShowWindow(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_HideWindow(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RaiseWindow(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_MaximizeWindow(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_MinimizeWindow(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RestoreWindow(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetWindowFullscreen(IntPtr window, SDLBool fullscreen);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SyncWindow(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_WindowHasSurface(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Surface* SDL_GetWindowSurface(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetWindowSurfaceVSync(IntPtr window, int vsync);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetWindowSurfaceVSync(IntPtr window, out int vsync);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_UpdateWindowSurface(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_UpdateWindowSurfaceRects(IntPtr window, Span<SDL_Rect> rects, int numrects);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_DestroyWindowSurface(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetWindowKeyboardGrab(IntPtr window, SDLBool grabbed);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetWindowMouseGrab(IntPtr window, SDLBool grabbed);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetWindowKeyboardGrab(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetWindowMouseGrab(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetGrabbedWindow();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetWindowMouseRect(IntPtr window, ref SDL_Rect rect);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Rect* SDL_GetWindowMouseRect(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetWindowOpacity(IntPtr window, float opacity);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial float SDL_GetWindowOpacity(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetWindowParent(IntPtr window, IntPtr parent);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetWindowModal(IntPtr window, SDLBool modal);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetWindowFocusable(IntPtr window, SDLBool focusable);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_ShowWindowSystemMenu(IntPtr window, int x, int y);

    public enum SDL_HitTestResult
    {
        SDL_HITTEST_NORMAL              = 0,
        SDL_HITTEST_DRAGGABLE           = 1,
        SDL_HITTEST_RESIZE_TOPLEFT      = 2,
        SDL_HITTEST_RESIZE_TOP          = 3,
        SDL_HITTEST_RESIZE_TOPRIGHT     = 4,
        SDL_HITTEST_RESIZE_RIGHT        = 5,
        SDL_HITTEST_RESIZE_BOTTOMRIGHT  = 6,
        SDL_HITTEST_RESIZE_BOTTOM       = 7,
        SDL_HITTEST_RESIZE_BOTTOMLEFT   = 8,
        SDL_HITTEST_RESIZE_LEFT         = 9,
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate SDL_HitTestResult SDL_HitTest(IntPtr win, SDL_Point* area, IntPtr data);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetWindowHitTest(IntPtr window, SDL_HitTest callback, IntPtr callback_data);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetWindowShape(IntPtr window, IntPtr shape);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_FlashWindow(IntPtr window, SDL_FlashOperation operation);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_DestroyWindow(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_ScreenSaverEnabled();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_EnableScreenSaver();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_DisableScreenSaver();

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GL_LoadLibrary(string path);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GL_GetProcAddress(string proc);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_EGL_GetProcAddress(string proc);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_GL_UnloadLibrary();

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GL_ExtensionSupported(string extension);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_GL_ResetAttributes();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GL_SetAttribute(SDL_GLAttr attr, int value);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GL_GetAttribute(SDL_GLAttr attr, out int value);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GL_CreateContext(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GL_MakeCurrent(IntPtr window, IntPtr context);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GL_GetCurrentWindow();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GL_GetCurrentContext();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_EGL_GetCurrentDisplay();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_EGL_GetCurrentConfig();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_EGL_GetWindowSurface(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_EGL_SetAttributeCallbacks(SDL_EGLAttribArrayCallback platformAttribCallback, SDL_EGLIntArrayCallback surfaceAttribCallback, SDL_EGLIntArrayCallback contextAttribCallback, IntPtr userdata);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GL_SetSwapInterval(int interval);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GL_GetSwapInterval(out int interval);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GL_SwapWindow(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GL_DestroyContext(IntPtr context);

    // /usr/local/include/SDL3/SDL_guid.h

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GUID
    {
        public fixed byte data[16];
    }

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void SDL_GUIDToString(SDL_GUID guid, string pszGUID, int cbGUID);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial SDL_GUID SDL_StringToGUID(string pchGUID);

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

    public const string
        SDL_PROP_TEXTINPUT_TYPE_NUMBER              = "SDL.textinput.type",
        SDL_PROP_TEXTINPUT_CAPITALIZATION_NUMBER    = "SDL.textinput.capitalization",
        SDL_PROP_TEXTINPUT_AUTOCORRECT_BOOLEAN      = "SDL.textinput.autocorrect",
        SDL_PROP_TEXTINPUT_MULTILINE_BOOLEAN        = "SDL.textinput.multiline",
        SDL_PROP_TEXTINPUT_ANDROID_INPUTTYPE_NUMBER = "SDL.textinput.android.inputtype"
        ;

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

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_StartTextInput(IntPtr window);

    public enum SDL_TextInputType
    {
        SDL_TEXTINPUT_TYPE_TEXT                    = 0,
        SDL_TEXTINPUT_TYPE_TEXT_NAME               = 1,
        SDL_TEXTINPUT_TYPE_TEXT_EMAIL              = 2,
        SDL_TEXTINPUT_TYPE_TEXT_USERNAME           = 3,
        SDL_TEXTINPUT_TYPE_TEXT_PASSWORD_HIDDEN    = 4,
        SDL_TEXTINPUT_TYPE_TEXT_PASSWORD_VISIBLE   = 5,
        SDL_TEXTINPUT_TYPE_NUMBER                  = 6,
        SDL_TEXTINPUT_TYPE_NUMBER_PASSWORD_HIDDEN  = 7,
        SDL_TEXTINPUT_TYPE_NUMBER_PASSWORD_VISIBLE = 8,
    }

    public enum SDL_Capitalization
    {
        SDL_CAPITALIZE_NONE      = 0,
        SDL_CAPITALIZE_SENTENCES = 1,
        SDL_CAPITALIZE_WORDS     = 2,
        SDL_CAPITALIZE_LETTERS   = 3,
    }

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_StartTextInputWithProperties(IntPtr window, uint props);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_TextInputActive(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_StopTextInput(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_ClearComposition(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetTextInputArea(IntPtr window, ref SDL_Rect rect, int cursor);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetTextInputArea(IntPtr window, out SDL_Rect rect, out int cursor);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_HasScreenKeyboardSupport();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_ScreenKeyboardShown(IntPtr window);

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
    public struct SDL_TextEditingEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint windowID;
        public byte* text;
        public int start;
        public int length;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_TextEditingCandidatesEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint windowID;
        public byte** candidates;
        public int num_candidates;
        public int selected_candidate;
        public SDLBool horizontal;
        public byte padding1;
        public byte padding2;
        public byte padding3;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_TextInputEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint windowID;
        public byte* text;
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
    public struct SDL_AudioDeviceEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint which;
        public SDLBool recording;
        public byte padding1;
        public byte padding2;
        public byte padding3;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_CameraDeviceEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint which;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_RenderEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint windowID;
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
    public struct SDL_PenProximityEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint windowID;
        public uint which;
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
    public struct SDL_ClipboardEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public SDLBool owner;
        public int num_mime_types;
        public byte** mime_types;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_SensorEvent
    {
        public SDL_EventType type;
        public uint reserved;
        public ulong timestamp;
        public uint which;
        public fixed float data[6];
        public ulong sensor_timestamp;
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
        [FieldOffset(0)]
        public SDL_TextEditingEvent edit;
        [FieldOffset(0)]
        public SDL_TextEditingCandidatesEvent edit_candidates;
        [FieldOffset(0)]
        public SDL_TextInputEvent text;
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
        [FieldOffset(0)]
        public SDL_AudioDeviceEvent adevice;
        [FieldOffset(0)]
        public SDL_CameraDeviceEvent cdevice;
        [FieldOffset(0)]
        public SDL_SensorEvent sensor;
        [FieldOffset(0)]
        public SDL_QuitEvent quit;
        [FieldOffset(0)]
        public SDL_UserEvent user;
        [FieldOffset(0)]
        public SDL_TouchFingerEvent tfinger;
        [FieldOffset(0)]
        public SDL_PenProximityEvent pproximity;
        [FieldOffset(0)]
        public SDL_RenderEvent render;
        [FieldOffset(0)]
        public SDL_DropEvent drop;
        [FieldOffset(0)]
        public SDL_ClipboardEvent clipboard;
        [FieldOffset(0)]
        public fixed byte padding[128];
    }

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_PumpEvents();

    public enum SDL_EventAction
    {
        SDL_ADDEVENT  = 0,
        SDL_PEEKEVENT = 1,
        SDL_GETEVENT  = 2,
    }

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_PeepEvents(Span<SDL_Event> events, int numevents, SDL_EventAction action, uint minType, uint maxType);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_HasEvent(uint type);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_HasEvents(uint minType, uint maxType);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_FlushEvent(uint type);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_FlushEvents(uint minType, uint maxType);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_PollEvent(out SDL_Event @event);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_WaitEvent(out SDL_Event @event);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_WaitEventTimeout(out SDL_Event @event, int timeoutMS);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_PushEvent(ref SDL_Event @event);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool SDL_EventFilter(IntPtr userdata, SDL_Event* evt);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_SetEventFilter(SDL_EventFilter filter, IntPtr userdata);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetEventFilter(out SDL_EventFilter filter, out IntPtr userdata);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_AddEventWatch(SDL_EventFilter filter, IntPtr userdata);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_RemoveEventWatch(SDL_EventFilter filter, IntPtr userdata);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_FilterEvents(SDL_EventFilter filter, IntPtr userdata);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_SetEventEnabled(uint type, SDLBool enabled);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_EventEnabled(uint type);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_RegisterEvents(int numevents);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetWindowFromEvent(ref SDL_Event @event);

    // /usr/local/include/SDL3/SDL_hidapi.h

    public enum SDL_hid_bus_type
    {
        SDL_HID_API_BUS_UNKNOWN = 0,
        SDL_HID_API_BUS_USB = 1,
        SDL_HID_API_BUS_BLUETOOTH = 2,
        SDL_HID_API_BUS_I2C = 3,
        SDL_HID_API_BUS_SPI = 4,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_hid_device_info
    {
        public byte* path;
        public ushort vendor_id;
        public ushort product_id;
        public byte* serial_number;
        public ushort release_number;
        public byte* manufacturer_string;
        public byte* product_string;
        public ushort usage_page;
        public ushort usage;
        public int interface_number;
        public int interface_class;
        public int interface_subclass;
        public int interface_protocol;
        public SDL_hid_bus_type bus_type;
        public SDL_hid_device_info* next;
    }

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_hid_init();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_hid_exit();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_hid_device_change_count();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_hid_device_info* SDL_hid_enumerate(ushort vendor_id, ushort product_id);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_hid_free_enumeration(IntPtr devs); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_hid_open(ushort vendor_id, ushort product_id, string serial_number);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_hid_open_path(string path);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_hid_write(IntPtr dev, IntPtr data, UIntPtr length); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_hid_read_timeout(IntPtr dev, IntPtr data, UIntPtr length, int milliseconds); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_hid_read(IntPtr dev, IntPtr data, UIntPtr length); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_hid_set_nonblocking(IntPtr dev, int nonblock);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_hid_send_feature_report(IntPtr dev, IntPtr data, UIntPtr length); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_hid_get_feature_report(IntPtr dev, IntPtr data, UIntPtr length); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_hid_get_input_report(IntPtr dev, IntPtr data, UIntPtr length); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_hid_close(IntPtr dev);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_hid_get_manufacturer_string(IntPtr dev, string @string, UIntPtr maxlen);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_hid_get_product_string(IntPtr dev, string @string, UIntPtr maxlen);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_hid_get_serial_number_string(IntPtr dev, string @string, UIntPtr maxlen);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_hid_get_indexed_string(IntPtr dev, int string_index, string @string, UIntPtr maxlen);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_hid_device_info* SDL_hid_get_device_info(IntPtr dev);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_hid_get_report_descriptor(IntPtr dev, IntPtr buf, UIntPtr buf_size); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_hid_ble_scan(SDLBool active);

    // /usr/local/include/SDL3/SDL_hints.h

    public const string SDL_HINT_ALLOW_ALT_TAB_WHILE_GRABBED = "SDL_ALLOW_ALT_TAB_WHILE_GRABBED";
    public const string SDL_HINT_ANDROID_ALLOW_RECREATE_ACTIVITY = "SDL_ANDROID_ALLOW_RECREATE_ACTIVITY";
    public const string SDL_HINT_ANDROID_BLOCK_ON_PAUSE = "SDL_ANDROID_BLOCK_ON_PAUSE";
    public const string SDL_HINT_ANDROID_LOW_LATENCY_AUDIO = "SDL_ANDROID_LOW_LATENCY_AUDIO";
    public const string SDL_HINT_ANDROID_TRAP_BACK_BUTTON = "SDL_ANDROID_TRAP_BACK_BUTTON";
    public const string SDL_HINT_APP_ID = "SDL_APP_ID";
    public const string SDL_HINT_APP_NAME = "SDL_APP_NAME";
    public const string SDL_HINT_APPLE_TV_CONTROLLER_UI_EVENTS = "SDL_APPLE_TV_CONTROLLER_UI_EVENTS";
    public const string SDL_HINT_APPLE_TV_REMOTE_ALLOW_ROTATION = "SDL_APPLE_TV_REMOTE_ALLOW_ROTATION";
    public const string SDL_HINT_AUDIO_ALSA_DEFAULT_DEVICE = "SDL_AUDIO_ALSA_DEFAULT_DEVICE";
    public const string SDL_HINT_AUDIO_ALSA_DEFAULT_PLAYBACK_DEVICE = "SDL_AUDIO_ALSA_DEFAULT_PLAYBACK_DEVICE";
    public const string SDL_HINT_AUDIO_ALSA_DEFAULT_RECORDING_DEVICE = "SDL_AUDIO_ALSA_DEFAULT_RECORDING_DEVICE";
    public const string SDL_HINT_AUDIO_CATEGORY = "SDL_AUDIO_CATEGORY";
    public const string SDL_HINT_AUDIO_CHANNELS = "SDL_AUDIO_CHANNELS";
    public const string SDL_HINT_AUDIO_DEVICE_APP_ICON_NAME = "SDL_AUDIO_DEVICE_APP_ICON_NAME";
    public const string SDL_HINT_AUDIO_DEVICE_SAMPLE_FRAMES = "SDL_AUDIO_DEVICE_SAMPLE_FRAMES";
    public const string SDL_HINT_AUDIO_DEVICE_STREAM_NAME = "SDL_AUDIO_DEVICE_STREAM_NAME";
    public const string SDL_HINT_AUDIO_DEVICE_STREAM_ROLE = "SDL_AUDIO_DEVICE_STREAM_ROLE";
    public const string SDL_HINT_AUDIO_DISK_INPUT_FILE = "SDL_AUDIO_DISK_INPUT_FILE";
    public const string SDL_HINT_AUDIO_DISK_OUTPUT_FILE = "SDL_AUDIO_DISK_OUTPUT_FILE";
    public const string SDL_HINT_AUDIO_DISK_TIMESCALE = "SDL_AUDIO_DISK_TIMESCALE";
    public const string SDL_HINT_AUDIO_DRIVER = "SDL_AUDIO_DRIVER";
    public const string SDL_HINT_AUDIO_DUMMY_TIMESCALE = "SDL_AUDIO_DUMMY_TIMESCALE";
    public const string SDL_HINT_AUDIO_FORMAT = "SDL_AUDIO_FORMAT";
    public const string SDL_HINT_AUDIO_FREQUENCY = "SDL_AUDIO_FREQUENCY";
    public const string SDL_HINT_AUDIO_INCLUDE_MONITORS = "SDL_AUDIO_INCLUDE_MONITORS";
    public const string SDL_HINT_AUTO_UPDATE_JOYSTICKS = "SDL_AUTO_UPDATE_JOYSTICKS";
    public const string SDL_HINT_AUTO_UPDATE_SENSORS = "SDL_AUTO_UPDATE_SENSORS";
    public const string SDL_HINT_BMP_SAVE_LEGACY_FORMAT = "SDL_BMP_SAVE_LEGACY_FORMAT";
    public const string SDL_HINT_CAMERA_DRIVER = "SDL_CAMERA_DRIVER";
    public const string SDL_HINT_CPU_FEATURE_MASK = "SDL_CPU_FEATURE_MASK";
    public const string SDL_HINT_JOYSTICK_DIRECTINPUT = "SDL_JOYSTICK_DIRECTINPUT";
    public const string SDL_HINT_FILE_DIALOG_DRIVER = "SDL_FILE_DIALOG_DRIVER";
    public const string SDL_HINT_DISPLAY_USABLE_BOUNDS = "SDL_DISPLAY_USABLE_BOUNDS";
    public const string SDL_HINT_EMSCRIPTEN_ASYNCIFY = "SDL_EMSCRIPTEN_ASYNCIFY";
    public const string SDL_HINT_EMSCRIPTEN_CANVAS_SELECTOR = "SDL_EMSCRIPTEN_CANVAS_SELECTOR";
    public const string SDL_HINT_EMSCRIPTEN_KEYBOARD_ELEMENT = "SDL_EMSCRIPTEN_KEYBOARD_ELEMENT";
    public const string SDL_HINT_ENABLE_SCREEN_KEYBOARD = "SDL_ENABLE_SCREEN_KEYBOARD";
    public const string SDL_HINT_EVDEV_DEVICES = "SDL_EVDEV_DEVICES";
    public const string SDL_HINT_EVENT_LOGGING = "SDL_EVENT_LOGGING";
    public const string SDL_HINT_FORCE_RAISEWINDOW = "SDL_FORCE_RAISEWINDOW";
    public const string SDL_HINT_FRAMEBUFFER_ACCELERATION = "SDL_FRAMEBUFFER_ACCELERATION";
    public const string SDL_HINT_GAMECONTROLLERCONFIG = "SDL_GAMECONTROLLERCONFIG";
    public const string SDL_HINT_GAMECONTROLLERCONFIG_FILE = "SDL_GAMECONTROLLERCONFIG_FILE";
    public const string SDL_HINT_GAMECONTROLLERTYPE = "SDL_GAMECONTROLLERTYPE";
    public const string SDL_HINT_GAMECONTROLLER_IGNORE_DEVICES = "SDL_GAMECONTROLLER_IGNORE_DEVICES";
    public const string SDL_HINT_GAMECONTROLLER_IGNORE_DEVICES_EXCEPT = "SDL_GAMECONTROLLER_IGNORE_DEVICES_EXCEPT";
    public const string SDL_HINT_GAMECONTROLLER_SENSOR_FUSION = "SDL_GAMECONTROLLER_SENSOR_FUSION";
    public const string SDL_HINT_GDK_TEXTINPUT_DEFAULT_TEXT = "SDL_GDK_TEXTINPUT_DEFAULT_TEXT";
    public const string SDL_HINT_GDK_TEXTINPUT_DESCRIPTION = "SDL_GDK_TEXTINPUT_DESCRIPTION";
    public const string SDL_HINT_GDK_TEXTINPUT_MAX_LENGTH = "SDL_GDK_TEXTINPUT_MAX_LENGTH";
    public const string SDL_HINT_GDK_TEXTINPUT_SCOPE = "SDL_GDK_TEXTINPUT_SCOPE";
    public const string SDL_HINT_GDK_TEXTINPUT_TITLE = "SDL_GDK_TEXTINPUT_TITLE";
    public const string SDL_HINT_HIDAPI_LIBUSB = "SDL_HIDAPI_LIBUSB";
    public const string SDL_HINT_HIDAPI_LIBUSB_WHITELIST = "SDL_HIDAPI_LIBUSB_WHITELIST";
    public const string SDL_HINT_HIDAPI_UDEV = "SDL_HIDAPI_UDEV";
    public const string SDL_HINT_GPU_DRIVER = "SDL_GPU_DRIVER";
    public const string SDL_HINT_HIDAPI_ENUMERATE_ONLY_CONTROLLERS = "SDL_HIDAPI_ENUMERATE_ONLY_CONTROLLERS";
    public const string SDL_HINT_HIDAPI_IGNORE_DEVICES = "SDL_HIDAPI_IGNORE_DEVICES";
    public const string SDL_HINT_IME_IMPLEMENTED_UI = "SDL_IME_IMPLEMENTED_UI";
    public const string SDL_HINT_IOS_HIDE_HOME_INDICATOR = "SDL_IOS_HIDE_HOME_INDICATOR";
    public const string SDL_HINT_JOYSTICK_ALLOW_BACKGROUND_EVENTS = "SDL_JOYSTICK_ALLOW_BACKGROUND_EVENTS";
    public const string SDL_HINT_JOYSTICK_ARCADESTICK_DEVICES = "SDL_JOYSTICK_ARCADESTICK_DEVICES";
    public const string SDL_HINT_JOYSTICK_ARCADESTICK_DEVICES_EXCLUDED = "SDL_JOYSTICK_ARCADESTICK_DEVICES_EXCLUDED";
    public const string SDL_HINT_JOYSTICK_BLACKLIST_DEVICES = "SDL_JOYSTICK_BLACKLIST_DEVICES";
    public const string SDL_HINT_JOYSTICK_BLACKLIST_DEVICES_EXCLUDED = "SDL_JOYSTICK_BLACKLIST_DEVICES_EXCLUDED";
    public const string SDL_HINT_JOYSTICK_DEVICE = "SDL_JOYSTICK_DEVICE";
    public const string SDL_HINT_JOYSTICK_ENHANCED_REPORTS = "SDL_JOYSTICK_ENHANCED_REPORTS";
    public const string SDL_HINT_JOYSTICK_FLIGHTSTICK_DEVICES = "SDL_JOYSTICK_FLIGHTSTICK_DEVICES";
    public const string SDL_HINT_JOYSTICK_FLIGHTSTICK_DEVICES_EXCLUDED = "SDL_JOYSTICK_FLIGHTSTICK_DEVICES_EXCLUDED";
    public const string SDL_HINT_JOYSTICK_GAMEINPUT = "SDL_JOYSTICK_GAMEINPUT";
    public const string SDL_HINT_JOYSTICK_GAMECUBE_DEVICES = "SDL_JOYSTICK_GAMECUBE_DEVICES";
    public const string SDL_HINT_JOYSTICK_GAMECUBE_DEVICES_EXCLUDED = "SDL_JOYSTICK_GAMECUBE_DEVICES_EXCLUDED";
    public const string SDL_HINT_JOYSTICK_HIDAPI = "SDL_JOYSTICK_HIDAPI";
    public const string SDL_HINT_JOYSTICK_HIDAPI_COMBINE_JOY_CONS = "SDL_JOYSTICK_HIDAPI_COMBINE_JOY_CONS";
    public const string SDL_HINT_JOYSTICK_HIDAPI_GAMECUBE = "SDL_JOYSTICK_HIDAPI_GAMECUBE";
    public const string SDL_HINT_JOYSTICK_HIDAPI_GAMECUBE_RUMBLE_BRAKE = "SDL_JOYSTICK_HIDAPI_GAMECUBE_RUMBLE_BRAKE";
    public const string SDL_HINT_JOYSTICK_HIDAPI_JOY_CONS = "SDL_JOYSTICK_HIDAPI_JOY_CONS";
    public const string SDL_HINT_JOYSTICK_HIDAPI_JOYCON_HOME_LED = "SDL_JOYSTICK_HIDAPI_JOYCON_HOME_LED";
    public const string SDL_HINT_JOYSTICK_HIDAPI_LUNA = "SDL_JOYSTICK_HIDAPI_LUNA";
    public const string SDL_HINT_JOYSTICK_HIDAPI_NINTENDO_CLASSIC = "SDL_JOYSTICK_HIDAPI_NINTENDO_CLASSIC";
    public const string SDL_HINT_JOYSTICK_HIDAPI_PS3 = "SDL_JOYSTICK_HIDAPI_PS3";
    public const string SDL_HINT_JOYSTICK_HIDAPI_PS3_SIXAXIS_DRIVER = "SDL_JOYSTICK_HIDAPI_PS3_SIXAXIS_DRIVER";
    public const string SDL_HINT_JOYSTICK_HIDAPI_PS4 = "SDL_JOYSTICK_HIDAPI_PS4";
    public const string SDL_HINT_JOYSTICK_HIDAPI_PS4_REPORT_INTERVAL = "SDL_JOYSTICK_HIDAPI_PS4_REPORT_INTERVAL";
    public const string SDL_HINT_JOYSTICK_HIDAPI_PS5 = "SDL_JOYSTICK_HIDAPI_PS5";
    public const string SDL_HINT_JOYSTICK_HIDAPI_PS5_PLAYER_LED = "SDL_JOYSTICK_HIDAPI_PS5_PLAYER_LED";
    public const string SDL_HINT_JOYSTICK_HIDAPI_SHIELD = "SDL_JOYSTICK_HIDAPI_SHIELD";
    public const string SDL_HINT_JOYSTICK_HIDAPI_STADIA = "SDL_JOYSTICK_HIDAPI_STADIA";
    public const string SDL_HINT_JOYSTICK_HIDAPI_STEAM = "SDL_JOYSTICK_HIDAPI_STEAM";
    public const string SDL_HINT_JOYSTICK_HIDAPI_STEAM_HOME_LED = "SDL_JOYSTICK_HIDAPI_STEAM_HOME_LED";
    public const string SDL_HINT_JOYSTICK_HIDAPI_STEAMDECK = "SDL_JOYSTICK_HIDAPI_STEAMDECK";
    public const string SDL_HINT_JOYSTICK_HIDAPI_STEAM_HORI = "SDL_JOYSTICK_HIDAPI_STEAM_HORI";
    public const string SDL_HINT_JOYSTICK_HIDAPI_SWITCH = "SDL_JOYSTICK_HIDAPI_SWITCH";
    public const string SDL_HINT_JOYSTICK_HIDAPI_SWITCH_HOME_LED = "SDL_JOYSTICK_HIDAPI_SWITCH_HOME_LED";
    public const string SDL_HINT_JOYSTICK_HIDAPI_SWITCH_PLAYER_LED = "SDL_JOYSTICK_HIDAPI_SWITCH_PLAYER_LED";
    public const string SDL_HINT_JOYSTICK_HIDAPI_VERTICAL_JOY_CONS = "SDL_JOYSTICK_HIDAPI_VERTICAL_JOY_CONS";
    public const string SDL_HINT_JOYSTICK_HIDAPI_WII = "SDL_JOYSTICK_HIDAPI_WII";
    public const string SDL_HINT_JOYSTICK_HIDAPI_WII_PLAYER_LED = "SDL_JOYSTICK_HIDAPI_WII_PLAYER_LED";
    public const string SDL_HINT_JOYSTICK_HIDAPI_XBOX = "SDL_JOYSTICK_HIDAPI_XBOX";
    public const string SDL_HINT_JOYSTICK_HIDAPI_XBOX_360 = "SDL_JOYSTICK_HIDAPI_XBOX_360";
    public const string SDL_HINT_JOYSTICK_HIDAPI_XBOX_360_PLAYER_LED = "SDL_JOYSTICK_HIDAPI_XBOX_360_PLAYER_LED";
    public const string SDL_HINT_JOYSTICK_HIDAPI_XBOX_360_WIRELESS = "SDL_JOYSTICK_HIDAPI_XBOX_360_WIRELESS";
    public const string SDL_HINT_JOYSTICK_HIDAPI_XBOX_ONE = "SDL_JOYSTICK_HIDAPI_XBOX_ONE";
    public const string SDL_HINT_JOYSTICK_HIDAPI_XBOX_ONE_HOME_LED = "SDL_JOYSTICK_HIDAPI_XBOX_ONE_HOME_LED";
    public const string SDL_HINT_JOYSTICK_IOKIT = "SDL_JOYSTICK_IOKIT";
    public const string SDL_HINT_JOYSTICK_LINUX_CLASSIC = "SDL_JOYSTICK_LINUX_CLASSIC";
    public const string SDL_HINT_JOYSTICK_LINUX_DEADZONES = "SDL_JOYSTICK_LINUX_DEADZONES";
    public const string SDL_HINT_JOYSTICK_LINUX_DIGITAL_HATS = "SDL_JOYSTICK_LINUX_DIGITAL_HATS";
    public const string SDL_HINT_JOYSTICK_LINUX_HAT_DEADZONES = "SDL_JOYSTICK_LINUX_HAT_DEADZONES";
    public const string SDL_HINT_JOYSTICK_MFI = "SDL_JOYSTICK_MFI";
    public const string SDL_HINT_JOYSTICK_RAWINPUT = "SDL_JOYSTICK_RAWINPUT";
    public const string SDL_HINT_JOYSTICK_RAWINPUT_CORRELATE_XINPUT = "SDL_JOYSTICK_RAWINPUT_CORRELATE_XINPUT";
    public const string SDL_HINT_JOYSTICK_ROG_CHAKRAM = "SDL_JOYSTICK_ROG_CHAKRAM";
    public const string SDL_HINT_JOYSTICK_THREAD = "SDL_JOYSTICK_THREAD";
    public const string SDL_HINT_JOYSTICK_THROTTLE_DEVICES = "SDL_JOYSTICK_THROTTLE_DEVICES";
    public const string SDL_HINT_JOYSTICK_THROTTLE_DEVICES_EXCLUDED = "SDL_JOYSTICK_THROTTLE_DEVICES_EXCLUDED";
    public const string SDL_HINT_JOYSTICK_WGI = "SDL_JOYSTICK_WGI";
    public const string SDL_HINT_JOYSTICK_WHEEL_DEVICES = "SDL_JOYSTICK_WHEEL_DEVICES";
    public const string SDL_HINT_JOYSTICK_WHEEL_DEVICES_EXCLUDED = "SDL_JOYSTICK_WHEEL_DEVICES_EXCLUDED";
    public const string SDL_HINT_JOYSTICK_ZERO_CENTERED_DEVICES = "SDL_JOYSTICK_ZERO_CENTERED_DEVICES";
    public const string SDL_HINT_JOYSTICK_HAPTIC_AXES = "SDL_JOYSTICK_HAPTIC_AXES";
    public const string SDL_HINT_KEYCODE_OPTIONS = "SDL_KEYCODE_OPTIONS";
    public const string SDL_HINT_KMSDRM_DEVICE_INDEX = "SDL_KMSDRM_DEVICE_INDEX";
    public const string SDL_HINT_KMSDRM_REQUIRE_DRM_MASTER = "SDL_KMSDRM_REQUIRE_DRM_MASTER";
    public const string SDL_HINT_LOGGING = "SDL_LOGGING";
    public const string SDL_HINT_MAC_BACKGROUND_APP = "SDL_MAC_BACKGROUND_APP";
    public const string SDL_HINT_MAC_CTRL_CLICK_EMULATE_RIGHT_CLICK = "SDL_MAC_CTRL_CLICK_EMULATE_RIGHT_CLICK";
    public const string SDL_HINT_MAC_OPENGL_ASYNC_DISPATCH = "SDL_MAC_OPENGL_ASYNC_DISPATCH";
    public const string SDL_HINT_MAC_OPTION_AS_ALT = "SDL_MAC_OPTION_AS_ALT";
    public const string SDL_HINT_MAC_SCROLL_MOMENTUM = "SDL_MAC_SCROLL_MOMENTUM";
    public const string SDL_HINT_MAIN_CALLBACK_RATE = "SDL_MAIN_CALLBACK_RATE";
    public const string SDL_HINT_MOUSE_AUTO_CAPTURE = "SDL_MOUSE_AUTO_CAPTURE";
    public const string SDL_HINT_MOUSE_DOUBLE_CLICK_RADIUS = "SDL_MOUSE_DOUBLE_CLICK_RADIUS";
    public const string SDL_HINT_MOUSE_DOUBLE_CLICK_TIME = "SDL_MOUSE_DOUBLE_CLICK_TIME";
    public const string SDL_HINT_MOUSE_DEFAULT_SYSTEM_CURSOR = "SDL_MOUSE_DEFAULT_SYSTEM_CURSOR";
    public const string SDL_HINT_MOUSE_EMULATE_WARP_WITH_RELATIVE = "SDL_MOUSE_EMULATE_WARP_WITH_RELATIVE";
    public const string SDL_HINT_MOUSE_FOCUS_CLICKTHROUGH = "SDL_MOUSE_FOCUS_CLICKTHROUGH";
    public const string SDL_HINT_MOUSE_NORMAL_SPEED_SCALE = "SDL_MOUSE_NORMAL_SPEED_SCALE";
    public const string SDL_HINT_MOUSE_RELATIVE_MODE_CENTER = "SDL_MOUSE_RELATIVE_MODE_CENTER";
    public const string SDL_HINT_MOUSE_RELATIVE_SPEED_SCALE = "SDL_MOUSE_RELATIVE_SPEED_SCALE";
    public const string SDL_HINT_MOUSE_RELATIVE_SYSTEM_SCALE = "SDL_MOUSE_RELATIVE_SYSTEM_SCALE";
    public const string SDL_HINT_MOUSE_RELATIVE_WARP_MOTION = "SDL_MOUSE_RELATIVE_WARP_MOTION";
    public const string SDL_HINT_MOUSE_RELATIVE_CURSOR_VISIBLE = "SDL_MOUSE_RELATIVE_CURSOR_VISIBLE";
    public const string SDL_HINT_MOUSE_TOUCH_EVENTS = "SDL_MOUSE_TOUCH_EVENTS";
    public const string SDL_HINT_MUTE_CONSOLE_KEYBOARD = "SDL_MUTE_CONSOLE_KEYBOARD";
    public const string SDL_HINT_NO_SIGNAL_HANDLERS = "SDL_NO_SIGNAL_HANDLERS";
    public const string SDL_HINT_OPENGL_LIBRARY = "SDL_OPENGL_LIBRARY";
    public const string SDL_HINT_EGL_LIBRARY = "SDL_EGL_LIBRARY";
    public const string SDL_HINT_OPENGL_ES_DRIVER = "SDL_OPENGL_ES_DRIVER";
    public const string SDL_HINT_OPENVR_LIBRARY = "SDL_OPENVR_LIBRARY";
    public const string SDL_HINT_ORIENTATIONS = "SDL_ORIENTATIONS";
    public const string SDL_HINT_POLL_SENTINEL = "SDL_POLL_SENTINEL";
    public const string SDL_HINT_PREFERRED_LOCALES = "SDL_PREFERRED_LOCALES";
    public const string SDL_HINT_QUIT_ON_LAST_WINDOW_CLOSE = "SDL_QUIT_ON_LAST_WINDOW_CLOSE";
    public const string SDL_HINT_RENDER_DIRECT3D_THREADSAFE = "SDL_RENDER_DIRECT3D_THREADSAFE";
    public const string SDL_HINT_RENDER_DIRECT3D11_DEBUG = "SDL_RENDER_DIRECT3D11_DEBUG";
    public const string SDL_HINT_RENDER_VULKAN_DEBUG = "SDL_RENDER_VULKAN_DEBUG";
    public const string SDL_HINT_RENDER_GPU_DEBUG = "SDL_RENDER_GPU_DEBUG";
    public const string SDL_HINT_RENDER_GPU_LOW_POWER = "SDL_RENDER_GPU_LOW_POWER";
    public const string SDL_HINT_RENDER_DRIVER = "SDL_RENDER_DRIVER";
    public const string SDL_HINT_RENDER_LINE_METHOD = "SDL_RENDER_LINE_METHOD";
    public const string SDL_HINT_RENDER_METAL_PREFER_LOW_POWER_DEVICE = "SDL_RENDER_METAL_PREFER_LOW_POWER_DEVICE";
    public const string SDL_HINT_RENDER_VSYNC = "SDL_RENDER_VSYNC";
    public const string SDL_HINT_RETURN_KEY_HIDES_IME = "SDL_RETURN_KEY_HIDES_IME";
    public const string SDL_HINT_ROG_GAMEPAD_MICE = "SDL_ROG_GAMEPAD_MICE";
    public const string SDL_HINT_ROG_GAMEPAD_MICE_EXCLUDED = "SDL_ROG_GAMEPAD_MICE_EXCLUDED";
    public const string SDL_HINT_RPI_VIDEO_LAYER = "SDL_RPI_VIDEO_LAYER";
    public const string SDL_HINT_SCREENSAVER_INHIBIT_ACTIVITY_NAME = "SDL_SCREENSAVER_INHIBIT_ACTIVITY_NAME";
    public const string SDL_HINT_SHUTDOWN_DBUS_ON_QUIT = "SDL_SHUTDOWN_DBUS_ON_QUIT";
    public const string SDL_HINT_STORAGE_TITLE_DRIVER = "SDL_STORAGE_TITLE_DRIVER";
    public const string SDL_HINT_STORAGE_USER_DRIVER = "SDL_STORAGE_USER_DRIVER";
    public const string SDL_HINT_THREAD_FORCE_REALTIME_TIME_CRITICAL = "SDL_THREAD_FORCE_REALTIME_TIME_CRITICAL";
    public const string SDL_HINT_THREAD_PRIORITY_POLICY = "SDL_THREAD_PRIORITY_POLICY";
    public const string SDL_HINT_TIMER_RESOLUTION = "SDL_TIMER_RESOLUTION";
    public const string SDL_HINT_TOUCH_MOUSE_EVENTS = "SDL_TOUCH_MOUSE_EVENTS";
    public const string SDL_HINT_TRACKPAD_IS_TOUCH_ONLY = "SDL_TRACKPAD_IS_TOUCH_ONLY";
    public const string SDL_HINT_TV_REMOTE_AS_JOYSTICK = "SDL_TV_REMOTE_AS_JOYSTICK";
    public const string SDL_HINT_VIDEO_ALLOW_SCREENSAVER = "SDL_VIDEO_ALLOW_SCREENSAVER";
    public const string SDL_HINT_VIDEO_DISPLAY_PRIORITY = "SDL_VIDEO_DISPLAY_PRIORITY";
    public const string SDL_HINT_VIDEO_DOUBLE_BUFFER = "SDL_VIDEO_DOUBLE_BUFFER";
    public const string SDL_HINT_VIDEO_DRIVER = "SDL_VIDEO_DRIVER";
    public const string SDL_HINT_VIDEO_DUMMY_SAVE_FRAMES = "SDL_VIDEO_DUMMY_SAVE_FRAMES";
    public const string SDL_HINT_VIDEO_EGL_ALLOW_GETDISPLAY_FALLBACK = "SDL_VIDEO_EGL_ALLOW_GETDISPLAY_FALLBACK";
    public const string SDL_HINT_VIDEO_FORCE_EGL = "SDL_VIDEO_FORCE_EGL";
    public const string SDL_HINT_VIDEO_MAC_FULLSCREEN_SPACES = "SDL_VIDEO_MAC_FULLSCREEN_SPACES";
    public const string SDL_HINT_VIDEO_MAC_FULLSCREEN_MENU_VISIBILITY = "SDL_VIDEO_MAC_FULLSCREEN_MENU_VISIBILITY";
    public const string SDL_HINT_VIDEO_MINIMIZE_ON_FOCUS_LOSS = "SDL_VIDEO_MINIMIZE_ON_FOCUS_LOSS";
    public const string SDL_HINT_VIDEO_OFFSCREEN_SAVE_FRAMES = "SDL_VIDEO_OFFSCREEN_SAVE_FRAMES";
    public const string SDL_HINT_VIDEO_SYNC_WINDOW_OPERATIONS = "SDL_VIDEO_SYNC_WINDOW_OPERATIONS";
    public const string SDL_HINT_VIDEO_WAYLAND_ALLOW_LIBDECOR = "SDL_VIDEO_WAYLAND_ALLOW_LIBDECOR";
    public const string SDL_HINT_VIDEO_WAYLAND_MODE_EMULATION = "SDL_VIDEO_WAYLAND_MODE_EMULATION";
    public const string SDL_HINT_VIDEO_WAYLAND_MODE_SCALING = "SDL_VIDEO_WAYLAND_MODE_SCALING";
    public const string SDL_HINT_VIDEO_WAYLAND_PREFER_LIBDECOR = "SDL_VIDEO_WAYLAND_PREFER_LIBDECOR";
    public const string SDL_HINT_VIDEO_WAYLAND_SCALE_TO_DISPLAY = "SDL_VIDEO_WAYLAND_SCALE_TO_DISPLAY";
    public const string SDL_HINT_VIDEO_WIN_D3DCOMPILER = "SDL_VIDEO_WIN_D3DCOMPILER";
    public const string SDL_HINT_VIDEO_X11_NET_WM_BYPASS_COMPOSITOR = "SDL_VIDEO_X11_NET_WM_BYPASS_COMPOSITOR";
    public const string SDL_HINT_VIDEO_X11_NET_WM_PING = "SDL_VIDEO_X11_NET_WM_PING";
    public const string SDL_HINT_VIDEO_X11_NODIRECTCOLOR = "SDL_VIDEO_X11_NODIRECTCOLOR";
    public const string SDL_HINT_VIDEO_X11_SCALING_FACTOR = "SDL_VIDEO_X11_SCALING_FACTOR";
    public const string SDL_HINT_VIDEO_X11_VISUALID = "SDL_VIDEO_X11_VISUALID";
    public const string SDL_HINT_VIDEO_X11_WINDOW_VISUALID = "SDL_VIDEO_X11_WINDOW_VISUALID";
    public const string SDL_HINT_VIDEO_X11_XRANDR = "SDL_VIDEO_X11_XRANDR";
    public const string SDL_HINT_VITA_ENABLE_BACK_TOUCH = "SDL_VITA_ENABLE_BACK_TOUCH";
    public const string SDL_HINT_VITA_ENABLE_FRONT_TOUCH = "SDL_VITA_ENABLE_FRONT_TOUCH";
    public const string SDL_HINT_VITA_MODULE_PATH = "SDL_VITA_MODULE_PATH";
    public const string SDL_HINT_VITA_PVR_INIT = "SDL_VITA_PVR_INIT";
    public const string SDL_HINT_VITA_RESOLUTION = "SDL_VITA_RESOLUTION";
    public const string SDL_HINT_VITA_PVR_OPENGL = "SDL_VITA_PVR_OPENGL";
    public const string SDL_HINT_VITA_TOUCH_MOUSE_DEVICE = "SDL_VITA_TOUCH_MOUSE_DEVICE";
    public const string SDL_HINT_VULKAN_DISPLAY = "SDL_VULKAN_DISPLAY";
    public const string SDL_HINT_VULKAN_LIBRARY = "SDL_VULKAN_LIBRARY";
    public const string SDL_HINT_WAVE_FACT_CHUNK = "SDL_WAVE_FACT_CHUNK";
    public const string SDL_HINT_WAVE_CHUNK_LIMIT = "SDL_WAVE_CHUNK_LIMIT";
    public const string SDL_HINT_WAVE_RIFF_CHUNK_SIZE = "SDL_WAVE_RIFF_CHUNK_SIZE";
    public const string SDL_HINT_WAVE_TRUNCATION = "SDL_WAVE_TRUNCATION";
    public const string SDL_HINT_WINDOW_ACTIVATE_WHEN_RAISED = "SDL_WINDOW_ACTIVATE_WHEN_RAISED";
    public const string SDL_HINT_WINDOW_ACTIVATE_WHEN_SHOWN = "SDL_WINDOW_ACTIVATE_WHEN_SHOWN";
    public const string SDL_HINT_WINDOW_ALLOW_TOPMOST = "SDL_WINDOW_ALLOW_TOPMOST";
    public const string SDL_HINT_WINDOW_FRAME_USABLE_WHILE_CURSOR_HIDDEN = "SDL_WINDOW_FRAME_USABLE_WHILE_CURSOR_HIDDEN";
    public const string SDL_HINT_WINDOWS_CLOSE_ON_ALT_F4 = "SDL_WINDOWS_CLOSE_ON_ALT_F4";
    public const string SDL_HINT_WINDOWS_ENABLE_MENU_MNEMONICS = "SDL_WINDOWS_ENABLE_MENU_MNEMONICS";
    public const string SDL_HINT_WINDOWS_ENABLE_MESSAGELOOP = "SDL_WINDOWS_ENABLE_MESSAGELOOP";
    public const string SDL_HINT_WINDOWS_GAMEINPUT = "SDL_WINDOWS_GAMEINPUT";
    public const string SDL_HINT_WINDOWS_RAW_KEYBOARD = "SDL_WINDOWS_RAW_KEYBOARD";
    public const string SDL_HINT_WINDOWS_FORCE_SEMAPHORE_KERNEL = "SDL_WINDOWS_FORCE_SEMAPHORE_KERNEL";
    public const string SDL_HINT_WINDOWS_INTRESOURCE_ICON = "SDL_WINDOWS_INTRESOURCE_ICON";
    public const string SDL_HINT_WINDOWS_INTRESOURCE_ICON_SMALL = "SDL_WINDOWS_INTRESOURCE_ICON_SMALL";
    public const string SDL_HINT_WINDOWS_USE_D3D9EX = "SDL_WINDOWS_USE_D3D9EX";
    public const string SDL_HINT_WINDOWS_ERASE_BACKGROUND_MODE = "SDL_WINDOWS_ERASE_BACKGROUND_MODE";
    public const string SDL_HINT_X11_FORCE_OVERRIDE_REDIRECT = "SDL_X11_FORCE_OVERRIDE_REDIRECT";
    public const string SDL_HINT_X11_WINDOW_TYPE = "SDL_X11_WINDOW_TYPE";
    public const string SDL_HINT_X11_XCB_LIBRARY = "SDL_X11_XCB_LIBRARY";
    public const string SDL_HINT_XINPUT_ENABLED = "SDL_XINPUT_ENABLED";
    public const string SDL_HINT_ASSERT = "SDL_ASSERT";
    public const string SDL_HINT_PEN_MOUSE_EVENTS = "SDL_PEN_MOUSE_EVENTS";
    public const string SDL_HINT_PEN_TOUCH_EVENTS = "SDL_PEN_TOUCH_EVENTS";

    public enum SDL_HintPriority
    {
        SDL_HINT_DEFAULT = 0,
        SDL_HINT_NORMAL = 1,
        SDL_HINT_OVERRIDE = 2,
    }

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetHintWithPriority(string name, string value, SDL_HintPriority priority);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetHint(string name, string value);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_ResetHint(string name);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_ResetHints();

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetHint(string name);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetHintBoolean(string name, SDLBool default_value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SDL_HintCallback(IntPtr userdata, byte* name, byte* oldValue, byte* newValue);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_AddHintCallback(string name, SDL_HintCallback callback, IntPtr userdata);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_RemoveHintCallback(string name, SDL_HintCallback callback, IntPtr userdata);

    // /usr/local/include/SDL3/SDL_init.h

    public const string SDL_PROP_APP_METADATA_NAME_STRING = "SDL.app.metadata.name";
    public const string SDL_PROP_APP_METADATA_VERSION_STRING = "SDL.app.metadata.version";
    public const string SDL_PROP_APP_METADATA_IDENTIFIER_STRING = "SDL.app.metadata.identifier";
    public const string SDL_PROP_APP_METADATA_CREATOR_STRING = "SDL.app.metadata.creator";
    public const string SDL_PROP_APP_METADATA_COPYRIGHT_STRING = "SDL.app.metadata.copyright";
    public const string SDL_PROP_APP_METADATA_URL_STRING = "SDL.app.metadata.url";
    public const string SDL_PROP_APP_METADATA_TYPE_STRING = "SDL.app.metadata.type";

    [Flags]
    public enum SDL_InitFlags : uint
    {
        SDL_INIT_TIMER = 0x1,
        SDL_INIT_AUDIO = 0x10,
        SDL_INIT_VIDEO = 0x20,
        SDL_INIT_JOYSTICK = 0x200,
        SDL_INIT_HAPTIC = 0x1000,
        SDL_INIT_GAMEPAD = 0x2000,
        SDL_INIT_EVENTS = 0x4000,
        SDL_INIT_SENSOR = 0x08000,
        SDL_INIT_CAMERA = 0x10000,
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
    internal static partial SDLBool SDL_InitSubSystem(SDL_InitFlags flags);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_QuitSubSystem(SDL_InitFlags flags);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_InitFlags SDL_WasInit(SDL_InitFlags flags);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_Quit();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_IsMainThread();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SDL_MainThreadCallback(IntPtr userdata);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RunOnMainThread(SDL_MainThreadCallback callback, IntPtr userdata, SDLBool wait_complete);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetAppMetadata(string appname, string appversion, string appidentifier);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetAppMetadataProperty(string name, string value);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetAppMetadataProperty(string name);

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

    public enum SDL_LogCategory
    {
        SDL_LOG_CATEGORY_APPLICATION = 0,
        SDL_LOG_CATEGORY_ERROR = 1,
        SDL_LOG_CATEGORY_ASSERT = 2,
        SDL_LOG_CATEGORY_SYSTEM = 3,
        SDL_LOG_CATEGORY_AUDIO = 4,
        SDL_LOG_CATEGORY_VIDEO = 5,
        SDL_LOG_CATEGORY_RENDER = 6,
        SDL_LOG_CATEGORY_INPUT = 7,
        SDL_LOG_CATEGORY_TEST = 8,
        SDL_LOG_CATEGORY_GPU = 9,
        SDL_LOG_CATEGORY_RESERVED2 = 10,
        SDL_LOG_CATEGORY_RESERVED3 = 11,
        SDL_LOG_CATEGORY_RESERVED4 = 12,
        SDL_LOG_CATEGORY_RESERVED5 = 13,
        SDL_LOG_CATEGORY_RESERVED6 = 14,
        SDL_LOG_CATEGORY_RESERVED7 = 15,
        SDL_LOG_CATEGORY_RESERVED8 = 16,
        SDL_LOG_CATEGORY_RESERVED9 = 17,
        SDL_LOG_CATEGORY_RESERVED10 = 18,
        SDL_LOG_CATEGORY_CUSTOM = 19,
    }

    public enum SDL_LogPriority
    {
        SDL_LOG_PRIORITY_INVALID = 0,
        SDL_LOG_PRIORITY_TRACE = 1,
        SDL_LOG_PRIORITY_VERBOSE = 2,
        SDL_LOG_PRIORITY_DEBUG = 3,
        SDL_LOG_PRIORITY_INFO = 4,
        SDL_LOG_PRIORITY_WARN = 5,
        SDL_LOG_PRIORITY_ERROR = 6,
        SDL_LOG_PRIORITY_CRITICAL = 7,
        SDL_LOG_PRIORITY_COUNT = 8,
    }

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_SetLogPriorities(SDL_LogPriority priority);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_SetLogPriority(int category, SDL_LogPriority priority);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_LogPriority SDL_GetLogPriority(int category);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_ResetLogPriorities();

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetLogPriorityPrefix(SDL_LogPriority priority, string prefix);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_Log(string fmt);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_LogTrace(int category, string fmt);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_LogVerbose(int category, string fmt);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_LogDebug(int category, string fmt);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_LogInfo(int category, string fmt);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_LogWarn(int category, string fmt);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_LogError(int category, string fmt);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_LogCritical(int category, string fmt);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_LogMessage(int category, SDL_LogPriority priority, string fmt);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SDL_LogOutputFunction(IntPtr userdata, int category, SDL_LogPriority priority, byte* message);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_LogOutputFunction SDL_GetDefaultLogOutputFunction();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_GetLogOutputFunction(out SDL_LogOutputFunction callback, out IntPtr userdata);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_SetLogOutputFunction(SDL_LogOutputFunction callback, IntPtr userdata);

    // /usr/local/include/SDL3/SDL_render.h

    public const string SDL_PROP_RENDERER_CREATE_NAME_STRING = "SDL.renderer.create.name";
    public const string SDL_PROP_RENDERER_CREATE_WINDOW_POINTER = "SDL.renderer.create.window";
    public const string SDL_PROP_RENDERER_CREATE_SURFACE_POINTER = "SDL.renderer.create.surface";
    public const string SDL_PROP_RENDERER_CREATE_OUTPUT_COLORSPACE_NUMBER = "SDL.renderer.create.output_colorspace";
    public const string SDL_PROP_RENDERER_CREATE_PRESENT_VSYNC_NUMBER = "SDL.renderer.create.present_vsync";
    public const string SDL_PROP_RENDERER_CREATE_VULKAN_INSTANCE_POINTER = "SDL.renderer.create.vulkan.instance";
    public const string SDL_PROP_RENDERER_CREATE_VULKAN_SURFACE_NUMBER = "SDL.renderer.create.vulkan.surface";
    public const string SDL_PROP_RENDERER_CREATE_VULKAN_PHYSICAL_DEVICE_POINTER = "SDL.renderer.create.vulkan.physical_device";
    public const string SDL_PROP_RENDERER_CREATE_VULKAN_DEVICE_POINTER = "SDL.renderer.create.vulkan.device";
    public const string SDL_PROP_RENDERER_CREATE_VULKAN_GRAPHICS_QUEUE_FAMILY_INDEX_NUMBER = "SDL.renderer.create.vulkan.graphics_queue_family_index";
    public const string SDL_PROP_RENDERER_CREATE_VULKAN_PRESENT_QUEUE_FAMILY_INDEX_NUMBER = "SDL.renderer.create.vulkan.present_queue_family_index";
    public const string SDL_PROP_RENDERER_NAME_STRING = "SDL.renderer.name";
    public const string SDL_PROP_RENDERER_WINDOW_POINTER = "SDL.renderer.window";
    public const string SDL_PROP_RENDERER_SURFACE_POINTER = "SDL.renderer.surface";
    public const string SDL_PROP_RENDERER_VSYNC_NUMBER = "SDL.renderer.vsync";
    public const string SDL_PROP_RENDERER_MAX_TEXTURE_SIZE_NUMBER = "SDL.renderer.max_texture_size";
    public const string SDL_PROP_RENDERER_TEXTURE_FORMATS_POINTER = "SDL.renderer.texture_formats";
    public const string SDL_PROP_RENDERER_OUTPUT_COLORSPACE_NUMBER = "SDL.renderer.output_colorspace";
    public const string SDL_PROP_RENDERER_HDR_ENABLED_BOOLEAN = "SDL.renderer.HDR_enabled";
    public const string SDL_PROP_RENDERER_SDR_WHITE_POINT_FLOAT = "SDL.renderer.SDR_white_point";
    public const string SDL_PROP_RENDERER_HDR_HEADROOM_FLOAT = "SDL.renderer.HDR_headroom";
    public const string SDL_PROP_RENDERER_D3D9_DEVICE_POINTER = "SDL.renderer.d3d9.device";
    public const string SDL_PROP_RENDERER_D3D11_DEVICE_POINTER = "SDL.renderer.d3d11.device";
    public const string SDL_PROP_RENDERER_D3D11_SWAPCHAIN_POINTER = "SDL.renderer.d3d11.swap_chain";
    public const string SDL_PROP_RENDERER_D3D12_DEVICE_POINTER = "SDL.renderer.d3d12.device";
    public const string SDL_PROP_RENDERER_D3D12_SWAPCHAIN_POINTER = "SDL.renderer.d3d12.swap_chain";
    public const string SDL_PROP_RENDERER_D3D12_COMMAND_QUEUE_POINTER = "SDL.renderer.d3d12.command_queue";
    public const string SDL_PROP_RENDERER_VULKAN_INSTANCE_POINTER = "SDL.renderer.vulkan.instance";
    public const string SDL_PROP_RENDERER_VULKAN_SURFACE_NUMBER = "SDL.renderer.vulkan.surface";
    public const string SDL_PROP_RENDERER_VULKAN_PHYSICAL_DEVICE_POINTER = "SDL.renderer.vulkan.physical_device";
    public const string SDL_PROP_RENDERER_VULKAN_DEVICE_POINTER = "SDL.renderer.vulkan.device";
    public const string SDL_PROP_RENDERER_VULKAN_GRAPHICS_QUEUE_FAMILY_INDEX_NUMBER = "SDL.renderer.vulkan.graphics_queue_family_index";
    public const string SDL_PROP_RENDERER_VULKAN_PRESENT_QUEUE_FAMILY_INDEX_NUMBER = "SDL.renderer.vulkan.present_queue_family_index";
    public const string SDL_PROP_RENDERER_VULKAN_SWAPCHAIN_IMAGE_COUNT_NUMBER = "SDL.renderer.vulkan.swapchain_image_count";
    public const string SDL_PROP_RENDERER_GPU_DEVICE_POINTER = "SDL.renderer.gpu.device";
    public const string SDL_PROP_TEXTURE_CREATE_COLORSPACE_NUMBER = "SDL.texture.create.colorspace";
    public const string SDL_PROP_TEXTURE_CREATE_FORMAT_NUMBER = "SDL.texture.create.format";
    public const string SDL_PROP_TEXTURE_CREATE_ACCESS_NUMBER = "SDL.texture.create.access";
    public const string SDL_PROP_TEXTURE_CREATE_WIDTH_NUMBER = "SDL.texture.create.width";
    public const string SDL_PROP_TEXTURE_CREATE_HEIGHT_NUMBER = "SDL.texture.create.height";
    public const string SDL_PROP_TEXTURE_CREATE_SDR_WHITE_POINT_FLOAT = "SDL.texture.create.SDR_white_point";
    public const string SDL_PROP_TEXTURE_CREATE_HDR_HEADROOM_FLOAT = "SDL.texture.create.HDR_headroom";
    public const string SDL_PROP_TEXTURE_CREATE_D3D11_TEXTURE_POINTER = "SDL.texture.create.d3d11.texture";
    public const string SDL_PROP_TEXTURE_CREATE_D3D11_TEXTURE_U_POINTER = "SDL.texture.create.d3d11.texture_u";
    public const string SDL_PROP_TEXTURE_CREATE_D3D11_TEXTURE_V_POINTER = "SDL.texture.create.d3d11.texture_v";
    public const string SDL_PROP_TEXTURE_CREATE_D3D12_TEXTURE_POINTER = "SDL.texture.create.d3d12.texture";
    public const string SDL_PROP_TEXTURE_CREATE_D3D12_TEXTURE_U_POINTER = "SDL.texture.create.d3d12.texture_u";
    public const string SDL_PROP_TEXTURE_CREATE_D3D12_TEXTURE_V_POINTER = "SDL.texture.create.d3d12.texture_v";
    public const string SDL_PROP_TEXTURE_CREATE_METAL_PIXELBUFFER_POINTER = "SDL.texture.create.metal.pixelbuffer";
    public const string SDL_PROP_TEXTURE_CREATE_OPENGL_TEXTURE_NUMBER = "SDL.texture.create.opengl.texture";
    public const string SDL_PROP_TEXTURE_CREATE_OPENGL_TEXTURE_UV_NUMBER = "SDL.texture.create.opengl.texture_uv";
    public const string SDL_PROP_TEXTURE_CREATE_OPENGL_TEXTURE_U_NUMBER = "SDL.texture.create.opengl.texture_u";
    public const string SDL_PROP_TEXTURE_CREATE_OPENGL_TEXTURE_V_NUMBER = "SDL.texture.create.opengl.texture_v";
    public const string SDL_PROP_TEXTURE_CREATE_OPENGLES2_TEXTURE_NUMBER = "SDL.texture.create.opengles2.texture";
    public const string SDL_PROP_TEXTURE_CREATE_OPENGLES2_TEXTURE_UV_NUMBER = "SDL.texture.create.opengles2.texture_uv";
    public const string SDL_PROP_TEXTURE_CREATE_OPENGLES2_TEXTURE_U_NUMBER = "SDL.texture.create.opengles2.texture_u";
    public const string SDL_PROP_TEXTURE_CREATE_OPENGLES2_TEXTURE_V_NUMBER = "SDL.texture.create.opengles2.texture_v";
    public const string SDL_PROP_TEXTURE_CREATE_VULKAN_TEXTURE_NUMBER = "SDL.texture.create.vulkan.texture";
    public const string SDL_PROP_TEXTURE_COLORSPACE_NUMBER = "SDL.texture.colorspace";
    public const string SDL_PROP_TEXTURE_FORMAT_NUMBER = "SDL.texture.format";
    public const string SDL_PROP_TEXTURE_ACCESS_NUMBER = "SDL.texture.access";
    public const string SDL_PROP_TEXTURE_WIDTH_NUMBER = "SDL.texture.width";
    public const string SDL_PROP_TEXTURE_HEIGHT_NUMBER = "SDL.texture.height";
    public const string SDL_PROP_TEXTURE_SDR_WHITE_POINT_FLOAT = "SDL.texture.SDR_white_point";
    public const string SDL_PROP_TEXTURE_HDR_HEADROOM_FLOAT = "SDL.texture.HDR_headroom";
    public const string SDL_PROP_TEXTURE_D3D11_TEXTURE_POINTER = "SDL.texture.d3d11.texture";
    public const string SDL_PROP_TEXTURE_D3D11_TEXTURE_U_POINTER = "SDL.texture.d3d11.texture_u";
    public const string SDL_PROP_TEXTURE_D3D11_TEXTURE_V_POINTER = "SDL.texture.d3d11.texture_v";
    public const string SDL_PROP_TEXTURE_D3D12_TEXTURE_POINTER = "SDL.texture.d3d12.texture";
    public const string SDL_PROP_TEXTURE_D3D12_TEXTURE_U_POINTER = "SDL.texture.d3d12.texture_u";
    public const string SDL_PROP_TEXTURE_D3D12_TEXTURE_V_POINTER = "SDL.texture.d3d12.texture_v";
    public const string SDL_PROP_TEXTURE_OPENGL_TEXTURE_NUMBER = "SDL.texture.opengl.texture";
    public const string SDL_PROP_TEXTURE_OPENGL_TEXTURE_UV_NUMBER = "SDL.texture.opengl.texture_uv";
    public const string SDL_PROP_TEXTURE_OPENGL_TEXTURE_U_NUMBER = "SDL.texture.opengl.texture_u";
    public const string SDL_PROP_TEXTURE_OPENGL_TEXTURE_V_NUMBER = "SDL.texture.opengl.texture_v";
    public const string SDL_PROP_TEXTURE_OPENGL_TEXTURE_TARGET_NUMBER = "SDL.texture.opengl.target";
    public const string SDL_PROP_TEXTURE_OPENGL_TEX_W_FLOAT = "SDL.texture.opengl.tex_w";
    public const string SDL_PROP_TEXTURE_OPENGL_TEX_H_FLOAT = "SDL.texture.opengl.tex_h";
    public const string SDL_PROP_TEXTURE_OPENGLES2_TEXTURE_NUMBER = "SDL.texture.opengles2.texture";
    public const string SDL_PROP_TEXTURE_OPENGLES2_TEXTURE_UV_NUMBER = "SDL.texture.opengles2.texture_uv";
    public const string SDL_PROP_TEXTURE_OPENGLES2_TEXTURE_U_NUMBER = "SDL.texture.opengles2.texture_u";
    public const string SDL_PROP_TEXTURE_OPENGLES2_TEXTURE_V_NUMBER = "SDL.texture.opengles2.texture_v";
    public const string SDL_PROP_TEXTURE_OPENGLES2_TEXTURE_TARGET_NUMBER = "SDL.texture.opengles2.target";
    public const string SDL_PROP_TEXTURE_VULKAN_TEXTURE_NUMBER = "SDL.texture.vulkan.texture";

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_Vertex
    {
        public SDL_FPoint position;
        public SDL_FColor color;
        public SDL_FPoint tex_coord;
    }

    public enum SDL_TextureAccess
    {
        SDL_TEXTUREACCESS_STATIC = 0,
        SDL_TEXTUREACCESS_STREAMING = 1,
        SDL_TEXTUREACCESS_TARGET = 2,
    }

    public enum SDL_RendererLogicalPresentation
    {
        SDL_LOGICAL_PRESENTATION_DISABLED = 0,
        SDL_LOGICAL_PRESENTATION_STRETCH = 1,
        SDL_LOGICAL_PRESENTATION_LETTERBOX = 2,
        SDL_LOGICAL_PRESENTATION_OVERSCAN = 3,
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

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_GetNumRenderDrivers();

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetRenderDriver(int index);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_CreateWindowAndRenderer(string title, int width, int height, SDL_WindowFlags window_flags, out IntPtr window, out IntPtr renderer);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_CreateRenderer(IntPtr window, string name);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_CreateRendererWithProperties(uint props);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_CreateSoftwareRenderer(IntPtr surface);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetRenderer(IntPtr window);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetRenderWindow(IntPtr renderer);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetRendererName(IntPtr renderer);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_GetRendererProperties(IntPtr renderer);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetRenderOutputSize(IntPtr renderer, out int w, out int h);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetCurrentRenderOutputSize(IntPtr renderer, out int w, out int h);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Texture* SDL_CreateTexture(IntPtr renderer, SDL_PixelFormat format, SDL_TextureAccess access, int w, int h);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Texture* SDL_CreateTextureFromSurface(IntPtr renderer, IntPtr surface);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Texture* SDL_CreateTextureWithProperties(IntPtr renderer, uint props);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint SDL_GetTextureProperties(IntPtr texture); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetRendererFromTexture(IntPtr texture); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetTextureSize(IntPtr texture, out float w, out float h); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetTextureColorMod(IntPtr texture, byte r, byte g, byte b); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetTextureColorModFloat(IntPtr texture, float r, float g, float b); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetTextureColorMod(IntPtr texture, out byte r, out byte g, out byte b); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetTextureColorModFloat(IntPtr texture, out float r, out float g, out float b); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetTextureAlphaMod(IntPtr texture, byte alpha); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetTextureAlphaModFloat(IntPtr texture, float alpha); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetTextureAlphaMod(IntPtr texture, out byte alpha); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetTextureAlphaModFloat(IntPtr texture, out float alpha); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetTextureBlendMode(IntPtr texture, uint blendMode); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetTextureBlendMode(IntPtr texture, IntPtr blendMode); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetTextureScaleMode(IntPtr texture, SDL_ScaleMode scaleMode); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetTextureScaleMode(IntPtr texture, out SDL_ScaleMode scaleMode); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_UpdateTexture(IntPtr texture, ref SDL_Rect rect, IntPtr pixels, int pitch); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_UpdateYUVTexture(IntPtr texture, ref SDL_Rect rect, IntPtr Yplane, int Ypitch, IntPtr Uplane, int Upitch, IntPtr Vplane, int Vpitch); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_UpdateNVTexture(IntPtr texture, ref SDL_Rect rect, IntPtr Yplane, int Ypitch, IntPtr UVplane, int UVpitch); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_LockTexture(IntPtr texture, ref SDL_Rect rect, out IntPtr pixels, out int pitch); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_LockTextureToSurface(IntPtr texture, ref SDL_Rect rect, out IntPtr surface); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_UnlockTexture(IntPtr texture); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetRenderTarget(IntPtr renderer, IntPtr texture); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Texture* SDL_GetRenderTarget(IntPtr renderer);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetRenderLogicalPresentation(IntPtr renderer, int w, int h, SDL_RendererLogicalPresentation mode);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetRenderLogicalPresentation(IntPtr renderer, out int w, out int h, out SDL_RendererLogicalPresentation mode);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetRenderLogicalPresentationRect(IntPtr renderer, out SDL_FRect rect);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderCoordinatesFromWindow(IntPtr renderer, float window_x, float window_y, out float x, out float y);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderCoordinatesToWindow(IntPtr renderer, float x, float y, out float window_x, out float window_y);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_ConvertEventToRenderCoordinates(IntPtr renderer, ref SDL_Event @event);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetRenderViewport(IntPtr renderer, ref SDL_Rect rect);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetRenderViewport(IntPtr renderer, out SDL_Rect rect);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderViewportSet(IntPtr renderer);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetRenderSafeArea(IntPtr renderer, out SDL_Rect rect);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetRenderClipRect(IntPtr renderer, ref SDL_Rect rect);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetRenderClipRect(IntPtr renderer, out SDL_Rect rect);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderClipEnabled(IntPtr renderer);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetRenderScale(IntPtr renderer, float scaleX, float scaleY);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetRenderScale(IntPtr renderer, out float scaleX, out float scaleY);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetRenderDrawColor(IntPtr renderer, byte r, byte g, byte b, byte a);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetRenderDrawColorFloat(IntPtr renderer, float r, float g, float b, float a);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetRenderDrawColor(IntPtr renderer, out byte r, out byte g, out byte b, out byte a);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetRenderDrawColorFloat(IntPtr renderer, out float r, out float g, out float b, out float a);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetRenderColorScale(IntPtr renderer, float scale);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetRenderColorScale(IntPtr renderer, out float scale);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetRenderDrawBlendMode(IntPtr renderer, uint blendMode);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetRenderDrawBlendMode(IntPtr renderer, IntPtr blendMode);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderClear(IntPtr renderer);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderPoint(IntPtr renderer, float x, float y);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderPoints(IntPtr renderer, Span<SDL_FPoint> points, int count);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderLine(IntPtr renderer, float x1, float y1, float x2, float y2);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderLines(IntPtr renderer, Span<SDL_FPoint> points, int count);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderRect(IntPtr renderer, ref SDL_FRect rect);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderRects(IntPtr renderer, Span<SDL_FRect> rects, int count);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderFillRect(IntPtr renderer, ref SDL_FRect rect);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderFillRects(IntPtr renderer, Span<SDL_FRect> rects, int count);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderTexture(IntPtr renderer, IntPtr texture, ref SDL_FRect srcrect, ref SDL_FRect dstrect); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderTextureRotated(IntPtr renderer, IntPtr texture, ref SDL_FRect srcrect, ref SDL_FRect dstrect, double angle, ref SDL_FPoint center, SDL_FlipMode flip); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderTextureAffine(IntPtr renderer, IntPtr texture, in SDL_FRect srcrect, in SDL_FPoint origin, in SDL_FPoint right, in SDL_FPoint down);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderTextureTiled(IntPtr renderer, IntPtr texture, ref SDL_FRect srcrect, float scale, ref SDL_FRect dstrect); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderTexture9Grid(IntPtr renderer, IntPtr texture, ref SDL_FRect srcrect, float left_width, float right_width, float top_height, float bottom_height, float scale, ref SDL_FRect dstrect); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderGeometry(IntPtr renderer, IntPtr texture, Span<SDL_Vertex> vertices, int num_vertices, Span<int> indices, int num_indices); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderGeometryRaw(IntPtr renderer, IntPtr texture, IntPtr xy, int xy_stride, IntPtr color, int color_stride, IntPtr uv, int uv_stride, int num_vertices, IntPtr indices, int num_indices, int size_indices); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDL_Surface* SDL_RenderReadPixels(IntPtr renderer, ref SDL_Rect rect);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderPresent(IntPtr renderer);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_DestroyTexture(IntPtr texture); // WARN_UNKNOWN_POINTER_PARAMETER

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_DestroyRenderer(IntPtr renderer);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_FlushRenderer(IntPtr renderer);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetRenderMetalLayer(IntPtr renderer);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr SDL_GetRenderMetalCommandEncoder(IntPtr renderer);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_AddVulkanRenderSemaphores(IntPtr renderer, uint wait_stage_mask, long wait_semaphore, long signal_semaphore);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_SetRenderVSync(IntPtr renderer, int vsync);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_GetRenderVSync(IntPtr renderer, out int vsync);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderDebugText(IntPtr renderer, float x, float y, string str);

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SDLBool SDL_RenderDebugTextFormat(IntPtr renderer, float x, float y, string fmt);

    // /usr/local/include/SDL3/SDL_version.h

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_GetVersion();

    [LibraryImport(SDL3DllName, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(SDLOwnedStringMarshaller))]
    internal static partial string SDL_GetRevision();

    // /usr/local/include/SDL3/SDL_main.h

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int SDL_main_func(int argc, IntPtr argv);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_SetMainReady();

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_RunApp(int argc, IntPtr argv, SDL_main_func mainFunction, IntPtr reserved);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int SDL_EnterAppMainCallbacks(int argc, IntPtr argv, SDL_AppInit_func appinit, SDL_AppIterate_func appiter, SDL_AppEvent_func appevent, SDL_AppQuit_func appquit);

    [LibraryImport(SDL3DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_GDKSuspendComplete();
}