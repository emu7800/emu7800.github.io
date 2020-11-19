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
        A7800PAL
    };

    public static class MachineTypeUtil
    {
        public static MachineType From(string machineTypeStr)
            => Enum.TryParse<MachineType>(machineTypeStr, true, out var mt) ? mt : MachineType.Unknown;

        public static string To2600or7800WordString(MachineType machineType)
            => machineType switch
            {
                MachineType.A2600NTSC or MachineType.A2600PAL => "2600",
                MachineType.A7800NTSC or MachineType.A7800PAL => "7800",
                _ => string.Empty,
            };

        public static string ToMachineTypeWordString(MachineType machineType)
            => machineType switch
            {
                MachineType.A2600NTSC => "2600 NTSC",
                MachineType.A2600PAL  => "2600 PAL",
                MachineType.A7800NTSC => "7800 NTSC",
                MachineType.A7800PAL  => "7800 PAL",
                _ => string.Empty,
            };

        public static string ToTvTypeWordString(MachineType machineType)
            => machineType switch
            {
                MachineType.A2600NTSC or MachineType.A7800NTSC => "NTSC",
                MachineType.A2600PAL  or MachineType.A7800PAL  => "PAL",
                _ => string.Empty,
            };

        public static string ToString(MachineType machineType)
            => machineType.ToString();

        public static IEnumerable<MachineType> GetAllValues(bool excludeUnknown = true)
            => Enum.GetValues<MachineType>()
                .Where(mt => !excludeUnknown || mt != MachineType.Unknown);
    }
}
