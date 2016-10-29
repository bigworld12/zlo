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

namespace Zlo
{
    public class ZloClient
    {
        public ZloClient()
        {
            try
            {
                BF3Servers = new List<BF3ServerBase>();
                BF4Servers = new List<BF4ServerBase>();
                BFHServers = new List<BFHServerBase>();

                m_client = new ZloTCPClient(this);
                PingTimer = new System.Timers.Timer(20 * 1000);


                //CreateFile(@"\\\\.\\pipe\\warsaw_snowroller", FileAccess.ReadWrite,0,IntPtr.Zero, FileMode.Open, FileAttributes.Wr)

                BF3_Pipe = new NamedPipeClientStream("." , "venice_snowroller");
                BF4_Pipe = new NamedPipeClientStream("." , "warsaw_snowroller");
                BFH_Pipe = new NamedPipeClientStream("." , "omaha_snowroller");

                BF3_Pipe_Listener = new Thread(BF3_Pipe_Loop);
                BF3_Pipe_Listener.IsBackground = true;

                BF4_Pipe_Listener = new Thread(BF4_Pipe_Loop);
                BF4_Pipe_Listener.IsBackground = true;

                BFH_Pipe_Listener = new Thread(BFH_Pipe_Loop);
                BFH_Pipe_Listener.IsBackground = true;

                BF3_Pipe_Listener.Start();

                BF4_Pipe_Listener.Start();

                BFH_Pipe_Listener.Start();

                ListenerClient.ZloPacketReceived += new ZloTCPClient.ZloPacketReceivedEventHandler(ListenerClient_DataReceived);
                //uint UserID , string UserName
                UserInfoReceived += ZloClient_UserInfoReceived;

            }
            catch (Exception ex)
            {
                WriteLog(ex.ToString());
            }
        }

        private void ZloClient_UserInfoReceived(uint UserID , string UserName)
        {
            PlayerID = UserID;
            UserInfoReceived -= ZloClient_UserInfoReceived;
        }

        #region Properties
        /*
       connect to localhost:48486
       packet - 4byte size, 1byte type, payload[size]
       */
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

        private uint PlayerID { get; set; }

        private List<Request> RequestQueue = new List<Request>();
        #endregion

        #region Delegates

        public delegate void StatsReceivedEventHandler(ZloGame Game , List<Stat> List);
        public delegate void ItemsReceivedEventHandler(ZloGame Game , List<Item> List);
        public delegate void UserInfoReceivedEventHandler(uint UserID , string UserName);
        public delegate void ErrorOccuredEventHandler(Exception Error , string CustomMessage);
        public delegate void DisconnectedEventHandler(DisconnectionReasons Reason);
        public delegate void GameStateReceivedEventHandler(ZloGame game , string type , string message);


        public delegate void BF3ServerEventHandler(uint id , BF3ServerBase server , bool IsPlayerChangeOnly);
        public delegate void BF4ServerEventHandler(uint id , BF4ServerBase server , bool IsPlayerChangeOnly);
        public delegate void BFHServerEventHandler(uint id , BFHServerBase server , bool IsPlayerChangeOnly);

        public delegate void ServerRemovedEventHandler(ZloGame game , uint id , IServerBase server);
        #endregion

        #region API events
        public event BF3ServerEventHandler BF3ServerAdded;
        public event BF4ServerEventHandler BF4ServerAdded;
        public event BFHServerEventHandler BFHServerAdded;

        public event BF3ServerEventHandler BF3ServerUpdated;
        public event BF4ServerEventHandler BF4ServerUpdated;
        public event BFHServerEventHandler BFHServerUpdated;

        public event ServerRemovedEventHandler ServerRemoved;


        /// <summary>
        /// Gets triggered after receiving user stats and passes the game and a list of stats
        /// </summary>    
        public event StatsReceivedEventHandler StatsReceived;

        /// <summary>
        /// Gets triggered after receiving user items and passes the game and a list of items
        /// </summary>    
        public event ItemsReceivedEventHandler ItemsReceived;

        /// <summary>
        /// Gets triggered after receiving user info and passes user id and name
        /// </summary>
        public event UserInfoReceivedEventHandler UserInfoReceived;

