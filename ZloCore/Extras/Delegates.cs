using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zlo.Extras
{
    public delegate void API_StatsReceivedEventHandler(ZloGame Game , Dictionary<string , float> List);
    public delegate void API_ItemsReceivedEventHandler(ZloGame Game , Dictionary<string , API_Item> List);
    public delegate void API_UserInfoReceivedEventHandler(uint UserID , string UserName);
    public delegate void API_ErrorOccuredEventHandler(Exception Error , string CustomMessage);
    public delegate void API_GameStateReceivedEventHandler(ZloGame game , string type , string message);
    public delegate void API_ClanDogTagsReceivedEventHandler(ZloGame game, ushort dogtag_advanced, ushort dogtag_basic, string clanTag);

    public delegate void API_ConnectionStateChanged(bool IsConnectedToZloClient);

    public delegate void API_BF3ServerEventHandler(uint id , API_BF3ServerBase server);
    public delegate void API_BF4ServerEventHandler(uint id , API_BF4ServerBase server);
    public delegate void API_BFHServerEventHandler(uint id , API_BFHServerBase server);     
}
