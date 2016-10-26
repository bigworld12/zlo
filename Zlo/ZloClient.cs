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
using SimpleTCP;
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
                m_client = new SimpleTcpClient();
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

                ListenerClient.DataReceived += ListenerClient_DataReceived;
                //uint UserID , string UserName
                UserInfoReceived += ZloClient_UserInfoReceived;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
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
        private SimpleTcpClient m_client;
        private SimpleTcpClient ListenerClient
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

        #endregion

        #region API events
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
        bool IsPingTimerStarted = false;
        public bool Connect()
        {
            try
            {

                ListenerClient.Connect("127.0.0.1" , 48486);

                GetUserInfo();
                return true;
            }
            catch (Exception ex)
            {
                PingTimer.Elapsed -= PingTimer_Elapsed;
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
        #endregion

        #region Other Methods

        int temppid = -1;
        private void ListenerClient_DataReceived(object sender , Message e)
        {
            var bytes = e.Data;
            if (CurrentRequest.IsRespondable && CurrentRequest.IsDone == false)
            {
                CurrentRequest.IsDone = true;
                CurrentRequest.Responce = bytes;
                ProcessQueue();
            }
            Debug.WriteLine($"RECV : {e.Data.Length}");
            using (MemoryStream tempstream = new MemoryStream(bytes))
            using (BinaryReader br = new BinaryReader(tempstream))
            {
                try
                {
                    if (bytes.Length > 0)
                    {
                        byte pid;
                        if (temppid != -1)
                        {
                            pid = (byte)temppid;
                            temppid = -1;
                        }
                        else
                        {
                            pid = br.ReadByte();
                        }

                        if (bytes.Length < 5)
                        {
                            temppid = pid;
                            return;
                        }
                        uint len = br.ReadZUInt32();
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
                                string bytestr = bytes.Select(x => x.ToString()).Aggregate((x , y) => $"{x};{y}");
                                Console.WriteLine($"Received Ping,Packet Info : \n{bytestr}");
                                break;
                            case 1:
                                {
                                    try
                                    {
                                        uint id = br.ReadZUInt32();
                                        string name = br.ReadZString();

                                        UserInfoReceived?.Invoke(id , name);
                                        if (!IsPingTimerStarted)
                                        {
                                            PingTimer.Elapsed += PingTimer_Elapsed;
                                            PingTimer.Start();

                                            IsPingTimerStarted = true;
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
                                    Console.WriteLine($"ServerList Packet Recv : {bytes.Length}");
                                    Console.WriteLine($"{string.Join(";" , bytes)}");
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
                    ErrorOccured?.Invoke(ex , "Failed to read packet id/Packet length");
                }
            }

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
            using (BinaryReader br = new BinaryReader(tempstream))
            {
                br.ReadBytes(2);
                ushort len = br.ReadUInt16();
                if (readelements < len - 4)
                {
                    Console.WriteLine($"Packet Not Full,Please Report these Information : \ngame = {game.ToString()}\nlen = {len}\nread = {readelements}\npacket = {string.Join(";" , buffer)}");
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
            RequestQueue.Add(req);
            ProcessQueue();
        }
        Request CurrentRequest = null;
        bool IsProcessing = false;
        private void ProcessQueue()
        {
            if (IsProcessing) return;

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
                        try
                        {
                            ListenerClient.Write(req.data);
                        }
                        catch (Exception ex)
                        {
                            ErrorOccured?.Invoke(ex , "Error Occured when sending the Request");
                            req.GiveResponce(null);
                        }
                        if (!CurrentRequest.IsRespondable)
                            req.GiveResponce(null);
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

        public void SubToServerList(ZloGame game)
        {
            var req = new Request();
            req.IsRespondable = true;


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
            req.IsRespondable = true;

            List<byte> ar = new List<byte>();
            ar.Add(3);
            byte[] size = BitConverter.GetBytes(2);
            Array.Reverse(size);
            ar.AddRange(size);
            ar.Add(1);
            ar.Add((byte)game);
            ListenerClient.Write(ar.ToArray());

            req.data = ar.ToArray();
            RequestQueue.Add(req);
            ProcessQueue();
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
