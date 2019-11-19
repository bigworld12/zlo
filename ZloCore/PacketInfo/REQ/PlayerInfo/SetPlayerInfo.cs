using System;
using System.Collections.Generic;
using System.Text;
using Zlo.Extras;

namespace Zlo.PacketInfo.REQ
{
    internal class SetPlayerInfo : BasePlayerInfoRequest
    {       
        public SetPlayerInfo(ZloBFGame game, ushort dogTagBasic, ushort dogTagAdv, string clanTag) : base(game)
        {
            DogTagBasic = dogTagBasic;
            DogTagAdvanced = dogTagAdv;
            ClanTag = clanTag;
        }
        public override PlayerInfoMode PlayerInfoMode => PlayerInfoMode.Set;
        public override bool IsRespondable => false;

        public ushort DogTagBasic { get; }
        public ushort DogTagAdvanced { get; }
        public string ClanTag { get; }
        public override void SerializeCustom(List<byte> bytes)
        {
            base.SerializeCustom(bytes);
            bytes.AddRange(DogTagAdvanced.QBitConv());
            bytes.AddRange(DogTagBasic.QBitConv());
            bytes.AddRange(ClanTag.QBitConv());
        }
    }
}
