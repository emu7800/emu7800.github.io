/*
 * YM2151.cs
 *
 * Emulation of the Yamaha YM2151 audio chip. (Incomplete)
 *
 *
 */
namespace EMU7800.Core;

#pragma warning disable IDE0052, IDE0060, CA1822, CS0649, CS0414  // disable while under construction

public sealed class YM2151
{
    #region Fields

    public static readonly YM2151 Default = new(MachineBase.Default);

    readonly MachineBase M;

    #region YM2151 State

    struct YM2151Operator
    {
        public uint Phase; // accumulated operator phase

        // channel specific data; operator number 0 contains channel specific data
        public uint fb_shift;    // feedback shift value for operators 0 in each channel
        public int fb_out_curr;  // operator feedback value (used only by operators 0)
        public int fb_out_prev;  // previous feedback value (used only by operators 0)
        public uint kc;          // channel KC (copied to all operators)
        public uint kc_i;        // just for speedup
        public uint pms;         // channel PMS
        public uint ams;         // channel AMS

        public uint TL;          // Total attenuation level
        public int Volume;       // Current envelope attenuation level

        public bool Key;         // false=last key was off, true=last key was on
        public byte State;       // Envelope state: 4-attack(AR) 3-decay(D1R) 2-sustain(D2R) 1-release(RR) 0-off
    }

    readonly YM2151Operator[] _operators = new YM2151Operator[0x20];
    byte _lastReg0;

    byte _lfo_phase;       // accumulated LFO phase (0 to 255)
    uint _lfo_overflow;    // LFO generates new output when lfo_timer reaches this value
    uint _lfo_counter_add; // Step of LFO counter
    byte _lfo_wsel;        // LFO waveform (0-saw, 1-square, 2-triangle, 3-random noise)
    byte _amd;             // LFO Amplitude Modulation Depth
    sbyte _pmd;            // LFO Phase Modulation Depth

    byte _test;         // TEST register
    byte _ct;           // output control pins (bit1-CT2, bit0-CT1)

    byte _noise;        // noise enable/period register, bit 7 - noise enable (NE), bits 4-0 - noise period (NFRQ)
    uint _noise_rng;    // 17 bit noise shift register
    uint _noise_p;      // current noise phase
    uint _noise_f;      // current noise period

    byte _csmReq;       // CSM  KEY ON / KEY OFF sequence request

    byte _irqEnable;    // IRQ enable for timer B (bit 3) and timer A (bit 2); bit 7 - CSM mode (keyon to all slots, everytime timer A overflows)
    byte _status;       // chip status

    // ASG 980324
    static uint[] _timerAtime = new uint[0x400];
    static uint[] _timerBtime = new uint[0x100];
    uint _timerAindex, _timerBindex;
    ulong _timerA, _timerB;
    int irqlinestate;

    #endregion

    static uint[] _noiseTable = new uint[0x20];

    int _bufferIndex;

    #endregion

    public void Reset()
    {
        // TODO temporary workaround due to preview .NET releases
        if (_timerAtime == null) _timerAtime = new uint[0x400];
        if (_timerBtime == null) _timerBtime = new uint[0x100];
        if (_noiseTable == null) _noiseTable = new uint[0x20];

        for (var i = 0; i < 32; i++)
        {
            _operators[i].Volume = 0x3ff;
            _operators[i].kc_i = 768; // min kc_i value
        }
        _lfo_phase = 0;
        _lfo_wsel = 0;
        _pmd = 0;
        _amd = 0;
        _test = 0;
        _irqEnable = 0;
        _timerA = 0;
        _timerB = 0;
        _timerAindex = 0;
        _timerBindex = 0;
        _noise = 0;
        _noise_rng = 0;
        _noise_p = 0;
        _noise_f = _noiseTable[0];
        _csmReq = 0;
        _status = 0;
        WriteReg(0x1b, 0); // because of CT1, CT2 output pins
        WriteReg(0x18, 0); // set LFO freq
        for (var i = 0; i < 0x100; i++)
        {
            WriteReg((byte)i, 0);
        }
    }

