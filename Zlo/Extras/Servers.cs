using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using static Zlo.Extentions.Helpers;
namespace Zlo.Extras
{
    #region Server Bases   
    public abstract class ServerBase
    {
        internal ServerBase(uint id)
        {
            m_ServerID = id;
        }

        #region Props
        private uint m_ServerID;

        public uint ServerID
        {
            get { return m_ServerID; }
        }

        public bool IsPasswordProtected
        {
            get;private set;
        }

        /// <summary>
        /// Actual server ip
        /// </summary>
        public uint EXIP { get; internal set; }

        /// <summary>
        /// Actual port
        /// </summary>
        public ushort EXPORT { get; internal set; }

        public uint INIP { get; internal set; }
        public ushort INPORT { get; internal set; }


        private Dictionary<string , string> m_ATTRS;
        /// <summary>
        /// Attributes
        /// </summary>
        public Dictionary<string , string> ATTRS
        {
            get
            {
                if (m_ATTRS == null)
                {
                    m_ATTRS = new Dictionary<string , string>();
                }
                return m_ATTRS;
            }
        }

        /// <summary>
        /// Server name
        /// </summary>
        public string GNAM { get; internal set; }

        /// <summary>
        /// game set
        /// </summary>
        public uint GSET { get; internal set; }

        /// <summary>
        /// server state [only run if it's 131]
        /// </summary>
        public byte GSTA { get; internal set; }

        public byte IGNO { get; internal set; }

        /// <summary>
        /// total player max
        /// </summary>
        public byte PMAX { get; internal set; }

        public ulong NATT { get; internal set; }
        public byte NRES { get; internal set; }
        public byte NTOP { get; internal set; }
        public string PGID { get; internal set; }
        public byte PRES { get; internal set; }

        /// <summary>
        /// slot capacity
        /// </summary>
        public byte QCAP { get; internal set; }

        public uint SEED { get; internal set; }
        public string UUID { get; internal set; }
        public byte VOIP { get; internal set; }

        /// <summary>
        /// server version
        /// </summary>
        public string VSTR { get; internal set; }


        private API_PlayerListBase m_Players;

        public API_PlayerListBase Players
        {
            get
            {
                if (m_Players == null)
                {
                    m_Players = new API_PlayerListBase();
                    BindingOperations.EnableCollectionSynchronization(m_Players , new object());
                }
                return m_Players;
            }
        }

        private Dictionary<string , string> m_ATTRS_Settings;
        public Dictionary<string , string> ATTRS_Settings
        {
            get
            {
                if (m_ATTRS_Settings == null)
                {
                    m_ATTRS_Settings = new Dictionary<string , string>();
                }
                return m_ATTRS_Settings;
            }
        }


        private API_MapRotationBase m_ATTRS_MapRotation;
        public API_MapRotationBase ATTRS_MapRotation
        {
            get
            {
                if (m_ATTRS_MapRotation == null)
                {
                    m_ATTRS_MapRotation = new API_MapRotationBase();
                }
                return m_ATTRS_MapRotation;
            }
        }


        #endregion
        internal void Parse(byte[] serverbuffer)
        {
            ATTRS.Clear();
            using (var ms = new MemoryStream(serverbuffer))
            using (var br = new BinaryReader(ms , Encoding.ASCII))
            {

                EXIP = br.ReadZUInt32();
                EXPORT = br.ReadZUInt16();
                INIP = br.ReadZUInt32();
                INPORT = br.ReadZUInt16();
                byte t = br.ReadByte();

                for (byte i = 0; i < t; ++i)
                {
                    string key = br.ReadZString();
                    string value = br.ReadZString();
                    ATTRS.Add(key , value);
                }


                GNAM = br.ReadZString();
                GSET = br.ReadZUInt32();
                GSTA = br.ReadByte();
                IGNO = br.ReadByte();
                PMAX = br.ReadByte();
                //was 64
                NATT = br.ReadZUInt64();
                //=========
                NRES = br.ReadByte();
                NTOP = br.ReadByte();
                PGID = br.ReadZString();
                PRES = br.ReadByte();
                QCAP = br.ReadByte();
                SEED = br.ReadZUInt32();
                UUID = br.ReadZString();
                VOIP = br.ReadByte();
                VSTR = br.ReadZString();
            }
            FixAttrs();

        }

