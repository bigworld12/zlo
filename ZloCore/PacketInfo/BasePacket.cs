namespace Zlo.PacketInfo
{    
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Zlo.Extras;

    internal abstract class BasePacket
    {
        public abstract ZloPacketId PacketId { get; }
    }
}
