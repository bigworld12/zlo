using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using ZloGUILauncher.Servers;

namespace ZloGUILauncher.Views
{
    /// <summary>
    /// Interaction logic for BF4ServerListView.xaml
    /// </summary>
    public partial class BF4ServerListView : UserControl
    {
        public CollectionViewSource ViewSource
        {
            get { return TryFindResource("ServersView") as CollectionViewSource; }
        }
        public BF4ServerListView()
        {
            InitializeComponent();

            App.Client.BF4ServerAdded += Client_BF4ServerAdded;
            App.Client.BF4ServerUpdated += Client_BF4ServerUpdated;
            App.Client.ServerRemoved += Client_ServerRemoved;

            ViewSource.Source = BF4_Servers;
        }

        private void Client_ServerRemoved(Zlo.Extras.ZloGame game , uint id , Zlo.Extras.IServerBase server)
        {
            if (game == Zlo.Extras.ZloGame.BF_4 && server != null)
            {
                Dispatcher.Invoke(() =>
                {
                    var ser = BF4_Servers.Find(s => s.raw == server);
                    BF4_Servers.Remove(ser);                    
                });
            }
        }

        private void Client_BF4ServerUpdated(uint id , Zlo.Extras.BF4ServerBase server , bool IsPlayerChangeOnly)
        {
            Dispatcher.Invoke(() =>
            {
                var equi = BF4_Servers.Find(x => x.raw == server);
                if (equi != null)
                {
                    equi.UpdateAllProps();
                }
            });
        }

        private void Client_BF4ServerAdded(uint id , Zlo.Extras.BF4ServerBase server , bool IsPlayerChangeOnly)
        {
            Dispatcher.Invoke(() =>
            {
                var newserv = new BF4_Server(server);
                BF4_Servers.Add(newserv);
            });
        }
        private void JoinButton_Click(object sender , RoutedEventArgs e)
        {
            var b = sender as Button;
            var server = (BF4_Server)b.DataContext;
            App.Client.JoinOnlineGame(Zlo.Extras.OnlinePlayModes.BF4_Multi_Player , server.ID);
        }
        private void JoinSpectatorButton_Click(object sender , RoutedEventArgs e)
        {
            var b = sender as Button;
            var server = (BF4_Server)b.DataContext;
            App.Client.JoinOnlineGame(Zlo.Extras.OnlinePlayModes.BF4_Spectator , server.ID);
        }
        private void JoinCommanderButton_Click(object sender , RoutedEventArgs e)
        {
            var b = sender as Button;
            var server = (BF4_Server)b.DataContext;
            App.Client.JoinOnlineGame(Zlo.Extras.OnlinePlayModes.BF4_Commander , server.ID);
        }
        private ObservableCollection<BF4_Server> m_BF4_Servers;
        public ObservableCollection<BF4_Server> BF4_Servers
        {
            get
            {
                if (m_BF4_Servers == null)
                {
                    m_BF4_Servers = new ObservableCollection<BF4_Server>();
                }
                return m_BF4_Servers;
            }
        }

       
    }
}
