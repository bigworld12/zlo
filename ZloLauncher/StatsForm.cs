using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zlo.Extras;

namespace ZloLauncher
{
    public partial class StatsForm : Form
    {
        public StatsForm()
        {
            InitializeComponent();
        }

        public void ShowWithStats(ZloGame game , List<Stat> stats)
        {
            // StatsDGV.DataSource = stats;
            Show();
            StatsDGV.DataSource = stats;
            this.Text = $"Stats For : {game.ToString().Replace("_" , string.Empty)}";
        }
        public void ShowWithItems(ZloGame game , List<Item> items)
        {
            Show();
            StatsDGV.DataSource = items;
            this.Text = $"Items For : {game.ToString().Replace("_" , string.Empty)}";           
        }

        private void StatsForm_Load(object sender , EventArgs e)
        {

        }
    }
}
