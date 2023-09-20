/*
 * M6502DASM.cs
 *
 * Provides disassembly services.
 *
 * Copyright © 2003, 2004 Mike Murphy
 *
 */
using System;
using System.Text;

namespace EMU7800.Core;

public static class M6502DASM
{
    // Instruction Mnemonics
    enum M : uint
    {
        ADC = 1, AND, ASL,
        BIT, BCC, BCS, BEQ, BMI, BNE, BPL, BRK, BVC, BVS,
        CLC, CLD, CLI, CLV, CMP, CPX, CPY,
        DEC, DEX, DEY,
        EOR,
        INC, INX, INY,
        JMP, JSR,
        LDA, LDX, LDY, LSR,
        NOP,
        ORA,
        PLA, PLP, PHA, PHP,
        ROL, ROR, RTI, RTS,
        SEC, SEI, STA, SBC, SED, STX, STY,
        TAX, TAY, TSX, TXA, TXS, TYA,

        // Illegal/undefined opcodes
        isb,
        kil,
        lax,
        rla,
        sax,
        top
    }

    // Addressing Modes
    enum A : uint
    {
        REL,    // Relative: $aa (branch instructions only)
        ZPG,    // Zero Page: $aa
        ZPX,    // Zero Page Indexed X: $aa,X
        ZPY,    // Zero Page Indexed Y: $aa,Y
        ABS,    // Absolute: $aaaa
        ABX,    // Absolute Indexed X: $aaaa,X
        ABY,    // Absolute Indexed Y: $aaaa,Y
        IDX,    // Indexed Indirect: ($aa,X)
        IDY,    // Indirect Indexed: ($aa),Y
        IND,    // Indirect Absolute: ($aaaa) (JMP only)
        IMM,    // Immediate: #aa
        IMP,    // Implied
        ACC     // Accumulator
    }

    static readonly M[] MnemonicMatrix = [
//        0      1      2      3      4      5      6      7      8      9      A  B      C      D      E      F
/*0*/ M.BRK, M.ORA, M.kil,     0,     0, M.ORA, M.ASL,     0, M.PHP, M.ORA, M.ASL, 0, M.top, M.ORA, M.ASL,     0,/*0*/
/*1*/ M.BPL, M.ORA, M.kil,     0,     0, M.ORA, M.ASL,     0, M.CLC, M.ORA,     0, 0, M.top, M.ORA, M.ASL,     0,/*1*/
/*2*/ M.JSR, M.AND, M.kil,     0, M.BIT, M.AND, M.ROL,     0, M.PLP, M.AND, M.ROL, 0, M.BIT, M.AND, M.ROL,     0,/*2*/
/*3*/ M.BMI, M.AND, M.kil,     0,     0, M.AND, M.ROL,     0, M.SEC, M.AND,     0, 0, M.top, M.AND, M.ROL, M.rla,/*3*/
/*4*/ M.RTI, M.EOR, M.kil,     0,     0, M.EOR, M.LSR,     0, M.PHA, M.EOR, M.LSR, 0, M.JMP, M.EOR, M.LSR,     0,/*4*/
/*5*/ M.BVC, M.EOR, M.kil,     0,     0, M.EOR, M.LSR,     0, M.CLI, M.EOR,     0, 0, M.top, M.EOR, M.LSR,     0,/*5*/
/*6*/ M.RTS, M.ADC, M.kil,     0,     0, M.ADC, M.ROR,     0, M.PLA, M.ADC, M.ROR, 0, M.JMP, M.ADC, M.ROR,     0,/*6*/
/*7*/ M.BVS, M.ADC, M.kil,     0,     0, M.ADC, M.ROR,     0, M.SEI, M.ADC,     0, 0, M.top, M.ADC, M.ROR,     0,/*7*/
/*8*/     0, M.STA,     0, M.sax, M.STY, M.STA, M.STX, M.sax, M.DEY,     0, M.TXA, 0, M.STY, M.STA, M.STX, M.sax,/*8*/
/*9*/ M.BCC, M.STA, M.kil,     0, M.STY, M.STA, M.STX, M.sax, M.TYA, M.STA, M.TXS, 0, M.top, M.STA,     0,     0,/*9*/
/*A*/ M.LDY, M.LDA, M.LDX, M.lax, M.LDY, M.LDA, M.LDX, M.lax, M.TAY, M.LDA, M.TAX, 0, M.LDY, M.LDA, M.LDX, M.lax,/*A*/
/*B*/ M.BCS, M.LDA, M.kil, M.lax, M.LDY, M.LDA, M.LDX, M.lax, M.CLV, M.LDA, M.TSX, 0, M.LDY, M.LDA, M.LDX, M.lax,/*B*/
/*C*/ M.CPY, M.CMP,     0,     0, M.CPY, M.CMP, M.DEC,     0, M.INY, M.CMP, M.DEX, 0, M.CPY, M.CMP, M.DEC,     0,/*C*/
/*D*/ M.BNE, M.CMP, M.kil,     0,     0, M.CMP, M.DEC,     0, M.CLD, M.CMP,     0, 0, M.top, M.CMP, M.DEC,     0,/*D*/
/*E*/ M.CPX, M.SBC,     0,     0, M.CPX, M.SBC, M.INC,     0, M.INX, M.SBC, M.NOP, 0, M.CPX, M.SBC, M.INC, M.isb,/*E*/
/*F*/ M.BEQ, M.SBC, M.kil,     0,     0, M.SBC, M.INC,     0, M.SED, M.SBC,     0, 0, M.top, M.SBC, M.INC, M.isb /*F*/
];

