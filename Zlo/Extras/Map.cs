using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zlo.Extras
{
    public class Map : INotifyPropertyChanged
    {
        private MapRotation _parent;
        public MapRotation Parent
        {
            get
            {
                return _parent;
            }
        }

        private string m_MapName;
        public string MapName
        {
            get { return m_MapName; }
            set {
                m_MapName = value;
               OPC(nameof(MapName));
            }
        }

        private string m_GameModeName;
        public string GameModeName
        {
            get { return m_GameModeName; }
            set { m_GameModeName = value;
                OPC(nameof(GameModeName));
            }
        }

        public bool IsCurrent
        {
            get
            {
                return this == Parent.LogicalCurrentMap;
            }
        }
        public bool IsNext
        {
            get
            {
                return this == Parent.LogicalNextMap;
            }
        }

        public Map(MapRotation p) { _parent = p; }
        public Map(string mname , string gmname, MapRotation p)
        {
            MapName = mname;
            GameModeName = gmname;
            _parent = p;
        }

        public void OPC(string propname)
        {
            PropertyChanged?.Invoke(this , new PropertyChangedEventArgs(propname));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
    public class MapRotation : Dictionary<int , Map>, INotifyPropertyChanged
    {
        private int m_CurrentMapIndex;
        public int CurrentMapIndex
        {
            get { return m_CurrentMapIndex; }
        }

        private int m_NextMapIndex;
        public int NextMapIndex
        {
            get { return m_NextMapIndex; }
        }


        public Map LogicalCurrentMap
        {
            get
            {
                if (ContainsKey(CurrentMapIndex))
                {
                    return this[CurrentMapIndex];
                }
                else
                {
                    return null;
                }
            }
        }
        public Map LogicalNextMap
        {
            get
            {
                if (ContainsKey(NextMapIndex))
                {
                    return this[NextMapIndex];
                }
                else
                {
                    return null;
                }
            }
        }


        private Map m_CurrentActualMap;
        public Map CurrentActualMap
        {
            get
            {
                if (m_CurrentActualMap == null)
                {
                    m_CurrentActualMap = new Map(this);
                }
                return m_CurrentActualMap;
            }
        }

        public void Parse(string mapsinfo , string mapsraw , ZloGame game)
        {
            Clear();
            //parse maps rotation
            string[] rawmgms = mapsraw.Split(new[] { ';' } , StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < rawmgms.Length; i++)
            {
                //each map
                //name,gamemode
                var rawmgm = rawmgms[i].Split(new[] { ',' } , StringSplitOptions.RemoveEmptyEntries);
                if (rawmgm.Length > 0)
                {
                    var m = new Map(this);
                    switch (game)
                    {
                        case ZloGame.BF_3:
                            if (Dictionaries.BF3_Maps.ContainsKey(rawmgm[0]))
                            {
                                m.MapName = Dictionaries.BF3_Maps[rawmgm[0]];
                            }
                            else
                            {
                                m.MapName = rawmgm[0];
                            }
                            if (rawmgm.Length > 1)
                            {
                                if (Dictionaries.BF3_GameModes.ContainsKey(rawmgm[1]))
                                {
                                    m.GameModeName = Dictionaries.BF3_GameModes[rawmgm[1]];
                                }
                                else
                                {
                                    m.GameModeName = rawmgm[1];
                                }
                            }
                            else
                            {
                                m.GameModeName = string.Empty;
                            }

                            break;
                        case ZloGame.BF_4:
                            if (Dictionaries.BF4_Maps.ContainsKey(rawmgm[0]))
                            {
                                m.MapName = Dictionaries.BF4_Maps[rawmgm[0]];
                            }
                            else
                            {
                                m.MapName = rawmgm[0];
                            }
                            if (rawmgm.Length > 1)
                            {
                                if (Dictionaries.BF4_GameModes.ContainsKey(rawmgm[1]))
                                {
                                    m.GameModeName = Dictionaries.BF4_GameModes[rawmgm[1]];
                                }
                                else
                                {
                                    m.GameModeName = rawmgm[1];
                                }
                            }
                            else
                            {
                                m.GameModeName = string.Empty;
                            }
                            break;
                        case ZloGame.BF_HardLine:
                        default:
                            m.MapName = rawmgm[0];
                            m.GameModeName = rawmgm[1];
                            break;
                    }
                    Add(i , m);
                }

            }


            //parse maps info
            var infos = mapsinfo.Split(';').Select(x => x.Split(',')).ToArray();
            m_CurrentMapIndex = int.Parse(infos[1][0]);
            m_NextMapIndex = int.Parse(infos[1][1]);
            OPC(nameof(CurrentMapIndex));
            OPC(nameof(NextMapIndex));

            if (LogicalCurrentMap != null                )
            {
                LogicalCurrentMap.OPC(nameof(LogicalCurrentMap.IsCurrent));
                LogicalCurrentMap.OPC(nameof(LogicalCurrentMap.IsNext));
            }
            if (LogicalNextMap != null)
            {
                LogicalNextMap.OPC(nameof(LogicalCurrentMap.IsCurrent));
                LogicalCurrentMap.OPC(nameof(LogicalCurrentMap.IsNext));
            }
        }        
        public event PropertyChangedEventHandler PropertyChanged;
        public void OPC(string propname)
        {
            PropertyChanged?.Invoke(this , new PropertyChangedEventArgs(propname));
        }
    }
}
