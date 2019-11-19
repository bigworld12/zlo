using System;
using System.Collections.Generic;
using System.Text;
using Zlo.Extras;

namespace Zlo.PacketInfo.RESP
{
    internal class Empty : BaseResponsePacket
    {
        public Empty(ZloPacketId packetId)
        {
            PacketId = packetId;
        }
        public override ZloPacketId PacketId { get; }
    }
}
