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
    public class API_MapBase
    {


        private API_MapRotationBase _parent;
        public API_MapRotationBase ParentMapRotation
        {
            get
            {
                return _parent;
            }
        }




        public string MapName { get; internal set; }


        public string GameModeName { get; internal set; }

        public bool IsCurrentInRotation
        {
            get
            {
                return ReferenceEquals(this , ParentMapRotation.LogicalCurrentMap);
            }
        }
        public bool IsNextInRotation
        {
            get
            {
                return ReferenceEquals(this , ParentMapRotation.LogicalNextMap);
            }
        }
        public bool IsActualCurrentMap
        {
            get
            {
                return Equals(ParentMapRotation.CurrentActualMap);
            }
        }

        public static bool operator ==(API_MapBase first , API_MapBase second)
        {
            if (ReferenceEquals(first , null))
                return ReferenceEquals(second , null);

            return first.Equals(second);
        }
        public static bool operator !=(API_MapBase first , API_MapBase second)
        {
            return !(first == second);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj , null) || GetType() != obj.GetType())
            {
                return false;
            }

            if (ReferenceEquals(this , obj))
                return true;
            var goodobj = obj as API_MapBase;
            return (goodobj.GameModeName == GameModeName) && (goodobj.MapName == MapName);
        }
        public override int GetHashCode()
        {
            return MapName.GetHashCode() ^ GameModeName.GetHashCode();
        }

        internal API_MapBase() { }
        internal API_MapBase(API_MapRotationBase p) { _parent = p; }
        internal API_MapBase(string mname , string gmname , API_MapRotationBase p)
        {
            MapName = mname;
            GameModeName = gmname;
            _parent = p;
        }
    }
    public class API_MapRotationBase : Dictionary<int , API_MapBase>
    {
        internal API_MapRotationBase() { }

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


        public API_MapBase LogicalCurrentMap
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
        public API_MapBase LogicalNextMap
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


        private API_MapBase m_CurrentActualMap;
        public API_MapBase CurrentActualMap
        {
            get
            {
                if (m_CurrentActualMap == null)
                {
                    m_CurrentActualMap = new API_MapBase(this);
                }
                return m_CurrentActualMap;
            }
        }

        internal void Parse(string mapsinfo , string mapsraw , ZloGame game)
        {
            API_MapBase[] oldmaps;
            if (Values != null)
            {
                oldmaps = Values.ToArray();
            }
            else
            {
                oldmaps = null;
            }

            int oldcur = CurrentMapIndex;
            int oldnext = NextMapIndex;
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
                    var m = new API_MapBase(this);
                    switch (game)
                    {
                        case ZloGame.BF_3:
                            if (API_Dictionaries.API_BF3_Maps.ContainsKey(rawmgm[0]))
                            {
                                m.MapName = API_Dictionaries.API_BF3_Maps[rawmgm[0]];
                            }
                            else
                            {
                                m.MapName = rawmgm[0];
                            }
                            if (rawmgm.Length > 1)
                            {
                                if (API_Dictionaries.API_BF3_GameModes.ContainsKey(rawmgm[1]))
                                {
                                    m.GameModeName = API_Dictionaries.API_BF3_GameModes[rawmgm[1]];
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
                            if (API_Dictionaries.API_BF4_Maps.ContainsKey(rawmgm[0]))
                            {
                                m.MapName = API_Dictionaries.API_BF4_Maps[rawmgm[0]];
                            }
                            else
                            {
                                m.MapName = rawmgm[0];
                            }
                            if (rawmgm.Length > 1)
                            {
                                if (API_Dictionaries.API_BF4_GameModes.ContainsKey(rawmgm[1]))
                                {
                                    m.GameModeName = API_Dictionaries.API_BF4_GameModes[rawmgm[1]];
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

            //if (LogicalCurrentMap != null)
            //{
            //    LogicalCurrentMap.OPC(nameof(LogicalCurrentMap.IsCurrent));
            //    LogicalCurrentMap.OPC(nameof(LogicalCurrentMap.IsNext));
            //}
            //if (LogicalNextMap != null)
            //{
            //    LogicalNextMap.OPC(nameof(LogicalCurrentMap.IsCurrent));
            //    LogicalCurrentMap.OPC(nameof(LogicalCurrentMap.IsNext));
            //}            
        }
    }
}
