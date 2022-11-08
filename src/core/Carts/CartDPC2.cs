namespace EMU7800.Core
{
    /// <summary>
    /// Another DPC cart supporting the bankswitching scheme of the Harmony cart.
    /// There are six 4K program banks, a 4K display bank, 1K frequency table and the DPC chip.
    /// For complete details on the DPC chip see David P. Crane's United States Patent Number 4,644,495.
    /// </summary>
    public sealed class CartDPC2 : Cart
    {
        // DPC chip access is mapped to $1000 - $1080 ($1000 - $103F is read port, $1040 - $107F is write port).

        const ushort
            DisplayBaseAddr     = 0x0c00,
            FrequencyBaseAddr   = DisplayBaseAddr + 0x1000;

        const uint
            InitialRandomNumber = 0x2b435044; // "DPC+"

        const int
            MinimumSize         = 0x1000 * 6 + 0x1000 + 0x400 + 0xff;

        ushort _bankBaseAddr;
        readonly byte[] _ram = new byte[0x2000];
        readonly byte[] _tops = new byte[8];
        readonly byte[] _bots = new byte[8];
        readonly ushort[] _counters = new ushort[8];
        readonly uint[] _fractionalCounters = new uint[8];
        readonly byte[] _fractionalIncrements = new byte[8];
        readonly byte[] _parameter = new byte[8];
        readonly uint[] _musicCounters = new uint[3];
        readonly uint[] _musicFrequencies = new uint[3];
        readonly ushort[] _musicWaveforms = new ushort[3];
        byte _parameterPointer;
        bool _fastFetch, _ldaImmediate;
        ulong _lastSystemClock;
        double _fractionalClocks;
        uint _randomNumber;

        int Bank
        {
            set { _bankBaseAddr = (ushort)(value * 0x1000); }
        }

        #region IDevice Members

        public override void Reset()
        {
            _lastSystemClock = 3 * M.CPU.Clock;
            _fractionalClocks = 0.0;

            for (var i = 0; i < _ram.Length; i++)
            {
                _ram[i] = 0;
            }
            for (var i = 0; i < 0x1400; i++)
            {
                _ram[DisplayBaseAddr + i] = ROM[0x6000 + i];
            }
            for (var i = 0; i < 8; i++)
            {
                _tops[i] = 0;
                _bots[i] = 0;
                _counters[i] = 0;
                _fractionalIncrements[i] = 0;
                _fractionalCounters[i] = 0;
            }
            for (var i = 0; i < 3; i++)
            {
                _musicWaveforms[i] = 0;
            }

            _randomNumber = InitialRandomNumber;

            Bank = 5;
        }

        public override byte this[ushort addr]
        {
            get
            {
                addr &= 0x0fff;

                var peekVal = ROM[_bankBaseAddr + addr];

                if (_fastFetch && _ldaImmediate)
                {
                    if (peekVal < 0x0028)
                    {
                        addr = peekVal;
                    }
                }
                _ldaImmediate = false;

                if (addr < 0x0028)
                {
                    return ReadReg(addr);
                }

                UpdateBank(addr);

                if (_fastFetch)
                    _ldaImmediate = peekVal == 0xa9;

                return peekVal;
            }
            set
            {
                addr &= 0x0fff;
                if (addr >= 0x0028 && addr < 0x0080)
                {
                    WriteReg(addr, value);
                }
                else
                {
                    UpdateBank(addr);
                }
            }
        }

        #endregion

        public CartDPC2(byte[] romBytes)
        {
            if (romBytes.Length > 29 * 1024)
                System.Diagnostics.Debugger.Break();
            LoadRom(romBytes, MinimumSize);
            Bank = 5;
        }

        void UpdateBank(ushort addr)
        {
            switch (addr)
            {
                case 0x0ff6:
                    Bank = 0;
                    break;
                case 0x0ff7:
                    Bank = 1;
                    break;
                case 0x0ff8:
                    Bank = 2;
                    break;
                case 0x0ff9:
                    Bank = 3;
                    break;
                case 0x0ffa:
                    Bank = 4;
                    break;
                case 0x0ffb:
                    Bank = 5;
                    break;
            }
        }

        byte ReadReg(ushort addr)
        {
            byte result = 0;

            var i = addr & 0x07;
            var fn = (addr >> 3) & 0x07;

            switch (fn)
            {
                case 0x00:
                    switch (i)
                    {
                        case 0x00: // RANDOM0NEXT
                            NextRandomNumber();
                            result = (byte)_randomNumber;
                            break;
                        case 0x01: // RANDOM0PRIOR
                            PrevRandomNumber();
                            result = (byte)_randomNumber;
                            break;
                        case 0x02: // RANDOM1
                            result = (byte)(_randomNumber >> 8);
                            break;
                        case 0x03: // RANDOM2
                            result = (byte)(_randomNumber >> 16);
                            break;
                        case 0x04: // RANDOM3
                            result = (byte)(_randomNumber >> 24);
                            break;
                        case 0x05: // AMPLITUDE
                            UpdateMusicModeDataFetchers();
                            var amp = _ram[DisplayBaseAddr + (_musicWaveforms[0] << 5) + (_musicCounters[0] >> 27)]
                                    + _ram[DisplayBaseAddr + (_musicWaveforms[1] << 5) + (_musicCounters[1] >> 27)]
                                    + _ram[DisplayBaseAddr + (_musicWaveforms[2] << 5) + (_musicCounters[2] >> 27)];
                            result = (byte)amp;
                            break;
                    }
                    break;
                case 0x01: // DFxDATA
                    result = _ram[DisplayBaseAddr + _counters[i]];
                    _counters[i]++;
                    _counters[i] &= 0x0fff;
                    break;
                case 0x02: // DFxDATAW
                    result = (byte)(_ram[DisplayBaseAddr + _counters[i]] & GetFlag(i));
                    _counters[i]++;
                    _counters[i] &= 0x0fff;
                    break;
                case 0x03: // DFxFRACDATA
                    result = _ram[DisplayBaseAddr + _fractionalCounters[i] >> 8];
                    _fractionalCounters[i] = (_fractionalCounters[i] + _fractionalIncrements[i]) & 0xfffff;
                    break;
                case 0x04:
                    switch (i)
                    {
                        case 0x00: // DF0FLAG
                        case 0x01: // DF1FLAG
                        case 0x02: // DF2FLAG
                        case 0x03: // DF3FLAG
                            result = GetFlag(i);
                            break;
                    }
                    break;
                case 0x05:
                    break;
                case 0x06:
                    break;
                case 0x07:
                    break;
            }

            return result;
        }

        void WriteReg(ushort addr, byte val)
        {
            var i = addr & 0x07;
            var fn = (addr >> 3) & 0x0f;

            switch (fn)
            {
                case 0x00: // DFxFRACLOW
                    _fractionalCounters[i] = _fractionalCounters[i] | (uint)(val << 8);
                    break;
                case 0x01: // DFxFRACINC
                    _fractionalCounters[i] = (uint)((val & 0xf) << 16) | _fractionalCounters[i];
                    break;
                case 0x02: // DFxFRACINC - fractional increment amount
                    _fractionalIncrements[i] = val;
                    _fractionalCounters[i] &= 0xfff00;
                    break;
                case 0x03: // DFxTOP
                    _tops[i] = val;
                    break;
                case 0x04: // DFxBOT
                    _bots[i] = val;
                    break;
                case 0x05: // DFxLOW - data pointer low byte
                    _counters[i] &= 0xf00;
                    _counters[i] |= val;
                    break;
                case 0x06: // control registers
                    switch (i)
                    {
                        case 0x00: // FASTFETCH - turns on LDA #<DFxDATA mode if value is 0
                            _fastFetch = val == 0;
                            break;
                        case 0x01: // PARAMETER - set parameter used by CALLFUNCTION
                            if (_parameterPointer < 8)
                                _parameter[_parameterPointer++] = val;
                            break;
                        case 0x02: // CALLFUNCTION
                            switch (val)
                            {
                                case 0: // parameter pointer reset
                                    _parameterPointer = 0;
                                    break;
                                case 1: // copy rom to fetcher
                                    var romaddr = (_parameter[1] << 8) + _parameter[0];
                                    for (var j = 0; j < _parameter[3]; j++)
                                    {
                                        _ram[DisplayBaseAddr + _counters[_parameter[2] & 0x7] + j] = ROM[romaddr + i];
                                    }
                                    _parameterPointer = 0;
                                    break;
                                case 2: // copy value to fetcher
                                    for (var j = 0; j < _parameter[3]; j++)
                                    {
                                        _ram[DisplayBaseAddr + _counters[_parameter[2]] + j] = _parameter[0];
                                    }
                                    _parameterPointer = 0;
                                    break;
                            }
                            break;
                        case 0x03:
                        case 0x04:
                            break;
                        case 0x05: // WAVEFORM0
                        case 0x06: // WAVEFORM1
                        case 0x07: // WAVEFORM2
                            _musicWaveforms[i - 5] = (ushort)(val & 0x7f);
                            break;
                    }
                    break;
                case 0x07: // DFxPUSH
                    _counters[i]--;
                    _counters[i] &= 0x0fff;
                    _ram[DisplayBaseAddr + _counters[i]] = val;
                    break;
                case 0x08: // DFxHI
                    _counters[i] = (ushort)(((val & 0xf) << 8) | (_counters[i] & 0xff));
                    break;
                case 0x09:
                    switch (i)
                    {
                        case 0x00: // RRESET
                            _randomNumber = InitialRandomNumber;
                            break;
                        case 0x01: // RWRITE0
                            _randomNumber &= 0xffffff00;
                            _randomNumber |= val;
                            break;
                        case 0x02: // RWRITE1
                            _randomNumber &= 0xffff00ff;
                            _randomNumber |= (byte)(val << 8);
                            break;
                        case 0x03: // RWRITE2
                            _randomNumber &= 0xff00ffff;
                            _randomNumber |= (byte)(val << 16);
                            break;
                        case 0x04: // RWRITE3
                            _randomNumber &= 0x00ffffff;
                            _randomNumber |= (byte)(val << 24);
                            break;
                        case 0x05: // NOTE0
                        case 0x06: // NOTE1
                        case 0x07: // NOTE2
                            var ri = FrequencyBaseAddr + (val << 2);
                            _musicFrequencies[i - 5] =
                               (uint)(_ram[ri]
                                   | (_ram[ri + 1] << 8)
                                   | (_ram[ri + 2] << 16)
                                   | (_ram[ri + 3] << 24));
                            break;
                    }
                    break;
                case 0x0a: // DFxWRITE
                    _ram[DisplayBaseAddr + _counters[i]] = val;
                    _counters[i]++;
                    _counters[i] &= 0x0fff;
                    break;
            }
        }

        void NextRandomNumber()
        {
            // 32-bit LFSR
            var a1 = (uint)((_randomNumber & (1 << 10)) != 0 ? 0x10adab1e : 0);
            var a2 = _randomNumber >> 11;
            var a3 = _randomNumber << 21;
            _randomNumber = a1 ^ (a2 | a3);
        }

        void PrevRandomNumber()
        {
            // 32-bit LFSR reversed
            var a1 = (uint)(_randomNumber & (1 << 31));
            var a2 = _randomNumber << 11;
            var a3 = _randomNumber >> 21;
            var a4 = (0x10adab1e ^ _randomNumber) << 11;
            var a5 = (0x10adab1e ^ _randomNumber) >> 21;
            _randomNumber = a1 != 0 ? (a4 | a5) : (a2 | a3);
        }

        void UpdateMusicModeDataFetchers()
        {
            var sysClockDelta = 3 * M.CPU.Clock - _lastSystemClock;
            _lastSystemClock = 3 * M.CPU.Clock;

            var OSCclocks = ((20000.0 * sysClockDelta) / 1193191.66666667) + _fractionalClocks;

            var wholeClocks = (int)OSCclocks;
            _fractionalClocks = OSCclocks - wholeClocks;
            if (wholeClocks <= 0)
            {
                return;
            }

            for (var i = 0; i < 3; i++)
            {
                _musicCounters[i] += _musicFrequencies[i];
            }
        }

        byte GetFlag(int i)
        {
            var a1 = (_tops[i] - (_counters[i] & 0x00ff)) & 0xff;
            var a2 = _tops[i] - _bots[i];
            var flag = (byte)(a1 > a2 ? 0xff : 0x00);
            return flag;
        }

        #region Serialization Members

        public CartDPC2(DeserializationContext input) : base(input)
        {
            input.CheckVersion(1);
            LoadRom(input.ReadExpectedBytes(MinimumSize), MinimumSize);
        }

        public override void GetObjectData(SerializationContext output)
        {
            base.GetObjectData(output);

            output.WriteVersion(1);
            output.Write(ROM);
        }

        #endregion
    }
}
