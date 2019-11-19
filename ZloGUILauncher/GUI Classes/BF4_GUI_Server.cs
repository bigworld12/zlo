using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    public class BF4_GUI_Server : BF_GUI_Server
    {
      
        public BF4_GUI_Server(IBFServerBase b) : base(b)
        {
        }

        public string ServerType => Raw.Attributes.TryGetValue("servertype", out var typ) ? typ : null;
        public bool IsHasFF => Raw.Attributes.TryGetValue("fairfight", out var ff) ? YesNo(ff) : false;

        
        public override void UpdateAllProps()
        {
            base.UpdateAllProps();
            OPC(nameof(ServerType));
            OPC(nameof(IsHasFF));
        }       
    }

    
}