        /// <summary>
        /// Gets triggered when an error occurs and passes the exception and a custom message from the dll
        /// </summary>
        public event ErrorOccuredEventHandler ErrorOccured;

        /// <summary>
        /// occurs when the client disconnects from the server
        /// </summary>
        public event DisconnectedEventHandler Disconnected;

        /// <summary>
        /// occurs when the game state changes (connecting to server/closing the server,etc..)
        /// </summary>
        public event GameStateReceivedEventHandler GameStateReceived;
        #endregion

        #region API Methods
        public bool Connect()
        {
            try
            {

                ListenerClient.Connect();

                GetUserInfo();
                return true;
            }
            catch (Exception ex)
            {
                PingTimer.Elapsed -= PingTimer_Elapsed;
                PingTimer.Stop();
                ErrorOccured?.Invoke(ex , "Error when connecting");
                return false;
            }
        }

        public void JoinOnlineGame(OnlinePlayModes playmode , uint serverid = 0)
        {
            string rungame = string.Empty;
            switch (playmode)
            {
                case OnlinePlayModes.BF3_Multi_Player:
                    rungame = GetGameJoinID(ZloGame.BF_3 , PlayerID , serverid , 1);
                    break;
                case OnlinePlayModes.BF3_COOP:
                    rungame = GetGameJoinID(ZloGame.BF_3 , PlayerID , serverid , 5);
                    break;

                case OnlinePlayModes.BF4_Multi_Player:
                    rungame = GetGameJoinID(ZloGame.BF_4 , PlayerID , serverid , 1);
                    break;
                case OnlinePlayModes.BF4_Spectator:
                    rungame = GetGameJoinID(ZloGame.BF_4 , PlayerID , serverid , 3);
                    break;
                case OnlinePlayModes.BF4_Commander:
                    rungame = GetGameJoinID(ZloGame.BF_4 , PlayerID , serverid , 2);
                    break;
                case OnlinePlayModes.BF4_COOP:
                    rungame = GetGameJoinID(ZloGame.BF_4 , PlayerID , serverid , 5);
                    break;

                case OnlinePlayModes.BFH_Multi_Player:
                    rungame = GetGameJoinID(ZloGame.BF_HardLine , PlayerID , serverid , 1);
                    break;
                default:
                    return;
            }
            if (string.IsNullOrWhiteSpace(rungame))
            {
                return;
            }
            else
            {
                Process.Start(rungame);
            }
        }
        public void JoinOfflineGame(OfflinePlayModes playmode)
        {
            string rungame = string.Empty;
            switch (playmode)
            {
                case OfflinePlayModes.BF3_Single_Player:
                    rungame = GetGameJoinID(ZloGame.BF_3 , PlayerID , 0 , 0);
                    break;

                case OfflinePlayModes.BF4_Single_Player:
                    rungame = GetGameJoinID(ZloGame.BF_4 , PlayerID , 0 , 0);
                    break;
                case OfflinePlayModes.BF4_Test_Range:
                    rungame = GetGameJoinID(ZloGame.BF_4 , PlayerID , 0 , 4);
                    break;


                case OfflinePlayModes.BFH_Single_Player:
                    rungame = GetGameJoinID(ZloGame.BF_HardLine , PlayerID , 0 , 0);
                    break;

                default:
                    return;
            }


            if (string.IsNullOrWhiteSpace(rungame))
            {
                return;
            }
            else
            {
                Process.Start(rungame);
            }
        }

        public void GetUserInfo()
        {
            SendRequest(ZloRequest.User_Info);
        }
        public void GetStats(ZloGame game)
        {
            SendRequest(ZloRequest.Stats , game);
        }
        public void GetItems(ZloGame game)
        {
            SendRequest(ZloRequest.Items , game);
        }

