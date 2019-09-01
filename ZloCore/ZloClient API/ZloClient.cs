using DiscordRPC;
using DiscordRPC.Logging;
using DiscordRPC.Message;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Zlo.Extras;
using static Zlo.Extentions.Helpers;

namespace Zlo
{
    /// <summary>
    /// the main controller for the api, please note : [ONLY CREATE A SINGLE INSTANCE OF THIS CLASS]
    /// </summary>
    public partial class API_ZloClient : IAPI_ZloClient
    {
        public Version CurrentApiVersion { get; } = new Version(16, 0, 0, 0);
        private static bool IsInitlaized = false;



        /// <summary>
        /// when zclient disconnects it will automatiically reconnect, use this variable to define how long it should take to reconnect each time
        /// default is 1 second
        /// <para>To disable auto reconnect, assign this to <see cref="TimeSpan.MaxValue"/></para>
        /// </summary>
        public TimeSpan ReconnectInterval { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// MUST BE INITIALIZED ONLY ONCE
        /// The API intializer (doesn't connect to zclient until you call <see cref="Connect"/>
        /// </summary>
        public API_ZloClient()
        {
            if (IsInitlaized)
            {
                return;
            }
            ErrorOccured += API_ZloClient_ErrorOccured;
            IsInitlaized = true;
            try
            {
                GameStateReceived += ZloClient_GameStateReceived;

                ListenerClient = new ZloTCPClient(this);

                InitializePipes();
                ListenerClient.ZloPacketReceived += ListenerClient_DataReceived;
                UserInfoReceived += ZloClient_UserInfoReceived;
                ClanDogTagsReceived += API_ZloClient_ClanDogTagsReceived;
            }
            catch (Exception ex)
            {
                ErrorOccured?.Invoke(ex, "Error When Initializing the client");
            }
        }



        private void API_ZloClient_ClanDogTagsReceived(ZloGame game, ushort dogtag1, ushort dogtag2, string clanTag)
        {
            ClanDogTagsPerGame[game] = new Tuple<ushort, ushort, string>(dogtag1, dogtag2, clanTag);
        }

        private void API_ZloClient_ErrorOccured(Exception Error, string CustomMessage)
        {
            Log.WriteLog($"[{CustomMessage}] : \n{Error.ToString()}");
        }

        private void ZloClient_GameStateReceived(ZloGame game, string type, string message)
        {
            //[BF_4] [StateChanged] State_Game State_ClaimReservation 14
            //= in-game
            //[BF_4] [StateChanged] State_GameLeaving State_ClaimReservation 0
            //= left game

            //[BF_3] [StateChanged] State_Game State_NA 14
            //= in-game
            //
            //no left game
            LatestGameState_Game = game;
            LatestGameState_Type = type;
            LatestGameState_Message = message;
            latestDate = DateTime.UtcNow;

            string trimmed = message.Trim(' ');
            var dllz = GetDllsList(game);
            if (dllz == null || game == ZloGame.None)
            {
                return;
            }
            switch (game)
            {
                case ZloGame.BF_3:
                    if (trimmed == $"State_Game State_NA {CurrentPlayerID}")
                    {
                        foreach (var item in dllz)
                        {
                            RequestDLLInject(game, item);
                        }
                    }
                    break;

                case ZloGame.BF_4:
                case ZloGame.BF_HardLine:
                    if (trimmed == $"State_Game State_ClaimReservation {CurrentPlayerID}")
                    {
                        foreach (var item in dllz)
                        {
                            RequestDLLInject(game, item);
                        }
                    }
                    break;
                default:
                    break;
            }

        }



        private void ZloClient_UserInfoReceived(uint UserID, string UserName)
        {
            CurrentPlayerID = UserID;
            CurrentPlayerName = UserName;
        }
        #region Properties
        /*
       connect to localhost:48486
       packet - 4byte size, 1byte type, payload[size]
       */




        ZloTCPClient ListenerClient { get; }

        System.Timers.Timer PingTimer;


        /// <summary>
        /// represents the latest player id that was received
        /// </summary>
        public uint CurrentPlayerID { get; private set; }

        /// <summary>
        /// represents the latest player name that was received
        /// </summary>
        public string CurrentPlayerName { get; private set; }
        private Dictionary<ZloGame, Tuple<ushort, ushort, string>> ClanDogTagsPerGame = new Dictionary<ZloGame, Tuple<ushort, ushort, string>>();
        private List<Request> RequestQueue = new List<Request>();
        #endregion

        #region API events
        /// <summary>
        /// Gets triggered after receiving user stats and passes the game and a list of stats
        /// </summary>    
        public event API_StatsReceivedEventHandler StatsReceived;

        internal void RaiseStatsReceived(ZloGame game, Dictionary<string, float> stats)
        {
            StatsReceived?.Invoke(game, stats);
        }

        /// <summary>
        /// Gets triggered after receiving user items and passes the game and a list of items
        /// </summary>    
        public event API_ItemsReceivedEventHandler ItemsReceived;

        internal void RaiseItemsReceived(ZloGame game, Dictionary<string, API_Item> stats)
        {
            ItemsReceived?.Invoke(game, stats);
        }
        /// <summary>
        /// Gets triggered after receiving user info and passes user id and name
        /// </summary>
        public event API_UserInfoReceivedEventHandler UserInfoReceived;


        internal void RaiseClanDogTagsReceived(ZloGame game, ushort dg1, ushort dg2, string ct)
        {
            ClanDogTagsReceived?.Invoke(game, dg1, dg2, ct);
        }
        /// <summary>
        /// gets triggered after receiving clan tags and dog tags [they are received together]
        /// </summary>
        public event API_ClanDogTagsReceivedEventHandler ClanDogTagsReceived;


        /// <summary>
        /// Gets triggered when an error occurs and passes the exception and a custom message from the dll
        /// </summary>
        public event API_ErrorOccuredEventHandler ErrorOccured;


        /// <summary>
        /// occurs when the game state changes (connecting to server/closing the server,etc..)
        /// </summary>
        public event API_GameStateReceivedEventHandler GameStateReceived;


        /// <summary>
        /// occurs when the api connects/disconnects from ZClient
        /// </summary>
        public event API_ConnectionStateChanged ConnectionStateChanged
        {
            add
            {
                ListenerClient.IsConnectedChanged += value;
            }
            remove
            {
                ListenerClient.IsConnectedChanged -= value;
            }
        }

        #endregion

        #region API Methods
        /// <summary>
        /// if this returns false, the reconnect timer will start automatically
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            if (!ConnectPipes())
                return false;
            PingTimer = new System.Timers.Timer(20 * 1000);
            PingTimer.Elapsed -= PingTimer_Elapsed;
            PingTimer.Elapsed += PingTimer_Elapsed;
            PingTimer.Start();
            if (ListenerClient.Connect())
            {
                GetUserInfo();
                if (IsEnableDiscordRPC)
                    StartDiscordRPC();
                return true;
            }
            else
            {
                ListenerClient.StartReconnectTimer();
                return false;
            }
        }

