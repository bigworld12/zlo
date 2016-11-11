using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zlo.Extras;


namespace ZloGUILauncher
{
    public class GUI_Player : INotifyPropertyChanged
    {
        public API_PlayerBase raw;
        public GUI_Player(API_PlayerBase r)
        {
            raw = r;
            //raw.ID;
            //raw.Name
            //raw.Slot
        }
        public uint ID
        {
            get
            {
                return raw.ID;
            }
        }
        public string Name
        {
            get
            {
                return raw.Name;
            }
        }
        public byte Slot
        {
            get
            {
                return raw.Slot;
            }
        }


        public bool IsCurrent
        {
            get
            {
                return ID == App.Client.CurrentPlayerID;
            }
        }
        public void OPC(string prop)
        {
            PropertyChanged?.Invoke(this , new PropertyChangedEventArgs(prop));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}