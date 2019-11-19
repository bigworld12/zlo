using System;
using System.Collections.Generic;
using System.Text;
using Zlo.Extras;

namespace Zlo.PacketInfo.REQ
{
    internal class RunGame : BaseRequestPacket
    {    
        public RunGame(RunnableGame game, string commandLineParameters)
        {
            Game = game;
            CommandLineParameters = commandLineParameters;
        }

        public RunnableGame Game { get; }
        public string CommandLineParameters { get; }

        public override ZloPacketId PacketId => ZloPacketId.RunGame;
        public override void SerializeCustom(List<byte> bytes)
        {
            bytes.AddRange(Game.RunName.QBitConv());
            bytes.AddRange(CommandLineParameters.QBitConv());
        }
    }
}
