using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Zlo.Extras;

namespace Zlo.PacketInfo.RESP
{
    internal class Stats : BaseResponsePacket
    {
        public override ZloPacketId PacketId => ZloPacketId.Stats;

        public ZloBFGame Game { get; private set; }
        public Dictionary<string, float> Values { get; private set; }
        public override void DeserializeCustom(byte[] packetData, BinaryReader br)
        {
            Game = (ZloBFGame)br.ReadByte();
            var count = br.ReadZUInt16();
            Values = new Dictionary<string, float>(count);
            for (ushort i = 0; i < count; i++)
            {
                var statname = br.ReadZString();
                var statvalue = br.ReadZFloat();

                Values[statname] = statvalue;
            }
        }
    }
}
