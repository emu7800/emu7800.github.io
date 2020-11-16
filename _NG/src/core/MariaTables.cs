/*
 * MariaTables.cs
 *
 * Palette tables for the Maria class.
 * All derived from Dan Boris' 7800/MAME code.
 *
 * Copyright © 2004 Mike Murphy
 *
 */
namespace EMU7800.Core
{
    public static class MariaTables
    {
        public static readonly int[] NTSCPalette =
        {
            0x000000, 0x1c1c1c, 0x393939, 0x595959,  // Grey
            0x797979, 0x929292, 0xababab, 0xbcbcbc,
            0xcdcdcd, 0xd9d9d9, 0xe6e6e6, 0xececec,
            0xf2f2f2, 0xf8f8f8, 0xffffff, 0xffffff,

            0x391701, 0x5e2304, 0x833008, 0xa54716,  // Gold
            0xc85f24, 0xe37820, 0xff911d, 0xffab1d,
            0xffc51d, 0xffce34, 0xffd84c, 0xffe651,
            0xfff456, 0xfff977, 0xffff98, 0xffff98,

            0x451904, 0x721e11, 0x9f241e, 0xb33a20,  // Orange
            0xc85122, 0xe36920, 0xff811e, 0xff8c25,
            0xff982c, 0xffae38, 0xffc545, 0xffc559,
            0xffc66d, 0xffd587, 0xffe4a1, 0xffe4a1,

            0x4a1704, 0x7e1a0d, 0xb21d17, 0xc82119,  // Red Orange
            0xdf251c, 0xec3b38, 0xfa5255, 0xfc6161,
            0xff706e, 0xff7f7e, 0xff8f8f, 0xff9d9e,
            0xffabad, 0xffb9bd, 0xffc7ce, 0xffc7ce,

            0x050568, 0x3b136d, 0x712272, 0x8b2a8c,  // Pink
            0xa532a6, 0xb938ba, 0xcd3ecf, 0xdb47dd,
            0xea51eb, 0xf45ff5, 0xfe6dff, 0xfe7afd,
            0xff87fb, 0xff95fd, 0xffa4ff, 0xffa4ff,

            0x280479, 0x400984, 0x590f90, 0x70249d,  // Purple
            0x8839aa, 0xa441c3, 0xc04adc, 0xd054ed,
            0xe05eff, 0xe96dff, 0xf27cff, 0xf88aff,
            0xff98ff, 0xfea1ff, 0xfeabff, 0xfeabff,

            0x35088a, 0x420aad, 0x500cd0, 0x6428d0,  // Purple Blue
            0x7945d0, 0x8d4bd4, 0xa251d9, 0xb058ec,
            0xbe60ff, 0xc56bff, 0xcc77ff, 0xd183ff,
            0xd790ff, 0xdb9dff, 0xdfaaff, 0xdfaaff,

            0x051e81, 0x0626a5, 0x082fca, 0x263dd4,  // Blue1
            0x444cde, 0x4f5aee, 0x5a68ff, 0x6575ff,
            0x7183ff, 0x8091ff, 0x90a0ff, 0x97a9ff,
            0x9fb2ff, 0xafbeff, 0xc0cbff, 0xc0cbff,

            0x0c048b, 0x2218a0, 0x382db5, 0x483ec7,  // Blue2
            0x584fda, 0x6159ec, 0x6b64ff, 0x7a74ff,
            0x8a84ff, 0x918eff, 0x9998ff, 0xa5a3ff,
            0xb1aeff, 0xb8b8ff, 0xc0c2ff, 0xc0c2ff,

            0x1d295a, 0x1d3876, 0x1d4892, 0x1c5cac,  // Light Blue
            0x1c71c6, 0x3286cf, 0x489bd9, 0x4ea8ec,
            0x55b6ff, 0x70c7ff, 0x8cd8ff, 0x93dbff,
            0x9bdfff, 0xafe4ff, 0xc3e9ff, 0xc3e9ff,

            0x2f4302, 0x395202, 0x446103, 0x417a12,  // Turquoise
            0x3e9421, 0x4a9f2e, 0x57ab3b, 0x5cbd55,
            0x61d070, 0x69e27a, 0x72f584, 0x7cfa8d,
            0x87ff97, 0x9affa6, 0xadffb6, 0xadffb6,

            0x0a4108, 0x0d540a, 0x10680d, 0x137d0f,  // Green Blue
            0x169212, 0x19a514, 0x1cb917, 0x1ec919,
            0x21d91b, 0x47e42d, 0x6ef040, 0x78f74d,
            0x83ff5b, 0x9aff7a, 0xb2ff9a, 0xb2ff9a,

            0x04410b, 0x05530e, 0x066611, 0x077714,  // Green
            0x088817, 0x099b1a, 0x0baf1d, 0x48c41f,
            0x86d922, 0x8fe924, 0x99f927, 0xa8fc41,
            0xb7ff5b, 0xc9ff6e, 0xdcff81, 0xdcff81,

            0x02350f, 0x073f15, 0x0c4a1c, 0x2d5f1e,  // Yellow Green
            0x4f7420, 0x598324, 0x649228, 0x82a12e,
            0xa1b034, 0xa9c13a, 0xb2d241, 0xc4d945,
            0xd6e149, 0xe4f04e, 0xf2ff53, 0xf2ff53,

            0x263001, 0x243803, 0x234005, 0x51541b,  // Orange Green
            0x806931, 0x978135, 0xaf993a, 0xc2a73e,
            0xd5b543, 0xdbc03d, 0xe1cb38, 0xe2d836,
            0xe3e534, 0xeff258, 0xfbff7d, 0xfbff7d,

            0x401a02, 0x581f05, 0x702408, 0x8d3a13,  // Light Orange
            0xab511f, 0xb56427, 0xbf7730, 0xd0853a,
            0xe19344, 0xeda04e, 0xf9ad58, 0xfcb75c,
            0xffc160, 0xffc671, 0xffcb83, 0xffcb83
        };

