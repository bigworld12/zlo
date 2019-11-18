using System;
using System.Collections.Generic;
using System.Text;
using Zlo.Extras;

namespace Zlo.PacketInfo.REQ
{
    internal class RunGame : BaseRequestPacket
    {
        public RunnableGame Game { get; }

        public override ZloPacketId PacketId => ZloPacketId.RunGame;
    }
}
