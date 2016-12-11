using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Zlo.Extras;
using ZloGUILauncher.GUI_Classes;

namespace ZloGUILauncher
{
    public class GUI_MapRotation : ObservableCollection<GUI_Map>
    {
        private API_MapRotationBase m_raw;
        public API_MapRotationBase Raw
        {
            get
            {
                return m_raw;
            }
        }

        private GUI_Map m_CurrentActualMap;
        public GUI_Map CurrentActualMap
        {
            get
            {
                if (m_CurrentActualMap == null)
                {
                    m_CurrentActualMap = new GUI_Map(Raw.CurrentActualMap);
                }
                return m_CurrentActualMap;
            }            
        }

        private GUI_Map m_LogicalCurrentMap;
        public GUI_Map LogicalCurrentMap
        {
            get
            {
                if (m_LogicalCurrentMap == null)
                {
                    m_LogicalCurrentMap = new GUI_Map(Raw.LogicalCurrentMap);
                }
                return m_LogicalCurrentMap;
            }
        }

        private GUI_Map m_LogicalNextMap;
        public GUI_Map LogicalNextMap
        {
            get
            {
                if (m_LogicalNextMap == null)
                {
                    m_LogicalNextMap = new GUI_Map(Raw.LogicalNextMap);
                }
                return m_LogicalNextMap;
            }
        }

        public int CurrentMapIndex
        {
            get
            {
                return Raw.CurrentMapIndex;
            }
        }
        public int NextMapIndex
        {
            get
            {
                return Raw.NextMapIndex;
            }
        }

        public GUI_MapRotation(API_MapRotationBase b)
        {
            m_raw = b;
            Update();
        }
        public void Update()
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                Clear();
                for (int i = 0; i < Raw.Count; i++)
                {
                    var n = new GUI_Map(Raw[i]);
                    Add(n);
                }

                CurrentActualMap.raw = Raw.CurrentActualMap;
                LogicalCurrentMap.raw = Raw.LogicalCurrentMap;
                LogicalNextMap.raw = Raw.LogicalNextMap;



                OPC(nameof(CurrentActualMap));
                OPC(nameof(LogicalCurrentMap));
                OPC(nameof(LogicalNextMap));

                OPC(nameof(CurrentMapIndex));
                OPC(nameof(NextMapIndex));
            });
        }
        public void OPC(string prop)
        {
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(prop));
        }
    }

    public class GUI_PlayerList : ObservableCollection<GUI_Player>
    {
        private API_PlayerListBase m_raw;
        public API_PlayerListBase Raw
        {
            get
            {
                return m_raw;
            }
        }
        public GUI_PlayerList(API_PlayerListBase b)
        {
            m_raw = b;
            Update();
        }
        public void Update()
        {
            
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                Clear();
                for (int i = 0; i < Raw.Count; i++)
                {
                    var n = new GUI_Player(Raw[i]);
                    Add(n);
                    
                }
            });
        }
    }
}