        char[] numbs = new char[]
            { '0' , '1' , '2' , '3' , '4' , '5' , '6' , '7' , '8' , '9' };
        private void FixAttrs()
        {
            ATTRS_Settings.Clear();
            List<string> KeysToRemove = new List<string>();
            for (int i = 0; i < ATTRS.Count; i++)
            {
                string key = ATTRS.Keys.ElementAt(i);
                if (KeysToRemove.Contains(key))
                {
                    continue;
                }
                string value = ATTRS[key];
                if (key.IndexOfAny(numbs) > -1)
                {
                    //it contains a number
                    //remove the number and call it CleanKey
                    string cleankey = new string(key.Where(x => !numbs.Contains(x)).ToArray());

                    var allkeys = ATTRS.Keys.Where(x => new string(x.Where(z => !numbs.Contains(z)).ToArray()) == cleankey).OrderBy(q => q).ToList();
                    var allvalues = new List<string>();
                    foreach (var item in allkeys)
                    {
                        allvalues.Add(ATTRS[item]);
                    }
                    string finalvalues = string.Join(string.Empty , allvalues);
                    ATTRS.Add(cleankey , finalvalues);
                    KeysToRemove.AddRange(allkeys);
                }
            }

            foreach (var item in KeysToRemove)
            {
                ATTRS.Remove(item);
            }
            if (ATTRS.ContainsKey("settings"))
            {
                var pairset = ATTRS["settings"].Split(new[] { ';' } , StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in pairset)
                {
                    var splitpair = item.Split('=');
                    ATTRS_Settings.Add(splitpair[0] , splitpair[1]);
                }
                ATTRS.Remove("settings");
            }
            if (ATTRS_Settings.ContainsKey("vmsp"))
            {
                if (ATTRS.ContainsKey("servertype"))
                {
                    if (ATTRS["servertype"] == "PRIVATE")
                    {
                        IsPasswordProtected = bool.Parse(ATTRS_Settings["vmsp"]);
                    }
                    else
                    {
                        IsPasswordProtected = false;
                    }
                }
                else
                {
                    IsPasswordProtected = bool.Parse(ATTRS_Settings["vmsp"]);
                }
                ATTRS_Settings.Remove("vmsp");
            }
            else
            {
                IsPasswordProtected = false;
            }
        }
        internal void Parse(BinaryReader br)
        {
            if (br == null)
            {
                return;
            }
            ATTRS.Clear();

            EXIP = br.ReadZUInt32();
            EXPORT = br.ReadZUInt16();
            INIP = br.ReadZUInt32();
            INPORT = br.ReadZUInt16();
            byte t = br.ReadByte();

            for (byte i = 0; i < t; ++i)
            {
                string key = br.ReadZString();
                string value = br.ReadZString();
                ATTRS.Add(key , value);
            }

            GNAM = br.ReadZString();
            GSET = br.ReadZUInt32();
            GSTA = br.ReadByte();
            IGNO = br.ReadByte();
            PMAX = br.ReadByte();
            //was 64
            NATT = br.ReadZUInt64();
            //=========
            NRES = br.ReadByte();
            NTOP = br.ReadByte();
            PGID = br.ReadZString();
            PRES = br.ReadByte();
            QCAP = br.ReadByte();
            SEED = br.ReadZUInt32();
            UUID = br.ReadZString();
            VOIP = br.ReadByte();
            VSTR = br.ReadZString();

            FixAttrs();
        }
        internal void ParsePlayers(byte[] playersbuffer)
        {
            if (playersbuffer.Length < 2)
            {
                return;
            }
            Players.Parse(playersbuffer);
        }

        public override string ToString()
        {
            return !string.IsNullOrWhiteSpace(GNAM) ? GNAM : string.Empty;
        }
    }

    public class API_BF3ServerBase : ServerBase
    {
        internal API_BF3ServerBase(uint id) : base(id)
        {
        }

        /// <summary>
        /// player cap
        /// </summary>
        public byte PCAP { get; internal set; }

        /// <summary>
        /// total cap
        /// </summary>
        public uint TCAP { get; internal set; }

