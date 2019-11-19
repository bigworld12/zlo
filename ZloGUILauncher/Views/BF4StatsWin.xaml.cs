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
using System.Windows.Shapes;
using System.ComponentModel;
using Zlo;
using Zlo.Extras;
using Newtonsoft.Json.Linq;

namespace ZloGUILauncher.Views
{
    /// <summary>
    /// Interaction logic for BF4Stats.xaml
    /// </summary>
    public partial class BF4StatsWin : Window
    {
        public BF4StatsWin()
        {
            InitializeComponent();
            DataContext = BF4Stats;
            App.Client.StatsReceived += Client_StatsReceived;
        }

        private void Client_StatsReceived(ZloBFGame Game , Dictionary<string , float> List)
        {
            if (Game == ZloBFGame.BF_4)
            {
                BF4Stats.UpdateObject();
            }
        }

        private void Window_Closing(object sender , CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private static BF4StatsDataContext m_BF4Stats;
        public static BF4StatsDataContext BF4Stats
        {
            get
            {
                if (m_BF4Stats == null)
                {
                    m_BF4Stats = new BF4StatsDataContext();
                }
                return m_BF4Stats;
            }
        }

        private void BackGroundViewer_MediaEnded(object sender , RoutedEventArgs e)
        {
            var me = (MediaElement)sender;
            me.Position = TimeSpan.Zero;
        }

        private void RefreshBorder_MouseDown(object sender , MouseButtonEventArgs e)
        {
            App.Client.GetStats(ZloBFGame.BF_4);
        }

        private void CloseBorder_MouseDown(object sender , MouseButtonEventArgs e)
        {
            Hide();
        }
    }
    public class BF4StatsDataContext : INotifyPropertyChanged
    {
        public void UpdateObject()
        {
            OPC(nameof(raw));
            OPC(nameof(player));
            OPC(nameof(CurrRankImage));
            OPC(nameof(NextRankImage));
        }

        public JObject raw
        {
            get
            {
                return App.Client.BF4_Stats;
            }
        }
        public JObject player
        {
            get
            {
                return (JObject)raw["player"];
            }
        }
        public BitmapImage CurrRankImage
        {
            get
            {
                return new BitmapImage(new Uri("pack://application:,,,/Media/" + player["rank"]["imgLarge"]));
            }
        }
        public BitmapImage NextRankImage
        {
            get
            {
                return new BitmapImage(new Uri("pack://application:,,,/Media/" + player["rank"]["next"]["imgLarge"]));
            }
        }

        public void OPC(string propname)
        {
            PropertyChanged?.Invoke(this , new PropertyChangedEventArgs(propname));
        }


        public event PropertyChangedEventHandler PropertyChanged;
    }
}
