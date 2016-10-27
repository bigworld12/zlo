using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zlo.Extras
{
    class Request
    {
        public byte pid = 0;

        public byte[] data = null;
        public bool IsDone = false;

        public bool IsRespondable = false;
        public byte[] Responce = null;

        public delegate void ReceivedResponceEventHandler(Request Sender);
        public event ReceivedResponceEventHandler ReceivedResponce;

        public void GiveResponce(byte[] resp)
        {
            Responce = resp;
            IsDone = true;
            ReceivedResponce?.Invoke(this);
        }

    }
}
