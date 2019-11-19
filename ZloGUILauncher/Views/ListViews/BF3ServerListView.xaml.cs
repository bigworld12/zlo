using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Media.Animation;
using Zlo.Extras;
using ZloGUILauncher.Servers;
using System.Windows.Input;
using Zlo.Extras.BF_Servers;

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

            App.BFListViewModel.DataGrids[ZloBFGame.BF_3] = ServersDG;
            ViewSource.Source = App.BFListViewModel.BF3_GUI_Servers;
        }
        string requestmsg = "Please Enter the server password : \nNote : If you are sure the server doesn't have a password, press done and leave the password box empty";

        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            var b = sender as Button;
            var server = (BF3_GUI_Server)b.DataContext;
            if (server.IsHasPW)
            {
                InputBox inb = new InputBox(requestmsg);
                var ish = inb.ShowDialog();
                if (ish.HasValue && ish.Value)
                {
                    var pw = inb.OutPut;
                    App.Client.JoinOnlineGameWithPassWord(OnlinePlayModes.BF3_Multi_Player, server.ID, pw);
                }
            }
            else
            {
                App.Client.JoinOnlineServer(OnlinePlayModes.BF3_Multi_Player, server.ID);
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
                    if (VisualTreeHelper.GetChild(d, i) is ScrollViewer)
                    {
                        ScrollViewer scroll = (ScrollViewer)(VisualTreeHelper.GetChild(d, i));
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
