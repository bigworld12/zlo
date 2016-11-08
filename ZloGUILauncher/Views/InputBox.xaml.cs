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
    /// Interaction logic for InputBox.xaml
    /// </summary>
    public partial class InputBox : Window
    {
        public InputBox(string Request,string DefaultAnswer = "")
        {
            InitializeComponent();
            RequestText.Text = Request;
            ResultText.Text = DefaultAnswer;
        }

       

        private void OkButton_Click(object sender , RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender , RoutedEventArgs e)
        {
            DialogResult = false;
        }
        public string OutPut
        {
            get
            {
                return ResultText.Text;
            }
        }

    }
}
