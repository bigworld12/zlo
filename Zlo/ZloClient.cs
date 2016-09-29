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

using SimpleTCP;

namespace Zlo
{
    public class ZloClient
    {
        /*
         connect to localhost:48486
         packet - 4byte size, 1byte type, payload[size]
         */
        private SimpleTcpClient m_client;
        public SimpleTcpClient ListenerClient
        {
            get { return m_client; }
        }


        public ZloClient()
        {
            try
            {
                m_client = new SimpleTcpClient();
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
            ListenerClient.Write(ar);
        }



        int ic = 0;
        private void ListenerClient_DataReceived(object sender , Message e)
        {
            var bytes = e.Data;
            ic += 1;
            Console.WriteLine($"RECV {e.Data.Length}");
            File.WriteAllLines($@"E:\TestPackets\Packet {ic}.txt" , bytes.Select(x => x.ToString()));



            using (MemoryStream tempstream = new MemoryStream(bytes))
            using (BinaryReader br = new BinaryReader(tempstream))
            {
                try
                {
                    byte pid = br.ReadByte();
                    uint len = br.ReadZUInt();
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
                                Console.WriteLine($"Receiving {count} Stats");
                                List<Tuple<string , float>> TempStats = new List<Tuple<string , float>>();
                                try
                                {
                                    for (ushort i = 0; i < count; i++)
                                    {

                                        string statname = br.ReadZString();
                                        float statvalue = br.ReadZFloat();
                                        TempStats.Add(new Tuple<string , float>(statname , statvalue));

                                    }
                                }
                                catch { Debug.WriteLine($"Failed To Parse All number ,Base stream pos at : {br.BaseStream.Position}"); }

                                var res = TempStats.Select((x , y) => $"{x.Item1} = {x.Item2}");
                                File.WriteAllLines(@"E:\TestPackets\Stats.txt" , res);

                                Console.WriteLine("Done Parsing Stats");
                                break;
                            }
                        case 5:
                            {
                                byte game = br.ReadByte();
                                ushort count = br.ReadZUInt16();
                                List<Tuple<string , byte>> TempItems = new List<Tuple<string , byte>>();
                                for (ushort i = 0; i < count; i++)
                                {
                                    string statname = br.ReadZString();
                                    byte statvalue = br.ReadByte();

                                    TempItems.Add(new Tuple<string , byte>(statname , statvalue));
                                }

                                var res = TempItems.Select((x , y) => $"{x.Item1} = {x.Item2}");
                                File.WriteAllLines(@"E:\TestPackets\Items.txt",res);
                                break;
                            }
                        default:
                            break;

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

        }

       

        public void Connect()
        {
            ListenerClient.DataReceived += ListenerClient_DataReceived;

            ListenerClient.Connect("127.0.0.1" , 48486);
            ListenerClient.TcpClient.ReceiveBufferSize = 1024;
            Console.WriteLine($"Connected ? {ListenerClient.TcpClient.Connected}");
        }


    }
}
