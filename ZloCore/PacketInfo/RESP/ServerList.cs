using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Zlo.Extras;

namespace Zlo.PacketInfo.RESP
{
    internal class ServerList : BaseResponsePacket
    {
        public override ZloPacketId PacketId => ZloPacketId.Server_List;
        public ServerListEvent Event { get; private set; }
        public ZloBFGame Game { get; private set; }
        public uint ServerId { get; private set; }

        public byte[] DataBuffer { get; private set; }
        public override void DeserializeCustom(byte[] packetData, BinaryReader br)
        {
            Event = (ServerListEvent)br.ReadByte();
            Game = (ZloBFGame)br.ReadByte();
            ServerId = br.ReadZUInt32();

            var pos = (int)br.BaseStream.Position;
            DataBuffer = new byte[packetData.Length - pos];
            Buffer.BlockCopy(packetData, pos, DataBuffer, 0, DataBuffer.Length);
        }
    }
}
