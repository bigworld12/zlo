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
using System.Windows.Shapes;

namespace ZloGUILauncher.Views
{
    /// <summary>
    /// Interaction logic for BF4StatsListWindow.xaml
    /// </summary>
    public partial class StatsListWindow : Window
    {
        public StatsListWindow()
        {
            InitializeComponent();  
        }

        private void Window_Closing(object sender , System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
