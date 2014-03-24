// © Mike Murphy

#pragma once

using namespace Microsoft::WRL;
using namespace Platform;

namespace EMU7800 { namespace D2D { namespace Interop {

public ref class DynamicBitmap sealed
{
private:
    ComPtr<ID2D1Bitmap> m_pBitmap;
    HRESULT m_hr;
    UINT m_expectedDataLength;
    UINT m_expectedPitch;

internal:
    property ID2D1Bitmap* D2DBitmap { ID2D1Bitmap* get() { return m_pBitmap.Get(); } }
    DynamicBitmap(ID2D1DeviceContext* pD2DContext, SizeU size);

public:
    property int HR { int get() { return m_hr; } };
    int CopyFromMemory(const Array<uint8>^ data);
    virtual ~DynamicBitmap();
};

} } }