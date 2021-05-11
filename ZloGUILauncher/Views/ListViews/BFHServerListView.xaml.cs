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
using Zlo.Extras.BF_Servers;
using ZloGUILauncher.Servers;

namespace ZloGUILauncher.Views
{
    /// <summary>
    /// Interaction logic for BFHServerListView.xaml
    /// </summary>
    public partial class BFHServerListView : UserControl
    {

        public CollectionViewSource ViewSource
        {
            get { return TryFindResource("ServersView") as CollectionViewSource; }
        }      
        public BFHServerListView()
        {
            InitializeComponent();

            App.BFListViewModel.DataGrids[ZloBFGame.BF_HardLine] = ServersDG;
            ViewSource.Source = App.BFListViewModel.BFH_GUI_Servers;
        }     

        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            var b = sender as Button;
            var server = (BFH_GUI_Server)b.DataContext;
            if (server.IsHasPW)
            {
                InputBox inb = new InputBox(requestmsg);
                var ish = inb.ShowDialog();
                if (ish.HasValue && ish.Value)
                {
                    var pw = inb.OutPut;
                    App.Client.JoinOnlineServer(OnlinePlayModes.BFH_Multi_Player, server.ID, pw);
                }
            }
            else
            {
                App.Client.JoinOnlineServer(OnlinePlayModes.BFH_Multi_Player, server.ID);
            }
        }
        string requestmsg = "Please Enter the server password : \nNote : If you are sure the server doesn't have a password, press done and leave the password box empty";
        private void JoinSpectatorButton_Click(object sender, RoutedEventArgs e)
        {
            var b = sender as Button;
            var server = (BFH_GUI_Server)b.DataContext;
            if (server.IsHasPW)
            {
                InputBox inb = new InputBox(requestmsg);
                var ish = inb.ShowDialog();
                if (ish.HasValue && ish.Value)
                {
                    var pw = inb.OutPut;
                    App.Client.JoinOnlineServer(OnlinePlayModes.BFH_Spectator, server.ID, pw);
                }
            }
            else
            {
                App.Client.JoinOnlineServer(OnlinePlayModes.BFH_Spectator, server.ID);
            }
        }
        private void JoinCommanderButton_Click(object sender, RoutedEventArgs e)
        {
            var b = sender as Button;
            var server = (BFH_GUI_Server)b.DataContext;
            if (server.IsHasPW)
            {
                InputBox inb = new InputBox(requestmsg);
                var ish = inb.ShowDialog();
                if (ish.HasValue && ish.Value)
                {
                    var pw = inb.OutPut;
                    App.Client.JoinOnlineServer(OnlinePlayModes.BFH_Commander, server.ID, pw);
                }
            }
            else
            {
                App.Client.JoinOnlineServer(OnlinePlayModes.BFH_Commander, server.ID);
            }
        }




        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
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
                    if (VisualTreeHelper.GetChild(d, i) is ScrollViewer scroll)
                    {
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
