using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zlo.Extras;

namespace ZloGUILauncher.GUI_Classes
{
    public class GUI_Map : INotifyPropertyChanged
    {
        public API_MapBase raw;
        public GUI_Map(API_MapBase b)
        {
            raw = b;
        }
        public bool IsCurrentInRotation
        {
            get
            {
                return raw.IsCurrentInRotation;
            }
        }
        public bool IsActualCurrentMap
        {
            get
            {
                return raw.IsActualCurrentMap;
            }
        }
        public bool IsNextInRotation
        {
            get
            {
                return raw.IsNextInRotation;
            }
        }

        public string MapName
        {
            get
            {
                return raw.MapName;
            }
        }
        public string GameModeName
        {
            get
            {
                return raw.GameModeName;
            }
        }


        public void OPC(string prop)
        {
            PropertyChanged?.Invoke(this , new PropertyChangedEventArgs(prop));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
