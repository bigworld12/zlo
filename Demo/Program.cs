using System;
using System.Collections.Generic;
using System.Linq;
using Zlo;
using Zlo.Extras;
using HtmlAgilityPack;
using System.Net;
using System.Diagnostics;

namespace Demo
{
    class Program
    {

        public static void Main(string[] args)
        {
            new TestProgram();
            Console.ReadLine();
        }
    }
    public class TestProgram
    {
        public ZloClient Client { get; set; }
        public TestProgram()
        {
            Start();
            ConsoleKeyInfo key;
            while ((key = Console.ReadKey(true)).Key != ConsoleKey.Escape)
            {
                switch (key.Key)
                {
                    case ConsoleKey.S:
                        GetServerList();
                        break;
                    case ConsoleKey.L:
                        int serverid = 2;
                        int playerid = 14;
                        Process.Start($@"origin2://game/launch/?offerIds=1007968,1010268,1010960,1011576,1010959&title=Battlefield4&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20putinsquad%3D%5C%22true%5C%22%20gameid%3D%5C%22{serverid}%5C%22%20role%3D%5C%22soldier%5C%22%20personaref%3D%5C%22{playerid}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                        break;

                    case ConsoleKey.R:
                        {
                            try
                            {
                                Console.WriteLine($"Connected ? {Client.Connect()}");
                            }
                            catch { }
                            break;
                        }
                    case ConsoleKey.I:
                        Client.GetUserInfo();

                        break;
                    case ConsoleKey.NumPad1:
                        Client.GetStats(ZloGame.BF_3);
                        break;
                    case ConsoleKey.NumPad2:
                        Client.GetStats(ZloGame.BF_4);
                        break;
                    case ConsoleKey.NumPad3:
                        Client.GetItems(ZloGame.BF_3);
                        break;
                    case ConsoleKey.NumPad4:
                        Client.SubToServerList(ZloGame.BF_4);
                        break;
                    case ConsoleKey.NumPad5:
                        Client.UnSubServerList(ZloGame.BF_4);
                        break;
                    case ConsoleKey.NumPad6:
                        Process.Start(@"origin2://game/launch/?offerIds=1007968,1010268,1010960,1011576,1010959&title=Battlefield4&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20putinsquad%3D%5C%22true%5C%22%20gameid%3D%5C%225%5C%22%20role%3D%5C%22soldier%5C%22%20personaref%3D%5C%2214%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                        break;
                    case ConsoleKey.NumPad9:
                        Console.Clear();
                        break;
                    default:
                        break;

                }
            }

        }

        public void GetServerList()
        {
            using (WebClient x = new WebClient())
            {

                string page = x.DownloadString(@"http://bf4.zloemu.org/servers");
                var doc = new HtmlDocument();
                doc.LoadHtml(page);

                var table = doc.DocumentNode.SelectSingleNode("//table[@class='table']").Descendants("tr")
                    .Skip(1)
                    .Where(tr => tr.Elements("td").Count() > 1)
                    .Select(tr =>
                    {
                        var tdlist = tr.Elements("td");
                        List<string> Final = new List<string>();
                        Final.AddRange(tdlist.Select(td => td.InnerText.Trim()));


                        var anode = tdlist.Last().SelectSingleNode("//a[@class='btn']");
                        if (anode != null) Final.Add(anode.InnerText);
                        return Final;
                    })
                    .ToList();

                Console.WriteLine($"Got {table.Count} elements");
                Console.WriteLine($"{string.Join(Environment.NewLine , table.Select(z => string.Join(";" , z)))}");

            }

        }

        public void Start()
        {
            Client = new ZloClient();
            Client.ErrorOccured += Client_ErrorOccured;
            Client.Disconnected += Client_Disconnected;

            Client.StatsReceived += Client_StatsReceived;
            Client.UserInfoReceived += Client_UserInfoReceived;
            Client.ItemsReceived += Client_ItemsReceived;
            Client.GameStateReceived += Client_GameStateReceived;

            Client.BF3ServerAdded += Client_BF3ServerAdded;
            Client.BF4ServerAdded += Client_BF4ServerAdded;
            Client.BFHServerAdded += Client_BFHServerAdded;

            Client.BF3ServerUpdated += Client_BF3ServerUpdated;
            Client.BF4ServerUpdated += Client_BF4ServerUpdated;
            Client.BFHServerUpdated += Client_BFHServerUpdated;

            Client.ServerRemoved += Client_ServerRemoved;
            //Client.SendRequest(ZloRequest.User_Info);
            //Client.SendRequest(ZloRequest.Stats , ZloGame.BF_3);
            //Client.SendRequest(ZloRequest.Stats , ZloGame.BF_4);
            //Client.SendRequest(ZloRequest.Items , ZloGame.BF_4);            

        }

        private void Client_ServerRemoved(ZloGame game , uint id , IServerBase server)
        {

        }


        private void Client_BF4ServerAdded(uint id , BF4ServerBase server , bool IsPlayerChangeOnly)
        {
            Console.WriteLine($"Added a new bf4 server,id : {id},server name : {server.GNAM},IsPlayerChangeOnly : {IsPlayerChangeOnly}");
        }
        private void Client_BF4ServerUpdated(uint id , BF4ServerBase server , bool IsPlayerChangeOnly)
        {
            Console.WriteLine($"Updated an existing bf4 server,id : {id},server name : {server.GNAM},IsPlayerChangeOnly : {IsPlayerChangeOnly}");
        }



        private void Client_BF3ServerUpdated(uint id , BF3ServerBase server , bool IsPlayerChangeOnly)
        {
            Console.WriteLine($"Updated an existing bf3 server,id : {id},server name : {server.GNAM},IsPlayerChangeOnly : {IsPlayerChangeOnly}");
        }
        private void Client_BF3ServerAdded(uint id , BF3ServerBase server , bool IsPlayerChangeOnly)
        {
            Console.WriteLine($"Added a new bf3 server,id : {id},server name : {server.GNAM},IsPlayerChangeOnly : {IsPlayerChangeOnly}");
        }


        private void Client_BFHServerAdded(uint id , BFHServerBase server , bool IsPlayerChangeOnly)
        {
        }
        private void Client_BFHServerUpdated(uint id , BFHServerBase server , bool IsPlayerChangeOnly)
        {

        }


        private void Client_GameStateReceived(ZloGame game , string type , string message)
        {
            Console.WriteLine($"{game.ToString().Replace("_" , string.Empty)} : [{type}] {message}");
        }

        private void Client_Disconnected(DisconnectionReasons Reason)
        {
            Console.WriteLine($"Client Disconnected for reason : {Reason.ToString()}");
        }

        private void Client_ItemsReceived(ZloGame Game , List<Item> List)
        {
            Console.WriteLine($"Received Items,Game : {Game.ToString()},Count : {List.Count}");
        }

        private void Client_ErrorOccured(Exception Error , string CustomMessage)
        {
            ZloClient.WriteLog($"Error Occured event :\n{CustomMessage}\n{Error.ToString()}");
//            Console.WriteLine($"Error Occured event :\n{CustomMessage}\n{Error.ToString()}");
        }

        private void Client_UserInfoReceived(uint UserID , string UserName)
        {
            Console.WriteLine($"Received User Info,ID : {UserID},Name : {UserName}");
        }

        private void Client_StatsReceived(ZloGame Game , List<Stat> List)
        {
            Console.WriteLine($"Received Stats,Game : {Game.ToString()},Count : {List.Count}");
        }
    }
}