        public static readonly int[] PALPalette =
        {
            0x000000, 0x1c1c1c, 0x393939, 0x595959,  // Grey
            0x797979, 0x929292, 0xababab, 0xbcbcbc,
            0xcdcdcd, 0xd9d9d9, 0xe6e6e6, 0xececec,
            0xf2f2f2, 0xf8f8f8, 0xffffff, 0xffffff,

            0x263001, 0x243803, 0x234005, 0x51541b,  // Orange Green
            0x806931, 0x978135, 0xaf993a, 0xc2a73e,
            0xd5b543, 0xdbc03d, 0xe1cb38, 0xe2d836,
            0xe3e534, 0xeff258, 0xfbff7d, 0xfbff7d,

            0x263001, 0x243803, 0x234005, 0x51541b,  // Orange Green
            0x806931, 0x978135, 0xaf993a, 0xc2a73e,
            0xd5b543, 0xdbc03d, 0xe1cb38, 0xe2d836,
            0xe3e534, 0xeff258, 0xfbff7d, 0xfbff7d,

            0x401a02, 0x581f05, 0x702408, 0x8d3a13,  // Light Orange
            0xab511f, 0xb56427, 0xbf7730, 0xd0853a,
            0xe19344, 0xeda04e, 0xf9ad58, 0xfcb75c,
            0xffc160, 0xffc671, 0xffcb83, 0xffcb83,

            0x391701, 0x5e2304, 0x833008, 0xa54716,  // Gold
            0xc85f24, 0xe37820, 0xff911d, 0xffab1d,
            0xffc51d, 0xffce34, 0xffd84c, 0xffe651,
            0xfff456, 0xfff977, 0xffff98, 0xffff98,

            0x451904, 0x721e11, 0x9f241e, 0xb33a20,  // Orange
            0xc85122, 0xe36920, 0xff811e, 0xff8c25,
            0xff982c, 0xffae38, 0xffc545, 0xffc559,
            0xffc66d, 0xffd587, 0xffe4a1, 0xffe4a1,

            0x4a1704, 0x7e1a0d, 0xb21d17, 0xc82119,  // Red Orange
            0xdf251c, 0xec3b38, 0xfa5255, 0xfc6161,
            0xff706e, 0xff7f7e, 0xff8f8f, 0xff9d9e,
            0xffabad, 0xffb9bd, 0xffc7ce, 0xffc7ce,

            0x050568, 0x3b136d, 0x712272, 0x8b2a8c,  // Pink
            0xa532a6, 0xb938ba, 0xcd3ecf, 0xdb47dd,
            0xea51eb, 0xf45ff5, 0xfe6dff, 0xfe7afd,
            0xff87fb, 0xff95fd, 0xffa4ff, 0xffa4ff,

            0x280479, 0x400984, 0x590f90, 0x70249d,  // Purple
            0x8839aa, 0xa441c3, 0xc04adc, 0xd054ed,
            0xe05eff, 0xe96dff, 0xf27cff, 0xf88aff,
            0xff98ff, 0xfea1ff, 0xfeabff, 0xfeabff,

            0x051e81, 0x0626a5, 0x082fca, 0x263dd4,  // Blue1
            0x444cde, 0x4f5aee, 0x5a68ff, 0x6575ff,
            0x7183ff, 0x8091ff, 0x90a0ff, 0x97a9ff,
            0x9fb2ff, 0xafbeff, 0xc0cbff, 0xc0cbff,

            0x0c048b, 0x2218a0, 0x382db5, 0x483ec7,  // Blue2
            0x584fda, 0x6159ec, 0x6b64ff, 0x7a74ff,
            0x8a84ff, 0x918eff, 0x9998ff, 0xa5a3ff,
            0xb1aeff, 0xb8b8ff, 0xc0c2ff, 0xc0c2ff,

            0x1d295a, 0x1d3876, 0x1d4892, 0x1c5cac,  // Light Blue
            0x1c71c6, 0x3286cf, 0x489bd9, 0x4ea8ec,
            0x55b6ff, 0x70c7ff, 0x8cd8ff, 0x93dbff,
            0x9bdfff, 0xafe4ff, 0xc3e9ff, 0xc3e9ff,

            0x2f4302, 0x395202, 0x446103, 0x417a12,  // Turquoise
            0x3e9421, 0x4a9f2e, 0x57ab3b, 0x5cbd55,
            0x61d070, 0x69e27a, 0x72f584, 0x7cfa8d,
            0x87ff97, 0x9affa6, 0xadffb6, 0xadffb6,

            0x0a4108, 0x0d540a, 0x10680d, 0x137d0f,  // Green Blue
            0x169212, 0x19a514, 0x1cb917, 0x1ec919,
            0x21d91b, 0x47e42d, 0x6ef040, 0x78f74d,
            0x83ff5b, 0x9aff7a, 0xb2ff9a, 0xb2ff9a,

            0x04410b, 0x05530e, 0x066611, 0x077714,  // Green
            0x088817, 0x099b1a, 0x0baf1d, 0x48c41f,
            0x86d922, 0x8fe924, 0x99f927, 0xa8fc41,
            0xb7ff5b, 0xc9ff6e, 0xdcff81, 0xdcff81,

            0x02350f, 0x073f15, 0x0c4a1c, 0x2d5f1e,  // Yellow Green
            0x4f7420, 0x598324, 0x649228, 0x82a12e,
            0xa1b034, 0xa9c13a, 0xb2d241, 0xc4d945,
            0xd6e149, 0xe4f04e, 0xf2ff53, 0xf2ff53
        };
    }
}
