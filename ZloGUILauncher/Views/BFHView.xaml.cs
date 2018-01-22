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
    /// Interaction logic for BFHView.xaml
    /// </summary>
    public partial class BFHView : UserControl
    {
        public BFHView()
        {
            InitializeComponent();
            App.Client.StatsReceived += Client_StatsReceived;
            App.Client.ItemsReceived += Client_ItemsReceived;
            App.Client.ClanDogTagsReceived += Client_ClanDogTagsReceived;
        }

        private void Client_ClanDogTagsReceived(ZloGame game, ushort dogtag1, ushort dogtag2, string clanTag)
        {
            if (game == ZloGame.BF_HardLine)
            {
                Dispatcher.Invoke(() =>
                {
                    BFH_DT1.Text = dogtag1.ToString();
                    BFH_DT2.Text = dogtag2.ToString();
                    BFH_CT.Text = clanTag;
                });

            }
        }

        private void Client_ItemsReceived(ZloGame Game, Dictionary<string, API_Item> List)
        {
            if (Game == ZloGame.BF_HardLine)
            {
                Dispatcher.Invoke(() => { ItemsDG.ItemsSource = List; });

            }
        }

        private void Client_StatsReceived(ZloGame Game, Dictionary<string, float> List)
        {
            if (Game == ZloGame.BF_HardLine)
            {
                Dispatcher.Invoke(() => { StatsListWin.StatsDG.ItemsSource = List; });
            }
        }

        private static StatsListWindow m_StatsListWin;
        public static StatsListWindow StatsListWin
        {
            get
            {
                if (m_StatsListWin is null)
                {
                    m_StatsListWin = new StatsListWindow();
                }
                return m_StatsListWin;
            }
        }
        private void StatsRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            App.Client.GetStats(ZloGame.BF_HardLine);
        }

        private void ItemsRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            App.Client.GetItems(ZloGame.BF_HardLine);
        }

        private void JoinSinglePlayerButton_Click(object sender, RoutedEventArgs e)
        {
            App.Client.JoinOfflineGame(Zlo.Extras.OfflinePlayModes.BFH_Single_Player);
        }
        
        private void StatsAsListButton_Click(object sender, RoutedEventArgs e)
        {
            StatsListWin.Show();
        }
        

        private void SetterButton_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag.ToString();
            switch (tag)
            {
                case "dt1":
                    {
                        if (ushort.TryParse(BFH_DT1.Text, out ushort holder))
                        {
                            App.Client.SetClanDogTags(dt_advanced: holder);
                        }
                        break;
                    }
                case "dt2":
                    {
                        if (ushort.TryParse(BFH_DT2.Text, out ushort holder))
                        {
                            App.Client.SetClanDogTags(dt_basic: holder);
                        }
                        break;
                    }
                case "ct":
                    {
                        App.Client.SetClanDogTags(clantag: BFH_CT.Text);
                        break;
                    }
                case "all":
                    {
                        ushort? finaldt1 = null, finaldt2 = null;
                        if (ushort.TryParse(BFH_DT1.Text, out ushort holderdt1))
                        {
                            finaldt1 = holderdt1;
                        }
                        if (ushort.TryParse(BFH_DT2.Text, out ushort holderdt2))
                        {
                            finaldt2 = holderdt2;
                        }
                        App.Client.SetClanDogTags(finaldt1, finaldt2, BFH_CT.Text);
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
