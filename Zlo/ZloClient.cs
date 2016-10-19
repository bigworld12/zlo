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
        /*
         connect to localhost:48486
         packet - 4byte size, 1byte type, payload[size]
         */
        private SimpleTcpClient m_client;
        private SimpleTcpClient ListenerClient
        {
            get { return m_client; }
        }



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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

      

        Thread BF3_Pipe_Listener;
        Thread BF4_Pipe_Listener;
        Thread BFH_Pipe_Listener;

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
                    else { Thread.Sleep(200); }

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

                GameStateReceived?.Invoke(game , first.Trim() , string.IsNullOrWhiteSpace(second) ? string.Empty : second.Trim() );
                return true;
            }
        }

        public void SendRequest(ZloRequest request , ZloGame? game = null)
        {
            List<byte> final = new List<byte>();
            final.Add((byte)request);
            uint size = 0;
            switch (request)
            {
                case ZloRequest.Ping:
                case ZloRequest.User_Info:
                    size = 0;
                    break;

                case ZloRequest.Player_Info:
                case ZloRequest.Server_List:
                case ZloRequest.Stats:
                case ZloRequest.Items:
                    size = 1;
                    break;
                default:
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
            var ar = final.ToArray();

            Console.WriteLine($"Sending Array : {{{string.Join("," , ar)}}}");
            ListenerClient.Write(ar);
        }

        public delegate void StatsReceivedEventHandler(ZloGame Game , List<Stat> List);
        public delegate void ItemsReceivedEventHandler(ZloGame Game , List<Item> List);
        public delegate void UserInfoReceivedEventHandler(uint UserID , string UserName);
        public delegate void ErrorOccuredEventHandler(Exception Error , string CustomMessage);
        public delegate void DisconnectedEventHandler(DisconnectionReasons Reason);
        public delegate void GameStateReceivedEventHandler(ZloGame game , string type , string message);

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

        private void ListenerClient_DataReceived(object sender , Message e)
        {
            var bytes = e.Data;
            Debug.WriteLine($"RECV : {e.Data.Length}");
            using (MemoryStream tempstream = new MemoryStream(bytes))
            using (BinaryReader br = new BinaryReader(tempstream))
            {
                try
                {
                    byte pid = br.ReadByte();
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
                                }
                                catch (Exception ex)
                                {
                                    ErrorOccured?.Invoke(ex , "Failed to Parse user info");
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
                catch (Exception ex)
                {
                    ErrorOccured?.Invoke(ex , "Failed to read packet id/Packet length");
                }
            }

        }

        System.Timers.Timer PingTimer;


        public bool Connect()
        {
            try
            {
                ListenerClient.DataReceived += ListenerClient_DataReceived;
                ListenerClient.Connect("127.0.0.1" , 48486);

                PingTimer.Elapsed += PingTimer_Elapsed;
                PingTimer.Start();
                return true;
            }
            catch (Exception ex)
            {
                PingTimer.Elapsed -= PingTimer_Elapsed;
                ErrorOccured?.Invoke(ex , "Error when connecting");
                return false;
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


    }

}
