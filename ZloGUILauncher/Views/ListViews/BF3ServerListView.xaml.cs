using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ZloGUILauncher.Servers;

namespace ZloGUILauncher.Views
{
    /// <summary>
    /// Interaction logic for ServerListView.xaml
    /// </summary>
    public partial class BF3ServerListView : UserControl
    {
        public CollectionViewSource ViewSource
        {
            get { return TryFindResource("ServersView") as CollectionViewSource; }
        }

        public BF3ServerListView()
        {
            InitializeComponent();
            App.Client.BF3ServerAdded += Client_BF3ServerAdded;
            App.Client.BF3ServerUpdated += Client_BF3ServerUpdated;
            App.Client.ServerRemoved += Client_ServerRemoved;
            ViewSource.Source = BF3_Servers;
        }

        private void Client_ServerRemoved(Zlo.Extras.ZloGame game , uint id , Zlo.Extras.IServerBase server)
        {
            if (game == Zlo.Extras.ZloGame.BF_3 && server != null)
            {
                Dispatcher.Invoke(() =>
                {
                    //remove from current list
                    var ser = BF3_Servers.Find(s =>  s.raw == server);
                    BF3_Servers.Remove(ser);                    
                });
            }
        }
        private void Client_BF3ServerUpdated(uint id , Zlo.Extras.BF3ServerBase server , bool IsPlayerChangeOnly)
        {
            Dispatcher.Invoke(() =>
            {
                var equi = BF3_Servers.Find(x => x.raw == server);
                if (equi != null)
                {
                    equi.UpdateAllProps();
                }
            });
        }
        private void Client_BF3ServerAdded(uint id , Zlo.Extras.BF3ServerBase server , bool IsPlayerChangeOnly)
        {
            Dispatcher.Invoke(() =>
            {
                var newserv = new BF3_Server(server);
                BF3_Servers.Add(newserv);
            });
        }

        private void JoinButton_Click(object sender , RoutedEventArgs e)
        {
            var b = sender as Button;
            var server = (BF3_Server)b.DataContext;
            App.Client.JoinOnlineGame(Zlo.Extras.OnlinePlayModes.BF3_Multi_Player , server.ID);
        }

        private ObservableCollection<BF3_Server> m_BF3_Servers;
        public ObservableCollection<BF3_Server> BF3_Servers
        {
            get
            {
                if (m_BF3_Servers == null)
                {
                    m_BF3_Servers = new ObservableCollection<BF3_Server>();
                }
                return m_BF3_Servers;
            }
        }
    }
}
