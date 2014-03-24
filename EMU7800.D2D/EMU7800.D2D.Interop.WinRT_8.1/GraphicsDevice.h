// © Mike Murphy

#pragma once

#include "DXUtils.h"

using namespace Microsoft::WRL;
using namespace Platform;
using namespace Windows::Foundation;
using namespace Windows::UI::Core;

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

[Windows::Foundation::Metadata::WebHostHidden]
public ref class GraphicsDevice sealed
{
private:
    Agile<CoreWindow>               m_window;

    ComPtr<IDWriteFactory1>         m_pDWriteFactory;
    ComPtr<IWICImagingFactory2>     m_wicFactory;

    ComPtr<ID3D11Device1>           m_d3dDevice;
    ComPtr<ID3D11DeviceContext1>    m_d3dContext;
    ComPtr<IDXGIDevice3>            m_dxgiDevice;
    ComPtr<IDXGISwapChain1>         m_swapChain;
    ComPtr<ID3D11RenderTargetView>  m_d3dRenderTargetView;

    ComPtr<ID2D1Factory1>           m_d2dFactory;
    ComPtr<ID2D1Device>             m_d2dDevice;
    ComPtr<ID2D1DeviceContext>      m_d2dContext;
    ComPtr<ID2D1Bitmap1>            m_d2dTargetBitmap;

    D3D_FEATURE_LEVEL               m_featureLevel;
    Windows::Foundation::Size       m_renderTargetSize;
    Windows::Foundation::Rect       m_windowBounds;
    float                           m_dpi;
    bool                            m_windowSizeChangeInProgress;

    ComPtr<ID2D1SolidColorBrush>    m_solidColorBrushes[8];

    HRESULT CreateSolidColorBrush(D2DSolidColorBrush brush, D2D1::ColorF color);

public:
    property bool IsDeviceResourcesRefreshed;
    void BeginDraw();
    int EndDraw();
    DynamicBitmap^ CreateDynamicBitmap(SizeU size);
    StaticBitmap^ CreateStaticBitmap(const Array<uint8>^ data);
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
    [Windows::Foundation::Metadata::DefaultOverload]
    void DrawBitmap(StaticBitmap^ bitmap, RectF rect);
    void SetAntiAliasMode(D2DAntiAliasMode antiAliasMode);
    void PushAxisAlignedClip(RectF rect, D2DAntiAliasMode antiAliasMode);
    void PopAxisAlignedClip();

    void Initialize(CoreWindow^ window, float dpi);
    void HandleDeviceLost();
    void CreateDeviceIndependentResources();
    void CreateDeviceResources();
    void SetDpi(float dpi);
    void UpdateForWindowSizeChange();
    void CreateWindowSizeDependentResources();
    void Present();
    void ValidateDevice();
    void Trim();  // Windows App Certification Kit 3.1 requirement

    GraphicsDevice();
    virtual ~GraphicsDevice();
};

} } }