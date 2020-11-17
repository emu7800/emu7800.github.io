// © Mike Murphy

#pragma once

namespace EMU7800 { namespace D2D { namespace Interop {

public enum class D2DError
{
        RecreateTarget      = D2DERR_RECREATE_TARGET,
        WrongResourceDomain = D2DERR_WRONG_RESOURCE_DOMAIN,
        Win32Error          = D2DERR_WIN32_ERROR,
        UnsupportedVersion  = D2DERR_UNSUPPORTED_VERSION,
        NoHardwareDevice    = D2DERR_NO_HARDWARE_DEVICE,
        InvalidCall         = D2DERR_INVALID_CALL,
        InternalError       = D2DERR_INTERNAL_ERROR,
        NotInitialized      = D2DERR_NOT_INITIALIZED,
};

} } }