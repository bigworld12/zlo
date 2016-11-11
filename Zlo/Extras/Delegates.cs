using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zlo.Extras
{
    public delegate void API_StatsReceivedEventHandler(ZloGame Game , List<API_Stat> List);
    public delegate void API_ItemsReceivedEventHandler(ZloGame Game , List<API_Item> List);
    public delegate void API_UserInfoReceivedEventHandler(uint UserID , string UserName);
    public delegate void API_ErrorOccuredEventHandler(Exception Error , string CustomMessage);
    public delegate void API_DisconnectedEventHandler(DisconnectionReasons Reason);
    public delegate void API_GameStateReceivedEventHandler(ZloGame game , string type , string message);

    public delegate void API_ConnectionStateChanged(bool IsConnectedToZloClient);

    public delegate void API_BF3ServerEventHandler(uint id , API_BF3ServerBase server);
    public delegate void API_BF4ServerEventHandler(uint id , API_BF4ServerBase server);
    public delegate void API_BFHServerEventHandler(uint id , API_BFHServerBase server);
     
    public delegate void API_APIVersionReceivedEventHandler(Version Current , Version Latest , bool IsNeedUpdate , string DownloadAdress);
}
