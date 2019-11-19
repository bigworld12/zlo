using System;
using System.Collections.Generic;
using System.Text;
using Zlo.Extras;

namespace Zlo.PacketInfo.REQ
{
    internal class GetPlayerInfo : BasePlayerInfoRequest
    {
        public GetPlayerInfo(ZloBFGame game) : base(game)
        {
        }
        public override PlayerInfoMode PlayerInfoMode => PlayerInfoMode.Get;
    }
}
