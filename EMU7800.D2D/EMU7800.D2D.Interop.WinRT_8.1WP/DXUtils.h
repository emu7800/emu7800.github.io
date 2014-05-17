#pragma once

#include "pch.h"

namespace DX
{
    inline void ThrowIfFailed(HRESULT hr)
    {
        if (FAILED(hr))
        {
            // Set a breakpoint on this line to catch DX API errors.
            throw Platform::Exception::CreateException(hr);
        }
    }

    template <class T>
    inline void SafeRelease(T& iUnk)
    {
        if (iUnk)
        {
            iUnk->Release();
            iUnk = NULL;
        }
    }
}