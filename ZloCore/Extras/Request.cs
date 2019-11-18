using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zlo.PacketInfo;

namespace Zlo.Extras
{
    internal class Request
    {
        public byte pid = 0;

        public byte[] data = null;
        public bool IsDone = false;
        public BaseRequestPacket RequestInfo;

        public bool IsRespondable = false;
        public byte[] Responce = null;
        public event ReceivedResponseEventHandler ReceivedResponce;

        public TimeSpan WaitBeforePeriod = TimeSpan.Zero;
        public void GiveResponce(byte[] resp)
        {
            Responce = resp;
            IsDone = true;
            ReceivedResponce?.Invoke(this);
        }

       
    }
}
