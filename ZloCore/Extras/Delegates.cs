using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zlo.Extras
{
    public delegate void API_StatsReceivedEventHandler(ZloBFGame Game, Dictionary<string, float> List);
    public delegate void API_ItemsReceivedEventHandler(ZloBFGame Game, Dictionary<string, API_Item> List);
    public delegate void API_UserInfoReceivedEventHandler(uint UserID, string UserName);
    public delegate void API_ErrorOccuredEventHandler(Exception Error, string CustomMessage);
    public delegate void API_GameStateReceivedEventHandler(ZloBFGame game, string type, string message);
    public delegate void API_ClanDogTagsReceivedEventHandler(ZloBFGame game, ushort dogtag_advanced, ushort dogtag_basic, string clanTag);

    public delegate void API_ConnectionStateChangedEventHandler(bool IsConnectedToZloClient);

    public delegate void API_BFServerEventHandler<T>(IBFServerList<T> list,uint id, T server, ServerChangeTypes changeType) where T : class,IBFServerBase;

    public delegate void API_RunnableGameListReceivedEventHandler();

    public delegate void API_GameRunResultReceivedEventHandler(RunnableGame zname, GameRunResult result);
}
