// © Mike Murphy

#pragma once

namespace EMU7800 { namespace D2D { namespace Interop {

public ref class DynamicBitmap
{
private:
    ID2D1Bitmap* m_pBitmap;
    HRESULT m_hr;
    int m_expectedDataLength;
    int m_expectedPitch;

internal:
    property ID2D1Bitmap* D2DBitmap { ID2D1Bitmap* get() { return m_pBitmap; } }
    DynamicBitmap(ID2D1HwndRenderTarget* pRenderTarget, SizeU size);

public:
    property int HR { int get() { return m_hr; } };
    int CopyFromMemory(array<byte> ^data);
    ~DynamicBitmap();
    !DynamicBitmap();
};

} } }