        internal new void Parse(byte[] serverbuffer)
        {
            using (var ms = new MemoryStream(serverbuffer))
            using (var br = new BinaryReader(ms , Encoding.ASCII))
            {
                Parse(br);

                PCAP = br.ReadByte();
                TCAP = br.ReadZUInt32();
            }

            if (ATTRS.ContainsKey("maps") && ATTRS.ContainsKey("mapsinfo"))
            {
                ATTRS_MapRotation.Parse(ATTRS["mapsinfo"] , ATTRS["maps"] , ZloGame.BF_3);
                ATTRS.Remove("maps");
                ATTRS.Remove("mapsinfo");
            }
            if (ATTRS.ContainsKey("level"))
            {
                if (API_Dictionaries.API_BF3_Maps.ContainsKey(ATTRS["level"]))
                {
                    ATTRS_MapRotation.CurrentActualMap.MapName = API_Dictionaries.API_BF3_Maps[ATTRS["level"]];
                }
                else
                {
                    ATTRS_MapRotation.CurrentActualMap.MapName = ATTRS["level"];
                }
                ATTRS.Remove("level");
            }
            if (ATTRS.ContainsKey("mode"))
            {
                if (API_Dictionaries.API_BF3_GameModes.ContainsKey(ATTRS["mode"]))
                {
                    ATTRS_MapRotation.CurrentActualMap.GameModeName = API_Dictionaries.API_BF3_GameModes[ATTRS["mode"]];
                }
                else
                {
                    ATTRS_MapRotation.CurrentActualMap.GameModeName = ATTRS["mode"];
                }
                ATTRS.Remove("mode");
            }
        }
    }
    public class API_BF4ServerBase : ServerBase
    {
        internal API_BF4ServerBase(uint id) : base(id)
        {
        }

        public class tRNFO :
            Dictionary<
                string ,
                Tuple<
                    uint ,
                    Dictionary<
                        string ,
                        string>>>
        {

        }
        public uint MACI { get; internal set; }

