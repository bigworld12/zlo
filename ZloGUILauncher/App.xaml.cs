using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Zlo;
using ZloGUILauncher.Views;

namespace ZloGUILauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //var val = Settings.CurrentSettings.GetOrCreateCustomSetting("bf3Favs", new uint[] { 5, 6, 7 });


        }
        private static API_ZloClient m_Client;
        public static API_ZloClient Client => m_Client ?? (m_Client = new API_ZloClient());


        private static GameStateViewer m_gamestateviewer;
        public static GameStateViewer GameStateViewer => m_gamestateviewer ?? (m_gamestateviewer = new GameStateViewer());
    }
}
