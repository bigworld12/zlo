using System;
using System.Collections.Generic;
using System.Text;
using Zlo.Extras;

namespace Zlo.PacketInfo.REQ
{
    internal abstract class BaseServerList : BaseRequestPacket
    {
        protected BaseServerList(ZloBFGame game)
        {
            Game = game;
        }
        public override ZloPacketId PacketId => ZloPacketId.Server_List;
        public ZloBFGame Game { get; }
        public abstract ServerListMode Mode { get; }
        public override bool IsRespondable => false;

        public override void SerializeCustom(List<byte> bytes)
        {
            base.SerializeCustom(bytes);
            //mode then game
            bytes.Add((byte)Mode);
            bytes.Add((byte)Game);
        }
    }
}
