// © Mike Murphy

#pragma once

namespace EMU7800 { namespace D2D { namespace Interop {

using namespace System;

public value struct PointF
{
internal:
    void CopyFrom(__in const D2D1_POINT_2F &point_2f)
    {
        X = point_2f.x;
        Y = point_2f.y;
    }
    void CopyTo(__out D2D1_POINT_2F *ppoint_2f)
    {
        ppoint_2f->x = X;
        ppoint_2f->y = Y;
    }

public:
    property FLOAT X;
    property FLOAT Y;
};

public value struct SizeF
{
internal:
    void CopyFrom(__in const D2D1_SIZE_F &size_f)
    {
        Width = size_f.width;
        Height = size_f.height;
    }
    void CopyTo(__out D2D1_SIZE_F *size_f)
    {
        size_f->width = Width;
        size_f->height = Height;
    }

public:
    property FLOAT Width;
    property FLOAT Height;
};

public value struct SizeU
{
internal:
    void CopyFrom(__in const D2D1_SIZE_U &size_u)
    {
        Width = size_u.width;
        Height = size_u.height;
    }
    void CopyTo(__out D2D1_SIZE_U *size_u)
    {
        size_u->width = Width;
        size_u->height = Height;
    }

public:
    property UINT32 Width;
    property UINT32 Height;
};

public value struct RectF
{
internal:
    void CopyFrom(__in const D2D1_RECT_F &rect_f)
    {
        Left = rect_f.left;
        Top = rect_f.top;
        Right = rect_f.right;
        Bottom = rect_f.bottom;
    }

    void CopyTo(__out D2D1_RECT_F *prect_f)
    {
        prect_f->left = Left;
        prect_f->top = Top;
        prect_f->right = Right;
        prect_f->bottom = Bottom;
    }

public:
    property FLOAT Left;
    property FLOAT Top;
    property FLOAT Right;
    property FLOAT Bottom;
};

} } }