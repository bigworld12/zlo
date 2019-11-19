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
    /// <summary>
    /// describes a map in a map rotation
    /// </summary>
    public class API_MapBase
    {
        /// <summary>
        /// the owner map rotation
        /// </summary>
        public API_MapRotationBase ParentMapRotation { get; }



        /// <summary>
        /// the map name
        /// </summary>
        public string MapName { get; internal set; }

        /// <summary>
        /// the game mode name
        /// </summary>
        public string GameModeName { get; internal set; }

        /// <summary>
        /// is it the current map or not [related to map rotation]
        /// </summary>
        public bool IsCurrentInRotation
        {
            get
            {
                return ReferenceEquals(this, ParentMapRotation.LogicalCurrentMap);
            }
        }

        /// <summary>
        /// is it the next map or not [related to map rotation]
        /// </summary>
        public bool IsNextInRotation
        {
            get
            {
                return ReferenceEquals(this, ParentMapRotation.LogicalNextMap);
            }
        }

        /// <summary>
        /// is it the actual current map or not
        /// </summary>
        public bool IsActualCurrentMap
        {
            get
            {
                return Equals(ParentMapRotation.CurrentActualMap);
            }
        }

        public static bool operator ==(API_MapBase first, API_MapBase second)
        {
            if (first is null)
                return second is null;

            return first.Equals(second);
        }
        public static bool operator !=(API_MapBase first, API_MapBase second)
        {
            return !(first == second);
        }
        public override bool Equals(object obj)
        {
            if (obj is null || GetType() != obj.GetType())
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
                return true;
            var goodobj = obj as API_MapBase;
            return (goodobj.GameModeName == GameModeName) && (goodobj.MapName == MapName);
        }
        public override int GetHashCode()
        {
            return MapName.GetHashCode() ^ GameModeName.GetHashCode();
        }

        internal API_MapBase() { }
        internal API_MapBase(API_MapRotationBase p) { ParentMapRotation = p; }
        internal API_MapBase(string mname, string gmname, API_MapRotationBase p)
        {
            MapName = mname;
            GameModeName = gmname;
            ParentMapRotation = p;
        }
    }
    /// <summary>
    /// describes a map rotation
    /// </summary>
    public class API_MapRotationBase : Dictionary<int, API_MapBase>
    {
        internal API_MapRotationBase() { }
        /// <summary>
        /// the current map index <see cref="LogicalCurrentMap"/> to get the map related to that index
        /// </summary>
        public int CurrentMapIndex { get; private set; }
        /// <summary>
        /// the next map index <see cref="LogicalNextMap"/> to get the map related to that index
        /// </summary>
        public int NextMapIndex { get; private set; }


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

        internal void Parse(string mapsinfo, string mapsraw, ZloBFGame game)
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
            string[] rawmgms = mapsraw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < rawmgms.Length; i++)
            {
                //each map
                //name,gamemode
                var rawmgm = rawmgms[i].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (rawmgm.Length > 0)
                {
                    var m = new API_MapBase(this)
                    {
                        MapName = API_Dictionaries.GetMapName(game, rawmgm[0])
                    };
                    if (rawmgm.Length > 1)
                    {
                        m.GameModeName = API_Dictionaries.GetMapName(game, rawmgm[1]);
                    }
                    else
                    {
                        m.GameModeName = string.Empty;
                    }
                    Add(i, m);
                }

            }


            //parse maps info
            var infos = mapsinfo.Split(';').Select(x => x.Split(',')).ToArray();
            CurrentMapIndex = int.Parse(infos[1][0]);
            NextMapIndex = int.Parse(infos[1][1]);

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