    static readonly A[] AddressingModeMatrix = [
//        0      1      2      3      4      5      6      7      8      9      A  B      C      D      E      F
/*0*/ A.IMP, A.IDX, A.IMP,     0,     0, A.ZPG, A.ZPG,     0, A.IMP, A.IMM, A.ACC, 0, A.ABS, A.ABS, A.ABS,     0,/*0*/
/*1*/ A.REL, A.IDY, A.IMP,     0,     0, A.ZPG, A.ZPG,     0, A.IMP, A.ABY,     0, 0, A.ABS, A.ABX, A.ABX,     0,/*1*/
/*2*/ A.ABS, A.IDX, A.IMP,     0, A.ZPG, A.ZPG, A.ZPG,     0, A.IMP, A.IMM, A.ACC, 0, A.ABS, A.ABS, A.ABS,     0,/*2*/
/*3*/ A.REL, A.IDY, A.IMP,     0,     0, A.ZPG, A.ZPG,     0, A.IMP, A.ABY,     0, 0, A.ABS, A.ABX, A.ABX, A.ABX,/*3*/
/*4*/ A.IMP, A.IDY, A.IMP,     0,     0, A.ZPG, A.ZPG,     0, A.IMP, A.IMM, A.ACC, 0, A.ABS, A.ABS, A.ABS,     0,/*4*/
/*5*/ A.REL, A.IDY, A.IMP,     0,     0, A.ZPG, A.ZPG,     0, A.IMP, A.ABY,     0, 0, A.ABS, A.ABX, A.ABX,     0,/*5*/
/*6*/ A.IMP, A.IDX, A.IMP,     0,     0, A.ZPG, A.ZPG,     0, A.IMP, A.IMM, A.ACC, 0, A.IND, A.ABS, A.ABS,     0,/*6*/
/*7*/ A.REL, A.IDY, A.IMP,     0,     0, A.ZPX, A.ZPX,     0, A.IMP, A.ABY,     0, 0, A.ABS, A.ABX, A.ABX,     0,/*7*/
/*8*/     0, A.IDY,     0, A.IDX, A.ZPG, A.ZPG, A.ZPG, A.ZPG, A.IMP,     0, A.IMP, 0, A.ABS, A.ABS, A.ABS, A.ABS,/*8*/
/*9*/ A.REL, A.IDY, A.IMP,     0, A.ZPX, A.ZPX, A.ZPY, A.ZPY, A.IMP, A.ABY, A.IMP, 0, A.ABS, A.ABX,     0,     0,/*9*/
/*A*/ A.IMM, A.IND, A.IMM, A.IDX, A.ZPG, A.ZPG, A.ZPG, A.ZPX, A.IMP, A.IMM, A.IMP, 0, A.ABS, A.ABS, A.ABS, A.ABS,/*A*/
/*B*/ A.REL, A.IDY, A.IMP, A.IDY, A.ZPX, A.ZPX, A.ZPY, A.ZPY, A.IMP, A.ABY, A.IMP, 0, A.ABX, A.ABX, A.ABY, A.ABY,/*B*/
/*C*/ A.IMM, A.IDX,     0,     0, A.ZPG, A.ZPG, A.ZPG,     0, A.IMP, A.IMM, A.IMP, 0, A.ABS, A.ABS, A.ABS,     0,/*C*/
/*D*/ A.REL, A.IDY, A.IMP,     0,     0, A.ZPX, A.ZPX,     0, A.IMP, A.ABY,     0, 0, A.ABS, A.ABX, A.ABX,     0,/*D*/
/*E*/ A.IMM, A.IDX,     0,     0, A.ZPG, A.ZPG, A.ZPG,     0, A.IMP, A.IMM, A.IMP, 0, A.ABS, A.ABS, A.ABS, A.ABS,/*E*/
/*F*/ A.REL, A.IDY, A.IMP,     0,     0, A.ZPX, A.ZPX,     0, A.IMP, A.ABY,     0, 0, A.ABS, A.ABX, A.ABX, A.ABX /*F*/
];

