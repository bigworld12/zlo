using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Zlo.Extras;

namespace Zlo.PacketInfo.RESP
{
    internal class Items : BaseResponsePacket
    {
        public override ZloPacketId PacketId => ZloPacketId.Items;

        public ZloBFGame Game { get; private set; }
        public Dictionary<string, API_Item> Values { get; private set; }
        public override void DeserializeCustom(byte[] packetData, BinaryReader br)
        {
            Game = (ZloBFGame)br.ReadByte();
            var count = br.ReadZUInt16();
            Values = new Dictionary<string, API_Item>(count);
            for (ushort i = 0; i < count; i++)
            {
                var statname = br.ReadZString();
                var statvalue = br.ReadByte();

                Values[statname] = new API_Item(statname, statvalue == 1 ? true : false, Game);
            }
        }
    }
}
