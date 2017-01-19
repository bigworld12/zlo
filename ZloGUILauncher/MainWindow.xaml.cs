using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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

namespace ZloGUILauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            App.Client.ErrorOccured += Client_ErrorOccured;
            App.Client.UserInfoReceived += Client_UserInfoReceived;
            App.Client.GameStateReceived += Client_GameStateReceived;
            App.Client.APIVersionReceived += Client_APIVersionReceived;
            App.Client.Disconnected += Client_Disconnected;
            App.Client.ConnectionStateChanged += Client_ConnectionStateChanged;
            if (App.Client.Connect())
            {
                //App.Client.SubToServerList(Zlo.Extras.ZloGame.BF_3);
                App.Client.SubToServerList(Zlo.Extras.ZloGame.BF_4);

                App.Client.GetStats(Zlo.Extras.ZloGame.BF_4);
                App.Client.GetItems(Zlo.Extras.ZloGame.BF_4);

                App.Client.GetStats(Zlo.Extras.ZloGame.BF_3);
            }
        }

        private void Client_ConnectionStateChanged(bool IsConnectedToZloClient)
        {
            Dispatcher.Invoke(() =>
            {
                if (IsConnectedToZloClient)
                {
                    //connected
                    IsConnectedTextBlock.Text = "Connected";
                    IsConnectedTextBlock.Foreground = Brushes.Green;
                }
                else
                {
                    IsConnectedTextBlock.Text = "DisConnected";
                    IsConnectedTextBlock.Foreground = Brushes.Red;
                }

            });

        }

        private void Client_Disconnected(Zlo.Extras.DisconnectionReasons Reason)
        {
            MessageBox.Show($"Client Disconnected for reason : {Reason}");
        }

        private void Client_APIVersionReceived(Version Current , Version Latest , bool IsNeedUpdate , string DownloadAdress)
        {
            if (IsNeedUpdate)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Current dll version : {Current}\nLatest dll version : {Latest}\nPress Ok to start Updating Zlo.dll" , "Update Notification" , MessageBoxButton.OK);
                    string Sourcedll = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory , "Zlo.dll");
                    string Newdll = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory , "Zlo_New.dll");

                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadFileCompleted += Wc_DownloadFileCompleted;
                        wc.DownloadFileAsync(new Uri(DownloadAdress) , Newdll);
                    }
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    Title = $"Bigworld12 new API launcher (Version {Current.ToString()})";
                });
            }
        }

        private void Wc_DownloadFileCompleted(object sender , System.ComponentModel.AsyncCompletedEventArgs e)
        {
            //Zlo.dll completed
            if (e.Error != null)
            {
                //error occured
                Client_ErrorOccured(e.Error , "Error occured when updating Zlo.dll");
            }
            else
            {
                //no errors
                string Sourcedll = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory , "Zlo.dll");
                string Newdll = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory , "Zlo_New.dll");
                string BatchText =
                   $@"
@ECHO off
SETLOCAL EnableExtensions
set EXE={AppDomain.CurrentDomain.FriendlyName}
echo Waiting for process %EXE% to close ...
:LOOP
@Timeout /T 1 /NOBREAK>nul
tasklist /FI ""IMAGENAME eq %EXE%"" 2>NUL | find /I /N ""%EXE%"">NUL
if ""%ERRORLEVEL%""==""0"" goto LOOP
echo Process %EXE% closed
move /y ""{Newdll}"" ""{Sourcedll}"" 
start """" ""{System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory , AppDomain.CurrentDomain.FriendlyName)}"" ""done""
Exit
";
                var bat_path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory , "UpdateBat.bat");
                //create the bat file
                File.WriteAllText(bat_path , BatchText);
                ProcessStartInfo si = new ProcessStartInfo(bat_path);
                si.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                Process.Start(si);
                Dispatcher.Invoke(() => { Application.Current.Shutdown(); });
            }
        }

        private void Client_GameStateReceived(Zlo.Extras.ZloGame game , string type , string message)
        {
            Dispatcher.Invoke(() =>
            {
                App.GameStateViewer.StateReceived(game , type , message);
                LatestGameStateTextBlock.Text = $"[{game}] [{type}] {message}";
            });
        }

        private void Client_UserInfoReceived(uint UserID , string UserName)
        {
            Dispatcher.Invoke(() =>
            {
                PlayerInfoTextBlock.Text = $"{UserName} ({UserID})";
            });
        }

        private void Client_ErrorOccured(Exception Error , string CustomMessage)
        {
            MessageBox.Show($"{Error.ToString()}" , CustomMessage);
        }

        private void ViewAllGameStatesButton_Click(object sender , RoutedEventArgs e)
        {
            App.GameStateViewer.Show();
        }

        private void RestartLauncherButton_Click(object sender , RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                App.Client.Close();
                Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            });
        }

        private void MainTabControl_SelectionChanged(object sender , SelectionChangedEventArgs e)
        {
            var tc = sender as TabControl;
            if (tc != null)
            {
                if (tc.SelectedIndex < 0)
                {
                    return;
                }
                switch (tc.SelectedIndex)
                {
                    case 0:
                        App.Client.SubToServerList(Zlo.Extras.ZloGame.BF_4);
                        break;
                    case 1:
                        App.Client.SubToServerList(Zlo.Extras.ZloGame.BF_3);
                        break;
                    default:
                        break;
                }
            }
        }

        private void OfficialDiscordButton_Click(object sender , RoutedEventArgs e)
        {
            Process.Start("https://discord.me/zlocommunity");
        }

        private void ShowDllInjectorButton_Click(object sender , RoutedEventArgs e)
        {
            var di = new DllInjector();            
            di.Show();
        }
    }
}
