// © Mike Murphy

#pragma once

using namespace System;

namespace EMU7800 { namespace D2D { namespace Interop {

public enum struct D2DSolidColorBrush { Black, Red, Orange, Yellow, Green, Blue, Gray, White };
public enum struct D2DAntiAliasMode { PerPrimitive, Aliased };
public enum struct D2DBitmapInterpolationMode { NearestNeighbor, Linear };

value struct PointF;
value struct RectF;
value struct SizeU;
ref class DynamicBitmap;
ref class StaticBitmap;
ref class TextFormat;
ref class TextLayout;

public ref class GraphicsDevice
{
private:
    HWND m_hWnd;
    ID2D1Factory* m_pD2D1Factory;
    IDWriteFactory* m_pDWriteFactory;
    IWICImagingFactory* m_pWICFactory;
    ID2D1HwndRenderTarget* m_pRenderTarget;
    literal int nSolidColorBrushes = 8;
    ID2D1SolidColorBrush** m_ppSolidColorBrushes;
    HRESULT CreateDeviceResources();
    void DiscardDeviceResources();
    HRESULT CreateSolidColorBrush(D2DSolidColorBrush brush, D2D1::ColorF color);

public:
    property bool IsDeviceResourcesRefreshed;
    void BeginDraw();
    HRESULT EndDraw();
    DynamicBitmap^ CreateDynamicBitmap(SizeU size);
    StaticBitmap^ CreateStaticBitmap(array<byte>^ data);
    TextFormat^ CreateTextFormat(String^ fontFamilyName, FLOAT fontSize);
    TextFormat^ CreateTextFormat(String^ fontFamilyName, int fontWeight, int fontStyle, int fontStretch, FLOAT fontSize);
    TextLayout^ CreateTextLayout(String^ fontFamilyName, FLOAT fontSize, String ^text, FLOAT width, FLOAT height);
    TextLayout^ CreateTextLayout(String^ fontFamilyName, int fontWeight, int fontStyle, int fontStretch, FLOAT fontSize, String ^text, FLOAT width, FLOAT height);
    void DrawLine(PointF p0, PointF p1, float strokeWidth, D2DSolidColorBrush brush);
    void DrawRectangle(RectF rect, float strokeWidth, D2DSolidColorBrush brush);
    void FillRectangle(RectF rect, D2DSolidColorBrush brush);
    void DrawEllipse(RectF rect, float strokeWidth, D2DSolidColorBrush brush);
    void FillEllipse(RectF rect, D2DSolidColorBrush brush);
    void DrawText(TextFormat^ textFormat, String^ text, RectF rect, D2DSolidColorBrush brush);
    void DrawText(TextLayout^ textLayout, PointF location, D2DSolidColorBrush brush);
    void DrawBitmap(DynamicBitmap^ bitmap, RectF rect, D2DBitmapInterpolationMode interpolationMode);
    void DrawBitmap(StaticBitmap^ bitmap, RectF rect);
    void SetAntiAliasMode(D2DAntiAliasMode antiAliasMode);
    void PushAxisAlignedClip(RectF rect, D2DAntiAliasMode antiAliasMode);
    void PopAxisAlignedClip();

    HRESULT Initialize();
    void AttachHwnd(HWND hWnd);
    void Resize(SizeU size);

    GraphicsDevice();
    ~GraphicsDevice();
    !GraphicsDevice();
};

} } }