using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

using Zlo;

namespace ZloLauncher
{

    public partial class Main_Form : Form
    {
        public ZloClient Client { get; set; }

        public Main_Form()
        {
            InitializeComponent();
            Client = new ZloClient();
            AddEvents();
            Client.Connect();
            //Client.GetUserInfo();

            webBrowser1.DocumentCompleted +=
                (x , y) =>
                {
                    //Check if page is fully loaded or not
                    if (webBrowser1.ReadyState != WebBrowserReadyState.Complete)
                        return;
                    else
                        LoadListFromHTML(webBrowser1.DocumentText);
                };
            webBrowser1.Navigate(@"http://bf4.zloemu.org/servers");
        }

        void AddEvents()
        {
            Client.Disconnected += Client_Disconnected;
            Client.ErrorOccured += Client_ErrorOccured;
            Client.GameStateReceived += Client_GameStateReceived;
            Client.ItemsReceived += Client_ItemsReceived;
            Client.StatsReceived += Client_StatsReceived;
            Client.UserInfoReceived += Client_UserInfoReceived;
        }

        private void Client_UserInfoReceived(uint UserID , string UserName)
        {
            var action = (MethodInvoker)delegate
            {
                PlayerLabel.Text = $"{UserName} ({UserID})";
            };
            //gives id and name
            if (PlayerLabel.InvokeRequired)
                PlayerLabel.Invoke(action);
            else
                action.Invoke();
        }

        private void Client_StatsReceived(Zlo.Extras.ZloGame Game , List<Zlo.Extras.Stat> List)
        {
            this.Invoke((Action)delegate
            {
                StatsForm sf = new StatsForm();
                sf.ShowWithStats(Game , List);
            });
        }

        private void Client_ItemsReceived(Zlo.Extras.ZloGame Game , List<Zlo.Extras.Item> List)
        {
            //useless for now
            this.Invoke((Action)delegate
            {
                StatsForm sf = new StatsForm();
                sf.ShowWithItems(Game , List);
            });
        }



        private void Client_GameStateReceived(Zlo.Extras.ZloGame game , string type , string message)
        {
            var action = (MethodInvoker)delegate
            {
                string final = $"[{type}] {message}".Replace('\0'.ToString() , string.Empty);
                GameStateLabel.Text = final;
            };

            //gives id and name

            if (GameStateLabel.InvokeRequired)
                GameStateLabel.Invoke(action);
            else
                action.Invoke();
        }

        private void Client_ErrorOccured(Exception Error , string CustomMessage)
        {
            MessageBox.Show($"{CustomMessage}\n{Error.ToString()}");
        }

        private void Client_Disconnected(Zlo.Extras.DisconnectionReasons Reason)
        {
            MessageBox.Show($"Client Disconnected,Reason : {Reason.ToString()}");
        }

        public ServerList Servers { get; set; }

        public void LoadListFromHTML(string html)
        {

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            var table = doc.DocumentNode.SelectSingleNode("//table[@class='table']").Descendants("tr")
                .Skip(1)
                .Where(tr => tr.Elements("td").Count() > 1)
                .Select(tr =>
                {
                    var tdlist = tr.Elements("td");
                    List<string> Final = new List<string>();
                    Final.AddRange(tdlist.Select(td =>
                    {
                        var desc = td.Descendants("a");
                        if (desc != null && desc.Count() != 0)
                        {
                            return desc.FirstOrDefault().Attributes["href"].Value;
                        }
                        else
                        {
                            return td.InnerText.Trim();
                        }
                    }));


                    return Final;
                })
                .ToList();

            Servers = new ServerList();
            Servers.ParseTable(table);

            var source = new BindingSource(Servers , null);

            dataGridView1.DataSource = source;
            dataGridView1.Columns[dataGridView1.Columns.Count - 1].Visible = false;
            dataGridView1.Columns[dataGridView1.Columns.Count - 2].Visible = false;

            dataGridView1.Refresh();
        }

        private void RefreshListButton_Click(object sender , EventArgs e)
        {
            webBrowser1.Navigate(@"http://bf4.zloemu.org/servers");
        }

