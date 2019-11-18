﻿using System;
using System.Collections.Generic;
using System.Text;
using Zlo.Extras;

namespace Zlo.PacketInfo.REQ
{
    internal class GetStatsOrItems : BaseRequestPacket
    {
        public GetStatsOrItems(StatsOrItems statsOrItems, ZloGame game)
        {
            PacketId = statsOrItems == StatsOrItems.Stats ? ZloPacketId.Stats : ZloPacketId.Items;
            Game = game;
        }

        public override ZloPacketId PacketId { get; }
        public ZloGame Game { get; }        

        public override void SerializeCustom(List<byte> bytes)
        {
            base.SerializeCustom(bytes);
            bytes.Add((byte)Game);
        }
    }
}
