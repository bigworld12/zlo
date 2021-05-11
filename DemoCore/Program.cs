using System;
using System.Collections.Generic;
using Zlo;
using Zlo.Extras;
using System.Linq;


namespace DemoCore
{
    class Program
    {
        //the API is combined in this instance,see how to use it below
        public static API_ZloClient Client { get; set; }

        public static List<MyBF4Server> MyBF4ServerList { get; set; }
        public static void Main(string[] args)
        {
            //This is your starting point of the application

            //first[After making sure ZClient.exe is open],you have to initialize the API and/or any other instances you are going to use in your implmentation
            Client = new API_ZloClient();
            MyBF4ServerList = new List<MyBF4Server>();

            //second, subscribe to the events, if you are too lazy you can copy them from down below
            SubscribeToEvents();

            //third, connect to Zclient
            Client.Connect();

            //from this point you have full control over the api, and you can call whatever methods you like


            //Subscripe to servers list
            //only call it ONCE
            Client.SubToServerList(ZloBFGame.BF_3);
            Client.SubToServerList(ZloBFGame.BF_4);
            ////Client.SubToServerList(ZloGame.BF_HardLine);


            Client.RunnableGameListReceived += Client_RunnableGameListReceived;
            Client.RefreshRunnableGamesList();

            ////call this right before closing the application
            ////Client.Close()

            ////this property is updated everytime you reconnect with zclient,it represents the id of the player using the API
            ////Client.CurrentPlayerID

            ////same for player name
            ////Client.CurrentPlayerName

            ////use it as an indicator whether the API is connected to zclient or not
            ////Client.IsConnectedToZCLient

            ////Client.BF3Servers //a list of all bf3 servers
            ////Client.BF4Servers //a list of all bf4 servers
            ////Client.BFHServers //not implmented yet


            ////get items such as unlocks,battlepacks
            ////Client.GetItems(ZloGame.BF_3); Bf3 has no items
            //Client.GetItems(ZloGame.BF_4);
            ////Client.GetItems(ZloGame.BF_HardLine); not implmented yet

            //Client.GetStats(ZloGame.BF_3);
            //Client.GetStats(ZloGame.BF_4);
            ////Client.GetStats(ZloGame.BF_HardLine); not implmented yet

            ////no need to use it as the UserInfoReceived event is raised after connecting to zclient
            ////Client.GetUserInfo();

            ////to join an offline game
            //Client.JoinOfflineGame(OfflinePlayModes.BF3_Single_Player);

            //Client.JoinOfflineGame(OfflinePlayModes.BF4_Single_Player);
            //Client.JoinOfflineGame(OfflinePlayModes.BF4_Test_Range);

            ////Client.JoinOfflineGame(OfflinePlayModes.BFH_Single_Player); not implmented yet


            ////note : COOP in all games isn't yet implmented
            //Client.JoinOnlineGame(OnlinePlayModes.BF4_Multi_Player , 0);           
            ////Client.JoinOnlineGame(OnlinePlayModes.BF4_COOP , 0);
            //Client.JoinOnlineGame(OnlinePlayModes.BF4_Spectator , 0);
            ////joining Commander doesn't work yet
            //Client.JoinOnlineGame(OnlinePlayModes.BF4_Commander , 0);


            ////same as the above, but with extra password parameter
            ////note: you can check if server has a password using the IsPasswordProtected property of the server (more on that later)
            ////Client.JoinOnlineGameWithPassWord([play mode],[server id],[password])

            ////will stop sending server updates
            ////Client.UnSubServerList([game]);

            Console.WriteLine("Press any key to Close the application ...");
            Console.ReadKey();
            //the end of the application
            Client.Dispose();
        }
        private static bool isRunGameListHandled = false;
        private static void Client_RunnableGameListReceived()
        {

            if (isRunGameListHandled) return;
            isRunGameListHandled = true;
            Console.WriteLine("===========Start RunnableGameList=================");
            foreach (var item in Client.RunnableGameList)
            {
                Console.WriteLine($"Friendly name: {item.FriendlyName}, Run name: {item.RunName}, Zname:  {item.ZName}");
            }
            Console.WriteLine("===========End RunnableGameList=================");
            /*
             Friendly name: Battlefield 3T Limited Edition, Run name: Z.BF3, Zname:  Z.BF3
             Friendly name: Battlefield 4T Premium Edition x32, Run name: Z.BF4x32, Zname:  Z.BF4
             Friendly name: Battlefield 4T Premium Edition x64, Run name: Z.BF4x64, Zname:  Z.BF4
             Friendly name: Command & ConquerT Generals and Zero Hour, Run name: Z.CNC.GENERALS, Zname:  Z.CNC.GENERALS
             */
            //Client.SendRunGameRequest(Client.RunnableGameList.First(x => x.RunName == "Z.BF3"));
            Client.JoinOfflineGame(OfflinePlayModes.BF4_Single_Player);
        }