        /// <summary>
        /// first : public slots;
        /// second : private slots;
        /// third : public spect;
        /// fourth : private spect;
        /// </summary>
        public byte[] PCAP { get; internal set; } = new byte[4];
        public uint GMRG { get; internal set; }
        public tRNFO RNFO { get; internal set; }
        public string SCID { get; internal set; }
        internal new void Parse(byte[] serverbuffer)
        {
            try
            {
                using (var ms = new MemoryStream(serverbuffer))
                using (var br = new BinaryReader(ms , Encoding.ASCII))
                {
                    Parse(br);
                    var z = this;
                    RNFO = new tRNFO();



                    //uint8 t, t1; string ts;
                    MACI = br.ReadZUInt32();

                    PCAP[0] = br.ReadByte();
                    PCAP[1] = br.ReadByte();
                    PCAP[2] = br.ReadByte();
                    PCAP[3] = br.ReadByte();

                    GMRG = br.ReadByte();

                    byte t = br.ReadByte();
                    for (byte i = 0; i < t; ++i)
                    {
                        string first_key = br.ReadZString();

                        var second_dict = new Dictionary<string , string>();
                        uint first = 0;

                        first = br.ReadZUInt32();
                        var value_dict = new Tuple<uint , Dictionary<string , string>>
                            (
                             first , //first
                            second_dict //second
                            );


                        byte t1 = br.ReadByte();
                        for (byte j = 0; j < t1; ++j)
                        {
                            //server attributes
                            string key = br.ReadZString();
                            string value = br.ReadZString();
                            if (second_dict.ContainsKey(key))
                            {
                                second_dict[key] = value;
                            }
                            else
                            {
                                second_dict.Add(key , value);
                            }
                        }

                        if (RNFO.ContainsKey(first_key))
                        {
                            RNFO[first_key] = value_dict;
                        }
                        else
                        {
                            RNFO.Add(first_key , value_dict);
                        }
                    }

                    SCID = br.ReadZString();
                }
                /*
                  Map       raw.ATTRS?["level"]
                  GameMode  raw.ATTRS?["levellocation"]*/
                if (ATTRS.ContainsKey("maps") && ATTRS.ContainsKey("mapsinfo"))
                {
                    ATTRS_MapRotation.Parse(ATTRS["mapsinfo"] , ATTRS["maps"] , ZloGame.BF_4);
                    ATTRS.Remove("maps");
                    ATTRS.Remove("mapsinfo");
                }

                if (ATTRS.ContainsKey("level"))
                {
                    if (API_Dictionaries.API_BF4_Maps.ContainsKey(ATTRS["level"]))
                    {
                        ATTRS_MapRotation.CurrentActualMap.MapName = API_Dictionaries.API_BF4_Maps[ATTRS["level"]];
                    }
                    else
                    {
                        ATTRS_MapRotation.CurrentActualMap.MapName = ATTRS["level"];
                    }
                    ATTRS.Remove("level");
                }

                if (ATTRS.ContainsKey("levellocation"))
                {
                    if (API_Dictionaries.API_BF4_GameModes.ContainsKey(ATTRS["levellocation"]))
                    {
                        ATTRS_MapRotation.CurrentActualMap.GameModeName = API_Dictionaries.API_BF4_GameModes[ATTRS["levellocation"]];
                    }
                    else
                    {
                        ATTRS_MapRotation.CurrentActualMap.GameModeName = ATTRS["levellocation"];
                    }
                    ATTRS.Remove("levellocation");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
    public class API_BFHServerBase : API_BF4ServerBase
    {
        internal API_BFHServerBase(uint id) : base(id)
        {
        }
    }
    #endregion

    #region Player Bases
    public class API_PlayerListBase : List<API_PlayerBase>
    {
        public API_PlayerBase GetPlayer(uint id)
        {
            return Find(x => x.ID == id);
        }
        internal void Parse(byte[] buffer)
        {
            var old = ToArray();
            Clear();
            using (var ms = new MemoryStream(buffer))
            using (var br = new BinaryReader(ms , Encoding.ASCII))
            {
                byte t = br.ReadByte();
                for (int i = 0; i < t; ++i)
                {
                    var p = new API_PlayerBase();
                    p.Slot = br.ReadByte();
                    p.ID = br.ReadZUInt32();
                    p.Name = br.ReadZString();
                    Add(p);
                }
            }
        }
    }
    public struct API_PlayerBase
    {
        public byte Slot { get; internal set; }
        public uint ID { get; internal set; }
        public string Name { get; internal set; }
        public override string ToString()
        {
            return Name;
        }
    }
    #endregion

    #region ServersList
    public class API_BF3ServersListBase : List<API_BF3ServerBase>
    {
        internal API_BF3ServersListBase(API_ZloClient client)
        {
            _client = client;
        }

        public event API_BF3ServerEventHandler ServerAdded;
        public event API_BF3ServerEventHandler ServerUpdated;
        public event API_BF3ServerEventHandler ServerRemoved;

        private API_ZloClient _client;

        internal new void Add(API_BF3ServerBase server)
        {
            if (Contains(server) || this.Any(x => x.ServerID == server.ServerID))
            {
                return;
            }
            else
            {
                SafeAdd(server);
            }
        }
        internal new void Remove(API_BF3ServerBase server)
        {
            if (server != null)
            {
                base.Remove(server);
                ServerRemoved?.Invoke(server.ServerID , server);
            }
        }

        internal void Remove(uint ServerID)
        {
            var serv = Find(ServerID);
            if (serv != null)
            {
                //remove it
                base.Remove(serv);
                ServerRemoved?.Invoke(ServerID , serv);
            }
        }
        internal void SafeAdd(API_BF3ServerBase server)
        {
            base.Add(server);
            ServerAdded?.Invoke(server.ServerID , server);
        }

        internal void UpdateServerInfo(uint ServerID , byte[] info)
        {
            var serv = Find(ServerID);
            if (serv == null)
            {
                //server doesn't exist,create a new one
                serv = new API_BF3ServerBase(ServerID);
                serv.Parse(info);
                Add(serv);
            }
            else
            {
                //server exists
                serv.Parse(info);
                ServerUpdated?.Invoke(ServerID , serv);
            }
        }
        internal void UpdateServerPlayers(uint ServerID , byte[] info)
        {
            var serv = Find(ServerID);
            if (serv == null)
            {
                //server doesn't exist,create a new one
                serv = new API_BF3ServerBase(ServerID);
                serv.ParsePlayers(info);
                Add(serv);
            }
            else
            {
                //server exists
                serv.ParsePlayers(info);
                ServerUpdated?.Invoke(ServerID , serv);
            }
        }
        public API_BF3ServerBase Find(uint ServerID)
        {
            for (int i = 0; i < Count; i++)
            {
                var elem = this[i];
                if (elem.ServerID == ServerID)
                {
                    return elem;
                }
                else
                {
                    continue;
                }
            }
            return null;
        }
    }

    public class API_BF4ServersListBase : List<API_BF4ServerBase>
    {
        internal API_BF4ServersListBase(API_ZloClient client)
        {
            _client = client;
        }

        public event API_BF4ServerEventHandler ServerAdded;
        public event API_BF4ServerEventHandler ServerUpdated;
        public event API_BF4ServerEventHandler ServerRemoved;

        private API_ZloClient _client;

        internal new void Add(API_BF4ServerBase server)
        {
            if (Contains(server) || this.Any(x => x.ServerID == server.ServerID))
            {
                return;
            }
            else
            {
                SafeAdd(server);
            }
        }
        internal new void Remove(API_BF4ServerBase server)
        {
            if (server != null)
            {
                base.Remove(server);
                ServerRemoved?.Invoke(server.ServerID , server);
            }
        }

        internal void Remove(uint ServerID)
        {
            var serv = Find(ServerID);
            if (serv != null)
            {
                //remove it
                base.Remove(serv);
                ServerRemoved?.Invoke(ServerID , serv);
            }
        }
        internal void SafeAdd(API_BF4ServerBase server)
        {
            base.Add(server);
            ServerAdded?.Invoke(server.ServerID , server);
        }

        internal void UpdateServerInfo(uint ServerID , byte[] info)
        {
            var serv = Find(ServerID);
            if (serv == null)
            {
                //server doesn't exist,create a new one
                serv = new API_BF4ServerBase(ServerID);
                serv.Parse(info);
                Add(serv);
            }
            else
            {
                //server exists
                serv.Parse(info);
                ServerUpdated?.Invoke(ServerID , serv);
            }
        }
        internal void UpdateServerPlayers(uint ServerID , byte[] info)
        {
            var serv = Find(ServerID);
            if (serv == null)
            {
                //server doesn't exist,create a new one
                serv = new API_BF4ServerBase(ServerID);
                serv.ParsePlayers(info);
                Add(serv);
            }
            else
            {
                //server exists
                serv.ParsePlayers(info);
                ServerUpdated?.Invoke(ServerID , serv);
            }
        }
        public API_BF4ServerBase Find(uint ServerID)
        {
            for (int i = 0; i < Count; i++)
            {
                var elem = this[i];
                if (elem.ServerID == ServerID)
                {
                    return elem;
                }
                else
                {
                    continue;
                }
            }
            return null;
        }
    }

    public class API_BFHServersListBase : List<API_BFHServerBase>
    {
        internal API_BFHServersListBase(API_ZloClient client)
        {
            _client = client;
        }

        public event API_BFHServerEventHandler ServerAdded;
        public event API_BFHServerEventHandler ServerUpdated;
        public event API_BFHServerEventHandler ServerRemoved;

        private API_ZloClient _client;

        internal new void Add(API_BFHServerBase server)
        {
            if (Contains(server) || this.Any(x => x.ServerID == server.ServerID))
            {
                return;
            }
            else
            {
                SafeAdd(server);
            }
        }
        internal new void Remove(API_BFHServerBase server)
        {
            if (server != null)
            {
                base.Remove(server);
                ServerRemoved?.Invoke(server.ServerID , server);
            }
        }

        internal void Remove(uint ServerID)
        {
            var serv = Find(ServerID);
            if (serv != null)
            {
                //remove it
                base.Remove(serv);
                ServerRemoved?.Invoke(ServerID , serv);
            }
        }
        internal void SafeAdd(API_BFHServerBase server)
        {
            base.Add(server);
            ServerAdded?.Invoke(server.ServerID , server);
        }

        internal void UpdateServerInfo(uint ServerID , byte[] info)
        {
            var serv = Find(ServerID);
            if (serv == null)
            {
                //server doesn't exist,create a new one
                serv = new API_BFHServerBase(ServerID);
                serv.Parse(info);
                Add(serv);
            }
            else
            {
                //server exists
                serv.Parse(info);
                ServerUpdated?.Invoke(ServerID , serv);
            }
        }
        internal void UpdateServerPlayers(uint ServerID , byte[] info)
        {
            var serv = Find(ServerID);
            if (serv == null)
            {
                //server doesn't exist,create a new one
                serv = new API_BFHServerBase(ServerID);
                serv.ParsePlayers(info);
                Add(serv);
            }
            else
            {
                //server exists
                serv.ParsePlayers(info);
                ServerUpdated?.Invoke(ServerID , serv);
            }
        }
        public API_BFHServerBase Find(uint ServerID)
        {
            for (int i = 0; i < Count; i++)
            {
                var elem = this[i];
                if (elem.ServerID == ServerID)
                {
                    return elem;
                }
                else
                {
                    continue;
                }
            }
            return null;
        }
    }
    #endregion
}