    public void StartFrame()
    {
        CheckTimers();
    }

    public void EndFrame()
    {
        CheckTimers();
        RenderSamples(M.FrameBuffer.SoundBuffer.Length - _bufferIndex);
    }

    public byte Read(ushort _)
    {
        CheckTimers();
        return _status;
    }

    public void Update(ushort addr, byte data)
    {
        CheckTimers();
        switch (addr & 1)
        {
            case 0:
                _lastReg0 = data;
                break;
            default:
                WriteReg(_lastReg0, data);
                break;
        }
    }

    #region Constructors

    static YM2151()
    {
        CalculateTimersDeltas();
        CalculateNoisePeriodsTable();
    }

    public YM2151(MachineBase m)
    {
        M = m;
        Reset();
    }

    #endregion

    #region Serialization Members

    public YM2151(DeserializationContext input, MachineBase m) : this(m)
    {
    }

    public void GetObjectData(SerializationContext output)
    {
    }

    #endregion

    #region Helpers

    void WriteReg(byte reg0, byte data)
    {
        var opi = (reg0 & 0x07) << 2 | (reg0 & 0x18) >> 3;
        switch (reg0 & 0xe0)
        {
            case 0x00:
                switch (reg0)
                {
                    case 0x01: // LFO reset (bit 1), Test Register (remaining bits)
                        _test = data;
                        if ((data & 2) != 0)
                        {
                            _lfo_phase = 0;
                        }
                        break;
                    case 0x08:
                        var op = (data & 7) * 4;
                        if ((data & 0x08) != 0) KeyOn(op);     else KeyOff(op);     // M1
                        if ((data & 0x20) != 0) KeyOn(op + 1); else KeyOff(op + 1); // M2
                        if ((data & 0x10) != 0) KeyOn(op + 2); else KeyOff(op + 2); // C1
                        if ((data & 0x40) != 0) KeyOn(op + 3); else KeyOff(op + 3); // C2
                        break;
                    case 0x0f: // NE, NFRQ
                        _noise = data;
                        _noise_f = _noiseTable[data & 0x1f];
                        break;
                    case 0x10: // CLKA1 hi
                        _timerAindex = (_timerAindex & 0x003) | (uint)(data << 2);
                        break;
                    case 0x11: // CLKA2 lo
                        _timerAindex = (_timerAindex & 0x3fc) | (uint)(data & 3);
                        break;
                    case 0x12: // CLKB
                        _timerBindex = data;
                        break;
                    case 0x14: // CSM, irq flag reset, irq enable, timer start/stop
                        _irqEnable = data;      // bit 3-timer B, bit 2-timer A, bit 7 - CSM
                        if ((data & 0x10) != 0) // reset timer A irq flag
                        {
                            _status &= 0xfe;
                            IRQAoff();
                        }
                        if ((data & 0x20) != 0) // reset timer B irq flag
                        {
                            _status &= 0xfd;
                            IRQBoff();
                        }
                        if ((data & 0x02) != 0)
                        {
                            // load and start timer B if not already started
                            if (_timerB == 0)
                            {
                                _timerB = _timerBtime[_timerBindex] + M.CPU.Clock;
                            }
                        }
                        else
                        {
                            _timerB = 0; // stop timer B
                        }
                        if ((data & 0x01) != 0)
                        {
                            // load and start timer A if not already started
                            if (_timerA == 0)
                            {
                                _timerA = _timerAtime[_timerAindex] + M.CPU.Clock;
                            }
                        }
                        else
                        {
                            _timerA = 0; // stop timer A
                        }
                        break;
                    case 0x18: // LFO frequency
                        _lfo_overflow = (uint)((1 << ((15 - (data >> 4)) + 3)) * (1 << 10));
                        _lfo_counter_add = (uint)(0x10 + (data & 0x0f));
                        break;
                    case 0x19: // PMD (bit 7==1) or AMD (bit 7==0)
                        if ((data & 0x80) != 0)
                        {
                            _pmd = (sbyte)(data & 0x7f);
                        }
                        else
                        {
                            _amd = (byte)(data & 0x7f);
                        }
                        break;
                    case 0x1b: // CT2, CT1, LFO waveform
                        _ct = (byte)(data >> 6);
                        _lfo_wsel = (byte)(data & 3);
                        break;
                }
                break;
            case 0x20:
                break;
            case 0x40:
                break;
            case 0x60: // TL (7-bits)
                _operators[opi].TL = (uint)((data & 0x7f) << 3);
                break;
            case 0x80:
                break;
            case 0xa0:
                break;
            case 0xc0:
                break;
            case 0xe0:
                break;
        }
    }

