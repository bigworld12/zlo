using System;
using System.Collections.Generic;
using System.Text;
using Zlo.Extras;

namespace Zlo.PacketInfo.REQ
{
    internal class Ping : BaseRequestPacket
    {
        public override ZloPacketId PacketId => ZloPacketId.Ping;
    }
}