    public static string GetRegisters(M6502 cpu)
    {
        var dSB = new StringBuilder();
        dSB.Append($"PC:{cpu.PC:x4} A:{cpu.A:x2} X:{cpu.X:x2} Y:{cpu.Y:x2} S:{cpu.S:x2} P:");

        const string flags = "nv0bdizcNV1BDIZC";

        for (var i = 0; i < 8; i++)
        {
            dSB.Append(((cpu.P & (1 << (7 - i))) == 0) ? flags[i] : flags[i + 8]);
        }
        return dSB.ToString();
    }

    public static string Disassemble(AddressSpace addrSpace, ushort atAddr, ushort untilAddr)
    {
        var dSB = new StringBuilder();
        var dPC = atAddr;
        while (atAddr < untilAddr)
        {
            dSB.Append($"{dPC:x4}: ");
            var len = GetInstructionLength(addrSpace, dPC);
            for (var i = 0; i < 3; i++)
            {
                if (i < len)
                {
                    dSB.Append($"{addrSpace[atAddr++]:x2} ");
                }
                else
                {
                    dSB.Append("   ");
                }
            }
            dSB.Append($"{RenderOpCode(addrSpace, dPC),-15}{Environment.NewLine}");
            dPC += (ushort)len;
        }
        if (dSB.Length > 0)
        {
            dSB.Length--;  // Trim trailing newline
        }
        return dSB.ToString();
    }

    public static string MemDump(AddressSpace addrSpace, ushort atAddr, ushort untilAddr)
    {
        var dSB = new StringBuilder();
        var len = untilAddr - atAddr;
        while (len-- >= 0)
        {
            dSB.Append($"{atAddr:x4}: ");
            for (var i = 0; i < 8; i++)
            {
                dSB.Append($"{addrSpace[atAddr++]:x2} ");
                if (i == 3)
                {
                    dSB.Append(' ');
                }
            }
            dSB.Append('\n');
        }
        if (dSB.Length > 0)
        {
            dSB.Length--;  // Trim trailing newline
        }
        return dSB.ToString();
    }

    public static string RenderOpCode(AddressSpace addrSpace, ushort PC)
    {
        var num_operands = GetInstructionLength(addrSpace, PC) - 1;
        var PC1 = (ushort)(PC + 1);
        string addrmodeStr = (AddressingModeMatrix[addrSpace[PC]]) switch
        {
            A.REL          => $"${(ushort)(PC + (sbyte)addrSpace[PC1] + 2):x4}",
            A.ZPG or A.ABS => RenderEA(addrSpace, PC1, num_operands),
            A.ZPX or A.ABX => RenderEA(addrSpace, PC1, num_operands) + ",X",
            A.ZPY or A.ABY => RenderEA(addrSpace, PC1, num_operands) + ",Y",
            A.IDX          => "(" + RenderEA(addrSpace, PC1, num_operands) + ",X)",
            A.IDY          => "(" + RenderEA(addrSpace, PC1, num_operands) + "),Y",
            A.IND          => "(" + RenderEA(addrSpace, PC1, num_operands) + ")",
            A.IMM          => "#" + RenderEA(addrSpace, PC1, num_operands),
            _              => string.Empty,// a.IMP, a.ACC
        };
        return $"{MnemonicMatrix[addrSpace[PC]]} {addrmodeStr}";
    }

    static int GetInstructionLength(AddressSpace addrSpace, ushort PC)
        => (AddressingModeMatrix[addrSpace[PC]]) switch
        {
            A.ACC or A.IMP => 1,
            A.REL or A.ZPG or A.ZPX or A.ZPY or A.IDX or A.IDY or A.IMM => 2,
            _ => 3,
        };

    static string RenderEA(AddressSpace addrSpace, ushort PC, int bytes)
    {
        var lsb = addrSpace[PC];
        var msb = (bytes == 2) ? addrSpace[(ushort)(PC + 1)] : (byte)0;
        var ea = (ushort)(lsb | (msb << 8));
        return bytes == 1 ? $"${ea:x2}" : $"${ea:x4}";
    }
}