    void RenderSamples(int count)
    {
        while (count > 0 && _bufferIndex < M.FrameBuffer.SoundBuffer.Length)
        {
            M.FrameBuffer.SoundBuffer.Span[_bufferIndex++] += 0;
            count--;
        }
    }

    void KeyOn(int op)
    {
        if (_operators[op].Key == false)
        {
            _operators[op].Phase = 0; // clear phase
            _operators[op].State = 4; // KEY ON = attack
            _operators[op].Volume += 0;
            // TODO:
            //    (~_operators[op].Volume *
            //               eg_inc[_operators[op].eg_sel_ar + ((eg_cnt >> _operators[op].eg_sh_ar) & 7)]) >> 4;
            if (_operators[op].Volume <= 0)
            {
                _operators[op].Volume = 0;
                _operators[op].State = 3;
            }
        }
        _operators[op].Key = true;
    }

    void KeyOff(int op)
    {
        if (_operators[op].Key)
        {
            _operators[op].Key = false;
            if (_operators[op].State > 1)
            {
                _operators[op].State = 1; // KEY OFF = release
            }
        }
    }

    void IRQAon()
    {
        var oldstate = irqlinestate;
        irqlinestate |= 1;
        if (oldstate == 0)
        {
            // raise irqhandler(1);
        }
    }

    void IRQBon()
    {
        var oldstate = irqlinestate;
        irqlinestate |= 2;
        if (oldstate == 0)
        {
            // raise irqhandler(1);
        }
    }

    void IRQAoff()
    {
        var oldstate = irqlinestate;
        irqlinestate &= ~1;
        if (oldstate == 1)
        {
            // raise irqhandler(0);
        }
    }

    void IRQBoff()
    {
        var oldstate = irqlinestate;
        irqlinestate &= ~2;
        if (oldstate == 2)
        {
            // raise irqhandler(0);
        }
    }

    void CheckTimers()
    {
        if (_timerA > 0)
        {
            if (M.CPU.Clock > _timerA)
            {
                _timerA = _timerAtime[_timerAindex] + M.CPU.Clock;
                if ((_irqEnable & 0x04) != 0)
                {
                    _status |= 1;
                    IRQAon();
                }
                if ((_irqEnable & 0x80) != 0)
                {
                    _csmReq = 2;
                }
            }
        }
        if (_timerB > 0)
        {
            if (M.CPU.Clock > _timerB)
            {
                _timerB = _timerBtime[_timerBindex] + M.CPU.Clock;
                if ((_irqEnable & 0x04) != 0)
                {
                    _status |= 2;
                    IRQBon();
                }
            }
        }
    }

    static void CalculateTimersDeltas()
    {
        // User's Manual pages 15, 16
        for (var i = 0; i < _timerAtime.Length; i++)
        {
            _timerAtime[i] = (uint)(64 * (1024 - i)); // * clock ticks / clock cycle?
        }
        for (var i = 0; i < 256; i++)
        {
            _timerBtime[i] = (uint)(1024 * (256 - i)); // * clock ticks / clock cycle?
        }
    }

    static void CalculateNoisePeriodsTable()
    {
        for (var i = 0; i < _noiseTable.Length; i++)
        {
            var j = i != 31 ? i : 30; // rate 30 and 31 are the same
            var s = (int)(65536.0 / (double)((32 - j) * 32.0)); // number of samples per one shift of the shift register
            _noiseTable[i] = (uint)(s * 64); // TODO: each cell still needs to be multiplied with: clock / 64.0 / sampfreq;
        }
    }

    #endregion
}
