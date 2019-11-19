using System;
using System.Collections.Generic;
using System.Text;
using Zlo.Extras;

namespace Zlo.PacketInfo.REQ
{
    internal class Empty : BaseRequestPacket
    {
        public Empty(ZloPacketId packetId)
        {
            PacketId = packetId;
        }

        public override ZloPacketId PacketId { get; }
    }
}