        public void SubToServerList(ZloGame game)
        {
            var req = new Request();
            req.IsRespondable = false;

            req.pid = 3;

            List<byte> ar = new List<byte>();
            ar.Add(3);
            byte[] size = BitConverter.GetBytes(2);
            Array.Reverse(size);
            ar.AddRange(size);
            ar.Add(0);
            ar.Add((byte)game);

            req.data = ar.ToArray();
            RequestQueue.Add(req);
            ProcessQueue();
        }
        public void UnSubServerList(ZloGame game)
        {
            var req = new Request();
            req.IsRespondable = false;

            req.pid = 3;

            List<byte> ar = new List<byte>();
            ar.Add(3);
            byte[] size = BitConverter.GetBytes(2);
            Array.Reverse(size);
            ar.AddRange(size);
            ar.Add(1);
            ar.Add((byte)game);

            req.data = ar.ToArray();
            RequestQueue.Add(req);
            ProcessQueue();
        }
        #endregion

        #region API Properties
        public List<BF3ServerBase> BF3Servers { get; set; }
        public List<BF4ServerBase> BF4Servers { get; set; }
        public List<BFHServerBase> BFHServers { get; set; }
        #endregion

        #region Other Methods
        public void RaiseError(Exception ex , string message)
        {
            ErrorOccured?.Invoke(ex , message);
        }

        public static void WriteLog(string log)
        {
            try
            {
                File.AppendAllText(@".\Demo-Log.txt" , log);
            }
            catch
            {
            }
            Console.WriteLine(log);
        }

