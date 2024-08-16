/*
 * Machine7800.cs
 *
 * The realization of a 7800 machine.
 *
 * Copyright © 2003-2005 Mike Murphy
 *
 */
using System;

namespace EMU7800.Core;

public class Machine7800 : MachineBase
{
    #region Fields

    protected Maria Maria { get; }
    protected RAM6116 RAM0 { get; }
    protected RAM6116 RAM1 { get; }
    protected Bios7800 BIOS { get; }

    #endregion

    public void SwapInBIOS()
    {
        if (BIOS != Bios7800.Default)
        {
            Mem.Map((ushort)(0x10000 - BIOS.Size), BIOS.Size, BIOS);
        }
    }

    public void SwapOutBIOS()
    {
        if (BIOS != Bios7800.Default)
        {
            Mem.Map((ushort)(0x10000 - BIOS.Size), BIOS.Size, Cart);
        }
    }

    public override void Reset()
    {
        base.Reset();
        SwapInBIOS();
        Cart.Reset();
        Maria.Reset();
        PIA.Reset();
        CPU.Reset();
    }

    public override void ComputeNextFrame(FrameBuffer frameBuffer)
    {
        base.ComputeNextFrame(frameBuffer);

        AssertDebug(!CPU.Jammed);
        AssertDebug(CPU.RunClocks <= 0 && CPU.RunClocks % CPU.RunClocksMultiple == 0);
        AssertDebug((CPU.Clock + (ulong)(CPU.RunClocks / CPU.RunClocksMultiple)) % (114 * (ulong)FrameBuffer.Scanlines) == 0);

        ulong startOfScanlineCpuClock = 0;

        Maria.StartFrame();
        Cart.StartFrame();
        for (var i = 0; i < FrameBuffer.Scanlines && !CPU.Jammed; i++)
        {
            AssertDebug(CPU.RunClocks <= 0 && CPU.RunClocks % CPU.RunClocksMultiple == 0);
            var newStartOfScanlineCpuClock = CPU.Clock + (ulong)(CPU.RunClocks / CPU.RunClocksMultiple);

            AssertDebug(startOfScanlineCpuClock == 0 || newStartOfScanlineCpuClock == startOfScanlineCpuClock + 114);
            startOfScanlineCpuClock = newStartOfScanlineCpuClock;

            CPU.RunClocks += 7 * CPU.RunClocksMultiple;
            var remainingRunClocks = (114 - 7) * CPU.RunClocksMultiple;

            CPU.Execute();
            if (CPU.Jammed)
                break;
            if (CPU.EmulatorPreemptRequest)
            {
                Maria.DoDMAProcessing();
                var remainingCpuClocks = 114 - (CPU.Clock - startOfScanlineCpuClock);
                CPU.Clock += remainingCpuClocks;
                CPU.RunClocks = 0;
                continue;
            }

            var dmaClocks = Maria.DoDMAProcessing();

            // CHEAT: Ace of Aces: Title screen has a single scanline flicker without this. Maria DMA clock counting probably not 100% accurate.
            if (i == 203 && FrameBuffer.Scanlines == 262 /*NTSC*/ || i == 228 && FrameBuffer.Scanlines == 312 /*PAL*/)
                if (dmaClocks == 152 && remainingRunClocks == 428 && CPU.RunClocks is -4 or -8)
                    dmaClocks -= 4;

            // Unsure exactly what to do if Maria DMA processing extends past the current scanline.
            // For now, throw away half remaining until we are within the current scanline.
            // KLAX initialization starts DMA without initializing the DLL data structure.
            // Maria processing then runs away causing an invalid CPU opcode to be executed that jams the machine.
            // So Maria must give up at some point, but not clear exactly how.
            // Anyway, this makes KLAX work without causing breakage elsewhere.
            while (CPU.RunClocks + remainingRunClocks < dmaClocks)
            {
                dmaClocks >>= 1;
            }

            // Assume the CPU waits until the next div4 boundary to proceed after DMA processing.
            if ((dmaClocks & 3) != 0)
            {
                dmaClocks += 4;
                dmaClocks -= dmaClocks & 3;
            }

            CPU.Clock += (ulong)(dmaClocks / CPU.RunClocksMultiple);
            CPU.RunClocks -= dmaClocks;

            CPU.RunClocks += remainingRunClocks;

            CPU.Execute();
            if (CPU.Jammed)
                break;
            if (CPU.EmulatorPreemptRequest)
            {
                var remainingCpuClocks = 114 - (CPU.Clock - startOfScanlineCpuClock);
                CPU.Clock += remainingCpuClocks;
                CPU.RunClocks = 0;
            }
        }
        Cart.EndFrame();
        Maria.EndFrame();
    }