        public List<string> GetDllsList(ZloGame game)
        {
            if (game == ZloGame.None)
            {
                return null;
            }
            return Settings.CurrentSettings.InjectedDlls.GetDllsList(game);
        }
        internal void RequestDLLInject(ZloGame game, string dllPath)
        {
            /*             
pid 7
size
uint8 game
string full path to dll
             */
            var req = new Request()
            {
                WaitBeforePeriod = TimeSpan.Zero,
                IsRespondable = false,

                pid = 7
            };
            var toadd = Encoding.UTF8.GetBytes(dllPath);

            var size = BitConverter.GetBytes(toadd.Length + 2);
            Array.Reverse(size);

            var ar = new List<byte> { 7 };
            ar.AddRange(size);
            ar.Add((byte)game);
            ar.AddRange(toadd);
            ar.Add(0);

            req.data = ar.ToArray();
            AddToQueue(req);
        }


        /// <summary>
        /// this method is automatically called by the api each sucessfull connect,
        /// so you don't need to call it , just listen to the UserInfoReceived event or use the 
        /// CurrentPlayerName and CurrentPlayerID properties
        /// </summary>
        public void GetUserInfo()
        {
            SendRequest(ZloRequest.User_Info);
        }
        public void GetStats(ZloGame game)
        {
            SendRequest(ZloRequest.Stats, game);
        }
        public void GetItems(ZloGame game)
        {
            SendRequest(ZloRequest.Items, game);
        }

