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
using Zlo;

namespace ZloGUILauncher.Views
{
    /// <summary>
    /// Interaction logic for RunnableGames.xaml
    /// </summary>
    public partial class RunnableGames : UserControl
    {
        public RunnableGames()
        {
            InitializeComponent();
            App.Client.RunnableGameListReceived += Client_RunnableGameListReceived;
            App.Client.GameRunResultReceived += Client_GameRunResultReceived;
            Client_RunnableGameListReceived();
        }

        private void Client_GameRunResultReceived(RunnableGame zname, Zlo.Extras.GameRunResult result)
        {
            Dispatcher.Invoke(() =>
            {
                LastRunStatus.Text = $"Last Run : {zname.FriendlyName}, Result : ({result})";
                switch (result)
                {
                    case Zlo.Extras.GameRunResult.Successful:
                        LastRunStatus.Foreground = Brushes.Green;
                        break;
                    case Zlo.Extras.GameRunResult.NotFound:                        
                    case Zlo.Extras.GameRunResult.Error:
                        LastRunStatus.Foreground = Brushes.Red;
                        break;
                    default:
                        break;
                }
            });
        }

        private void Client_RunnableGameListReceived()
        {
            Dispatcher.Invoke(() =>
            {
                GamesDG.ItemsSource = App.Client.RunnableGameList;
                arch.Text = App.Client.RunnableGameList.IsOSx64 ? "x64" : "x86";
            });
        }

       
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (GamesDG.SelectedItem is RunnableGame rg)
            {
                App.Client.SendRunGameRequest(rg, CMDInput.Text);
            }            
        }

        private void GamesDG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid dg && dg.SelectedItem is RunnableGame rg)
            {
                GameName.Text = rg.FriendlyName;
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
