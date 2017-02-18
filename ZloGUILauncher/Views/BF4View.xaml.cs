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
        }

        private void Client_ItemsReceived(Zlo.Extras.ZloGame Game , Dictionary<string , API_Item> List)
        {
            if (Game == Zlo.Extras.ZloGame.BF_4)
            {
                Dispatcher.Invoke(() => { ItemsDG.ItemsSource = List; });

            }
        }

        private void Client_StatsReceived(Zlo.Extras.ZloGame Game , Dictionary<string , float> List)
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

      

        private void StatsRefreshButton_Click(object sender , RoutedEventArgs e)
        {
            App.Client.GetStats(Zlo.Extras.ZloGame.BF_4);
        }

        private void ItemsRefreshButton_Click(object sender , RoutedEventArgs e)
        {
            App.Client.GetItems(Zlo.Extras.ZloGame.BF_4);
        }

        private void JoinSinglePlayerButton_Click(object sender , RoutedEventArgs e)
        {
            App.Client.JoinOfflineGame(Zlo.Extras.OfflinePlayModes.BF4_Single_Player);
        }

        private void JoinTestRangeButton_Click(object sender , RoutedEventArgs e)
        {
            App.Client.JoinOfflineGame(Zlo.Extras.OfflinePlayModes.BF4_Test_Range);
        }

        private void StatsAsListButton_Click(object sender , RoutedEventArgs e)
        {
            StatsListWin.Show();
        }

        private void StatsAsWindowButton_Click(object sender , RoutedEventArgs e)
        {
            StatsWin.Show();
            //StatsWin.BGPlayer.Source = new Uri("Media/bf4/City Background.mp4",uriKind: UriKind.Relative);
        }
    }
}
