using System;
using System.Collections.Generic;
using System.Text;
using Zlo.Extras;

namespace Zlo.PacketInfo.REQ
{
    internal class RunGame : BaseRequestPacket
    {
        public RunGame(string runName, string commandLineParameters)
        {
            RunName = runName;
            CommandLineParameters = commandLineParameters;
        }

        public string RunName { get; }
        public string CommandLineParameters { get; }

        public override ZloPacketId PacketId => ZloPacketId.RunGame;
        public override void SerializeCustom(List<byte> bytes)
        {
            bytes.AddRange(RunName.QBitConv());
            bytes.AddRange(CommandLineParameters.QBitConv());
        }
    }
}
