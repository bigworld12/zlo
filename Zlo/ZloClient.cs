using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using static Zlo.Extentions.Helpers;
using System.Diagnostics;
using System.Timers;
using Zlo.Extras;
using System.IO.Pipes;
using System.ComponentModel;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Windows;
using Microsoft.Win32;

namespace Zlo
{
    /// <summary>
    /// the main controller for the api, please note : [ONLY CREATE A SINGLE INSTANCE OF THIS CLASS]
    /// </summary>
    public partial class API_ZloClient
    {
        private Version _localVer = new Version(10, 0, 1, 0);

        private JObject serverJson;

        private static bool IsInitlaized = false;

        /// <summary>
        /// MUST BE INITIALIZED ONLY ONCE
        /// The API intializer (doesn't connect to zclient until you call <see cref="Connect"/>
        /// </summary>
        public API_ZloClient()
        {
            ErrorOccured -= API_ZloClient_ErrorOccured;
            ErrorOccured += API_ZloClient_ErrorOccured;
            if (IsInitlaized)
            {
                return;
            }
            IsInitlaized = true;
            try
            {
                Disconnected -= ZloClient_Disconnected;
                Disconnected += ZloClient_Disconnected;


                LoadSettings();

                GameStateReceived -= ZloClient_GameStateReceived;
                GameStateReceived += ZloClient_GameStateReceived;

                m_client = new ZloTCPClient(this);
                PingTimer = new System.Timers.Timer(20 * 1000);


                BF3_Pipe = new NamedPipeClientStream(".", "venice_snowroller");
                BF4_Pipe = new NamedPipeClientStream(".", "warsaw_snowroller");
                BFH_Pipe = new NamedPipeClientStream(".", "omaha_snowroller");

                BF3_Pipe_Listener = new Thread(BF3_Pipe_Loop) { IsBackground = true };

                BF4_Pipe_Listener = new Thread(BF4_Pipe_Loop) { IsBackground = true };

                BFH_Pipe_Listener = new Thread(BFH_Pipe_Loop) { IsBackground = true };

                ListenerClient.ZloPacketReceived -= ListenerClient_DataReceived;
                ListenerClient.ZloPacketReceived += ListenerClient_DataReceived;
                //uint UserID , string UserName
                UserInfoReceived -= ZloClient_UserInfoReceived;
                UserInfoReceived += ZloClient_UserInfoReceived;

                ClanDogTagsReceived -= API_ZloClient_ClanDogTagsReceived;
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
            WriteLog($"[{CustomMessage}] : \n{Error.ToString()}");
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

        private void ZloClient_Disconnected(DisconnectionReasons Reason)
        {
            Task.Run(() => { ConnectionStateChanged?.Invoke(false); });

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



        private Dictionary<ZloGame, List<string>> DllsToInject = new Dictionary<ZloGame, List<string>>
        {
            { ZloGame.BF_3 , new List<string>() },
            { ZloGame.BF_4 , new List<string>() },
            { ZloGame.BF_HardLine , new List<string>() }
        };

        private ZloTCPClient m_client;
        private ZloTCPClient ListenerClient
        {
            get { return m_client; }
        }

        System.Timers.Timer PingTimer;

        private NamedPipeClientStream m_BF3_pipe;
        private NamedPipeClientStream BF3_Pipe
        {
            get { return m_BF3_pipe; }
            set { m_BF3_pipe = value; }
        }

        private NamedPipeClientStream m_BF4_Pipe;
        private NamedPipeClientStream BF4_Pipe
        {
            get { return m_BF4_Pipe; }
            set { m_BF4_Pipe = value; }
        }

        private NamedPipeClientStream m_BFh_pipe;
        private NamedPipeClientStream BFH_Pipe
        {
            get { return m_BFh_pipe; }
            set { m_BFh_pipe = value; }
        }


        Thread BF3_Pipe_Listener;
        Thread BF4_Pipe_Listener;
        Thread BFH_Pipe_Listener;

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

        #region Settings
        internal string saveFileName = @".\Zlo_settings.json";
        internal const string defaultJSON =
            @"{
""dlls"" : {
""bf3"" : [],
""bf4"" : [],
""bfh"" : []
    }
}";
        internal JObject SavedObjects = null;
        /// <summary>
        /// This method runs in the background
        /// </summary>
        public void SaveSettings()
        {
            Task.Run(() =>
            {
                try
                {
                    if (SavedObjects == null)
                    {
                        SavedObjects = new JObject();
                    }
                    var bf3_list = GetDllsList(ZloGame.BF_3);
                    var bf4_list = GetDllsList(ZloGame.BF_4);
                    var bfh_list = GetDllsList(ZloGame.BF_HardLine);

                    if (bf3_list != null)
                    {
                        SavedObjects["dlls"]["bf3"] = JArray.FromObject(bf3_list);
                    }

                    if (bf4_list != null)
                    {
                        SavedObjects["dlls"]["bf4"] = JArray.FromObject(bf4_list);
                    }

                    if (bfh_list != null)
                    {
                        SavedObjects["dlls"]["bfh"] = JArray.FromObject(bfh_list);
                    }

                    File.WriteAllText(saveFileName, SavedObjects.ToString());
                }
                catch (Exception ex)
                {
                    RaiseError(ex, "Error Occured when Saving settings from Path : " + Path.GetFullPath(saveFileName));
                }
            });

        }
        /// <summary>
        /// this method hangs the thread [doesn't run in the background]
        /// </summary>
        internal void LoadSettings()
        {
            try
            {
                if (File.Exists(saveFileName))
                {
                    SavedObjects = JObject.Parse(File.ReadAllText(saveFileName));
                    if (SavedObjects["dlls"] == null)
                    {
                        GetDefaultSets();
                    }
                    else
                    {
                        var bf3_list = GetDllsList(ZloGame.BF_3);
                        var bf4_list = GetDllsList(ZloGame.BF_4);
                        var bfh_list = GetDllsList(ZloGame.BF_HardLine);

                        bf3_list.Clear();
                        bf4_list.Clear();
                        bfh_list.Clear();

                        if (SavedObjects["dlls"]["bf3"] != null)
                        {
                            foreach (string item in (JArray)SavedObjects["dlls"]["bf3"])
                            {
                                bf3_list.Add(item);
                            }
                        }

                        if (SavedObjects["dlls"]["bf4"] != null)
                        {
                            foreach (string item in (JArray)SavedObjects["dlls"]["bf4"])
                            {
                                bf4_list.Add(item);
                            }
                        }

                        if (SavedObjects["dlls"]["bfh"] != null)
                        {
                            foreach (string item in (JArray)SavedObjects["dlls"]["bfh"])
                            {
                                bfh_list.Add(item);
                            }
                        }
                    }
                }
                else
                {

                    if (SavedObjects == null || SavedObjects["dlls"] == null)
                    {
                        GetDefaultSets();
                    }
                    SaveSettings();
                }
            }
            catch (Exception ex)
            {
                GetDefaultSets();
                RaiseError(ex, "Error Occured when Reading saved settings from Path : " + Path.GetFullPath(saveFileName));
            }
        }
        internal void GetDefaultSets()
        {
            SavedObjects = JObject.Parse(defaultJSON);
        }
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
        /// occurs when the client disconnects from the server
        /// </summary>
        public event API_DisconnectedEventHandler Disconnected;

        /// <summary>
        /// occurs when the game state changes (connecting to server/closing the server,etc..)
        /// </summary>
        public event API_GameStateReceivedEventHandler GameStateReceived;

        /// <summary>
        /// occurs after receiving the current api version
        /// </summary>
        public event API_APIVersionReceivedEventHandler APIVersionReceived;

        /// <summary>
        /// occurs when the api connects/disconnects from ZClient
        /// </summary>
        public event API_ConnectionStateChanged ConnectionStateChanged;
        #endregion

        #region API Methods
        public bool Connect()
        {
            Task.Run(() =>
            {
                try
                {
                    //check for the version here                                        
                    string check = @"https://onedrive.live.com/download?cid=0AF30EAB900CEF1B&resid=AF30EAB900CEF1B%21916&authkey=AG6BDDR2epUlUNo";

                    using (WebClient wc = new WebClient())
                    {
                        serverJson = JObject.Parse(wc.DownloadString(check));
                        if (!Version.TryParse(serverJson["version"].ToObject<string>(), out Version newver))
                        {
                            newver = _localVer;
                        }
                        var isne = newver > _localVer;
                        if (isne)
                        {
                            APIVersionReceived?.Invoke(_localVer, newver, true, serverJson["file"].ToObject<string>());
                        }
                        else
                        {
                            APIVersionReceived?.Invoke(_localVer, newver, false, string.Empty);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorOccured?.Invoke(ex, "Error when Checking updates");
                }
            });


            try
            {
                IsOn = true;

                BF3_Pipe_Listener.Start();
                BF4_Pipe_Listener.Start();
                BFH_Pipe_Listener.Start();

                ListenerClient.Connect();
                ConnectionStateChanged?.Invoke(true);
                //Thread.Sleep(1000);
                GetUserInfo();
                return true;
            }
            catch (SocketException se)
            {
                Disconnected?.Invoke(DisconnectionReasons.ZClientNotOpen);
                PingTimer.Elapsed -= PingTimer_Elapsed;
                PingTimer.Stop();
                ErrorOccured?.Invoke(se, "ZClient isn't open");
                return false;
            }
            catch (Exception ex)
            {
                if (PingTimer != null)
                {
                    PingTimer.Elapsed -= PingTimer_Elapsed;
                }

                PingTimer?.Stop();
                ErrorOccured?.Invoke(ex, "Error when connecting");
                return false;
            }
        }

        public List<string> GetDllsList(ZloGame game)
        {
            if (game == ZloGame.None)
            {
                return null;
            }
            return DllsToInject[game];
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

        public void JoinOnlineGame(OnlinePlayModes playmode, uint serverid = 0)
        {

            ProcessStartInfo rungame = null;
            switch (playmode)
            {
                case OnlinePlayModes.BF3_Multi_Player:
                    rungame = GetGameJoinID(ZloGame.BF_3, CurrentPlayerID, serverid, 1);
                    break;
                case OnlinePlayModes.BF3_COOP:
                    rungame = GetGameJoinID(ZloGame.BF_3, CurrentPlayerID, serverid, 5);
                    break;

                case OnlinePlayModes.BF4_Multi_Player:
                    rungame = GetGameJoinID(ZloGame.BF_4, CurrentPlayerID, serverid, 1);
                    break;
                case OnlinePlayModes.BF4_Spectator:
                    rungame = GetGameJoinID(ZloGame.BF_4, CurrentPlayerID, serverid, 3);
                    break;
                case OnlinePlayModes.BF4_Commander:
                    rungame = GetGameJoinID(ZloGame.BF_4, CurrentPlayerID, serverid, 2);
                    break;
                case OnlinePlayModes.BF4_COOP:
                    rungame = GetGameJoinID(ZloGame.BF_4, CurrentPlayerID, serverid, 5);
                    break;

                case OnlinePlayModes.BFH_Multi_Player:
                    rungame = GetGameJoinID(ZloGame.BF_HardLine, CurrentPlayerID, serverid, 1);
                    break;                  
                default:
                    return;
            }
            if (rungame == null)
            {
                return;
            }
            else
            {
                try
                {
                    if (rungame.FileName.StartsWith("origin2"))
                    {
                        Process.Start(rungame.FileName);
                    }
                    else
                    {
                        rungame.UseShellExecute = false;
                        rungame.WorkingDirectory = Path.GetDirectoryName(rungame.FileName);
                        Process.Start(rungame);
                    }
                }
                catch (Exception ex)
                {
                    RaiseError(ex, $"Error occured when Starting the game with:\nfile name : {rungame.FileName}\nand arguments : {rungame.Arguments}\nuse shell execute ? {rungame.UseShellExecute}");
                }              
            }
        }
        public void JoinOfflineGame(OfflinePlayModes playmode)
        {
            ProcessStartInfo rungame = null;
            switch (playmode)
            {
                case OfflinePlayModes.BF3_Single_Player:
                    rungame = GetGameJoinID(ZloGame.BF_3, CurrentPlayerID, 0, 0);
                    break;

                case OfflinePlayModes.BF4_Single_Player:
                    rungame = GetGameJoinID(ZloGame.BF_4, CurrentPlayerID, 0, 0);
                    break;
                case OfflinePlayModes.BF4_Test_Range:
                    rungame = GetGameJoinID(ZloGame.BF_4, CurrentPlayerID, 0, 4);
                    break;


                case OfflinePlayModes.BFH_Single_Player:
                    rungame = GetGameJoinID(ZloGame.BF_HardLine, CurrentPlayerID, 0, 0);
                    break;

                default:
                    return;
            }

            try
            {
                if (rungame == null)
                {
                    return;
                }
                else
                {
                    if (rungame.FileName.StartsWith("origin2"))
                    {
                        Process.Start(rungame.FileName);
                    }
                    else
                    {
                        rungame.UseShellExecute = false;
                        rungame.WorkingDirectory = Path.GetDirectoryName(rungame.FileName);
                        Process.Start(rungame);
                    }
                }
            }
            catch (Exception ex)
            {
                RaiseError(ex, $"Error occured when Starting the game with:\nfile name : {rungame.FileName}\nand arguments : {rungame.Arguments}\nuse shell execute ? {rungame.UseShellExecute}");
            } 
        }
        public void JoinOnlineGameWithPassWord(OnlinePlayModes playmode, uint serverid, string password)
        {
            ProcessStartInfo rungame = null;
            switch (playmode)
            {
                case OnlinePlayModes.BF3_Multi_Player:
                    rungame = GetGameJoinID(ZloGame.BF_3, CurrentPlayerID, serverid, 1, password);
                    break;
                case OnlinePlayModes.BF4_Multi_Player:
                    rungame = GetGameJoinID(ZloGame.BF_4, CurrentPlayerID, serverid, 1, password);
                    break;
                case OnlinePlayModes.BF4_Spectator:
                    rungame = GetGameJoinID(ZloGame.BF_4, CurrentPlayerID, serverid, 3, password);
                    break;
                case OnlinePlayModes.BF4_Commander:
                    rungame = GetGameJoinID(ZloGame.BF_4, CurrentPlayerID, serverid, 2, password);
                    break;

                    
                case OnlinePlayModes.BFH_Multi_Player:
                    rungame = GetGameJoinID(ZloGame.BF_HardLine, CurrentPlayerID, serverid, 1, password);
                    break;
                case OnlinePlayModes.BFH_Spectator:
                    rungame = GetGameJoinID(ZloGame.BF_HardLine, CurrentPlayerID, serverid, 3, password);
                    break;
                case OnlinePlayModes.BFH_Commander:
                    rungame = GetGameJoinID(ZloGame.BF_HardLine, CurrentPlayerID, serverid, 2, password);
                    break;
                default:
                    return;
            }
            if (rungame == null)
            {
                return;
            }
            else
            {
                try
                {
                    if (rungame.FileName.StartsWith("origin2"))
                    {
                        Process.Start(rungame.FileName);
                    }
                    else
                    {
                        rungame.UseShellExecute = false;
                        rungame.WorkingDirectory = Path.GetDirectoryName(rungame.FileName);
                        Process.Start(rungame);
                    }
                }
                catch (Exception ex)
                {
                    RaiseError(ex, $"Error occured when Starting the game with:\nfile name : {rungame.FileName}\nand arguments : {rungame.Arguments}\nuse shell execute ? {rungame.UseShellExecute}");
                }
            }
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

        private ZloGame ActiveServerListener = ZloGame.None;


        public void SubToServerList(ZloGame game)
        {
            #region SubChecker
            if (ActiveServerListener == game)
            {
                return;
            }
            else if (ActiveServerListener != ZloGame.None)
            {
                //unsub first
                UnSubServerList();
            }
            #endregion
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


        private bool IsOn = false;
        public void Close()
        {
            try
            {
                CurrentPlayerID = 0;
                CurrentPlayerName = string.Empty;

                UnSubServerList();


                IsOn = false;
                ListenerClient.Disconnect();
                ConnectionStateChanged?.Invoke(false);
            }
            catch (Exception ex) { ErrorOccured?.Invoke(ex, "Error occured when disconnecting zclient"); }

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

        public bool IsConnectedToZCLient
        {
            get { return ListenerClient.IsConnected; }
        }


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
                    case ZloRequest.Ping:
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
                                        if (!PingTimer.Enabled)
                                        {
                                            PingTimer.Elapsed -= PingTimer_Elapsed;
                                            PingTimer.Elapsed += PingTimer_Elapsed;
                                            PingTimer.Start();
                                        }
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
                                                        WriteLog($"BF4 Player Packet Received and updated for server {server_id}\n{Hexlike(actualbuffer)}");
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
                                                        WriteLog($"BFH Player Packet Received and updated for server {server_id}\n{Hexlike(actualbuffer)}");
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

                                            FinalItems[statname] = new API_Item(statname, statvalue == 1 ? true : false);
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
            if (IsOn)
            {
                ErrorOccured?.Invoke(ex, message);
            }
        }


        public static void WriteLog(string log)
        {
            if (ToWrite.Count > 0)
                ToWrite.Enqueue(log);
            if (ToWrite.Count == 1)
            {
                ActualWriteLog();
            }
        }

        static Queue<string> ToWrite = new Queue<string>();
        private static void ActualWriteLog()
        {
            Task.Run(() =>
            {
                try
                {

                    File.AppendAllText(@".\Demo-Log.txt", $"\n================================\n{DateTime.Now.ToString()}\n{ToWrite.Dequeue()}\n================================");
                    if (ToWrite.Count > 0)
                    {
                        ActualWriteLog();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            });
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
            try
            {
                SendRequest(ZloRequest.Ping);
            }
            catch (Exception ex)
            {
                ErrorOccured?.Invoke(ex, "Error occured when requesting ping");
                PingTimer.Elapsed -= PingTimer_Elapsed;
                PingTimer.Stop();
                Disconnected?.Invoke(DisconnectionReasons.PingFail);
            }
        }

        private void BF3_Pipe_Loop()
        {
            while (true)
            {
                try
                {
                    if (!IsOn)
                    {
                        return;
                    }
                    if (!BF3_Pipe.IsConnected && NamedPipeExists("venice_snowroller"))
                    {
                        BF3_Pipe.Connect();
                    }
                    else { Thread.Sleep(200); }

                    while (BF3_Pipe.IsConnected)
                    {

                        if (!IsOn)
                        {
                            return;
                        }
                        byte[] buffer = new byte[1024];
                        int read = BF3_Pipe.Read(buffer, 0, buffer.Length);
                        if (read != 0)
                        {
                            byte[] final = new byte[read + 1];
                            Buffer.BlockCopy(buffer, 0, final, 0, read + 1);
                            ProcessPipeMessage(ZloGame.BF_3, final, read);
                        }
                        Thread.Sleep(50);
                    }
                }
                catch (Exception ex)
                {
                    ErrorOccured?.Invoke(ex, "Error Occured when Trying to connect to BF3 pipe");
                }
            }
        }
        private void BF4_Pipe_Loop()
        {
            while (true)
            {
                try
                {
                    if (!IsOn)
                    {
                        return;
                    }
                    if (!BF4_Pipe.IsConnected && NamedPipeExists("warsaw_snowroller"))
                    {
                        BF4_Pipe.Connect();
                    }
                    else
                    {
                        Thread.Sleep(200);
                    }

                    while (BF4_Pipe.IsConnected)
                    {
                        if (!IsOn)
                        {
                            return;
                        }
                        byte[] buffer = new byte[1024];
                        int read = BF4_Pipe.Read(buffer, 0, buffer.Length);
                        if (read != 0)
                        {
                            byte[] final = new byte[read + 1];
                            Buffer.BlockCopy(buffer, 0, final, 0, read + 1);
                            ProcessPipeMessage(ZloGame.BF_4, final, read);
                        }
                        Thread.Sleep(50);
                    }
                }
                catch (Exception ex)
                {
                    ErrorOccured?.Invoke(ex, "Error Occured when Trying to connect to BF4 pipe");

                }
            }
        }
        private void BFH_Pipe_Loop()
        {
            while (true)
            {
                if (!IsOn)
                {
                    return;
                }
                try
                {
                    //omaha_snowroller                  
                    if (!BFH_Pipe.IsConnected && NamedPipeExists("omaha_snowroller"))
                    {
                        BFH_Pipe.Connect();
                    }
                    else { Thread.Sleep(200); }

                    while (BFH_Pipe.IsConnected)
                    {
                        if (!IsOn)
                        {
                            return;
                        }
                        byte[] buffer = new byte[1024];
                        int read = BFH_Pipe.Read(buffer, 0, buffer.Length);
                        if (read != 0)
                        {
                            byte[] final = new byte[read + 1];
                            Buffer.BlockCopy(buffer, 0, final, 0, read + 1);
                            ProcessPipeMessage(ZloGame.BF_HardLine, final, read);
                        }
                        Thread.Sleep(50);
                    }
                }
                catch (Exception ex)
                {
                    ErrorOccured?.Invoke(ex, "Error Occured when Trying to connect to BF Hardline pipe");
                }


            }
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
                    WriteLog($"Packet Not Full,Please Report these Information : \ngame = {game.ToString()}\nlen = {len}\nread = {readelements}\npacket = {Hexlike(buffer)}");
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
            RequestQueue.Add(req);

            if (RequestQueue.Count == 1)
            {
                TriggerQueue();
            }
        }
        private void Req_ReceivedResponce(Request Sender)
        {
            //current request just got finished and received
            //remove it from the queue list to trigger the next one
            Sender.ReceivedResponce -= Req_ReceivedResponce;
            RequestQueue.Remove(Sender);
            TriggerQueue();
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
                }
                else
                {
                    WriteLog($"Packet Sent [pid = {CurrentRequest.pid},size = {CurrentRequest.data.Length}] : \n{Hexlike(CurrentRequest.data.Skip(5).ToArray())}");
                }
                if (!CurrentRequest.IsRespondable)
                {
                    CurrentRequest.GiveResponce(null);
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
                CurrentRequest.GiveResponce(null);
            }
            if (!CurrentRequest.IsRespondable)
            {
                CurrentRequest.GiveResponce(null);
            }
        }

        private Request CurrentRequest;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="PlayerID"></param>
        /// <param name="ServerID"></param>
        /// <param name="playmode">0 = single player
        /// 1 = multi player
        /// 2 = commander
        /// 3 = spectator
        /// 4 = test range
        /// 5 = co-op</param>
        /// <param name="pw">The server password</param>
        /// <returns></returns>
        private ProcessStartInfo GetGameJoinID(ZloGame game, uint PlayerID, uint ServerID, int playmode, string pw = "")
        {
            /*
             play mode : 
             0 = single player
             1 = multi player
             2 = commander
             3 = spectator
             4 = test range
             5 = co-op
             */
            string q = "\\" + "\"";
            ProcessStartInfo final = null;
            var title = string.Empty;
            var pwExpression = playmode == 1 && !string.IsNullOrWhiteSpace(pw) ? $@"password={q}{pw}{q}" : string.Empty;
            switch (game)
            {
                //%20password%3D%5C%22{pw}%5C%22%20
                case ZloGame.BF_3:
                    {
                        title = "Battlefield3";
                        string bf3offers = "70619,71067,DGR01609244,DGR01609245";
                        string requestState = playmode == 1 ? "State_ClaimReservation" : "State_ResumeCampaign";
                        string levelmode = playmode == 1 ? "mp" : "sp";
                        string gameIDstr = playmode == 1 ? $@"putinsquad={q}true{q} gameid={q}{ServerID}{q}" : string.Empty;
                        string ps = Uri.EscapeDataString($@"-webMode {levelmode.ToUpper()} -Origin_NoAppFocus -loginToken WAHAHA_IMMA_ZLO_TOKEN -requestState {requestState} -requestStateParams ""<data {pwExpression} {gameIDstr} role={q}soldier{q} personaref={q}{PlayerID}{q} levelmode={q}{levelmode}{q} logintoken={q}WAHAHA_IMMA_ZLO_TOKEN{q}></data>""");
                        //state 1 = from path
                        //state 2 = from origin2
                        int state = 2;
                        if (File.Exists("bf3.exe"))
                        {
                            final = new ProcessStartInfo(Path.GetFullPath("bf3.exe"));
                            state = 1;
                        }
                        else
                        {
                            try
                            {
                                //check registry 
                                using (var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\EA Games\Battlefield 3"))
                                {
                                    if (reg != null)
                                    {
                                        var val = reg.GetValue("Install Dir", null) as string;
                                        if (string.IsNullOrWhiteSpace(val))
                                        {
                                            state = 2;
                                            //doesn't exist in registry                                            
                                        }
                                        else
                                        {
                                            string bf3Path = Path.Combine(val, "bf3.exe");
                                            if (File.Exists(bf3Path))
                                            {
                                                state = 1;
                                                final = new ProcessStartInfo(bf3Path);
                                            }
                                            else
                                            {
                                                state = 2;
                                            }
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                state = 2;
                            }
                        }

                        if (state == 2)
                        {
                            final = new ProcessStartInfo($@"origin2://game/launch/?offerIds={bf3offers}&title={title}&cmdParams={ps}");
                        }
                        else if (state == 1)
                        {
                            final.Arguments = Uri.UnescapeDataString(ps);
                        }
                        return final;
                    }
                case ZloGame.BF_4:
                    {
                        title = "Battlefield4";
                        string bf4offers = "1007968,1011575,1011576,1011577,1010268,1010269,1010270,1010271,1010958,1010959,1010960,1010961,1007077,1016751,1016757,1016754,1015365,1015364,1015363,1015362";
                        switch (playmode)
                        {
                            case 0:
                                //single
                                return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20SP%20-Origin_NoAppFocus%20-requestState%20State_ResumeCampaign%20-requestStateParams%20%22%3Cdata%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22sp%5C%22%3E%3C/data%3E%22");
                            case 1:
                                //multi
                                if (pw != "")
                                {
                                    return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20password%3D%5C%22{pw}%5C%22%20putinsquad%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22soldier%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                                }
                                else
                                {
                                    return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20putinsquad%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22soldier%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                                }
                            case 2:
                                //commander
                                if (pw != "")
                                {
                                    return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20password%3D%5C%22{pw}%5C%22%20putinsquad%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22commander%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                                }
                                else
                                {
                                    return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20putinsquad%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22commander%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                                }
                            case 3:
                                //spectator
                                if (pw != "")
                                {
                                    return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20password%3D%5C%22{pw}%5C%22%20putinsquad%3D%5C%22true%5C%22%20isspectator%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22soldier%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                                }
                                else
                                {

                                    return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20putinsquad%3D%5C%22true%5C%22%20isspectator%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22soldier%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                                }
                            case 4:
                                //test range
                                return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20SP%20-Origin_NoAppFocus%20-requestState%20State_LaunchPlayground%20-requestStateParams%20%22%3Cdata%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                            case 5:
                                //co-op
                                //currently returns single player
                                return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20SP%20-Origin_NoAppFocus%20-requestState%20State_ResumeCampaign%20-requestStateParams%20%22%3Cdata%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22sp%5C%22%3E%3C/data%3E%22");
                            default:
                                //default is single player
                                return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20SP%20-Origin_NoAppFocus%20-requestState%20State_ResumeCampaign%20-requestStateParams%20%22%3Cdata%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22sp%5C%22%3E%3C/data%3E%22");
                        }
                    }
                case ZloGame.BF_HardLine:
                    {
                        //title = BattlefieldHardline
                        //1013920
                        title = "BattlefieldHardline";
                        string bfhoffers = "1013920";
                        switch (playmode)
                        {
                            case 0:
                                //single
                                return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bfhoffers}&title={title}&cmdParams=-webMode%20SP%20-Origin_NoAppFocus%20-requestState%20State_ResumeCampaign%20-requestStateParams%20%22%3Cdata%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22sp%5C%22%3E%3C/data%3E%22");
                            case 1:
                                //multi
                                if (pw != "")
                                {
                                    return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bfhoffers }&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20password%3D%5C%22{pw}%5C%22%20putinsquad%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22soldier%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                                }
                                else
                                {
                                    return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bfhoffers }&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20putinsquad%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22soldier%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                                }
                            case 2:
                                //commander
                                if (pw != "")
                                {
                                    return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bfhoffers }&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20password%3D%5C%22{pw}%5C%22%20putinsquad%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22commander%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                                }
                                else
                                {
                                    return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bfhoffers }&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20putinsquad%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22commander%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                                }
                            case 3:
                                //spectator
                                if (pw != "")
                                {
                                    return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bfhoffers }&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20password%3D%5C%22{pw}%5C%22%20putinsquad%3D%5C%22true%5C%22%20isspectator%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22soldier%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                                }
                                else
                                {

                                    return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bfhoffers }&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20putinsquad%3D%5C%22true%5C%22%20isspectator%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22soldier%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                                }
                            case 4:
                                //test range
                                return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bfhoffers }&title={title}&cmdParams=-webMode%20SP%20-Origin_NoAppFocus%20-requestState%20State_LaunchPlayground%20-requestStateParams%20%22%3Cdata%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                            case 5:
                                //co-op
                                //currently returns single player
                                return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bfhoffers }&title={title}&cmdParams=-webMode%20SP%20-Origin_NoAppFocus%20-requestState%20State_ResumeCampaign%20-requestStateParams%20%22%3Cdata%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22sp%5C%22%3E%3C/data%3E%22");
                            default:
                                //default is single player
                                return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bfhoffers }&title={title}&cmdParams=-webMode%20SP%20-Origin_NoAppFocus%20-requestState%20State_ResumeCampaign%20-requestStateParams%20%22%3Cdata%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22sp%5C%22%3E%3C/data%3E%22");
                        }
                    }
                case ZloGame.None:
                    return null;
                default:
                    return null;
            }
        }
        #endregion
    }
}
