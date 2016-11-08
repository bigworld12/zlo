
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Zlo.Extras;
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

        public BF4ServersList DataServersList
        {
            get
            {
                return App.Client.BF4Servers;
            }
        }

        public BF4ServerListView()
        {
            InitializeComponent();
            DataServersList.ServerAdded += DataServersList_ServerAdded;
            DataServersList.ServerUpdated += DataServersList_ServerUpdated;
            DataServersList.ServerRemoved += DataServersList_ServerRemoved;
            ViewSource.Source = BF4_GUI_Servers;
        }

        private void DataServersList_ServerRemoved(uint id , BF4ServerBase server)
        {
            if (server != null)
            {
                Dispatcher.Invoke(() =>
                {
                    //remove from current list
                    var ser = BF4_GUI_Servers.Find(s => s.raw == server);
                    BF4_GUI_Servers.Remove(ser);
                });
            }
        }
        private void DataServersList_ServerUpdated(uint id , BF4ServerBase server)
        {
            Dispatcher.Invoke(() =>
            {
                var equi = BF4_GUI_Servers.Find(x => x.raw == server);
                if (equi != null)
                {
                    equi.UpdateAllProps();
                    AnimateRow(equi);
                }
            });
        }
        private void DataServersList_ServerAdded(uint id , BF4ServerBase server)
        {
            Dispatcher.Invoke(() =>
            {


                var newserv = new BF4_GUI_Server(server);
                BF4_GUI_Servers.Add(newserv);

                AnimateRow(newserv);
            });
        }

        public void AnimateRow(BF4_GUI_Server element)
        {
            var row = ServersDG.ItemContainerGenerator.ContainerFromItem(element) as DataGridRow;
            if (row == null) return;



            ColorAnimation switchOnAnimation = new ColorAnimation
            {
                From = Colors.White ,
                To = Colors.Pink ,
                Duration = TimeSpan.FromSeconds(1) ,
                AutoReverse = true
            };
            Storyboard blinkStoryboard = new Storyboard();


            blinkStoryboard.Children.Add(switchOnAnimation);
            Storyboard.SetTargetProperty(switchOnAnimation , new PropertyPath("Background.Color"));
            //animate changed server
            Storyboard.SetTarget(switchOnAnimation , row);

            row.BeginStoryboard(blinkStoryboard);
        }

        private void JoinButton_Click(object sender , RoutedEventArgs e)
        {
            var b = sender as Button;
            var server = (BF4_GUI_Server)b.DataContext;
            if (server.IsHasPW)
            {
                InputBox inb = new InputBox(requestmsg);
                var ish = inb.ShowDialog();
                if (ish.HasValue && ish.Value)
                {
                    var pw = inb.OutPut;
                    App.Client.JoinOnlineGameWithPassWord(OnlinePlayModes.BF4_Multi_Player , server.ID , pw);
                }
            }
            else
            {
                App.Client.JoinOnlineGame(OnlinePlayModes.BF4_Multi_Player , server.ID);
            }
        }
        string requestmsg = "Please Enter the server password : \nNote : If you are sure the server doesn't have a password, press done and leave the password box empty";
        private void JoinSpectatorButton_Click(object sender , RoutedEventArgs e)
        {
            var b = sender as Button;
            var server = (BF4_GUI_Server)b.DataContext;
            if (server.IsHasPW)
            {
                InputBox inb = new InputBox(requestmsg);
                var ish = inb.ShowDialog();
                if (ish.HasValue && ish.Value)
                {
                    var pw = inb.OutPut;
                    App.Client.JoinOnlineGameWithPassWord(OnlinePlayModes.BF4_Spectator , server.ID , pw);
                }
            }
            else
            {
                App.Client.JoinOnlineGame(OnlinePlayModes.BF4_Spectator , server.ID);
            }
        }
        private void JoinCommanderButton_Click(object sender , RoutedEventArgs e)
        {
            var b = sender as Button;
            var server = (BF4_GUI_Server)b.DataContext;
            if (server.IsHasPW)
            {
                InputBox inb = new InputBox(requestmsg);
                var ish = inb.ShowDialog();
                if (ish.HasValue && ish.Value)
                {
                    var pw = inb.OutPut;
                    App.Client.JoinOnlineGameWithPassWord(OnlinePlayModes.BF4_Commander , server.ID , pw);
                }
            }
            else
            {
                App.Client.JoinOnlineGame(OnlinePlayModes.BF4_Commander , server.ID);
            }
        }


        private ObservableCollection<BF4_GUI_Server> m_BF4_Servers;
        public ObservableCollection<BF4_GUI_Server> BF4_GUI_Servers
        {
            get
            {
                if (m_BF4_Servers == null)
                {
                    m_BF4_Servers = new ObservableCollection<BF4_GUI_Server>();
                }
                return m_BF4_Servers;
            }
        }      

        private void ScrollViewer_PreviewMouseWheel(object sender , MouseWheelEventArgs e)
        {
            if (sender.GetType() == typeof(ScrollViewer))
            {
                ScrollViewer scrollviewer = sender as ScrollViewer;
                if (e.Delta > 0)
                    scrollviewer.LineLeft();
                else
                    scrollviewer.LineRight();
                e.Handled = true;
            }
            else
            {
                var d = sender as DependencyObject;
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
                {
                    if (VisualTreeHelper.GetChild(d , i) is ScrollViewer)
                    {
                        ScrollViewer scroll = (ScrollViewer)(VisualTreeHelper.GetChild(d , i));
                        if (e.Delta > 0)
                            scroll.LineLeft();
                        else
                            scroll.LineRight();
                        e.Handled = true;
                    }
                }
            }
        }
    }
}
