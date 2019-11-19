using DiscordRPC;
using DiscordRPC.Logging;
using DiscordRPC.Message;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
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
using Zlo.PacketInfo;
using Zlo.PacketInfo.RESP;
using REQ = Zlo.PacketInfo.REQ;
using RESP = Zlo.PacketInfo.RESP;
namespace Zlo
{
    /// <summary>
    /// the main controller for the api, please note : [ONLY CREATE A SINGLE INSTANCE OF THIS CLASS]
    /// </summary>
    public partial class API_ZloClient : IAPI_ZloClient
    {
        public Version CurrentApiVersion { get; } = new Version(16, 8, 0, 0);
        private static bool IsInitlaized = false;



        /// <summary>
        /// when zclient disconnects it will automatically reconnect, use this variable to define how long it should take to reconnect each time
        /// default is 1 second
        /// <para>To disable auto reconnect, assign this to <see cref="TimeSpan.MaxValue"/></para>
        /// </summary>
        public TimeSpan ReconnectInterval { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// MUST BE INITIALIZED ONLY ONCE
        /// The API initializer (doesn't connect to zclient until you call <see cref="Connect"/>
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



        private void API_ZloClient_ClanDogTagsReceived(ZloBFGame game, ushort dogtag1, ushort dogtag2, string clanTag)
        {
            ClanDogTagsPerGame[game] = new Tuple<ushort, ushort, string>(dogtag1, dogtag2, clanTag);
        }

        private void API_ZloClient_ErrorOccured(Exception Error, string CustomMessage)
        {
            Log.WriteLog($"[{CustomMessage}] : \n{Error.ToString()}");
        }

        private void ZloClient_GameStateReceived(ZloBFGame game, string type, string message)
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
            if (dllz == null || game == ZloBFGame.None)
            {
                return;
            }
            switch (game)
            {
                case ZloBFGame.BF_3:
                    if (trimmed == $"State_Game State_NA {CurrentPlayerID}")
                    {
                        foreach (var item in dllz)
                        {
                            RequestDLLInject(game, item);
                        }
                    }
                    break;

                case ZloBFGame.BF_4:
                case ZloBFGame.BF_HardLine:
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
        private readonly Dictionary<ZloBFGame, Tuple<ushort, ushort, string>> ClanDogTagsPerGame = new Dictionary<ZloBFGame, Tuple<ushort, ushort, string>>();
        private ConcurrentQueue<BaseRequestPacket> RequestQueue { get; } = new ConcurrentQueue<BaseRequestPacket>();
        #endregion

        #region API events
        /// <summary>
        /// Gets triggered after receiving user stats and passes the game and a list of stats
        /// </summary>    
        public event API_StatsReceivedEventHandler StatsReceived;

        internal void RaiseStatsReceived(ZloBFGame game, Dictionary<string, float> stats)
        {
            StatsReceived?.Invoke(game, stats);
        }

        /// <summary>
        /// Gets triggered after receiving user items and passes the game and a list of items
        /// </summary>    
        public event API_ItemsReceivedEventHandler ItemsReceived;

        internal void RaiseItemsReceived(ZloBFGame game, Dictionary<string, API_Item> stats)
        {
            ItemsReceived?.Invoke(game, stats);
        }
        /// <summary>
        /// Gets triggered after receiving user info and passes user id and name
        /// </summary>
        public event API_UserInfoReceivedEventHandler UserInfoReceived;


        internal void RaiseClanDogTagsReceived(ZloBFGame game, ushort dg1, ushort dg2, string ct)
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
        /// Occurs when the game list is received
        /// <para>see : <see cref="RunnableGameList"/></para>
        /// </summary>
        public event API_RunnableGameListReceivedEventHandler RunnableGameListReceived;
        public event API_GameRunResultReceivedEventHandler GameRunResultReceived;


        /// <summary>
        /// occurs when the api connects/disconnects from ZClient
        /// </summary>
        public event API_ConnectionStateChangedEventHandler ConnectionStateChanged
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
            if (PingTimer != null) PingTimer.Elapsed -= PingTimer_Elapsed;
            PingTimer = new System.Timers.Timer(10 * 1000);
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
        public void RefreshRunnableGamesList()
        {
            //Send request            
            SendRequest(ZloPacketId.RunnableGameList);
        }
        public void SendRunGameRequest(RunnableGame game, string cmd = "")
        {
            var final = new List<byte>();
            final.AddRange(QBitConv(game.ZName));
            final.AddRange(QBitConv(cmd));

            var req = ProcessRequest(ZloPacketId.RunGame, ZloBFGame.None, final.ToArray());
            req.RequestInfo = new RunGame() { game = game };
            SendRequest(req);
        }
        public List<string> GetDllsList(ZloBFGame game)
        {
            if (game == ZloBFGame.None)
            {
                return null;
            }
            return Settings.CurrentSettings.InjectedDlls.GetDllsList(game);
        }
        internal void RequestDLLInject(ZloBFGame game, string dllPath)
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
                pid = (int)ZloPacketId.InjectDll
            };
            var toadd = Encoding.UTF8.GetBytes(dllPath);

            var size = BitConverter.GetBytes(toadd.Length + 2);
            Array.Reverse(size);

            var ar = new List<byte> { req.pid };
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
            SendRequest(ZloPacketId.User_Info);
        }
        public void GetStats(ZloBFGame game)
        {
            SendRequest(ZloPacketId.Stats, game);
        }
        public void GetItems(ZloBFGame game)
        {
            SendRequest(ZloPacketId.Items, game);
        }

