// © Mike Murphy

#pragma once

namespace EMU7800 { namespace D2D { namespace Interop {

public ref class StaticBitmap
{
private:
    ID2D1Bitmap* m_pBitmap;
    HRESULT m_hr;

internal:
    property ID2D1Bitmap* D2DBitmap { ID2D1Bitmap* get() { return m_pBitmap; } }
    StaticBitmap(ID2D1HwndRenderTarget* pRenderTarget, IWICImagingFactory* pWICFactory, array<byte>^ data);

public:
    property int HR { int get() { return m_hr; } };
    ~StaticBitmap();
    !StaticBitmap();
};

} } }