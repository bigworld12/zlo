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
    public class BF4_Server : INotifyPropertyChanged
    {
        public BF4ServerBase raw;
      
        public BF4_Server(BF4ServerBase b)
        {
            raw = b;
        }      

        public uint ID
        {
            get { return raw.ServerID; }
        }
        public string Name
        {
            get { return raw.GNAM; }
        }

        public int Current_Players
        {
            get { return raw.Players.Count; }
        }
        public int Max_Players
        {
            get
            {
                return raw.PCAP[0];
            }
        }

        public string RepPlayers
        {
            get
            {
                return $"{Current_Players}/{Max_Players}";
            }
        }

        private IPAddress m_IP;
        public IPAddress IP
        {
            get
            {
                if (m_IP == null || BitConverter.ToUInt32(m_IP.GetAddressBytes() , 0) == raw.EXIP)
                {
                    m_IP = new IPAddress(BitConverter.GetBytes(raw.EXIP));
                }
                return m_IP;
            }
        }
        public ushort Port
        {
            get
            {
                return raw.EXPORT;
            }
        }

        public string Map
        {
            get { return raw.ATTRS?["level"]; }
        }
        public string GameMode
        {
            get { return raw.ATTRS?["levellocation"]; }
        }
        public string ServerType
        {
            get
            {
                return raw.ATTRS["servertype"];
            }
        }

        public void UpdateAllProps()
        {
            OPC(nameof(ID));
            OPC(nameof(Name));
            OPC(nameof(Current_Players));
            OPC(nameof(Max_Players));
            OPC(nameof(IP));
            OPC(nameof(Port));
            OPC(nameof(Map));
            OPC(nameof(GameMode));
            OPC(nameof(Ping));
            OPC(nameof(RepPlayers));
            OPC(nameof(ServerType));
        }
        public void OPC(string prop)
        {
            PropertyChanged?.Invoke(this , new PropertyChangedEventArgs(prop));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
