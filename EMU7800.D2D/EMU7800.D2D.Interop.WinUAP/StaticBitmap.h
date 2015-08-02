// © Mike Murphy

#pragma once

using namespace Microsoft::WRL;
using namespace Platform;

namespace EMU7800 { namespace D2D { namespace Interop {

public ref class StaticBitmap sealed
{
private:
    ComPtr<ID2D1Bitmap> m_pBitmap;
    HRESULT m_hr;

internal:
    property ID2D1Bitmap* D2DBitmap { ID2D1Bitmap* get() { return m_pBitmap.Get(); } }
    StaticBitmap(ID2D1DeviceContext* pD2DContext, IWICImagingFactory* pWICFactory, const Array<uint8>^ data);

public:
    property int HR { int get() { return m_hr; } };
    virtual ~StaticBitmap();
};

} } }