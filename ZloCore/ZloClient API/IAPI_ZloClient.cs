using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Zlo.Extras;

namespace Zlo
{
    public interface IAPI_ZloClient : IDisposable
    {
        Version CurrentApiVersion { get; }
        uint CurrentPlayerID { get; }
        string CurrentPlayerName { get; }

        bool IsConnectedToZCLient { get; }
        bool IsEnableDiscordRPC { get; set; }

        TimeSpan ReconnectInterval { get; set; }
        ZloGame ActiveServerListener { get; }

        JObject BF3_Stats { get; }
        JObject BF4_Stats { get; }


        event API_StatsReceivedEventHandler StatsReceived;
        event API_ItemsReceivedEventHandler ItemsReceived;
        event API_UserInfoReceivedEventHandler UserInfoReceived;
        event API_ClanDogTagsReceivedEventHandler ClanDogTagsReceived;
        event API_ErrorOccuredEventHandler ErrorOccured;
        event API_GameStateReceivedEventHandler GameStateReceived;
        event API_ConnectionStateChanged ConnectionStateChanged;

        bool Connect();
        List<string> GetDllsList(ZloGame game);
        void JoinOnlineServer(OnlinePlayModes playmode, uint serverid = 0);
        void JoinOfflineGame(OfflinePlayModes playmode);
        void JoinOnlineGameWithPassWord(OnlinePlayModes playmode, uint serverid, string password);
        void GetUserInfo();
        void GetStats(ZloGame game);
        void GetItems(ZloGame game);
        void SubToServerList(ZloGame game);
        void UnSubServerList();
        void GetClanDogTags();
        void SetClanDogTags(ushort? dt_advanced = null, ushort? dt_basic = null, string clantag = ";");

        API_BF3ServersListBase BF3Servers { get; }
        API_BF4ServersListBase BF4Servers { get; }
        API_BFHServersListBase BFHServers { get; }
    }
}
