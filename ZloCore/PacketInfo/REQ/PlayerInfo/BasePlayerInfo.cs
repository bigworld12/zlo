using System;
using System.Collections.Generic;
using System.Text;
using Zlo.Extras;

namespace Zlo.PacketInfo.REQ
{
    internal abstract class BasePlayerInfoRequest : BaseRequestPacket
    {
        protected BasePlayerInfoRequest(ZloGame game)
        {
            Game = game;
        }

        public override ZloPacketId PacketId => ZloPacketId.Player_Info;
        public abstract PlayerInfoMode PlayerInfoMode { get; }
        public ZloGame Game { get; }
        public override void SerializeCustom(List<byte> bytes)
        {
            bytes.Add((byte)PlayerInfoMode);
            bytes.Add((byte)Game);
        }
    }
}
