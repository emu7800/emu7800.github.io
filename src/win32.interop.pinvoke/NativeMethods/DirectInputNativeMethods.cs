// © Mike Murphy

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;

namespace EMU7800.Win32.Interop
{
    internal unsafe partial class DirectInputNativeMethods
    {
        public const int
            AXISRANGE = 1000,
            DEADZONE  = 500
            ;

        [StructLayout(LayoutKind.Sequential)]
        public struct DIJOYSTATE2
        {
            public int  lX;                     /* x-axis position              */
            public int  lY;                     /* y-axis position              */
            public int  lZ;                     /* z-axis position              */
            public int  lRx;                    /* x-axis rotation              */
            public int  lRy;                    /* y-axis rotation              */
            public int  lRz;                    /* z-axis rotation              */
            public fixed int  rglSlider[2];     /* extra axes positions         */
            public fixed uint rgdwPOV[4];       /* POV directions               */
            public fixed byte rgbButtons[128];  /* 128 buttons                  */
            public int  lVX;                    /* x-axis velocity              */
            public int  lVY;                    /* y-axis velocity              */
            public int  lVZ;                    /* z-axis velocity              */
            public int  lVRx;                   /* x-axis angular velocity      */
            public int  lVRy;                   /* y-axis angular velocity      */
            public int  lVRz;                   /* z-axis angular velocity      */
            public fixed int  rglVSlider[2];    /* extra axes velocities        */
            public int  lAX;                    /* x-axis acceleration          */
            public int  lAY;                    /* y-axis acceleration          */
            public int  lAZ;                    /* z-axis acceleration          */
            public int  lARx;                   /* x-axis angular acceleration  */
            public int  lARy;                   /* y-axis angular acceleration  */
            public int  lARz;                   /* z-axis angular acceleration  */
            public fixed int  rglASlider[2];    /* extra axes accelerations     */
            public int  lFX;                    /* x-axis force                 */
            public int  lFY;                    /* y-axis force                 */
            public int  lFZ;                    /* z-axis force                 */
            public int  lFRx;                   /* x-axis torque                */
            public int  lFRy;                   /* y-axis torque                */
            public int  lFRz;                   /* z-axis torque                */
            public fixed int  rglFSlider[2];    /* extra axes forces            */

            public bool InterpretJoyButtonDown(int i) => (rgbButtons[i] & 0x80) == 0x80;

            public readonly bool InterpretJoyLeft()  => lX < -DEADZONE;
            public readonly bool InterpretJoyRight() => lX >  DEADZONE;
            public readonly bool InterpretJoyUp()    => lY < -DEADZONE;
            public readonly bool InterpretJoyDown()  => lY >  DEADZONE;

            public readonly int InterpretStelladaptorDrivingPosition()
            {
                if      (lY < -DEADZONE)
                    return 3;                          // up
                else if (lY > (AXISRANGE - DEADZONE))
                    return 1;                          // down (full)
                else if (lY > DEADZONE)
                    return 2;                          // down (half)
                else
                    return 0;                          // center
            }

            public readonly int InterpretStelladaptorPaddlePosition(int paddleno)
                => (((paddleno & 1) == 0) ? lX : lY);

            public readonly int InterpretDaptor2Mode()
                => lZ switch
                {
                    -1000 =>  0,  // 2600 mode
                     -875 =>  1,  // 7800 mode
                     -750 =>  2,  // keypad mode
                        _ => -1,  // unknown mode
                };
        }

        public static int Initialize(IntPtr hWnd, out string[] joystickNames)
        {
            var hr = DInput8_Initialize(hWnd, AXISRANGE, out IntPtr productName1Ptr, out IntPtr productName2Ptr);
            joystickNames = new List<string>
                        {
                            Marshal.PtrToStringUni(productName1Ptr) ?? string.Empty,
                            Marshal.PtrToStringUni(productName2Ptr) ?? string.Empty
                        }
                        .Where(js => !string.IsNullOrWhiteSpace(js))
                        .ToArray();
            return hr;
        }

        public static int Poll(int deviceno, out DIJOYSTATE2 currState, out DIJOYSTATE2 prevState)
        {
            var currStatePtr = IntPtr.Zero;
            var prevStatePtr = IntPtr.Zero;
            var hr = DInput8_Poll(deviceno, ref currStatePtr, ref prevStatePtr);
            currState = Marshal.PtrToStructure<DIJOYSTATE2>(currStatePtr);
            prevState = Marshal.PtrToStructure<DIJOYSTATE2>(prevStatePtr);
            return hr;
        }

        public static int Shutdown()
            => DInput8_Shutdown();

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial int DInput8_Initialize(IntPtr hWnd, int axisRange, out IntPtr productName1Ptr, out IntPtr productName2Ptr);

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial int DInput8_Poll(int deviceno, ref IntPtr ppCurrState, ref IntPtr ppPrevState);

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial int DInput8_Shutdown();
    }
}