        /*
         How to use servers ?
         > use your own server class which wraps around the API server classes like this
         > then in events create/update/remove the servers from the list
         (see : BF4Servers_ServerUpdated,BF4Servers_ServerRemoved,BF4Servers_ServerAdded)
         */
        public class MyBF4Server
        {
            public API_BF4ServerBase RawServer;

            public MyBF4Server(API_BF4ServerBase raw)
            {
                //note: API_BF4ServerBase,API_BF3ServerBase and API_BFHServerBase are one instance been server
                //so you need to set RawServer one time only and update whenever you want
                RawServer = raw;

                UpdateMyServer();
            }

            //an example 
            public uint ServerID { get; set; }
            public string ServerName { get; set; }

            //here is a list of all needed server properties, others just ignore them
            //RawServer.ServerID //the most important,it represents the server id which you will need when joining the server
            //RawServer.GNAM //server name
            //RawServer.ATTRS //server attributes [punkbuster,farfight,etc..]
            //RawServer.ATTRS_MapRotation //server map rotation
            //RawServer.ATTRS_Settings //server settings
            //RawServer.EXIP //server IP
            //RawServer.EXPORT //server Port
            //RawServer.GSTA //server state (mostly useless, but it's prefered that you only join the server if it has state = 131)
            //RawServer.IsPasswordProtected //checks if the password has a game password
            //RawServer.PCAP[0] //maximum slots,others i am not sure about
            //RawServer.Players //a list of players
            //RawServer.Players.Count //players count
            //RawServer.PMAX //kinda the same as PCAP[0], but use PCAP[0] instead

            public void UpdateMyServer()
            {
                ServerID = RawServer.ServerID;
                ServerName = RawServer.ServerName;
            }
        }


        public static void SubscribeToEvents()
        {
            Client.ConnectionStateChanged += Client_ConnectionStateChanged;
            Client.ErrorOccured += Client_ErrorOccured;
            Client.GameStateReceived += Client_GameStateReceived;
            Client.ItemsReceived += Client_ItemsReceived;
            Client.StatsReceived += Client_StatsReceived;
            Client.UserInfoReceived += Client_UserInfoReceived;
        }
        #region Event Methods
        private static void BF4Servers_ServerUpdated(uint id, API_BF4ServerBase server)
        {
            Console.WriteLine($"BF4 Server Updated : id = {id},server name : {server.ServerName},Players = {server.Players.Count}");

            var MyServer = MyBF4ServerList.Find(x => x.ServerID == id);
            if (MyServer != null)
            {
                MyServer.UpdateMyServer();
            }
        }
        private static void BF4Servers_ServerRemoved(uint id, API_BF4ServerBase server)
        {
            Console.WriteLine($"BF4 Server Removed : id = {id},server name : {(server == null ? string.Empty : server.ServerName)},Players = {server.Players.Count}");

            if (MyBF4ServerList.Any(x => x.ServerID == id))
            {
                var MyServer = MyBF4ServerList.Find(x => x.ServerID == id);
                if (MyServer != null)
                {
                    MyBF4ServerList.Remove(MyServer);
                }
            }
        }
        private static void BF4Servers_ServerAdded(uint id, API_BF4ServerBase server)
        {
            Console.WriteLine($"BF4 Server Added : id = {id},server name : {server.ServerName},Players = {server.Players.Count}");

            var MyServer = new MyBF4Server(server);
            if (!MyBF4ServerList.Any(x => x.ServerID == id))
            {
                MyBF4ServerList.Add(MyServer);
            }
        }


        //Note : this method gets called everytime you connect to zclient
        private static void Client_UserInfoReceived(uint UserID, string UserName)
        {

            Console.WriteLine($"User Info : ID = {UserID},Name = {UserName}");
        }


