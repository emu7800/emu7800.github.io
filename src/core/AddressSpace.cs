/*
 * AddressSpace.cs
 *
 * The class representing the memory map or address space of a machine.
 *
 * Copyright © 2003, 2011 Mike Murphy
 *
 */
namespace EMU7800.Core
{
    public sealed class AddressSpace
    {
        public static readonly AddressSpace Default = new(MachineBase.Default, 16, 6);

        public MachineBase M { get; } = MachineBase.Default;

        readonly int AddrSpaceShift;
        readonly int AddrSpaceSize;
        readonly int AddrSpaceMask;

        readonly int PageShift;
        readonly int PageSize;

        readonly IDevice[] MemoryMap;

        IDevice Snooper = NullDevice.Default;

        public byte DataBusState { get; private set; }

        public override string ToString()
            => "EMU7800.Core.AddressSpace";

        public byte this[ushort addr]
        {
            get
            {
                // here DataBusState is just facilitating a dummy read to the snooper device
                // the read operation may have important side effects within the device
                DataBusState = Snooper[addr];
                var pageno = (addr & AddrSpaceMask) >> PageShift;
                var dev = MemoryMap[pageno];
                DataBusState = dev[addr];
                return DataBusState;
            }
            set
            {
                DataBusState = value;
                Snooper[addr] = DataBusState;
                var pageno = (addr & AddrSpaceMask) >> PageShift;
                var dev = MemoryMap[pageno];
                dev[addr] = DataBusState;
            }
        }

        public void Map(ushort basea, ushort size, IDevice device)
        {
            for (int addr = basea; addr < basea + size; addr += PageSize)
            {
                var pageno = (addr & AddrSpaceMask) >> PageShift;
                MemoryMap[pageno] = device;
            }

            LogDebug($"{this}: Mapped {device} to ${basea:x4}:${basea + size - 1:x4}");
        }

        public void Map(ushort basea, ushort size, Cart cart)
        {
            cart.Attach(M);
            var device = (IDevice)cart;
            if (cart.RequestSnooping)
            {
                Snooper = device;
            }
            Map(basea, size, device);
        }

        #region Constructors

        public AddressSpace(MachineBase m, int addrSpaceShift, int pageShift)
        {
            M = m;

            AddrSpaceShift = addrSpaceShift;
            AddrSpaceSize  = 1 << AddrSpaceShift;
            AddrSpaceMask = AddrSpaceSize - 1;

            PageShift = pageShift;
            PageSize = 1 << PageShift;

            MemoryMap = new IDevice[1 << addrSpaceShift >> PageShift];

            for (var pageno=0; pageno < MemoryMap.Length; pageno++)
            {
                MemoryMap[pageno] = NullDevice.Default;
            }
        }

        #endregion

        #region Serialization Members

        public AddressSpace(DeserializationContext input, MachineBase m, int addrSpaceShift, int pageShift) : this(m, addrSpaceShift, pageShift)
        {
            input.CheckVersion(1);
            DataBusState = input.ReadByte();
        }

        public void GetObjectData(SerializationContext output)
        {
            output.WriteVersion(1);
            output.Write(DataBusState);
        }

        #endregion

        #region Helpers

        [System.Diagnostics.Conditional("DEBUG")]
        void LogDebug(string message)
            => M.Logger.WriteLine(message);

        #endregion
    }
}