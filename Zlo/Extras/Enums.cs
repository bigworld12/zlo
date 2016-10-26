using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zlo.Extras
{
    public enum ZloRequest : byte
    {
        Ping = 0,
        User_Info = 1,
        Player_Info = 2,       
        Stats = 4,
        Items = 5
    }
    public enum ZloGame : byte
    {
        BF_3 = 0,
        BF_4 = 1,
        BF_HardLine = 2
    }
    public enum DisconnectionReasons : byte
    {
        UnKnown = 0,
        PingFail = 1,
        ServerRaised = 2
    }
    public enum OnlinePlayModes
    {
        BF3_Multi_Player,
        BF3_COOP,

        BF4_Multi_Player,
        BF4_Spectator,
        BF4_Commander,
        BF4_COOP,

        BFH_Multi_Player
    }
    public enum OfflinePlayModes
    {
        BF3_Single_Player,
        BF4_Single_Player,
        BFH_Single_Player,
        BF4_Test_Range
    }
}