        private static void Client_StatsReceived(ZloBFGame Game, Dictionary<string, float> List)
        {
            Console.WriteLine($"Stats Received for game : {Game},count = {List.Count}");
        }
        private static void Client_ItemsReceived(ZloBFGame Game, Dictionary<string, API_Item> List)
        {
            Console.WriteLine($"Items Received for game : {Game},count = {List.Count}");
        }


        private static void Client_GameStateReceived(ZloBFGame game, string type, string message)
        {
            Console.WriteLine($"[{game}] [{type}] {message}");
        }
        private static void Client_ErrorOccured(Exception Error, string CustomMessage)
        {
            Console.WriteLine($"{CustomMessage}\n{Error.ToString()}");
        }
        private static void Client_ConnectionStateChanged(bool IsConnectedToZloClient)
        {
            Console.WriteLine($"IsConnectedToZloClient ? : {IsConnectedToZloClient}");
        }
        private static void Client_APIVersionReceived(Version Current, Version Latest, bool IsNeedUpdate, string DownloadAdress)
        {
            Console.WriteLine("===================================================");
            Console.WriteLine($"Current version : {Current}\nServer Version : {Latest}\nIs my version old ? : {IsNeedUpdate}\nIf it's old where should i download new version ? {DownloadAdress}");
            Console.WriteLine("===================================================");
        }
        #endregion
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

            Client.SubToServerList(ZloBFGame.BF_3);
            Client.SubToServerList(ZloBFGame.BF_4);

            Client.GetItems(ZloBFGame.BF_3);
            Client.GetItems(ZloBFGame.BF_4);

            Client.GetStats(ZloBFGame.BF_3);
            Client.GetStats(ZloBFGame.BF_4);
        }
        public void SubscribeEvents()
        {
            Client.ConnectionStateChanged += Client_ConnectionStateChanged;
            Client.ErrorOccured += Client_ErrorOccured;
            Client.GameStateReceived += Client_GameStateReceived;
            Client.ItemsReceived += Client_ItemsReceived;
            Client.StatsReceived += Client_StatsReceived;
            Client.UserInfoReceived += Client_UserInfoReceived;
        }

        private void BF4Servers_ServerUpdated(uint id, API_BF4ServerBase server)
        {
            Console.WriteLine($"BF4 Server Updated : id = {id},server name : {server.ServerName}");
        }
        private void BF4Servers_ServerRemoved(uint id, API_BF4ServerBase server)
        {
            Console.WriteLine($"BF4 Server Removed : id = {id},server name : {(server == null ? string.Empty : server.ServerName)}");
        }
        private void BF4Servers_ServerAdded(uint id, API_BF4ServerBase server)
        {
            Console.WriteLine($"BF4 Server Added : id = {id},server name : {server.ServerName}");
        }



        private void BF3Servers_ServerUpdated(uint id, API_BF3ServerBase server)
        {
            Console.WriteLine($"BF3 Server Updated : id = {id},server name : {server.ServerName}");
        }
        private void BF3Servers_ServerRemoved(uint id, API_BF3ServerBase server)
        {
            Console.WriteLine($"BF3 Server Removed : id = {id},server name : {(server == null ? string.Empty : server.ServerName)}");
        }
        private void BF3Servers_ServerAdded(uint id, API_BF3ServerBase server)
        {
            Console.WriteLine($"BF3 Server Added : id = {id},server name : {server.ServerName}");
        }



        private void Client_UserInfoReceived(uint UserID, string UserName)
        {
            Console.WriteLine($"User Info : ID = {UserID},Name = {UserName}");
        }


        private void Client_StatsReceived(ZloBFGame Game, Dictionary<string, float> List)
        {
            Console.WriteLine($"Stats Received for game : {Game},count = {List.Count}");
        }
        private void Client_ItemsReceived(ZloBFGame Game, Dictionary<string, API_Item> List)
        {
            Console.WriteLine($"Items Received for game : {Game},count = {List.Count}");
        }


        private void Client_GameStateReceived(ZloBFGame game, string type, string message)
        {
            Console.WriteLine($"[{game}] [{type}] {message}");
        }
        private void Client_ErrorOccured(Exception Error, string CustomMessage)
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
        private void Client_APIVersionReceived(Version Current, Version Latest, bool IsNeedUpdate, string DownloadAdress)
        {
            Console.WriteLine("===================================================");
            Console.WriteLine($"Current version : {Current}\nServer Version : {Latest}\nIs my version old ? : {IsNeedUpdate}\nIf so where should i download new version ? {DownloadAdress}");
            Console.WriteLine("===================================================");
        }
    }
}

