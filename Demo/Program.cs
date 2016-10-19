using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zlo;
using Zlo.Extras;
using HtmlAgilityPack;
using System.Net;
using System.Threading;

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
                        Client.SendRequest(ZloRequest.User_Info);
                        break;
                    case ConsoleKey.NumPad1:
                        Client.SendRequest(ZloRequest.Stats , ZloGame.BF_3);
                        break;
                    case ConsoleKey.NumPad2:
                        Client.SendRequest(ZloRequest.Stats , ZloGame.BF_4);
                        break;
                    case ConsoleKey.NumPad3:
                        Client.SendRequest(ZloRequest.Items , ZloGame.BF_4);
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
            //Client.SendRequest(ZloRequest.User_Info);
            //Client.SendRequest(ZloRequest.Stats , ZloGame.BF_3);
            //Client.SendRequest(ZloRequest.Stats , ZloGame.BF_4);
            //Client.SendRequest(ZloRequest.Items , ZloGame.BF_4);            

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
            Console.WriteLine($"Error Occured event :\n{CustomMessage}\n{Error.ToString()}");
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
