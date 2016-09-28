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

namespace Zlo
{
    public class ZloClient
    {
        /*
         connect to localhost:48486
         packet - 4byte size, 1byte type, payload[size]
         */
        private EventDrivenTCPClient m_client;
        public EventDrivenTCPClient ListenerClient
        {
            get { return m_client; }
        }

        public UTF8Encoding Enc = new UTF8Encoding();
        public ZloClient()
        {
            try
            {
                m_client = new EventDrivenTCPClient(new IPAddress(new byte[] { 127 , 0 , 0 , 1 }) , 48486 , false);

                ListenerClient.ConnectionStatusChanged += Client_ConnectionStatusChanged;
                ListenerClient.DataReceived += ListenerClient_DataReceived;
                ListenerClient.DataEncoding = Enc;

                ListenerClient.ConnectTimeout = 2000;
                ListenerClient.SendTimeout = 2000;
                ListenerClient.ReceiveTimeout = 2000;


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        public enum ZloRequest : byte
        {
            Ping = 0,
            User_Info = 1,
            Player_Info = 2,
            Server_List = 3,
            Stats = 4,
            Items = 5
        }

        public enum ZloGame : byte
        {
            BF_3 = 0,
            BF_4 = 1,
            BF_HardLine = 2
        }
        public void SendRequest(ZloRequest request , ZloGame game)
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
                final.Add((byte)game);
            }
            var ar = final.ToArray();

            Console.WriteLine($"Sending Array : {{{string.Join("," , ar)}}}");
            ListenerClient.Send(ar);
        }


        public int temppid = -1;
        private void ListenerClient_DataReceived(EventDrivenTCPClient sender , object data)
        {
            var bytes = Enc.GetBytes(data.ToString());

            File.WriteAllLines($@"E:\TestPackets\Stats Full.txt" , bytes.Select(x => x.ToString()));
            //string final = string.Join(Environment.NewLine , bytes);
            //Console.WriteLine($"Received Raw :\n{final}");
            if (bytes.Length == 1)
            {
                //it's a pid
                temppid = bytes[0];
            }
            else
            {
                using (MemoryStream tempstream = new MemoryStream(bytes))
                using (BinaryReader br = new BinaryReader(tempstream))
                {
                    try
                    {
                        uint len = br.ReadZUInt();
                        /*
       0 - ping, just send it every 20secs
1 - userinfo, request - empty payload, responce - 4byte id, string
2 - playerinfo - dogtag, clantag, not done
3 - serverlist, not done
4 - stats, req - 1byte game, resp - 1byte game, 2byte size, (string, float)[size]
5 - items - uint8 game, uint16 count, (string, uint8)[count]
       */
                        switch (temppid)
                        {
                            case 1:
                                {
                                    uint id = br.ReadZUInt();
                                    string name = br.ReadZString();
                                    Console.WriteLine($"User Info Received \nPlayer id : {id},Player name : {name}");
                                    break;
                                }
                            case 4:
                                {

                                    byte game = br.ReadByte();
                                    ushort count = br.ReadZUInt16();
                                    for (ushort i = 0; i < count; i++)
                                    {
                                        try
                                        {
                                            string statname = br.ReadZString();
                                            float statvalue = br.ReadZFloat();
                                            Debug.WriteLine($"statname : {statname},statvalue : {statvalue}");
                                            if (statname == "sc_vehicleah")
                                            {
                                                Console.WriteLine("error here");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine(ex.ToString());
                                            throw;
                                        }
                                    }
                                    break;
                                }
                            case 5:
                                {
                                    byte game = br.ReadByte();
                                    ushort count = br.ReadZUInt16();

                                    for (ushort i = 0; i < count; i++)
                                    {
                                        string statname = br.ReadZString();
                                        float statvalue = br.ReadByte();

                                    }

                                    break;
                                }
                            default:
                                break;

                        }
                        temppid = -1;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
        }



        private void Client_ConnectionStatusChanged(EventDrivenTCPClient sender , EventDrivenTCPClient.ConnectionStatus status)
        {
            if (status == EventDrivenTCPClient.ConnectionStatus.Connected)
            {
                SendRequest(ZloRequest.Stats , ZloGame.BF_3);
                Console.WriteLine($"Log 0 : Client Connection Became : {status}");
            }
        }




        public void Connect()
        {
            ListenerClient.Connect();
        }


    }
}
