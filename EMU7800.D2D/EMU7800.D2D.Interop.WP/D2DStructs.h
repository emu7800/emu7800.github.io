// © Mike Murphy

#pragma once

namespace EMU7800 { namespace D2D { namespace Interop {

public value struct PointF
{
public:
    FLOAT X;
    FLOAT Y;
};

public value struct SizeF
{
public:
    FLOAT Width;
    FLOAT Height;
};

public value struct SizeU
{
public:
    UINT32 Width;
    UINT32 Height;
};

public value struct RectF
{
public:
    FLOAT Left;
    FLOAT Top;
    FLOAT Right;
    FLOAT Bottom;
};

} } }