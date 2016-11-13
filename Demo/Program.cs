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
            //using (var wc = new WebClient())
            //{
                
            //    Console.WriteLine(wc.DownloadString(@"https://onedrive.live.com/download?cid=0AF30EAB900CEF1B&resid=AF30EAB900CEF1B%21912&authkey=ANvWvBuvX90-elk"));
            //}
            Console.ReadLine();
            
        }
    }
    public class TestProgram
    {
        public API_ZloClient Client { get; set; }
        public TestProgram()
        {
            Start();
            //ConsoleKeyInfo key;
            //while ((key = Console.ReadKey(true)).Key != ConsoleKey.Escape)
            //{
            //    switch (key.Key)
            //    {
            //        case ConsoleKey.S:

            //            break;
            //        case ConsoleKey.L:
            //            Client.JoinOnlineGame(OnlinePlayModes.BF4_Commander , 3);
            //            break;
            //        case ConsoleKey.R:
            //            {
            //                try
            //                {
            //                    Console.WriteLine($"Connected ? {Client.Connect()}");
            //                }
            //                catch { }
            //                break;
            //            }
            //        case ConsoleKey.I:
            //            Client.GetUserInfo();

            //            break;
            //        case ConsoleKey.NumPad1:
            //            Client.GetStats(ZloGame.BF_3);
            //            break;
            //        case ConsoleKey.NumPad2:
            //            Client.GetStats(ZloGame.BF_4);
            //            break;
            //        case ConsoleKey.NumPad3:
            //            Client.GetItems(ZloGame.BF_3);
            //            break;
            //        case ConsoleKey.NumPad4:
            //            Client.SubToServerList(ZloGame.BF_3);
            //            Client.SubToServerList(ZloGame.BF_4);
            //            break;
            //        case ConsoleKey.NumPad5:
            //            Client.UnSubServerList(ZloGame.BF_3);
            //            Client.UnSubServerList(ZloGame.BF_4);
            //            break;
            //        case ConsoleKey.NumPad6:
            //            Process.Start(@"origin2://game/launch/?offerIds=1007968,1010268,1010960,1011576,1010959&title=Battlefield4&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20putinsquad%3D%5C%22true%5C%22%20gameid%3D%5C%225%5C%22%20role%3D%5C%22soldier%5C%22%20personaref%3D%5C%2214%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
            //            break;
            //        case ConsoleKey.NumPad9:
            //            Console.Clear();
            //            break;
            //        default:
            //            break;

            //    }
            //}
            Console.ReadKey();
        }

       

        public void Start()
        {
            Client = new API_ZloClient();

            SubscribeEvents();

            Client.Connect();
            Client.SubToServerList(ZloGame.BF_3);
            Client.SubToServerList(ZloGame.BF_4);

            Client.GetItems(ZloGame.BF_3);
            Client.GetItems(ZloGame.BF_4);

            Client.GetStats(ZloGame.BF_3);
            Client.GetStats(ZloGame.BF_4);
        }
        public void SubscribeEvents()
        {
            Client.APIVersionReceived += Client_APIVersionReceived;
            Client.ConnectionStateChanged += Client_ConnectionStateChanged;
            Client.Disconnected += Client_Disconnected;
            Client.ErrorOccured += Client_ErrorOccured;
            Client.GameStateReceived += Client_GameStateReceived;
            Client.ItemsReceived += Client_ItemsReceived;
            Client.StatsReceived += Client_StatsReceived;
            Client.UserInfoReceived += Client_UserInfoReceived;

            Client.BF3Servers.ServerAdded += BF3Servers_ServerAdded;
            Client.BF3Servers.ServerRemoved += BF3Servers_ServerRemoved;
            Client.BF3Servers.ServerUpdated += BF3Servers_ServerUpdated;

            Client.BF4Servers.ServerAdded += BF4Servers_ServerAdded;
            Client.BF4Servers.ServerRemoved += BF4Servers_ServerRemoved;
            Client.BF4Servers.ServerUpdated += BF4Servers_ServerUpdated;
        }

        private void BF4Servers_ServerUpdated(uint id , API_BF4ServerBase server)
        {
            Console.WriteLine($"BF4 Server Updated : id = {id},server name : {server.GNAM}");
        }
        private void BF4Servers_ServerRemoved(uint id , API_BF4ServerBase server)
        {
            Console.WriteLine($"BF4 Server Removed : id = {id},server name : {(server == null ? string.Empty : server.GNAM)}");
        }
        private void BF4Servers_ServerAdded(uint id , API_BF4ServerBase server)
        {
            Console.WriteLine($"BF4 Server Added : id = {id},server name : {server.GNAM}");
        }



        private void BF3Servers_ServerUpdated(uint id , API_BF3ServerBase server)
        {
            Console.WriteLine($"BF3 Server Updated : id = {id},server name : {server.GNAM}");
        }
        private void BF3Servers_ServerRemoved(uint id , API_BF3ServerBase server)
        {
            Console.WriteLine($"BF3 Server Removed : id = {id},server name : {(server == null ? string.Empty : server.GNAM)}");
        }
        private void BF3Servers_ServerAdded(uint id , API_BF3ServerBase server)
        {
            Console.WriteLine($"BF3 Server Added : id = {id},server name : {server.GNAM}");
        }



        private void Client_UserInfoReceived(uint UserID , string UserName)
        {
            Console.WriteLine($"User Info : ID = {UserID},Name = {UserName}");
        }


        private void Client_StatsReceived(ZloGame Game , List<API_Stat> List)
        {
            Console.WriteLine($"Stats Received for game : {Game},count = {List.Count}");
        }
        private void Client_ItemsReceived(ZloGame Game , List<API_Item> List)
        {
            Console.WriteLine($"Items Received for game : {Game},count = {List.Count}");
        }


        private void Client_GameStateReceived(ZloGame game , string type , string message)
        {
            Console.WriteLine($"[{game}] [{type}] {message}");
        }
        private void Client_ErrorOccured(Exception Error , string CustomMessage)
        {
            Console.WriteLine($"{CustomMessage}\n{Error.ToString()}");
        }
        private void Client_Disconnected(DisconnectionReasons Reason)
        {
            Console.WriteLine($"Client disconnected for reason : {Reason}");
        }
        private void Client_ConnectionStateChanged(bool IsConnectedToZloClient)
        {
            Console.WriteLine($"IsConnectedToZloClient ? : {IsConnectedToZloClient}");
        }
        private void Client_APIVersionReceived(Version Current , Version Latest , bool IsNeedUpdate , string DownloadAdress)
        {
            Console.WriteLine("===================================================");
            Console.WriteLine($"Current version : {Current}\nServer Version : {Latest}\nIs my version old ? : {IsNeedUpdate}\nIf so where should i download new version ? {DownloadAdress}");
            Console.WriteLine("===================================================");
        }
    }
}
