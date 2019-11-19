using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Zlo.Extras;

namespace Zlo.PacketInfo.RESP
{
    internal class RunGame : BaseResponsePacket
    {
        public override ZloPacketId PacketId => ZloPacketId.RunGame;
        public GameRunResult Result { get; private set; }
        public override void DeserializeCustom(byte[] packetData, BinaryReader br)
        {
            Result = (GameRunResult)br.ReadByte();
        }
    }
}
