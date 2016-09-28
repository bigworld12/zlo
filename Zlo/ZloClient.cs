using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

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
                ListenerClient.DataReceived += ListenerClient_DataReceived; ;
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
            Stats = 4
        }
       
        public enum ZloGame : byte
        {
            BF_3 = 0,
            BF_4 = 1,
            BF_HardLine = 2
        }
        public void SendRequest(ZloRequest request , ZloGame game)
        {

        }


        public int temppid = -1;
        private void ListenerClient_DataReceived(EventDrivenTCPClient sender , object data)
        {
            var bytes = Enc.GetBytes(data.ToString());
            string final = string.Join(Environment.NewLine , bytes);
            Console.WriteLine($"Received Raw :\n{final}");
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
                        uint len = ReadZUInt(br);

                        switch (temppid)
                        {
                            case 1:
                                uint id = ReadZUInt(br);
                                string name = ReadZString(br);
                                Console.WriteLine($"User Info Received (Packet Size = {len})\nPlayer id : {id},Player name : {name}");
                                break;
                            case 4:
                                byte game = br.ReadByte();
                                ushort count = br.ReadUInt16();
                                for (ushort i = 0; i < count; i++)
                                {
                                    string statname = ReadZString(br);
                                    float statvalue = br.ReadSingle();
                                    Console.WriteLine($"Stats Received  (Packet Size = {len}) \nstatname : {statname},statvalue : {statvalue}");
                                }
                                break;
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
            Console.WriteLine($"Log 0 : Client Connection Became : {status}");
            if (status == EventDrivenTCPClient.ConnectionStatus.Connected)
            {
                ListenerClient.Send(new byte[] { 4 , 0 , 0 , 0 , 0 });
                Console.WriteLine("Done Sending");

            }
        }

        uint ReadZUInt(BinaryReader br)
        {
            var rawidarray = br.ReadBytes(4);
            Array.Reverse(rawidarray);
            return BitConverter.ToUInt32(rawidarray , 0);
        }
        string ReadZString(BinaryReader br)
        {
            StringBuilder s = new StringBuilder();
            char t;
            while ((t = br.ReadChar()) > 0)
                s.Append(t);
            return s.ToString();
        }

        public void Connect()
        {
            ListenerClient.Connect();
        }
        /*
         0 - ping, just send it every 20secs
1 - userinfo, request - empty payload, responce - 4byte id, string
2 - playerinfo - dogtag, clantag, not done
3 - serverlist, not done
4 - stats, req - 1byte game, resp - 1byte game, 2byte size, (string, float)[size]
         */

    }
}
