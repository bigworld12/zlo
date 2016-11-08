using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zlo.Extras
{
    public delegate void StatsReceivedEventHandler(ZloGame Game , List<Stat> List);
    public delegate void ItemsReceivedEventHandler(ZloGame Game , List<Item> List);
    public delegate void UserInfoReceivedEventHandler(uint UserID , string UserName);
    public delegate void ErrorOccuredEventHandler(Exception Error , string CustomMessage);
    public delegate void DisconnectedEventHandler(DisconnectionReasons Reason);
    public delegate void GameStateReceivedEventHandler(ZloGame game , string type , string message);


    public delegate void BF3ServerEventHandler(uint id , BF3ServerBase server);
    public delegate void BF4ServerEventHandler(uint id , BF4ServerBase server);
    public delegate void BFHServerEventHandler(uint id , BFHServerBase server);

    public delegate void APIVersionReceivedEventHandler(Version Current , Version Latest , bool IsNeedUpdate , string DownloadAdress);
}