        public ZloGame ActiveServerListener
        {
            get => Settings.CurrentSettings.ActiveServerListener;
            private set { if (Settings.CurrentSettings.ActiveServerListener == value) return; Settings.CurrentSettings.ActiveServerListener = value; Settings.TrySave(); }
        }

        public void SubToServerList(ZloGame game)
        {
            
            //unsub first
            UnSubServerList();
            ActiveServerListener = game;
            //0 == subscribe            
            SendRequest(ZloRequest.Server_List, ActiveServerListener, 0);
            GetClanDogTags();
        }





        public void UnSubServerList()
        {
            if (ActiveServerListener == ZloGame.None)
            {
                return;
            }
            //1 == unsubscribe
            //first clear local cache

            //if (ActiveServerListener == ZloGame.BF_3)
            //    for (int i = BF3Servers.Count - 1; i >= 0; i--)
            //    {
            //        BF3Servers.RemoveAt(i);
            //    }
            //if (ActiveServerListener == ZloGame.BF_4)
            //    for (int i = BF4Servers.Count - 1; i >= 0; i--)
            //    {
            //        BF4Servers.RemoveAt(i);
            //    }
            //if (ActiveServerListener == ZloGame.BF_HardLine)
            //    for (int i = BFHServers.Count - 1; i >= 0; i--)
            //    {
            //        BFHServers.RemoveAt(i);
            //    }
            SendRequest(ZloRequest.Server_List, ActiveServerListener, 1);
            ActiveServerListener = ZloGame.None;
        }
        public void GetClanDogTags()
        {
            //bf3 not supported
            if (ActiveServerListener == ZloGame.None || ActiveServerListener == ZloGame.BF_3)
            {
                return;
            }
            SendRequest(ZloRequest.Player_Info, ActiveServerListener, 0);
        }

        private byte[] QBitConv(ushort s)
        {
            return BitConverter.GetBytes(s).Reverse().ToArray();
        }
        private byte[] QBitConv(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return new byte[1] { 0 };
            }
            List<byte> asci = Encoding.ASCII.GetBytes(s).ToList();
            //null termimated
            asci.Add(0);
            Console.WriteLine($"Converted string ({s}) to byte[] {string.Join(";", asci)}");
            return asci.ToArray();
        }

        /// <summary>
        /// <para>
        /// sets the clan tag of the current active game (the game you are listening to after calling SubToServerList)
        /// </para>
        /// clantag string can be set to ";" which will pass the old clantag,else it must not pass 4 characters, be (a-z) or (A-Z) or (0-9) or an empty string only
        /// <para>
        /// you can only set a clan tag after subscribing to a game
        /// </para>
        /// </summary>
        /// <param name="dt_advanced"></param>
        /// <param name="dt_basic"></param>
        /// <param name="clantag"></param>
        public void SetClanDogTags(ushort? dt_advanced = null, ushort? dt_basic = null, string clantag = ";")
        {
            if (ActiveServerListener == ZloGame.None || ActiveServerListener == ZloGame.BF_3)
            {
                return;
            }
            if (clantag != ";")
            {
                if (clantag.Length > 4 || !clantag.All(x => char.IsDigit(x) || (x >= 'a' && x <= 'z') || (x >= 'A' && x <= 'Z')))
                {
                    clantag = ";";
                }
            }

            //1 for set
            var final = new List<byte>
            {
                1
            };
            final.AddRange(dt_advanced.HasValue ? QBitConv(dt_advanced.Value) : QBitConv(ClanDogTagsPerGame[ActiveServerListener].Item1));
            final.AddRange(dt_basic.HasValue ? QBitConv(dt_basic.Value) : QBitConv(ClanDogTagsPerGame[ActiveServerListener].Item2));
            final.AddRange(clantag == ";" ? QBitConv(ClanDogTagsPerGame[ActiveServerListener].Item3) : QBitConv(clantag));

            SendRequest(ZloRequest.Player_Info, ActiveServerListener, final.ToArray());
        }
        #endregion

