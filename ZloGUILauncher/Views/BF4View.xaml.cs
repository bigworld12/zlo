using System;
using System.Collections.Generic;
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
    /// Interaction logic for BF4View.xaml
    /// </summary>
    public partial class BF4View : UserControl
    {
        public BF4View()
        {
            InitializeComponent();
            App.Client.StatsReceived += Client_StatsReceived;
            App.Client.ItemsReceived += Client_ItemsReceived;
            App.Client.ClanDogTagsReceived += Client_ClanDogTagsReceived;
        }

        private void Client_ClanDogTagsReceived(ZloGame game, ushort dogtag1, ushort dogtag2, string clanTag)
        {
            if (game == ZloGame.BF_4)
            {
                Dispatcher.Invoke(() =>
                {
                    BF4_DT1.Text = dogtag1.ToString();
                    BF4_DT2.Text = dogtag2.ToString();
                    BF4_CT.Text = clanTag;
                });

            }
        }

        private void Client_ItemsReceived(ZloGame Game, Dictionary<string, API_Item> List)
        {
            if (Game == ZloGame.BF_4)
            {
                Dispatcher.Invoke(() => { ItemsDG.ItemsSource = List; });

            }
        }

        private void Client_StatsReceived(Zlo.Extras.ZloGame Game, Dictionary<string, float> List)
        {
            if (Game == ZloGame.BF_4)
            {
                Dispatcher.Invoke(() => { StatsListWin.StatsDG.ItemsSource = List; });
            }
        }

        private static BF4StatsListWindow m_StatsListWin;
        public static BF4StatsListWindow StatsListWin
        {
            get
            {
                if (m_StatsListWin == null)
                {
                    m_StatsListWin = new BF4StatsListWindow();
                }
                return m_StatsListWin;
            }
        }

        private static BF4StatsWin m_StatsWin;
        public static BF4StatsWin StatsWin
        {
            get
            {
                if (m_StatsWin == null)
                {
                    m_StatsWin = new BF4StatsWin();
                }
                return m_StatsWin;
            }
        }



        private void StatsRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            App.Client.GetStats(Zlo.Extras.ZloGame.BF_4);
        }

        private void ItemsRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            App.Client.GetItems(Zlo.Extras.ZloGame.BF_4);
        }

        private void JoinSinglePlayerButton_Click(object sender, RoutedEventArgs e)
        {
            App.Client.JoinOfflineGame(Zlo.Extras.OfflinePlayModes.BF4_Single_Player);
        }

        private void JoinTestRangeButton_Click(object sender, RoutedEventArgs e)
        {
            App.Client.JoinOfflineGame(Zlo.Extras.OfflinePlayModes.BF4_Test_Range);
        }

        private void StatsAsListButton_Click(object sender, RoutedEventArgs e)
        {
            StatsListWin.Show();
        }

        private void StatsAsWindowButton_Click(object sender, RoutedEventArgs e)
        {
            StatsWin.Show();
            //StatsWin.BGPlayer.Source = new Uri("Media/bf4/City Background.mp4",uriKind: UriKind.Relative);
        }

        private void SetterButton_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag.ToString();
            switch (tag)
            {
                case "dt1":
                    {
                        if (ushort.TryParse(BF4_DT1.Text, out ushort holder))
                        {
                            App.Client.SetClanDogTags(dt_advanced: holder);
                        }
                        break;
                    }
                case "dt2":
                    {
                        if (ushort.TryParse(BF4_DT2.Text, out ushort holder))
                        {
                            App.Client.SetClanDogTags(dt_basic: holder);
                        }
                        break;
                    }
                case "ct":
                    {
                        App.Client.SetClanDogTags(clantag: BF4_CT.Text);
                        break;
                    }
                case "all":
                    {
                        ushort? finaldt1 = null, finaldt2 = null;
                        if (ushort.TryParse(BF4_DT1.Text, out ushort holderdt1))
                        {
                            finaldt1 = holderdt1;
                        }
                        if (ushort.TryParse(BF4_DT2.Text, out ushort holderdt2))
                        {
                            finaldt2 = holderdt2;
                        }
                        App.Client.SetClanDogTags(finaldt1, finaldt2, BF4_CT.Text);
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
    }
}
