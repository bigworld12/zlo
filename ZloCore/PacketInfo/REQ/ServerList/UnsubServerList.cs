using System;
using System.Collections.Generic;
using System.Text;
using Zlo.Extras;

namespace Zlo.PacketInfo.REQ
{
    internal class UnsubServerList : BaseServerList
    {
        public UnsubServerList(ZloGame game) : base(game)
        {
        }

        public override ServerListMode Mode => ServerListMode.Unsubscribe;
    }
}