        #region API Properties
        private API_BF3ServersListBase m_BF3Servers;
        /// <summary>
        /// Acts as a listener to all events related to bf3 servers
        /// </summary>
        public API_BF3ServersListBase BF3Servers
        {
            get
            {
                if (m_BF3Servers == null)
                {
                    m_BF3Servers = new API_BF3ServersListBase(this);
                }
                return m_BF3Servers;
            }
        }

        private API_BF4ServersListBase m_BF4Servers;
        /// <summary>
        /// Acts as a listener to all events related to bf4 servers
        /// </summary>
        public API_BF4ServersListBase BF4Servers
        {
            get
            {
                if (m_BF4Servers == null)
                {
                    m_BF4Servers = new API_BF4ServersListBase(this);
                }
                return m_BF4Servers;
            }
        }

        private API_BFHServersListBase m_BFHServers;
        /// <summary>
        /// Acts as a listener to all events related to bf hardline servers
        /// </summary>
        public API_BFHServersListBase BFHServers
        {
            get
            {
                if (m_BFHServers == null)
                {
                    m_BFHServers = new API_BFHServersListBase(this);
                }
                return m_BFHServers;
            }
        }

        public bool IsConnectedToZCLient => ListenerClient.IsConnected;


        private JObject m_BF3_Stats;
        /// <summary>
        /// The instance gets changed everytime StatsReceived event gets raised
        /// </summary>
        public JObject BF3_Stats
        {
            get
            {
                if (m_BF3_Stats == null)
                {
                    m_BF3_Stats = JObject.Parse(GameData.BF3_stats_def);
                }
                return m_BF3_Stats;
            }
        }


        private JObject m_BF4_Stats;
        /// <summary>
        /// The instance gets changed everytime StatsReceived event gets raised
        /// </summary>
        public JObject BF4_Stats
        {
            get
            {
                if (m_BF4_Stats == null)
                {
                    m_BF4_Stats = JObject.Parse(GameData.BF4_stats_def);
                }
                return m_BF4_Stats;
            }
        }

        #endregion

        private void SendRequest(ZloRequest request, ZloGame game = ZloGame.None, params byte[] additionalPayloads)
        {
            Task.Run(() =>
            {
                if (request == ZloRequest.Items && game == ZloGame.BF_3) return;
                List<byte> final = new List<byte> { (byte)request };
                var req = new Request();
                var Payloads = new List<byte>();
                req.WaitBeforePeriod = TimeSpan.Zero;
                if (request == ZloRequest.Server_List)
                {
                    req.IsRespondable = false;
                }
                else
                {
                    req.IsRespondable = true;
                }
                switch (request)
                {
                    case ZloRequest.Ping:
                        {
                            break;
                        }
                    case ZloRequest.Server_List:
                        {
                            if (game == ZloGame.None)
                            {
                                return;
                            }
                            //additionalPayloads is subscribe or not

                            Payloads.AddRange(additionalPayloads);
                            Payloads.Add((byte)game);
                        }
                        break;
                    case ZloRequest.Player_Info:
                        {
                            //0 for get,1 for set
                            if (game == ZloGame.None)
                            {
                                return;
                            }
                            //action
                            Payloads.Add(additionalPayloads[0]);
                            //game
                            Payloads.Add((byte)game);
                            if (additionalPayloads[0] == 0)
                            {
                                req.IsRespondable = true;
                            }
                            else
                            {
                                req.IsRespondable = false;
                                //params
                                Payloads.AddRange(additionalPayloads.Skip(1));
                            }
                            //additionalPayloads is action [get { 0 }, set {1,ushort,ushort,string}]                          


                            break;
                        }
                    case ZloRequest.Stats:
                    case ZloRequest.Items:
                        if (game == ZloGame.None)
                        {
                            return;
                        }
                        Payloads.Add((byte)game);
                        break;
                    case ZloRequest.User_Info:
                    default:
                        //empty payloads
                        break;
                }
                final.AddRange(BitConverter.GetBytes(Payloads.Count).Reverse());
                final.AddRange(Payloads);
                if (request == ZloRequest.Player_Info)
                {
                    Console.WriteLine(string.Join(",", final));
                }
                req.data = final.ToArray();
                req.pid = (byte)request;
                AddToQueue(req);
            });
        }



