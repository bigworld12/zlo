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

namespace ZloLauncher
{

    public partial class Main_Form : Form
    {

        public Main_Form()
        {
            InitializeComponent();

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

        private void button1_Click(object sender , EventArgs e)
        {
            webBrowser1.Navigate(@"http://bf4.zloemu.org/servers");
        }

        private void JoinServerButton_Click(object sender , EventArgs e)
        {
            Server selected = dataGridView1.SelectedRows[0].DataBoundItem as Server;

            ProcessStartInfo si = new ProcessStartInfo();
            si.WorkingDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            si.Arguments = selected.BF4StartupParams;
            string bf4procname;
            if (Environment.Is64BitOperatingSystem)
            {
                bf4procname = "bf4.exe";
            }
            else
            {
                bf4procname = "bf4_x86.exe";
            }
            si.FileName = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath) , bf4procname);
            Process bf4proc = new Process();
            bf4proc.StartInfo = si;
            bf4proc.Start();
            MessageBox.Show($"Joined the server Successfully\n{selected.Name}");
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
        /*
         [RU] ZloGames Official TEST server
         0/62
         Operation Mortar
         ConquestLarge
         origin2://game/launch/?offerIds=1007968,1010268,1010960,1011576,1010959&title=Battlefield4&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20putinsquad%3D%5C%22true%5C%22%20gameid%3D%5C%221%5C%22%20role%3D%5C%22soldier%5C%22%20personaref%3D%5C%2214%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22
         */
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
                            ID = int.Parse(splitted[1]);
                            break;
                        }
                    }
                    break;
                }

            }
            //"-webMode MP -Origin_NoAppFocus -requestState State_ClaimReservation -requestStateParams \"<data putinsquad=\\\"true\\\" gameid=\\\"1\\\" role=\\\"soldier\\\" personaref=\\\"14\\\" levelmode=\\\"mp\\\"></data>\""
        }
        public int ID { get; set; }
        public string Name { get; set; }
        public string MapName { get; set; }
        public string GameMode { get; set; }

        public int PlayerCount { get; set; }
        public int MaxPlayerCount { get; set; }

        public string OriginStartUpParams { get; set; }
        public string BF4StartupParams { get; set; }
    }

}
