using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Zlo.Extras;

namespace ZloGUILauncher.Servers
{
    public class BF3_GUI_Server : BF_GUI_Server
    {
        public BF3_GUI_Server(IBFServerBase raw) : base(raw)
        {
        }
    }
}
