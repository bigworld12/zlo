using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zlo.Extras
{
    public enum ServerChangeTypes
    {
        Add,
        Update,
        Remove
    }
    internal enum ServerListEvent : byte
    {
        /// <summary>
        /// Add or edit server
        /// </summary>
        ServerChange = 0,
        /// <summary>
        /// player list changed
        /// </summary>
        PlayerListChange = 1,
        /// <summary>
        /// Server removed
        /// </summary>
        ServerRemove = 2
    }
    public enum GameRunResult : byte
    {
        Successful = 0,
        NotFound = 1,
        Error = 2
    }
    internal enum PlayerInfoMode : byte
    {
        Get = 0,
        Set = 1
    }
    internal enum StatsOrItems : byte
    {
        Stats,
        Items
    }
    internal enum ServerListMode : byte
    {
        Subscribe = 0,
        Unsubscribe = 1
    }
    public enum ZloPacketId : byte
    {
        Ping = 0,
        User_Info = 1,
        Player_Info = 2,
        Server_List = 3,
        Stats = 4,
        Items = 5,
        Packs = 6,
        InjectDll = 7,
        RunnableGameList = 8,
        RunGame = 9
    }
    public enum ZloBFGame : byte
    {
        BF_3 = 0,
        BF_4 = 1,
        BF_HardLine = 2,
        None = 255
    }
    public enum BF3_COOP_LEVELS
    {
        [Description("Operation Exodus")]
        COOP_007,
        [Description("Fire from the Sky")]
        COOP_006,
        [Description("Exfiltration")]
        COOP_009,
        [Description("Hit and Run")]
        COOP_002,
        [Description("Drop 'Em Like Liquid")]
        COOP_003,
        [Description("The Eleventh Hour")]
        COOP_010
    }
    public enum COOP_Difficulty
    {
        Easy,
        Normal,
        Hard
    }
    [Obsolete("This enum shouldn't be used anymore, use the event ConnectionStateChanged")]
    public enum DisconnectionReasons : byte
    {
        UnKnown = 0,
        PingFail = 1,
        ServerRaised = 2,
        ZClientNotOpen = 3
    }
    public enum OnlinePlayModes
    {
        BF3_Multi_Player,

        BF4_Multi_Player,
        BF4_Spectator,
        BF4_Commander,

        BFH_Multi_Player,
        BFH_Spectator,
        BFH_Commander,

    }
    public enum OfflinePlayModes
    {
        BF3_Single_Player,
        BF4_Single_Player,
        BFH_Single_Player,
        BF4_Test_Range
    }
    public enum HostCoopModes
    {
        BF3_Host_Coop,
    }
}
