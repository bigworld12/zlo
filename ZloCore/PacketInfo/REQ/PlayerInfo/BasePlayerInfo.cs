using System;
using System.Collections.Generic;
using System.Text;
using Zlo.Extras;

namespace Zlo.PacketInfo.REQ
{
    internal abstract class BasePlayerInfoRequest : BaseRequestPacket
    {
        protected BasePlayerInfoRequest(ZloBFGame game)
        {
            Game = game;
        }

        public override ZloPacketId PacketId => ZloPacketId.Player_Info;
        public abstract PlayerInfoMode PlayerInfoMode { get; }
        public ZloBFGame Game { get; }
        public override void SerializeCustom(List<byte> bytes)
        {
            bytes.Add((byte)PlayerInfoMode);
            bytes.Add((byte)Game);
        }
    }
}
