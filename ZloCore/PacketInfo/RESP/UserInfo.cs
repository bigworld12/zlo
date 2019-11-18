namespace Zlo.PacketInfo.RESP
{
    using System.Collections.Generic;
    using System.IO;
    using Zlo.Extras;

    internal class UserInfo : BaseResponsePacket
    {
        public uint Id { get; private set; }
        public string Name { get; private set; }

        public override ZloPacketId PacketId => ZloPacketId.User_Info;

        public override void DeserializeCustom(BinaryReader from)
        {
            Id = from.ReadZUInt32();
            Name = from.ReadZString();
        }
    }
}
