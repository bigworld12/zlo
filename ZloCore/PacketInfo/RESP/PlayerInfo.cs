namespace Zlo.PacketInfo.RESP
{
    using System.IO;
    using Zlo.Extras;
    internal class PlayerInfo : BaseResponsePacket
    {
        public override ZloPacketId PacketId => ZloPacketId.Player_Info;
        public ZloBFGame Game { get; private set; }
        public ushort DogTagBasic { get; private set; }
        public ushort DogTagAdvanced { get; private set; }
        public string ClanTag { get; private set; }
        public override void DeserializeCustom(byte[] packetData, BinaryReader br)
        {
            Game = (ZloBFGame)br.ReadByte();
            DogTagAdvanced = br.ReadZUInt16();
            DogTagBasic = br.ReadZUInt16();
            ClanTag = br.ReadZString();
        }
    }
}
