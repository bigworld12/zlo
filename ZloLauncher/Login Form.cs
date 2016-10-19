using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZloLauncher
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender , EventArgs e)
        {
            //change is first time login to false            
            Properties.Settings.Default["IsFirstTime"] = false;
            Properties.Settings.Default.Save();
            
            ShowInTaskbar = false;
            Hide();
            (new Main_Form()).Show();
        }
    }
}
