﻿// © Mike Murphy

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace EMU7800.Win32.Interop
{
    internal unsafe static class Win32NativeMethods
    {
        delegate void KeyboardKeyPressedHandler(ushort vKey, bool down);
        delegate void MouseMovedHandler(int x, int y, int dx, int dy);
        delegate void MouseButtonChangedHandler(int x, int y, bool down);
        delegate void MouseWheelChangedHandler(int x, int y, int delta);
        delegate void LURCycleHandler();
        delegate void VisibilityChangedHandler(bool isVisible);
        delegate void ResizedHandler(int w, int h);
        delegate void DeviceChangedHandler();

        static readonly KeyboardKeyPressedHandler RaiseKeyboardKeyPressedDelegate = new(RaiseKeyboardKeyPressed);
        static void RaiseKeyboardKeyPressed(ushort vKey, bool down) => Win32Window.KeyboardKeyPressed(vKey, down);

        static readonly MouseMovedHandler RaiseMouseMovedDelegate = new(RaiseMouseMoved);
        static void RaiseMouseMoved(int x, int y, int dx, int dy) => Win32Window.MouseMoved(x, y, dx, dy);

        static readonly MouseButtonChangedHandler RaiseMouseButtonChangedDelegate = new(RaiseMouseButtonChanged);
        static void RaiseMouseButtonChanged(int x, int y, bool down) => Win32Window.MouseButtonChanged(x, y, down);

        static readonly MouseWheelChangedHandler RaiseMouseWheelChangedDelegate = new(RaiseMouseWheelChanged);
        static void RaiseMouseWheelChanged(int x, int y, int delta) => Win32Window.MouseWheelChanged(x, y, delta);

        static readonly LURCycleHandler RaiseLURCycleDelegate = new(RaiseLURCycle);
        static void RaiseLURCycle() => Win32Window.LURCycle();

        static readonly VisibilityChangedHandler RaiseVisibilityChangedDelegate = new(RaiseVisibilityChanged);
        static void RaiseVisibilityChanged(bool isVisible) => Win32Window.VisibilityChanged(isVisible);

        static readonly ResizedHandler RaiseResizedDelegate = new(RaiseResized);
        static void RaiseResized(int w, int h) => Win32Window.Resized(w, h);

        static readonly DeviceChangedHandler RaiseDeviceChangedDelegate = new(RaiseDeviceChanged);
        static void RaiseDeviceChanged() => Win32Window.DeviceChanged();

        static IntPtr GetIntPtr<TDelegate>(TDelegate d) where TDelegate : notnull
            => Marshal.GetFunctionPointerForDelegate(d);

        public static IntPtr Win32_Initialize()
            => Win32_Initialize(
                GetIntPtr(RaiseKeyboardKeyPressedDelegate),
                GetIntPtr(RaiseMouseMovedDelegate),
                GetIntPtr(RaiseMouseButtonChangedDelegate),
                GetIntPtr(RaiseMouseWheelChangedDelegate),
                GetIntPtr(RaiseLURCycleDelegate),
                GetIntPtr(RaiseVisibilityChangedDelegate),
                GetIntPtr(RaiseResizedDelegate),
                GetIntPtr(RaiseDeviceChangedDelegate));

        [DllImport("EMU7800.Win32.Interop.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        static extern IntPtr Win32_Initialize(
            IntPtr keyboardkeypressedcb,
            IntPtr mousemovedcb,
            IntPtr mousebuttonchangedcb,
            IntPtr mousewheelchangedcb,
            IntPtr lurcyclecb,
            IntPtr visibilitychangedcb,
            IntPtr resizedcb,
            IntPtr devicechangedcb);

        [DllImport("EMU7800.Win32.Interop.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Win32_ProcessEvents(IntPtr hWnd, int nCmdShow);

        [DllImport("EMU7800.Win32.Interop.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Win32_Quit();
    }
}
