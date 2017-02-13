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
            if (Game == Zlo.Extras.ZloGame.BF_4)
            {
                Dispatcher.Invoke(() => { StatsDG.ItemsSource = List; });
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
    }
}
