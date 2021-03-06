﻿using System;
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
using Zlo;
using Zlo.Extras;

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
            Application.Current.MainWindow = this;
            App.Client.ErrorOccured += Client_ErrorOccured;
            App.Client.UserInfoReceived += Client_UserInfoReceived;
            App.Client.GameStateReceived += Client_GameStateReceived;
            App.Client.ConnectionStateChanged += Client_ConnectionStateChanged;
            Title = $"Bigworld12 launcher (Version {App.Client.CurrentApiVersion.ToString()})";

            //Settings.CurrentSettings.SetCustomSetting("testFavs",new uint[] { 5 , 6 , 7 });
            //var saved = Settings.TrySave();

            App.Client.Connect();
            DiscordRPCCheck.IsChecked = App.Client.IsEnableDiscordRPC;
        }

        public void AfterSuccessfulConnect()
        {
            IsConnectedTextBlock.Text = "Connected";
            IsConnectedTextBlock.Foreground = Brushes.Green;
            switch (App.Client.SettingsServerListener)
            {
                case ZloBFGame.BF_3:
                    MainTabControl.SelectedIndex = 2;
                    //App.Client.SubToServerList(ZloGame.BF_3);
                    App.Client.GetStats(ZloBFGame.BF_3);
                    break;

                case ZloBFGame.BF_HardLine:
                    MainTabControl.SelectedIndex = 1;

                    //App.Client.SubToServerList(ZloGame.BF_HardLine);
                    App.Client.GetStats(ZloBFGame.BF_HardLine);
                    App.Client.GetItems(ZloBFGame.BF_HardLine);
                    break;

                default:
                case ZloBFGame.None:
                case ZloBFGame.BF_4:
                    MainTabControl.SelectedIndex = 0;

                    //App.Client.SubToServerList(ZloGame.BF_4);
                    App.Client.GetStats(ZloBFGame.BF_4);
                    App.Client.GetItems(ZloBFGame.BF_4);
                    break;
            }

        }
        public void AfterFailedConnect()
        {
            IsConnectedTextBlock.Text = "DisConnected";
            IsConnectedTextBlock.Foreground = Brushes.Red;
        }

        private void Client_ConnectionStateChanged(bool IsConnectedToZloClient)
        {
            Dispatcher.Invoke(() =>
            {
                if (IsConnectedToZloClient)
                {
                    //connected
                    AfterSuccessfulConnect();
                }
                else
                {
                    AfterFailedConnect();
                }
            });
        }



        //private void Client_APIVersionReceived(Version Current , Version Latest , bool IsNeedUpdate , string DownloadAdress)
        //{
        //    if (IsNeedUpdate)
        //    {
        //        Dispatcher.Invoke(() =>
        //        {
        //            MessageBox.Show($"Current dll version : {Current}\nLatest dll version : {Latest}\nPress Ok to start Updating Zlo.dll" , "Update Notification" , MessageBoxButton.OK);
        //            string Sourcedll = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory , "Zlo.dll");
        //            string Newdll = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory , "Zlo_New.dll");

        //            using (WebClient wc = new WebClient())
        //            {
        //                wc.DownloadFileCompleted += Wc_DownloadFileCompleted;
        //                wc.DownloadFileAsync(new Uri(DownloadAdress) , Newdll);
        //            }
        //        });
        //    }
        //    else
        //    {
        //        Dispatcher.Invoke(() =>
        //        {
        //            Title = $"Bigworld12 new API launcher (Version {Current.ToString()})";
        //        });
        //    }
        //}

        //        private void Wc_DownloadFileCompleted(object sender , System.ComponentModel.AsyncCompletedEventArgs e)
        //        {
        //            //Zlo.dll completed
        //            if (e.Error != null)
        //            {
        //                //error occured
        //                Client_ErrorOccured(e.Error , "Error occured when updating Zlo.dll");
        //            }
        //            else
        //            {
        //                //no errors
        //                string Sourcedll = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory , "Zlo.dll");
        //                string Newdll = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory , "Zlo_New.dll");
        //                string BatchText =
        //                   $@"
        //@ECHO off
        //SETLOCAL EnableExtensions
        //set EXE={AppDomain.CurrentDomain.FriendlyName}
        //echo Waiting for process %EXE% to close ...
        //:LOOP
        //@Timeout /T 1 /NOBREAK>nul
        //tasklist /FI ""IMAGENAME eq %EXE%"" 2>NUL | find /I /N ""%EXE%"">NUL
        //if ""%ERRORLEVEL%""==""0"" goto LOOP
        //echo Process %EXE% closed
        //move /y ""{Newdll}"" ""{Sourcedll}"" 
        //start """" ""{System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory , AppDomain.CurrentDomain.FriendlyName)}"" ""done""
        //Exit
        //";
        //                var bat_path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory , "UpdateBat.bat");
        //                //create the bat file
        //                File.WriteAllText(bat_path , BatchText);
        //                ProcessStartInfo si = new ProcessStartInfo(bat_path)
        //                {
        //                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
        //                };
        //                Process.Start(si);
        //                Dispatcher.Invoke(() => { Application.Current.Shutdown(); });
        //            }
        //        }

        private void Client_GameStateReceived(Zlo.Extras.ZloBFGame game, string type, string message)
        {
            Dispatcher.Invoke(() =>
            {
                App.GameStateViewer.StateReceived(game, type, message);
                LatestGameStateTextBlock.Text = $"[{game}] [{type}] {message}";
            });
        }

        private void Client_UserInfoReceived(uint UserID, string UserName)
        {
            Dispatcher.Invoke(() =>
            {
                PlayerInfoTextBlock.Text = $"{UserName} ({UserID})";
            });
        }

        private void Client_ErrorOccured(Exception Error, string CustomMessage)
        {
            Log.WriteLog($"{CustomMessage}\n{Error.ToString()}");
            //MessageBox.Show($"{Error.ToString()}" , CustomMessage);
        }

        private void ViewAllGameStatesButton_Click(object sender, RoutedEventArgs e)
        {
            App.GameStateViewer.Show();
        }

        private void RestartLauncherButton_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            });
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TabControl tc)
            {
                if (tc.SelectedIndex < 0)
                {
                    return;
                }
                switch (tc.SelectedIndex)
                {
                    case 0:
                        if (App.Client.CurrentServerListener != ZloBFGame.BF_4)
                            App.Client.SubToServerList(ZloBFGame.BF_4);
                        break;
                    case 1:
                        if (App.Client.CurrentServerListener != ZloBFGame.BF_HardLine)
                        App.Client.SubToServerList(ZloBFGame.BF_HardLine);
                        break;
                    case 2:
                        if (App.Client.CurrentServerListener != ZloBFGame.BF_3)
                        App.Client.SubToServerList(ZloBFGame.BF_3);
                        break;
                    default:
                        break;
                }
            }
        }

        private void OfficialDiscordButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://discord.gg/m3vEHyN");
        }

        private void ShowDllInjectorButton_Click(object sender, RoutedEventArgs e)
        {
            var di = new DllInjector();
            di.Show();
        }



        private void DiscordRPCCheck_Checked(object sender, RoutedEventArgs e)
        {
            App.Client.IsEnableDiscordRPC = true;
        }

        private void DiscordRPCCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            App.Client.IsEnableDiscordRPC = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.Client.Dispose();
        }
    }
}
