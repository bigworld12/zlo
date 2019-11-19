using System;
using System.Collections.Generic;
using System.Text;
using Zlo.Extras;

namespace Zlo.PacketInfo.REQ
{
    internal class SubServerList : BaseServerList
    {
        public SubServerList(ZloBFGame game) : base(game)
        {
        }
        public override ServerListMode Mode => ServerListMode.Subscribe;
    }
}