        private void ListenerClient_DataReceived(byte pid , byte[] bytes)
        {
            hexlike(bytes , bytes.Length);
            if (CurrentRequest != null && CurrentRequest.pid == pid && CurrentRequest.IsDone == false && CurrentRequest.IsRespondable)
            {
                CurrentRequest.GiveResponce(bytes);
            }
            using (MemoryStream tempstream = new MemoryStream(bytes))
            using (BinaryReader br = new BinaryReader(tempstream , Encoding.ASCII))
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
                         3 - serverlist, not done
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

                                        UserInfoReceived?.Invoke(id , name);
                                        if (!PingTimer.Enabled)
                                        {
                                            PingTimer.Elapsed -= PingTimer_Elapsed;
                                            PingTimer.Elapsed += PingTimer_Elapsed;
                                            PingTimer.Start();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ErrorOccured?.Invoke(ex , "Failed to Parse user info");
                                    }
                                    break;
                                }
                            case 3:
                                {
                                    try
                                    {
                                        #region Server list packet

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
                                        switch (game)
                                        {
                                            case ZloGame.BF_3:
                                                if (BF3Servers.Any(x => x.ServerID == server_id))
                                                {
                                                    //get the server
                                                    var server = BF3Servers.Find(x => x.ServerID == server_id);
                                                    if (server_event_id == 0)
                                                    {
                                                        server.Parse(bytes_list.Skip(6).ToArray());
                                                        BF3ServerUpdated?.Invoke(server_id , server , false);
                                                    }
                                                    else if (server_event_id == 1)
                                                    {
                                                        server.ParsePlayers(bytes_list.Skip(6).ToArray());
                                                        BF3ServerUpdated?.Invoke(server_id , server , true);
                                                    }
                                                    else
                                                    {
                                                        // 2
                                                        //remove the server
                                                        BF3Servers.Remove(server);
                                                        ServerRemoved?.Invoke(game , server_id , server);
                                                    }
                                                    //inform the user
                                                }
                                                else
                                                {
                                                    if (server_event_id != 2)
                                                    {
                                                        //create a new one
                                                        var server = new BF3ServerBase(server_id);
                                                        BF3Servers.Add(server);
                                                        if (server_event_id == 0)
                                                        {
                                                            server.Parse(bytes_list.Skip(6).ToArray());
                                                            BF3ServerAdded?.Invoke(server_id , server , false);
                                                        }
                                                        else if (server_event_id == 1)
                                                        {
                                                            server.ParsePlayers(bytes_list.Skip(6).ToArray());
                                                            BF3ServerAdded?.Invoke(server_id , server , true);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        //remove the already removed server
                                                        ServerRemoved?.Invoke(game , server_id , null);
                                                    }
                                                }
                                                break;
                                            case ZloGame.BF_4:
                                                //players : 01, bf4 : 01,server id : 00 00 00 01 ,buffer : 00 
                                                if (BF4Servers.Any(x => x.ServerID == server_id))
                                                {
                                                    //get the server
                                                    var server = BF4Servers.Find(x => x.ServerID == server_id);
                                                    if (server_event_id == 0)
                                                    {
                                                        server.Parse(bytes_list.Skip(6).ToArray());
                                                        BF4ServerUpdated?.Invoke(server_id , server , false);
                                                    }
                                                    else if (server_event_id == 1)
                                                    {
                                                        server.ParsePlayers(bytes_list.Skip(6).ToArray());
                                                        BF4ServerUpdated?.Invoke(server_id , server , true);
                                                    }
                                                    else
                                                    {
                                                        // 2
                                                        //remove the server
                                                        BF4Servers.Remove(server);
                                                        ServerRemoved?.Invoke(game , server_id , server);
                                                    }
                                                    //inform the user
                                                }
                                                else
                                                {
                                                    if (server_event_id != 2)
                                                    {
                                                        //create a new one
                                                        var server = new BF4ServerBase(server_id);
                                                        BF4Servers.Add(server);
                                                        if (server_event_id == 0)
                                                        {
                                                            server.Parse(bytes_list.Skip(6).ToArray());
                                                            BF4ServerAdded?.Invoke(server_id , server , false);
                                                        }
                                                        else if (server_event_id == 1)
                                                        {
                                                            server.ParsePlayers(bytes_list.Skip(6).ToArray());
                                                            BF4ServerAdded?.Invoke(server_id , server , true);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        //remove the already removed server
                                                        ServerRemoved?.Invoke(game , server_id , null);
                                                    }
                                                }
                                                break;
                                            case ZloGame.BF_HardLine:
                                                if (BFHServers.Any(x => x.ServerID == server_id))
                                                {
                                                    //get the server
                                                    var server = BFHServers.Find(x => x.ServerID == server_id);
                                                    if (server_event_id == 0)
                                                    {
                                                        server.Parse(bytes_list.Skip(6).ToArray());
                                                        BFHServerUpdated?.Invoke(server_id , server , false);
                                                    }
                                                    else if (server_event_id == 1)
                                                    {
                                                        server.ParsePlayers(bytes_list.Skip(6).ToArray());
                                                        BFHServerUpdated?.Invoke(server_id , server , true);
                                                    }
                                                    else
                                                    {
                                                        // 2
                                                        //remove the server
                                                        BFHServers.Remove(server);
                                                        ServerRemoved?.Invoke(game , server_id , server);
                                                    }
                                                    //inform the user
                                                }
                                                else
                                                {
                                                    if (server_event_id != 2)
                                                    {
                                                        //create a new one
                                                        var server = new BFHServerBase(server_id);
                                                        BFHServers.Add(server);
                                                        if (server_event_id == 0)
                                                        {
                                                            server.Parse(bytes_list.Skip(6).ToArray());
                                                            BFHServerAdded?.Invoke(server_id , server , false);
                                                        }
                                                        else if (server_event_id == 1)
                                                        {
                                                            server.ParsePlayers(bytes_list.Skip(6).ToArray());
                                                            BFHServerAdded?.Invoke(server_id , server , true);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        //remove the already removed server
                                                        ServerRemoved?.Invoke(game , server_id , null);
                                                    }
                                                }
                                                break;
                                            default:
                                                break;
                                        }

                                        #endregion
                                    }
                                    catch (Exception ex)
                                    {
                                        ErrorOccured?.Invoke(ex , "Error when Parsing server list packet");
                                    }
                                    break;
                                }
                            case 4:
                                {
                                    byte game = br.ReadByte();
                                    ushort count = br.ReadZUInt16();
                                    List<Stat> FinalStats = new List<Stat>();

                                    try
                                    {
                                        for (ushort i = 0; i < count; i++)
                                        {
                                            string statname = br.ReadZString();
                                            float statvalue = br.ReadZFloat();

                                            FinalStats.Add(new Stat(statname , statvalue));
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ErrorOccured?.Invoke(ex , $"Failed To Parse All Stats ,Base stream pos at : {br.BaseStream.Position},Successfully parsed '{FinalStats.Count}' stats");
                                    }
                                    StatsReceived?.Invoke((ZloGame)game , FinalStats);
                                    break;
                                }
                            case 5:
                                {
                                    byte game = br.ReadByte();
                                    ushort count = br.ReadZUInt16();
                                    List<Item> FinalItems = new List<Item>();
                                    try
                                    {
                                        for (ushort i = 0; i < count; i++)
                                        {
                                            string statname = br.ReadZString();
                                            byte statvalue = br.ReadByte();

                                            FinalItems.Add(new Item(statname , statvalue == 1 ? true : false));
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ErrorOccured?.Invoke(ex , $"Failed To Parse All Items ,Base stream pos at : {br.BaseStream.Position},Successfully parsed '{FinalItems.Count}' Items");
                                    }
                                    ItemsReceived?.Invoke((ZloGame)game , FinalItems);
                                    break;
                                }
                            default:
                                break;

                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorOccured?.Invoke(ex , "Failed to parse packet");
                }
            }
            if (CurrentRequest.IsRespondable && CurrentRequest.IsDone == false)
            {
                CurrentRequest.IsDone = true;
                CurrentRequest.Responce = bytes;
                ProcessQueue();
            }
        }

        public static void hexlike(byte[] buf , int size)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('\n');
            sb.AppendLine($"STORAGE_SIZE: {size}");
            uint j;
            for (uint i = 0; i < size; i += 16)
            {
                for (j = 0; j < 16; j++)
                    if (i + j < size)
                        sb.AppendFormat("{0:X2} " , buf[i + j]);
                    else
                        sb.Append("   ");
                sb.Append(" | ");
                for (j = 0; j < 16; j++)
                    if (i + j < size)
                        sb.Append(isprint(buf[i + j]) ? Convert.ToChar(buf[i + j]) : '.');
                sb.AppendLine();
            }
            WriteLog(sb.ToString());
        }

        ///CharINDec-is the character in ascii
        ///returns true or false.
        ///is char is printable ascii then returns true and if it's not then false
        public static bool isprint(int CharINDec)
        {
            if (CharINDec >= 32 && CharINDec <= 126)
                return true;
            else
                return false;
        }
        private void PingTimer_Elapsed(object sender , ElapsedEventArgs e)
        {
            try
            {
                SendRequest(ZloRequest.Ping);
            }
            catch (Exception ex)
            {
                ErrorOccured?.Invoke(ex , "Error occured when requesting ping");
                PingTimer.Elapsed -= PingTimer_Elapsed;
                PingTimer.Stop();
                Disconnected?.Invoke(DisconnectionReasons.PingFail);
            }
        }

        private void BF3_Pipe_Loop()
        {
            while (!BF3_Pipe.IsConnected)
            {
                try
                {
                    if (NamedPipeExists("venice_snowroller"))
                    {
                        BF3_Pipe.Connect();
                    }
                    else { Thread.Sleep(200); }

                    while (BF3_Pipe.IsConnected)
                    {

                        byte[] buffer = new byte[1024];
                        int read = BF3_Pipe.Read(buffer , 0 , buffer.Length);
                        if (read != 0)
                        {
                            byte[] final = new byte[read + 1];
                            Buffer.BlockCopy(buffer , 0 , final , 0 , read + 1);
                            ProcessPipeMessage(ZloGame.BF_3 , final , read);
                        }
                        Thread.Sleep(50);
                    }
                }
                catch (Exception ex)
                {
                    ErrorOccured?.Invoke(ex , "Error Occured when Trying to connect to BF3 pipe");
                }
            }
        }
        private void BF4_Pipe_Loop()
        {
            while (!BF4_Pipe.IsConnected)
            {
                try
                {
                    if (NamedPipeExists("warsaw_snowroller"))
                    {
                        BF4_Pipe.Connect();
                    }
                    else
                    {
                        Thread.Sleep(200);
                    }

                    while (BF4_Pipe.IsConnected)
                    {
                        byte[] buffer = new byte[1024];
                        int read = BF4_Pipe.Read(buffer , 0 , buffer.Length);
                        if (read != 0)
                        {
                            byte[] final = new byte[read + 1];
                            Buffer.BlockCopy(buffer , 0 , final , 0 , read + 1);
                            ProcessPipeMessage(ZloGame.BF_4 , final , read);
                        }
                        Thread.Sleep(50);
                    }
                }
                catch (Exception ex)
                {
                    ErrorOccured?.Invoke(ex , "Error Occured when Trying to connect to BF4 pipe");

                }
            }
        }
        private void BFH_Pipe_Loop()
        {
            while (!BFH_Pipe.IsConnected)
            {
                try
                {
                    //omaha_snowroller                  
                    if (NamedPipeExists("omaha_snowroller"))
                    {
                        BFH_Pipe.Connect();
                    }
                    else { Thread.Sleep(200); }

                    while (BFH_Pipe.IsConnected)
                    {
                        byte[] buffer = new byte[1024];
                        int read = BFH_Pipe.Read(buffer , 0 , buffer.Length);
                        if (read != 0)
                        {
                            byte[] final = new byte[read + 1];
                            Buffer.BlockCopy(buffer , 0 , final , 0 , read + 1);
                            ProcessPipeMessage(ZloGame.BF_HardLine , final , read);
                        }
                        Thread.Sleep(50);
                    }
                }
                catch (Exception ex)
                {
                    ErrorOccured?.Invoke(ex , "Error Occured when Trying to connect to BF Hardline pipe");
                }


            }
        }


        private bool ProcessPipeMessage(ZloGame game , byte[] buffer , int readelements)
        {
            using (MemoryStream tempstream = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(tempstream , Encoding.ASCII))
            {
                br.ReadBytes(2);
                ushort len = br.ReadUInt16();
                if (readelements < len - 4)
                {
                    WriteLog($"Packet Not Full,Please Report these Information : \ngame = {game.ToString()}\nlen = {len}\nread = {readelements}\npacket = {string.Join(";" , buffer)}");
                    return false;
                    //packet not full
                }
                byte firstlen = br.ReadByte();
                string first = br.ReadCountedString(firstlen);

                byte secondlen = br.ReadByte();
                string second = br.ReadCountedString(secondlen + 1);

                GameStateReceived?.Invoke(game , first.Trim() , string.IsNullOrWhiteSpace(second) ? string.Empty : second.Trim().Replace('\0'.ToString() , string.Empty));
                return true;
            }
        }

        private void SendRequest(ZloRequest request , ZloGame? game = null)
        {
            if (request == ZloRequest.Items && game == ZloGame.BF_3) return;
            List<byte> final = new List<byte>();
            final.Add((byte)request);
            uint size = 0;
            var req = new Request();
            switch (request)
            {
                case ZloRequest.Ping:
                case ZloRequest.User_Info:
                    req.IsRespondable = true;
                    size = 0;
                    break;

                case ZloRequest.Player_Info:
                case ZloRequest.Stats:
                case ZloRequest.Items:
                    req.IsRespondable = true;
                    size = 1;
                    break;
                default:
                    req.IsRespondable = false;
                    break;
            }
            var finalsizebytes = BitConverter.GetBytes(size);
            Array.Reverse(finalsizebytes);
            final.AddRange(finalsizebytes);

            if (size > 0)
            {
                if (game.HasValue)
                {
                    final.Add((byte)game.Value);
                }
            }

            req.data = final.ToArray();
            req.pid = (byte)request;
            RequestQueue.Add(req);
            ProcessQueue();
        }
        Request CurrentRequest = null;
        bool IsProcessing = false;
        private void ProcessQueue()
        {
            if (IsProcessing && (CurrentRequest == null || (CurrentRequest.IsRespondable == true && CurrentRequest.IsDone == false))) return;

            if (RequestQueue.Count == 0) return;
            else
            {
                IsProcessing = true;
                //check the first un-done request and process it
                for (int i = 0; i < RequestQueue.Count; i++)
                {
                    var req = RequestQueue[i];
                    if (req.IsDone)
                    {
                        RequestQueue.Remove(req);
                        i -= 1;
                        continue;
                    }
                    else
                    {
                        CurrentRequest = req;
                        if (!ListenerClient.WritePacket(req.data))
                        {
                            req.GiveResponce(null);
                        }
                        if (!CurrentRequest.IsRespondable)
                        {
                            req.GiveResponce(null);
                        }
                        break;
                    }
                }
                Thread.Sleep(50);
                if (!CurrentRequest.IsRespondable)
                {
                    IsProcessing = false;
                    if (RequestQueue.Count > 0) ProcessQueue();
                }
            }
        }


        //todo
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
        /// <returns></returns>
        private string GetGameJoinID(ZloGame game , uint PlayerID , uint ServerID , int playmode)
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
            string title = string.Empty;
            switch (game)
            {
                case ZloGame.BF_3:
                    {
                        title = "Battlefield3";
                        string bf3offers = "DGR01609244,DGR01609245,70619,71067";
                        switch (playmode)
                        {
                            case 0:
                                //single
                                return $@"origin2://game/launch/?offerIds={bf3offers}&title={title}&cmdParams=-webMode%20SP%20-Origin_NoAppFocus%20-loginToken%20WAHAHA_IMMA_ZLO_TOKEN%20-requestState%20State_ResumeCampaign%20-requestStateParams%20%22%3Cdata%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22sp%5C%22%20logintoken%3D%5C%22WAHAHA_IMMA_ZLO_TOKEN%5C%22%3E%3C/data%3E%22";
                            case 1:
                                //multi
                                return $@"origin2://game/launch/?offerIds={bf3offers}&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-loginToken%20WAHAHA_IMMA_ZLO_TOKEN%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20putinsquad%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22soldier%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%20logintoken%3D%5C%22WAHAHA_IMMA_ZLO_TOKEN%5C%22%3E%3C/data%3E%22";

                            case 5:
                                //co-op
                                //currently returns single player
                                return $@"origin2://game/launch/?offerIds={bf3offers}&title={title}&cmdParams=-webMode%20SP%20-Origin_NoAppFocus%20-loginToken%20WAHAHA_IMMA_ZLO_TOKEN%20-requestState%20State_ResumeCampaign%20-requestStateParams%20%22%3Cdata%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22sp%5C%22%20logintoken%3D%5C%22WAHAHA_IMMA_ZLO_TOKEN%5C%22%3E%3C/data%3E%22";
                            default:
                                //default is single
                                return $@"origin2://game/launch/?offerIds={bf3offers}&title={title}&cmdParams=-webMode%20SP%20-Origin_NoAppFocus%20-loginToken%20WAHAHA_IMMA_ZLO_TOKEN%20-requestState%20State_ResumeCampaign%20-requestStateParams%20%22%3Cdata%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22sp%5C%22%20logintoken%3D%5C%22WAHAHA_IMMA_ZLO_TOKEN%5C%22%3E%3C/data%3E%22";

                        }
                    }
                case ZloGame.BF_4:
                    title = "Battlefield4";
                    string bf4offers = "1007968,1011575,1011576,1011577,1010268,1010269,1010270,1010271,1010958,1010959,1010960,1010961,1007077,1016751,1016757,1016754,1015365,1015364,1015363,1015362";
                    switch (playmode)
                    {
                        case 0:
                            //single
                            return $@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20SP%20-Origin_NoAppFocus%20-requestState%20State_ResumeCampaign%20-requestStateParams%20%22%3Cdata%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22sp%5C%22%3E%3C/data%3E%22";
                        case 1:
                            //multi
                            return $@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20putinsquad%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22soldier%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22";
                        case 2:
                            //commander
                            return $@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20putinsquad%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22commander%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22";
                        case 3:
                            //spectator
                            return $@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20putinsquad%3D%5C%22true%5C%22%20isspectator%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22soldier%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22";
                        case 4:
                            //test range
                            return $@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20SP%20-Origin_NoAppFocus%20-requestState%20State_LaunchPlayground%20-requestStateParams%20%22%3Cdata%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22";
                        case 5:
                            //co-op
                            //currently returns single player
                            return $@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20SP%20-Origin_NoAppFocus%20-requestState%20State_ResumeCampaign%20-requestStateParams%20%22%3Cdata%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22sp%5C%22%3E%3C/data%3E%22";
                        default:
                            //default is single player
                            return $@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20SP%20-Origin_NoAppFocus%20-requestState%20State_ResumeCampaign%20-requestStateParams%20%22%3Cdata%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22sp%5C%22%3E%3C/data%3E%22";
                    }

                case ZloGame.BF_HardLine:
                    //title = BattlefieldHardline
                    //1013920
                    return string.Empty;
                default:
                    return string.Empty;

            }
        }
        #endregion

    }

}
