using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Zlo.Extras;

namespace Zlo.PacketInfo.RESP
{
    internal class RunnableGameList : BaseResponsePacket
    {
        public override ZloPacketId PacketId => ZloPacketId.RunnableGameList;
        public bool Isx64 { get; private set; }
        public RunnableGame[] Values { get; private set; }
        public override void DeserializeCustom(byte[] packetData, BinaryReader br)
        {
            Isx64 = br.ReadBoolean();
            uint count = br.ReadZUInt32();
            Values = new RunnableGame[count];
            for (int i = 0; i < count; i++)
            {
                var runName = br.ReadZString();
                var ZName = br.ReadZString();
                var name = br.ReadZString();

                Values[i] = new RunnableGame(name, runName, ZName);
            }
        }
    }
}