        public ZloBFGame ActiveServerListener
        {
            get => Settings.CurrentSettings.ActiveServerListener;
            private set { if (Settings.CurrentSettings.ActiveServerListener == value) return; Settings.CurrentSettings.ActiveServerListener = value; Settings.TrySave(); }
        }

        public void SubToServerList(ZloBFGame game)
        {

            //unsub first
            UnSubServerList();
            ActiveServerListener = game;
            //0 == subscribe            
            SendRequest(ZloPacketId.Server_List, ActiveServerListener, 0);
            GetClanDogTags();
        }





        public void UnSubServerList()
        {
            if (ActiveServerListener == ZloBFGame.None)
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
            SendRequest(ZloPacketId.Server_List, ActiveServerListener, 1);
            ActiveServerListener = ZloBFGame.None;
        }
        public void GetClanDogTags()
        {
            //bf3 not supported
            if (ActiveServerListener == ZloBFGame.None || ActiveServerListener == ZloBFGame.BF_3)
            {
                return;
            }
            SendRequest(ZloPacketId.Player_Info, ActiveServerListener, 0);
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
            if (ActiveServerListener == ZloBFGame.None || ActiveServerListener == ZloBFGame.BF_3)
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

            SendRequest(ZloPacketId.Player_Info, ActiveServerListener, final.ToArray());
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

        public RunnableGameList RunnableGameList { get; } = new RunnableGameList();

        #endregion

        private void SendRequest(BaseRequestPacket req)
        {
            Task.Run(() =>
            {
                AddToQueue(req);
            });
        }
        private void ListenerClient_DataReceived(byte pid, byte[] bytes)
        {
            var req = CurrentRequest;
            var packet = (ZloPacketId)pid;
            BaseResponsePacket responsePacket;
            if (req != null && req.PacketId == packet && !req.IsReceived && req.IsRespondable)
            {
                req.RaiseResponse(bytes);
                responsePacket = req.Response;
            }
            else
            {
                //Get responses that aren't tied to a single request
                switch (packet)
                {
                    case ZloPacketId.Server_List:
                        responsePacket = new ServerList();
                        responsePacket.Deserialize(bytes);
                        break;
                    default:
                        Log.WriteLog($"Illogical flow, zclient sent a packet that wasn't requested, skipping it\nPid: {pid}\nData: {Hexlike(bytes)}");
                        return;
                }
            }
            switch (responsePacket)
            {
                case UserInfo userInfo:
                    UserInfoReceived?.Invoke(userInfo.Id, userInfo.Name);
                    break;
                case PlayerInfo playerInfo:
                    RaiseClanDogTagsReceived(playerInfo.Game, playerInfo.DogTagAdvanced, playerInfo.DogTagBasic, playerInfo.ClanTag);
                    break;

                case ServerList serverList:
                    IBFServerList bfServerList;
                    switch (serverList.Game)
                    {
                        case ZloBFGame.BF_3:
                            bfServerList = BF3Servers;
                            break;
                        case ZloBFGame.BF_4:
                            bfServerList = BF4Servers;
                            break;
                        case ZloBFGame.BF_HardLine:
                            bfServerList = BFHServers;
                            break;
                        case ZloBFGame.None:
                        default:
                            bfServerList = null;
                            break;
                    }
                    switch (serverList.Game)
                    {
                        case ZloBFGame.BF_3:
                            switch (serverList.Event)
                            {
                                case ServerListEvent.ServerChange:
                                    BF3Servers.UpdateServerInfo(serverList.ServerId, serverList.DataBuffer);
                                    break;
                                case ServerListEvent.PlayerListChange:
                                    BF3Servers.UpdateServerPlayers(serverList.ServerId, serverList.DataBuffer);
                                    break;
                                case ServerListEvent.ServerRemove:
                                    BF3Servers.Remove(serverList.ServerId);
                                    break;
                            }
                            break;
                        case ZloBFGame.BF_4:
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
                        case ZloBFGame.BF_HardLine:
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
                    break;
                default:
                    break;
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

                                        ZloBFGame game = (ZloBFGame)br.ReadByte();
                                        uint server_id = br.ReadZUInt32();
                                        if (server_id == 0)
                                        {
                                            return;
                                        }

                                        var actualbuffer = bytes_list.Skip(6).ToArray();

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
                                        API_ZloClient_StatsReceived((ZloBFGame)game, FinalStats);
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

                                            FinalItems[statname] = new API_Item(statname, statvalue == 1 ? true : false, (ZloBFGame)game);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ErrorOccured?.Invoke(ex, $"Failed To Parse All Items ,Base stream pos at : {br.BaseStream.Position},Successfully parsed '{FinalItems.Count}' Items");
                                    }
                                    Task.Run(() =>
                                    {
                                        API_ZloClient_ItemsReceived((ZloBFGame)game, FinalItems);
                                    });
                                    break;
                                }
                            case 8:
                                {
                                    byte isx64 = br.ReadByte();

                                    RunnableGameList.IsOSx64 = isx64 == 1;
                                    uint count = br.ReadZUInt32();
                                    RunnableGameList.Clear();
                                    for (int i = 0; i < count; i++)
                                    {
                                        string runName = br.ReadZString();
                                        string ZName = br.ReadZString();
                                        string name = br.ReadZString();

                                        RunnableGameList.Add(new RunnableGame(name, runName, ZName));
                                    }
                                    RunnableGameListReceived?.Invoke();
                                    break;
                                }
                            case 9:
                                {
                                    var runState = (GameRunResult)br.ReadByte();
                                    if (req is REQ.RunGame reqInfo)
                                    {
                                        GameRunResultReceived?.Invoke(reqInfo.Game, runState);
                                    }
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

        ///CharINDec-is the character in ASCII
        ///returns true or false.
        ///is char is printable ASCII then returns true and if it's not then false
        internal static bool Isprint(int CharINDec)
        {
            if (CharINDec >= 32 && CharINDec <= 126)
                return true;
            else
                return false;
        }
        private void PingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SendRequest(new REQ.Empty(ZloPacketId.Ping));
        }


        private bool ProcessPipeMessage(ZloBFGame game, byte[] buffer, int readelements)
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

        internal void AddToQueue(BaseRequestPacket req)
        {
            //if it's the only one in the queue, trigger it
            //else just wait for the rest to finish
            req.ReceivedResponse -= Req_ReceivedResponse;
            req.ReceivedResponse += Req_ReceivedResponse;
            RequestQueue.Enqueue(req);
            TriggerQueue();
        }
        private void Req_ReceivedResponse(BaseRequestPacket request, BaseResponsePacket response)
        {
            //current request just got finished and received
            //remove it from the queue list to trigger the next one
            request.ReceivedResponse -= Req_ReceivedResponse;
            lock (RequestQueue)
            {
                TriggerQueue();
            }
        }
        internal void TriggerQueue()
        {
            //occurs when the next request is ready to be executed
            //proceed the current request
            if (RequestQueue.TryDequeue(out var req))
            {
                CurrentRequest = req;
                void doSendReq()
                {
                    if (ListenerClient.WritePacket(req.Serialize()))
                    {
                        req.IsSent = true;
                        if (!req.IsRespondable)
                        {
                            req.RaiseResponse(null);
                            return;
                        }
                        else
                        {
                            //should timeout after 2 seconds 
                            var t = new System.Timers.Timer
                            {
                                AutoReset = false,
                                Interval = TimeSpan.FromSeconds(2).TotalMilliseconds
                            };
                            void removeRequestDelegate(object s, ElapsedEventArgs e)
                            {
                                t.Elapsed -= removeRequestDelegate;
                                t.Stop();
                                if (!req.IsReceived)
                                    req.RaiseResponse(null);
                            }

                            t.Elapsed += removeRequestDelegate;
                            t.Start();
                        }
                    }
                    else
                    {
                        req.RaiseResponse(null);
                        return;
                    }
                }
                void ExecuteCMDTimer(object sender, ElapsedEventArgs e)
                {
                    if (sender is System.Timers.Timer timer)
                    {
                        timer.Stop();
                        timer.Elapsed -= ExecuteCMDTimer;
                    }
                    doSendReq();
                }
                if (req.WaitBeforePeriod != TimeSpan.Zero)
                {
                    var t = new System.Timers.Timer
                    {
                        AutoReset = false,
                        Interval = req.WaitBeforePeriod.TotalMilliseconds
                    };
                    t.Elapsed += ExecuteCMDTimer;
                    t.Start();
                }
                else
                {
                    doSendReq();
                }
            }
        }

        private BaseRequestPacket CurrentRequest { get; set; }
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
                    disposedValue = true;
                }
            }
            catch { }
        }

        ~API_ZloClient()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
        #endregion
    }
}