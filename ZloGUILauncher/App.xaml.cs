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
        App() : base()
        {
            try
            {                
                var args = Environment.GetCommandLineArgs();
                if (args.Length > 1 && args.Last().Trim('"') == "done")
                {
                    var bat_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory , "UpdateBat.bat");
                    File.Delete(bat_path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private static API_ZloClient m_Client;
        public static API_ZloClient Client
        {
            get
            {
                if (m_Client == null)
                {
                    m_Client = new API_ZloClient();
                }
                return m_Client;
            }
        }

        private static GameStateViewer m_gamestateviewer;
        public static GameStateViewer GameStateViewer
        {
            get
            {
                if (m_gamestateviewer == null)
                {
                    m_gamestateviewer = new GameStateViewer();
                }
                return m_gamestateviewer;
            }
        }
    }
}
