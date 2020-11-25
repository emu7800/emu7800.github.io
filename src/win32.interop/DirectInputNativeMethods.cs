// © Mike Murphy

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace EMU7800.Win32.Interop
{
    public enum JoystickType { Normal, Stelladaptor, Daptor, Daptor2 };

    internal unsafe class DirectInputNativeMethods
    {
        const int
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

            public bool InterpretJoyLeft()  => lX < -DEADZONE;
            public bool InterpretJoyRight() => lX >  DEADZONE;
            public bool InterpretJoyUp()    => lY < -DEADZONE;
            public bool InterpretJoyDown()  => lY >  DEADZONE;

            public int InterpretStelladaptorDrivingPosition()
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

            public int InterpretStelladaptorPaddlePosition(int paddleno)
                => (((paddleno & 1) == 0) ? lX : lY) + AXISRANGE;

            public int InterpretDaptor2Mode()
                => lZ switch
                {
                    -1000 =>  0,  // 2600 mode
                     -875 =>  1,  // 7800 mode
                     -750 =>  2,  // keypad mode
                        _ => -1, // unknown mode
                };
        }

        public static int Initialize(IntPtr hWnd, out JoystickType[] joystickTypes)
        {
             var hr = DInput8_Initialize(hWnd, AXISRANGE, out var stelladaptorTypesPtr, out int joysticksFound);
            joystickTypes = new JoystickType[joysticksFound % 3];
            for (var i=0; i < joystickTypes.Length; i++)
            {
                joystickTypes[i] = (JoystickType)stelladaptorTypesPtr[i];
            }
            return hr;
        }

        public static int Poll(out DIJOYSTATE2 currState, out DIJOYSTATE2 prevState)
        {
            var currStatePtr = IntPtr.Zero;
            var prevStatePtr = IntPtr.Zero;
            var hr = DInput8_Poll(ref currStatePtr, ref prevStatePtr);
            currState = Marshal.PtrToStructure<DIJOYSTATE2>(currStatePtr);
            prevState = Marshal.PtrToStructure<DIJOYSTATE2>(prevStatePtr);
            return hr;
        }

        public static int Shutdown()
            => DInput8_Shutdown();

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling=true), SuppressUnmanagedCodeSecurity]
        static extern int DInput8_Initialize(IntPtr hWnd, int axisRange, out byte* stelladaptorTypesPtr, out int joysticksFound);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        static extern int DInput8_Poll(ref IntPtr ppCurrState, ref IntPtr ppPrevState);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        static extern int DInput8_Shutdown();
    }
}