        private void JoinMultiButton_Click(object sender , EventArgs e)
        {
            Server selected = dataGridView1.SelectedRows[0].DataBoundItem as Server;

            if (selected != null)
            {
                Client.JoinOnlineGame(Zlo.Extras.OnlinePlayModes.BF4_Multi_Player , selected.ID);
            }
        }

        private void JoinCommanderButton_Click(object sender , EventArgs e)
        {
            Server selected = dataGridView1.SelectedRows[0].DataBoundItem as Server;

            if (selected != null)
            {
                Client.JoinOnlineGame(Zlo.Extras.OnlinePlayModes.BF4_Commander , selected.ID);
            }
        }

        private void JoinSpectatorButton_Click(object sender , EventArgs e)
        {
            Server selected = dataGridView1.SelectedRows[0].DataBoundItem as Server;

            if (selected != null)
            {
                Client.JoinOnlineGame(Zlo.Extras.OnlinePlayModes.BF4_Spectator , selected.ID);
            }
        }

        private void JoinTestRangeButton_Click(object sender , EventArgs e)
        {
            Client.JoinOfflineGame(Zlo.Extras.OfflinePlayModes.BF4_Test_Range);
        }

        private void JoinSinglePlayerButton_Click(object sender , EventArgs e)
        {
            Client.JoinOfflineGame(Zlo.Extras.OfflinePlayModes.BF4_Single_Player);
        }

        private void GetBF4StatsButton_Click(object sender , EventArgs e)
        {

            Client.GetStats(Zlo.Extras.ZloGame.BF_4);
        }

        private void GetBF4ItemsButton_Click(object sender , EventArgs e)
        {
            Client.GetItems(Zlo.Extras.ZloGame.BF_4);
        }
    }
    public class ServerList : BindingList<Server>
    {
        public void ParseTable(List<List<string>> table)
        {
            foreach (var row in table)
            {
                Add(new Server(row));
            }
        }
    }
    public class Server
    {
        public Server(List<string> row)
        {
            Name = row[0];
            var rawpcount = row[1].Split('/');
            PlayerCount = int.Parse(rawpcount[0]);
            MaxPlayerCount = int.Parse(rawpcount[1]);

            MapName = row[2];
            GameMode = row[3];

            OriginStartUpParams = Uri.UnescapeDataString(row[4]);
            BF4StartupParams = OriginStartUpParams.Substring(OriginStartUpParams.IndexOf(@"&cmdParams=") + @"&cmdParams=".Length);
            var splitArray = Regex.Split(BF4StartupParams , "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            for (int i = 0; i < splitArray.Length; i++)
            {
                string param = splitArray[i];
                if (param == "-requestStateParams")
                {
                    string rawdata =
                        splitArray[i + 1]
                        .Replace(@"\" , string.Empty)
                        .Replace(@"""" , string.Empty)
                        .Replace(@"<data " , string.Empty)
                        .Replace(@"></data>" , string.Empty);

                    var serverparams = rawdata.Split(new[] { ' ' } , StringSplitOptions.RemoveEmptyEntries);
                    foreach (var sparam in serverparams)
                    {
                        if (sparam.StartsWith("gameid"))
                        {
                            var splitted = sparam.Split('=');
                            ID = uint.Parse(splitted[1]);
                            break;
                        }
                    }
                    break;
                }

            }
            //"-webMode MP -Origin_NoAppFocus -requestState State_ClaimReservation -requestStateParams \"<data putinsquad=\\\"true\\\" gameid=\\\"1\\\" role=\\\"soldier\\\" personaref=\\\"14\\\" levelmode=\\\"mp\\\"></data>\""
        }
        public uint ID { get; set; }
        public string Name { get; set; }
        public string MapName { get; set; }
        public string GameMode { get; set; }

        public int PlayerCount { get; set; }
        public int MaxPlayerCount { get; set; }

        public string OriginStartUpParams { get; set; }
        public string BF4StartupParams { get; set; }
    }

}
