using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZloLauncher
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (Properties.Settings.Default["IsFirstTime"].Equals(true))
            {
                Application.Run(new LoginForm());
            }
            else
            {
                Application.Run(new Main_Form());
            }
            
        }
    }
}
