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

namespace EMU7800.Core;

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
        => machineType switch
        {
            MachineType.A2600NTSC     => "Atari 2600 NTSC",
            MachineType.A2600PAL      => "Atari 2600 PAL",
            MachineType.A7800NTSC     => "Atari 7800 NTSC",
            MachineType.A7800NTSCbios => "Atari 7800 NTSC w/BIOS",
            MachineType.A7800NTSChsc  => "Atari 7800 NTSC w/High Score Cart",
            MachineType.A7800NTSCxm   => "Atari 7800 NTSC w/eXpansion Module",
            MachineType.A7800PAL      => "Atari 7800 PAL",
            MachineType.A7800PALbios  => "Atari 7800 PAL w/BIOS",
            MachineType.A7800PALhsc   => "Atari 7800 PAL w/High Score Cart",
            MachineType.A7800PALxm    => "Atari 7800 PAL w/eXpansion Module",
            MachineType.Unknown       => "Unknown",
            _ => string.Empty
        };

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

    public static MachineType From(string mtStr)
    {
        if (Enum.TryParse<MachineType>(mtStr, true, out var mt)) return mt;
        var is2600 = mtStr.Contains("2600");
        var is7800 = mtStr.Contains("7800");
        var isNTSC = mtStr.Contains("NTSC", StringComparison.OrdinalIgnoreCase);
        var isPAL  = mtStr.Contains("PAL",  StringComparison.OrdinalIgnoreCase);
        var isBIOS = mtStr.Contains("bios", StringComparison.OrdinalIgnoreCase);
        var isHSC  = mtStr.Contains("hsc",  StringComparison.OrdinalIgnoreCase);
        var isXM   = mtStr.Contains("xm",   StringComparison.OrdinalIgnoreCase);
        if (is2600 && isNTSC)                               return MachineType.A2600NTSC;
        if (is2600 && isPAL)                                return MachineType.A2600PAL;
        if (is7800 && isNTSC && !isBIOS && !isHSC && !isXM) return MachineType.A7800NTSC;
        if (is7800 && isNTSC && isBIOS  && !isHSC && !isXM) return MachineType.A7800NTSCbios;
        if (is7800 && isNTSC && !isBIOS && isHSC  && !isXM) return MachineType.A7800NTSChsc;
        if (is7800 && isNTSC && !isBIOS && !isHSC && isXM)  return MachineType.A7800NTSCxm;
        if (is7800 && isPAL  && !isBIOS && !isHSC && !isXM) return MachineType.A7800PAL;
        if (is7800 && isPAL  && isBIOS  && !isHSC && !isXM) return MachineType.A7800PALbios;
        if (is7800 && isPAL  && !isBIOS && isHSC  && !isXM) return MachineType.A7800PALhsc;
        if (is7800 && isPAL  && !isBIOS && !isHSC && isXM)  return MachineType.A7800PALxm;
        return MachineType.Unknown;
    }

    public static IEnumerable<MachineType> GetAllValues(bool excludeUnknown = true)
        => Enum.GetValues<MachineType>()
            .Where(mt => !excludeUnknown || mt != MachineType.Unknown);
}
