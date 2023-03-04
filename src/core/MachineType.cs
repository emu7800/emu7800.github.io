/*
 * MachineType.cs
 *
 * The set of known machines.
 *
 * Copyright © 2010, 2020 Mike Murphy
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace EMU7800.Core
{
    public enum MachineType
    {
        Unknown,
        A2600NTSC,
        A2600PAL,
        A7800NTSC,
        A7800NTSCbios,
        A7800NTSChsc,
        A7800NTSCxm,
        A7800PAL,
        A7800PALbios,
        A7800PALhsc,
        A7800PALxm,
    };

    public static class MachineTypeUtil
    {
        public static string ToMachineTypeWordString(MachineType machineType)
            => To2600or7800WordString(machineType) + " " + ToTvTypeWordString(machineType);

        public static string To2600or7800WordString(MachineType machineType)
            => Is2600(machineType) ? "2600" : Is7800(machineType) ? "7800" : string.Empty;

        public static string ToTvTypeWordString(MachineType machineType)
            => IsNTSC(machineType) ? "NTSC" : IsPAL(machineType) ? "PAL" : string.Empty;

        public static bool Is2600NTSC(MachineType machineType)
            => Is2600(machineType) && IsNTSC(machineType);

        public static bool Is2600PAL(MachineType machineType)
            => Is2600(machineType) && IsPAL(machineType);

        public static bool Is7800NTSC(MachineType machineType)
            => Is7800(machineType) && IsNTSC(machineType);

        public static bool Is7800PAL(MachineType machineType)
            => Is7800(machineType) && IsPAL(machineType);

        public static bool Is2600(MachineType machineType)
            => machineType switch
            {
                MachineType.A2600NTSC => true,
                MachineType.A2600PAL => true,
                _ => false
            };

        public static bool Is7800(MachineType machineType)
            => machineType switch
            {
                MachineType.A7800NTSC => true,
                MachineType.A7800NTSCbios => true,
                MachineType.A7800NTSChsc => true,
                MachineType.A7800NTSCxm => true,
                MachineType.A7800PAL => true,
                MachineType.A7800PALbios => true,
                MachineType.A7800PALhsc => true,
                MachineType.A7800PALxm => true,
                _ => false
            };

        public static bool IsNTSC(MachineType machineType)
            => machineType switch
            {
                MachineType.A2600NTSC => true,
                MachineType.A7800NTSC => true,
                MachineType.A7800NTSCbios => true,
                MachineType.A7800NTSChsc => true,
                MachineType.A7800NTSCxm => true,
                _ => false
            };

        public static bool IsPAL(MachineType machineType)
            => machineType switch
            {
                MachineType.A2600PAL => true,
                MachineType.A7800PAL => true,
                MachineType.A7800PALbios => true,
                MachineType.A7800PALhsc => true,
                MachineType.A7800PALxm => true,
                _ => false
            };

        public static bool Is7800bios(MachineType machineType)
            => machineType switch
            {
                MachineType.A7800NTSCbios => true,
                MachineType.A7800PALbios => true,
                _ => false
            };

        public static bool Is7800hsc(MachineType machineType)
            => machineType switch
            {
                MachineType.A7800NTSChsc => true,
                MachineType.A7800PALhsc => true,
                _ => false
            };

        public static bool Is7800xm(MachineType machineType)
            => machineType switch
            {
                MachineType.A7800NTSCxm => true,
                MachineType.A7800PALxm => true,
                _ => false
            };

        public static string ToString(MachineType machineType)
            => machineType.ToString();

        public static MachineType From(string machineTypeStr)
            => Enum.TryParse<MachineType>(machineTypeStr, true, out var mt) ? mt : MachineType.Unknown;

        public static IEnumerable<MachineType> GetAllValues(bool excludeUnknown = true)
            => Enum.GetValues<MachineType>()
                .Where(mt => !excludeUnknown || mt != MachineType.Unknown);
    }
}
