/*
 * CartType.cs
 *
 * Defines the set of all known cartridges.
 *
 * Copyright © 2010, 2020 Mike Murphy
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace EMU7800.Core
{
    public enum CartType
    {
        Unknown,
        A2K,
        TV8K,
        A4K,
        PB8K,
        MN16K,
        A16K,
        A16KR,
        A8K,
        A8KR,
        A32K,
        A32KR,
        CBS12K,
        DC8K,
        DPC,
        M32N12K,
        A7808,
        A7816,
        A7832,
        A7832P,
        A7848,
        A78SG,
        A78SGP,
        A78SGR,
        A78S9,
        A78S4,
        A78S4R,
        A78AB,
        A78AC,
    };

    public static class CartTypeUtil
    {
        public static CartType From(string cartTypeStr)
            => Enum.TryParse<CartType>(cartTypeStr, true, out var ct) ? ct : CartType.Unknown;

        public static string ToCartTypeWordString(CartType cartType)
            => cartType switch
            {
                CartType.A2K     => "Atari 2kb cart",
                CartType.TV8K    => "Tigervision 8kb bankswitched cart",
                CartType.A4K     => "Atari 4kb cart",
                CartType.PB8K    => "Parker Brothers 8kb bankswitched cart",
                CartType.MN16K   => "M-Network 16kb bankswitched cart",
                CartType.A16K    => "Atari 16kb bankswitched cart",
                CartType.A16KR   => "Atari 16kb bankswitched cart w/128 bytes RAM",
                CartType.A8K     => "Atari 8KB bankswitched cart",
                CartType.A8KR    => "Atari 8KB bankswitched cart w/128 bytes RAM",
                CartType.A32K    => "Atari 32KB bankswitched cart",
                CartType.A32KR   => "Atari 32KB bankswitched cart w/128 bytes RAM",
                CartType.CBS12K  => "CBS RAM Plus bankswitched cart w/256 bytes RAM",
                CartType.DC8K    => "Special Activision cart (Robot Tank and Decathlon)",
                CartType.DPC     => "Pitfall II DPC cart",
                CartType.M32N12K => "32N1 Multicart: 32x2KB",
                CartType.A7808   => "Atari7800 non-bankswitched 8KB cart",
                CartType.A7816   => "Atari7800 non-bankswitched 16KB cart",
                CartType.A7832   => "Atari7800 non-bankswitched 32KB cart",
                CartType.A7832P  => "Atari7800 non-bankswitched 32KB cart w/Pokey",
                CartType.A7848   => "Atari7800 non-bankswitched 48KB cart",
                CartType.A78SG   => "Atari7800 SuperGame cart",
                CartType.A78SGP  => "Atari7800 SuperGame cart w/Pokey",
                CartType.A78SGR  => "Atari7800 SuperGame cart w/RAM",
                CartType.A78S9   => "Atari7800 SuperGame cart, nine banks",
                CartType.A78S4   => "Atari7800 SuperGame cart, four banks",
                CartType.A78S4R  => "Atari7800 SuperGame cart, four banks, w/RAM",
                CartType.A78AB   => "F18 Hornet cart (Absolute)",
                CartType.A78AC   => "Double Dragon cart (Activision)",
                _ => string.Empty
            };

        public static string ToString(CartType cartType)
            => cartType.ToString();

        public static IEnumerable<CartType> GetAllValues(bool excludeUnknown = true)
            => Enum.GetValues<CartType>()
                .Where(ct => !excludeUnknown || ct != CartType.Unknown);
    }
}