        private void ListenerClient_DataReceived(byte pid, byte[] bytes)
        {
            if (CurrentRequest != null && CurrentRequest.pid == pid && CurrentRequest.IsDone == false && CurrentRequest.IsRespondable)
            {
                CurrentRequest.GiveResponce(bytes);
            }
            //WriteLog($"Packet Received [pid = {pid},size = {CurrentRequest.data.Length}] : \n{hexlike(bytes)}");

            using (MemoryStream tempstream = new MemoryStream(bytes))
            using (BinaryReader br = new BinaryReader(tempstream, Encoding.ASCII))
            {
                try
                {
                    if (bytes.Length > 0)
                    {
                        List<byte> bytes_list = bytes.ToList();
                        uint len = (uint)bytes.Length;
                        /*
                         0 - ping, just send it every 20secs
                         1 - userinfo, request - empty payload, responce - 4byte id, string
                         2 - playerinfo - dogtag, clantag, not done
                         3 - serverlist
                         4 - stats, req - 1byte game, resp - 1byte game, 2byte size, (string, float)[size]
                         5 - items - uint8 game, uint16 count, (string, uint8)[count]
                         */
                        switch (pid)
                        {
                            case 0:
                                //receives just ping
                                break;
                            case 1:
                                {
                                    try
                                    {
                                        uint id = br.ReadZUInt32();
                                        string name = br.ReadZString();

                                        UserInfoReceived?.Invoke(id, name);

                                    }
                                    catch (Exception ex)
                                    {
                                        ErrorOccured?.Invoke(ex, "Failed to Parse user info");
                                    }
                                    break;
                                }
                            case 2:
                                {
                                    /*
                                     * playerinfo - pid 2
                                     * uint8 act
                                     * uint8 game
                                     * 
                                     * act 0 - will return current dogtags and clantag
                                     * act 1 - +uint16+uint16+string - set dogtags and clantag*/
                                    ZloGame game = (ZloGame)br.ReadByte();

                                    Console.WriteLine($"Received clan dogtags : {string.Join(",", bytes)}");

                                    ushort dgadvanced = br.ReadZUInt16();
                                    ushort dgbasic = br.ReadZUInt16();
                                    string ct = br.ReadZString();

                                    RaiseClanDogTagsReceived(game, dgadvanced, dgbasic, ct);
                                    break;
                                }
                            case 3:
                                {
                                    try
                                    {
                                        /*(byte) 0 - server, 1 - players, 2 - server removed
                                         *(byte) game
                                         *(uint32) server id
                                         *next is buffer
                                         */
                                        byte server_event_id = br.ReadByte();

                                        ZloGame game = (ZloGame)br.ReadByte();
                                        uint server_id = br.ReadZUInt32();
                                        if (server_id == 0)
                                        {
                                            return;
                                        }

                                        var actualbuffer = bytes_list.Skip(6).ToArray();
                                        switch (game)
                                        {
                                            case ZloGame.BF_3:
                                                switch (server_event_id)
                                                {
                                                    case 0:
                                                        BF3Servers.UpdateServerInfo(server_id, actualbuffer);
                                                        break;
                                                    case 1:
                                                        BF3Servers.UpdateServerPlayers(server_id, actualbuffer);
                                                        break;
                                                    case 2:
                                                        BF3Servers.Remove(server_id);
                                                        break;
                                                }
                                                break;
                                            case ZloGame.BF_4:
                                                //players : 01, bf4 : 01,server id : 00 00 00 01 ,buffer : 00 
                                                switch (server_event_id)
                                                {
                                                    case 0:
                                                        BF4Servers.UpdateServerInfo(server_id, actualbuffer);
                                                        break;
                                                    case 1:
                                                        BF4Servers.UpdateServerPlayers(server_id, actualbuffer);
                                                        //WriteLog($"BF4 Player Packet Received and updated for server {server_id}\n{Hexlike(actualbuffer)}");
                                                        break;
                                                    case 2:
                                                        BF4Servers.Remove(server_id);
                                                        break;
                                                }
                                                break;
                                            case ZloGame.BF_HardLine:
                                                switch (server_event_id)
                                                {
                                                    case 0:
                                                        BFHServers.UpdateServerInfo(server_id, actualbuffer);
                                                        break;
                                                    case 1:
                                                        BFHServers.UpdateServerPlayers(server_id, actualbuffer);
                                                        //WriteLog($"BFH Player Packet Received and updated for server {server_id}\n{Hexlike(actualbuffer)}");
                                                        break;
                                                    case 2:
                                                        BFHServers.Remove(server_id);
                                                        break;
                                                }
                                                break;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ErrorOccured?.Invoke(ex, "Error when Parsing server list packet");
                                    }
                                    break;
                                }
                            case 4:
                                {
                                    byte game = br.ReadByte();
                                    ushort count = br.ReadZUInt16();

                                    var FinalStats = new Dictionary<string, float>
                                    {
                                        [string.Empty] = 0f
                                    };
                                    try
                                    {
                                        for (ushort i = 0; i < count; i++)
                                        {
                                            string statname = br.ReadZString();
                                            float statvalue = br.ReadZFloat();

                                            FinalStats[statname] = statvalue;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ErrorOccured?.Invoke(ex, $"Failed To Parse All Stats ,Base stream pos at : {br.BaseStream.Position},Successfully parsed '{FinalStats.Count}' stats");
                                    }
                                    Task.Run(() =>
                                    {
                                        API_ZloClient_StatsReceived((ZloGame)game, FinalStats);
                                    });

                                    break;
                                }
                            case 5:
                                {

                                    byte game = br.ReadByte();
                                    ushort count = br.ReadZUInt16();
                                    Dictionary<string, API_Item> FinalItems = new Dictionary<string, API_Item>();
                                    try
                                    {
                                        for (ushort i = 0; i < count; i++)
                                        {
                                            string statname = br.ReadZString();
                                            byte statvalue = br.ReadByte();

                                            FinalItems[statname] = new API_Item(statname, statvalue == 1 ? true : false, (ZloGame)game);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ErrorOccured?.Invoke(ex, $"Failed To Parse All Items ,Base stream pos at : {br.BaseStream.Position},Successfully parsed '{FinalItems.Count}' Items");
                                    }
                                    Task.Run(() =>
                                    {
                                        API_ZloClient_ItemsReceived((ZloGame)game, FinalItems);
                                    });
                                    break;
                                }
                            default:
                                break;

                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorOccured?.Invoke(ex, "Failed to parse packet");
                }
            }
        }

        #region Other Methods
        internal void RaiseError(Exception ex, string message)
        {
            ErrorOccured?.Invoke(ex, message);
        }



        internal static string Hexlike(byte[] buf)
        {
            int size = buf.Length;
            StringBuilder sb = new StringBuilder();
            sb.Append('\n');
            uint j;
            for (uint i = 0; i < size; i += 16)
            {
                for (j = 0; j < 16; j++)
                    if (i + j < size)
                        sb.AppendFormat("{0:X2} ", buf[i + j]);
                    else
                        sb.Append("   ");
                sb.Append(" | ");
                for (j = 0; j < 16; j++)
                    if (i + j < size)
                        sb.Append(Isprint(buf[i + j]) ? Convert.ToChar(buf[i + j]) : '.');
                sb.AppendLine();
            }
            return sb.ToString();
        }

        ///CharINDec-is the character in ascii
        ///returns true or false.
        ///is char is printable ascii then returns true and if it's not then false
        internal static bool Isprint(int CharINDec)
        {
            if (CharINDec >= 32 && CharINDec <= 126)
                return true;
            else
                return false;
        }
        private void PingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SendRequest(ZloRequest.Ping);
        }


        private bool ProcessPipeMessage(ZloGame game, byte[] buffer, int readelements)
        {
            using (MemoryStream tempstream = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(tempstream, Encoding.ASCII))
            {
                br.ReadBytes(2);
                ushort len = br.ReadUInt16();
                if (readelements < len - 4)
                {
                    Log.WriteLog($"Packet Not Full,Please Report these Information : \ngame = {game.ToString()}\nlen = {len}\nread = {readelements}\npacket = {Hexlike(buffer)}");
                    return false;
                    //packet not full
                }
                byte firstlen = br.ReadByte();
                string first = br.ReadCountedString(firstlen);

                byte secondlen = br.ReadByte();
                string second = br.ReadCountedString(secondlen + 1);

                GameStateReceived?.Invoke(game, first.Trim(), string.IsNullOrWhiteSpace(second) ? string.Empty : Uri.UnescapeDataString(second.Trim().Replace('\0'.ToString(), string.Empty)));
                return true;
            }
        }

        internal void AddToQueue(Request req)
        {
            //if it's the only one in the queue, trigger it
            //else just wait for the rest to finish
            req.ReceivedResponce -= Req_ReceivedResponce;
            req.ReceivedResponce += Req_ReceivedResponce;
            lock (RequestQueue)
            {
                RequestQueue.Add(req);
                if (RequestQueue.Count == 1)
                {
                    TriggerQueue();
                }
            }


        }
        private void Req_ReceivedResponce(Request Sender)
        {
            //current request just got finished and received
            //remove it from the queue list to trigger the next one
            Sender.ReceivedResponce -= Req_ReceivedResponce;
            lock (RequestQueue)
            {
                if (RequestQueue.Contains(Sender))
                {
                    RequestQueue.Remove(Sender);
                }
                TriggerQueue();
            }
        }
        internal void TriggerQueue()
        {
            //occurs when the next request is ready to be executed
            //proceed the current request               
            if (RequestQueue.Count <= 0) return;
            CurrentRequest = RequestQueue[0];
            if (CurrentRequest.WaitBeforePeriod != TimeSpan.Zero)
            {
                var t = new System.Timers.Timer
                {
                    AutoReset = false,
                    Interval = CurrentRequest.WaitBeforePeriod.TotalMilliseconds
                };
                t.Elapsed += ExecuteCMDTimer;
                t.Start();
            }
            else
            {
                if (!ListenerClient.WritePacket(CurrentRequest.data))
                {
                    CurrentRequest.GiveResponce(null);
                    return;
                }
                else
                {
                    //WriteLog($"Packet Sent [pid = {CurrentRequest.pid},size = {CurrentRequest.data.Length}] : \n{Hexlike(CurrentRequest.data.Skip(5).ToArray())}");
                    if (!CurrentRequest.IsRespondable)
                    {
                        CurrentRequest.GiveResponce(null);
                        return;
                    }
                    else
                    {
                        //should wait for responce for 5 seconds
                        var t = new System.Timers.Timer
                        {
                            AutoReset = false,
                            Interval = TimeSpan.FromSeconds(5).TotalMilliseconds
                        };
                        void removeRequestDelegate(object s, ElapsedEventArgs e)
                        {
                            t.Elapsed -= removeRequestDelegate;
                            t.Stop();
                            CurrentRequest.GiveResponce(null);
                        }

                        t.Elapsed += removeRequestDelegate;
                        t.Start();
                    }
                }


            }
        }
        private void ExecuteCMDTimer(object sender, ElapsedEventArgs e)
        {
            if (CurrentRequest == null)
            {
                return;
            }
            if (sender is System.Timers.Timer timer)
            {
                timer.Stop();
                timer.Elapsed -= ExecuteCMDTimer;
            }

            if (!ListenerClient.WritePacket(CurrentRequest.data))
            {
                //failed to write
                CurrentRequest.GiveResponce(null);
                return;
            }
            else
            {
                //wrote successfully
                if (!CurrentRequest.IsRespondable)
                {
                    CurrentRequest.GiveResponce(null);
                }
                else
                {
                    //should wait for responce for 5 seconds
                    var t = new System.Timers.Timer
                    {
                        AutoReset = false,
                        Interval = new TimeSpan(0, 0, 5).TotalMilliseconds
                    };
                    void removeRequestDelegate(object s, ElapsedEventArgs args)
                    {
                        t.Elapsed -= removeRequestDelegate;
                        t.Stop();
                        CurrentRequest.GiveResponce(null);
                    }

                    t.Elapsed += removeRequestDelegate;
                    t.Start();
                }
            }
        }

        private Request CurrentRequest;



        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        CurrentPlayerID = 0;
                        CurrentPlayerName = string.Empty;
                        if (PingTimer != null)
                        {
                            PingTimer.Elapsed -= PingTimer_Elapsed;
                            PingTimer.Stop();
                        }
                        UnSubServerList();
                        ListenerClient.Dispose();
                    }
                    StopDiscordRPC();
                    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // TODO: set large fields to null.
                    disposedValue = true;
                }
            }
            catch { }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~API_ZloClient()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
        #endregion
    }
}