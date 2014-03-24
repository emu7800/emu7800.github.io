/*
 * HostDirectXFullscreen
 * 
 * A DirectX 9 based host.
 * 
 * Copyright © 2008 Mike Murphy
 * 
 */
using EMU7800.Core;

namespace EMU7800.Win.DirectX
{
    public class HostDirectXFullscreen : HostDirectX
    {
        public HostDirectXFullscreen(MachineBase m, ILogger logger) : base(m, logger, true)
        {
        }
    }
}
