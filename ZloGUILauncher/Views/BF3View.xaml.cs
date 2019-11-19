using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Zlo.Extras;

namespace ZloGUILauncher.Views
{
    /// <summary>
    /// Interaction logic for BF3View.xaml
    /// </summary>
    public partial class BF3View : UserControl
    {
        public BF3View()
        {
            InitializeComponent();

            App.Client.StatsReceived += Client_StatsReceived;
            App.Client.ClanDogTagsReceived += Client_ClanDogTagsReceived;
        }

        private void Client_ClanDogTagsReceived(Zlo.Extras.ZloBFGame game, ushort dogtag1, ushort dogtag2, string clanTag)
        {

            if (game == Zlo.Extras.ZloBFGame.BF_3)
            {
                Dispatcher.Invoke(() =>
                {
                    BF3_DT1.Text = dogtag1.ToString();
                    BF3_DT2.Text = dogtag2.ToString();
                    BF3_CT.Text = clanTag;
                });
            }
        }

        private void Client_StatsReceived(Zlo.Extras.ZloBFGame Game, Dictionary<string, float> List)
        {
            if (Game == Zlo.Extras.ZloBFGame.BF_3)
            {
                Dispatcher.Invoke(() => { StatsDG.ItemsSource = List; });
            }
        }

        private void JoinSPButton_Click(object sender, RoutedEventArgs e)
        {
            App.Client.JoinOfflineGame(Zlo.Extras.OfflinePlayModes.BF3_Single_Player);
        }

        private void StatsRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            App.Client.GetStats(Zlo.Extras.ZloBFGame.BF_3);
        }

        private void SetterButton_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag.ToString();
            switch (tag)
            {
                case "dt1":
                    {
                        if (ushort.TryParse(BF3_DT1.Text, out ushort holder))
                        {
                            App.Client.SetClanDogTags(dt_advanced: holder);
                        }
                        break;
                    }
                case "dt2":
                    {
                        if (ushort.TryParse(BF3_DT2.Text, out ushort holder))
                        {
                            App.Client.SetClanDogTags(dt_basic: holder);
                        }
                        break;
                    }
                case "ct":
                    {
                        App.Client.SetClanDogTags(clanTag: BF3_CT.Text);
                        break;
                    }
                case "all":
                    {
                        ushort? finaldt1 = null, finaldt2 = null;
                        if (ushort.TryParse(BF3_DT1.Text, out ushort holderdt1))
                        {
                            finaldt1 = holderdt1;
                        }
                        if (ushort.TryParse(BF3_DT2.Text, out ushort holderdt2))
                        {
                            finaldt2 = holderdt2;
                        }
                        App.Client.SetClanDogTags(finaldt1, finaldt2, BF3_CT.Text);
                        break;
                    }
                default:
                    break;
            }
        }

        private void GetterButton_Click(object sender, RoutedEventArgs e)
        {
            App.Client.GetClanDogTags();
        }

        private void HostCOOPButton_Click(object sender, RoutedEventArgs e)
        {
            var level = (BF3_COOP_LEVELS)COOPLevelSelector.SelectedIndex;
            var diff = (COOP_Difficulty)COOPDiffSelector.SelectedIndex;
            var host = App.Client.HostBf3Coop(level, diff);
            if (host != null)
                Process.Start(host);
        }
        private void JoinCOOPButton_Click(object sender, RoutedEventArgs e)
        {
            var friendId = FriendId.Text;
            if (uint.TryParse(friendId, out var fid))
            {
                var join = App.Client.JoinBf3Coop(fid);
                if (join != null)
                    Process.Start(join);
            }
        }
    }
}
