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
        }

        private void Client_StatsReceived(Zlo.Extras.ZloGame Game , Dictionary<string , float> List)
        {
            if (Game == Zlo.Extras.ZloGame.BF_3)
            {
                Dispatcher.Invoke(() => { StatsDG.ItemsSource = List; });
            }
        }

        private void JoinSPButton_Click(object sender , RoutedEventArgs e)
        {
            App.Client.JoinOfflineGame(Zlo.Extras.OfflinePlayModes.BF3_Single_Player);
        }

        private void StatsRefreshButton_Click(object sender , RoutedEventArgs e)
        {
            App.Client.GetStats(Zlo.Extras.ZloGame.BF_3);
        }
    }
}
