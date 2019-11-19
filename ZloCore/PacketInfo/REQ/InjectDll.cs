using System;
using System.Collections.Generic;
using System.Text;
using Zlo.Extras;

namespace Zlo.PacketInfo.REQ
{
    internal class InjectDll : BaseRequestPacket
    {
        public InjectDll(ZloBFGame game, string path)
        {
            Game = game;
            DllPath = path;
        }

        public override ZloPacketId PacketId => ZloPacketId.InjectDll;
        public override bool IsRespondable => false;

        public ZloBFGame Game { get; }
        public string DllPath { get; }

        public override void SerializeCustom(List<byte> bytes)
        {
            bytes.Add((byte)Game);
            bytes.AddRange(DllPath.QBitConv());
        }
    }
}