    public Machine7800(Cart cart, Bios7800 bios, ILogger logger, int scanlines, int startl, int fHZ, int sRate, ReadOnlyMemory<uint> p)
        : base(logger, scanlines, startl, fHZ, sRate, p, 320)
    {
        Mem = new AddressSpace(this, 16, 6);  // 7800: 16bit, 64byte pages

        CPU = new M6502(this, 4);

        Maria = new Maria(this, scanlines);
        Mem.Map(0x0000, 0x0040, Maria);
        Mem.Map(0x0100, 0x0040, Maria);
        Mem.Map(0x0200, 0x0040, Maria);
        Mem.Map(0x0300, 0x0040, Maria);

        PIA = new PIA(this);
        Mem.Map(0x0280, 0x0080, PIA);
        Mem.Map(0x0480, 0x0080, PIA);
        Mem.Map(0x0580, 0x0080, PIA);

        RAM0 = new RAM6116();
        RAM1 = new RAM6116();
        Mem.Map(0x1800, 0x0800, RAM0);
        Mem.Map(0x2000, 0x0800, RAM1);

        Mem.Map(0x0040, 0x00c0, RAM1);
        Mem.Map(0x0140, 0x00c0, RAM1);
        Mem.Map(0x2040, 0x00c0, RAM1);
        Mem.Map(0x2140, 0x00c0, RAM1);

        Mem.Map(0x2800, 0x0800, RAM1);
        Mem.Map(0x3000, 0x0800, RAM1);
        Mem.Map(0x3800, 0x0800, RAM1);

        BIOS = bios;
        Cart = cart;

        if (!Mem.Map(Cart))
        {
            Mem.Map(0x4000, 0xc000, Cart);
        }
    }

    #region Serialization Members

    public Machine7800(DeserializationContext input, ReadOnlyMemory<uint> palette, int scanlines) : base(input, palette)
    {
        input.CheckVersion(1);

        Mem = input.ReadAddressSpace(this, 16, 6);  // 7800: 16bit, 64byte pages

        CPU = input.ReadM6502(this, 4);

        Maria = input.ReadMaria(this, scanlines);
        Mem.Map(0x0000, 0x0040, Maria);
        Mem.Map(0x0100, 0x0040, Maria);
        Mem.Map(0x0200, 0x0040, Maria);
        Mem.Map(0x0300, 0x0040, Maria);

        PIA = input.ReadPIA(this);
        Mem.Map(0x0280, 0x0080, PIA);
        Mem.Map(0x0480, 0x0080, PIA);
        Mem.Map(0x0580, 0x0080, PIA);

        RAM0 = input.ReadRAM6116();
        RAM1 = input.ReadRAM6116();
        Mem.Map(0x1800, 0x0800, RAM0);
        Mem.Map(0x2000, 0x0800, RAM1);

        Mem.Map(0x0040, 0x00c0, RAM1);
        Mem.Map(0x0140, 0x00c0, RAM1);
        Mem.Map(0x2040, 0x00c0, RAM1);
        Mem.Map(0x2140, 0x00c0, RAM1);

        Mem.Map(0x2800, 0x0800, RAM1);
        Mem.Map(0x3000, 0x0800, RAM1);
        Mem.Map(0x3800, 0x0800, RAM1);

        BIOS = input.ReadOptionalBios7800();
        Cart = input.ReadCart(this);

        if (!Mem.Map(Cart))
        {
            Mem.Map(0x4000, 0xc000, Cart);
        }
    }

    public override void GetObjectData(SerializationContext output)
    {
        base.GetObjectData(output);

        output.WriteVersion(1);
        output.Write(Mem);
        output.Write(CPU);
        output.Write(Maria);
        output.Write(PIA);
        output.Write(RAM0);
        output.Write(RAM1);
        output.WriteOptional(BIOS);
        output.Write(Cart);
    }

    #endregion

    #region Helpers

    [System.Diagnostics.Conditional("DEBUG")]
    static void AssertDebug(bool cond)
    {
        if (!cond)
            System.Diagnostics.Debugger.Break();
    }

    #endregion
}