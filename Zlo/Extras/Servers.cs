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
    public interface IServerBase
    {
        uint ServerID { get; }
        uint EXIP { get; set; }
        ushort EXPORT { get; set; }
        uint INIP { get; set; }
        ushort INPORT { get; set; }
        Dictionary<string , string> ATTRS { get; set; }
        string GNAM { get; set; }
        uint GSET { get; set; }
        byte GSTA { get; set; }
        byte IGNO { get; set; }
        byte PMAX { get; set; }
        ulong NATT { get; set; }
        byte NRES { get; set; }
        byte NTOP { get; set; }
        string PGID { get; set; }
        byte PRES { get; set; }
        byte QCAP { get; set; }
        uint SEED { get; set; }
        string UUID { get; set; }
        byte VOIP { get; set; }
        string VSTR { get; set; }

        void Parse(byte[] serverbuffer);
        void ParsePlayers(byte[] playerbuffer);
    }
    public abstract class ServerBase : IServerBase
    {
        public ServerBase(uint id)
        {
            m_ServerID = id;
        }

        #region Props
        private uint m_ServerID;

        public uint ServerID
        {
            get { return m_ServerID; }
        }

        private bool m_IsPWProtected;
        public bool IsPasswordProtected
        {
            get
            {
                return m_IsPWProtected;
            }
        }

        /// <summary>
        /// Actual server ip
        /// </summary>
        public uint EXIP { get; set; }

        /// <summary>
        /// Actual port
        /// </summary>
        public ushort EXPORT { get; set; }

        public uint INIP { get; set; }
        public ushort INPORT { get; set; }

        /// <summary>
        /// Attributes
        /// </summary>
        public Dictionary<string , string> ATTRS { get; set; }

        /// <summary>
        /// Server name
        /// </summary>
        public string GNAM { get; set; }

        /// <summary>
        /// game set
        /// </summary>
        public uint GSET { get; set; }

        /// <summary>
        /// server state [only run if it's 131]
        /// </summary>
        public byte GSTA { get; set; }

        public byte IGNO { get; set; }

        /// <summary>
        /// total player max
        /// </summary>
        public byte PMAX { get; set; }

        public ulong NATT { get; set; }
        public byte NRES { get; set; }
        public byte NTOP { get; set; }
        public string PGID { get; set; }
        public byte PRES { get; set; }

        /// <summary>
        /// slot capacity
        /// </summary>
        public byte QCAP { get; set; }

        public uint SEED { get; set; }
        public string UUID { get; set; }
        public byte VOIP { get; set; }

        /// <summary>
        /// server version
        /// </summary>
        public string VSTR { get; set; }


        private PlayerListBase m_Players;

        public PlayerListBase Players
        {
            get
            {
                if (m_Players == null)
                {
                    m_Players = new PlayerListBase();
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


        private MapRotation m_ATTRS_MapRotation;
        public MapRotation ATTRS_MapRotation
        {
            get
            {
                if (m_ATTRS_MapRotation == null)
                {
                    m_ATTRS_MapRotation = new MapRotation();
                }
                return m_ATTRS_MapRotation;
            }
        }


        #endregion
        public void Parse(byte[] serverbuffer)
        {
            ATTRS = new Dictionary<string , string>();
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
                        m_IsPWProtected = bool.Parse(ATTRS_Settings["vmsp"]);
                    }
                    else
                    {
                        m_IsPWProtected = false;
                    }                               
                }
                else
                {
                    m_IsPWProtected = bool.Parse(ATTRS_Settings["vmsp"]);
                }
                ATTRS_Settings.Remove("vmsp");
            }
            else
            {
                m_IsPWProtected = false;
            }
        }
        public void Parse(BinaryReader br)
        {
            if (br == null)
            {
                return;
            }
            ATTRS = new Dictionary<string , string>();

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
        public void ParsePlayers(byte[] playersbuffer)
        {
            if (playersbuffer.Length < 2)
            {
                return;
            }
            Players.Parse(playersbuffer);
        }
    }

    public class BF3ServerBase : ServerBase
    {
        public BF3ServerBase(uint id) : base(id)
        {
        }

        /// <summary>
        /// player cap
        /// </summary>
        public byte PCAP { get; set; }

        /// <summary>
        /// total cap
        /// </summary>
        public uint TCAP { get; set; }

        public new void Parse(byte[] serverbuffer)
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
                if (Dictionaries.BF3_Maps.ContainsKey(ATTRS["level"]))
                {
                    ATTRS_MapRotation.CurrentActualMap.MapName = Dictionaries.BF3_Maps[ATTRS["level"]];
                }
                else
                {
                    ATTRS_MapRotation.CurrentActualMap.MapName = ATTRS["level"];
                }
                ATTRS.Remove("level");
            }
            if (ATTRS.ContainsKey("mode"))
            {
                if (Dictionaries.BF3_GameModes.ContainsKey(ATTRS["mode"]))
                {
                    ATTRS_MapRotation.CurrentActualMap.GameModeName = Dictionaries.BF3_Maps[ATTRS["mode"]];
                }
                else
                {
                    ATTRS_MapRotation.CurrentActualMap.GameModeName = ATTRS["mode"];
                }
                ATTRS.Remove("mode");
            }
        }
    }
    public class BF4ServerBase : ServerBase
    {
        public BF4ServerBase(uint id) : base(id)
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
        public uint MACI { get; set; }

        /// <summary>
        /// first : public slots;
        /// second : private slots;
        /// third : public spect;
        /// fourth : private spect;
        /// </summary>
        public byte[] PCAP { get; set; } = new byte[4];
        public uint GMRG { get; set; }
        public tRNFO RNFO { get; set; }
        public string SCID { get; set; }
        public new void Parse(byte[] serverbuffer)
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
                    if (Dictionaries.BF4_Maps.ContainsKey(ATTRS["level"]))
                    {
                        ATTRS_MapRotation.CurrentActualMap.MapName = Dictionaries.BF4_Maps[ATTRS["level"]];
                    }
                    else
                    {
                        ATTRS_MapRotation.CurrentActualMap.MapName = ATTRS["level"];
                    }
                    ATTRS.Remove("level");
                }

                if (ATTRS.ContainsKey("levellocation"))
                {
                    if (Dictionaries.BF4_GameModes.ContainsKey(ATTRS["levellocation"]))
                    {
                        ATTRS_MapRotation.CurrentActualMap.GameModeName = Dictionaries.BF4_GameModes[ATTRS["levellocation"]];
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
    public class BFHServerBase : BF4ServerBase
    {
        public BFHServerBase(uint id) : base(id)
        {
        }
    }

    #endregion

    #region Player Bases
    public interface IPlayerBase
    {
        byte Slot { get; set; }
        uint ID { get; set; }
        string Name { get; set; }
    }

    public class PlayerListBase : List<PlayerBase>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public PlayerBase GetPlayer(uint id)
        {
            return Find(x => x.ID == id);
        }
        public void Parse(byte[] buffer)
        {
            Clear();
            using (var ms = new MemoryStream(buffer))
            using (var br = new BinaryReader(ms , Encoding.ASCII))
            {
                byte t = br.ReadByte();
                for (int i = 0; i < t; ++i)
                {
                    var p = new PlayerBase();
                    p.Slot = br.ReadByte();
                    p.ID = br.ReadZUInt32();
                    p.Name = br.ReadZString();
                    Add(p);
                }
            }
            CollectionChanged?.Invoke(this , new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
    public class PlayerBase : IPlayerBase
    {
        public byte Slot { get; set; }
        public uint ID { get; set; }
        public string Name { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
    #endregion

    #region ServersList
    public class BF3ServersList : List<BF3ServerBase>
    {
        public BF3ServersList(ZloClient client)
        {
            _client = client;
        }

        public event BF3ServerEventHandler ServerAdded;
        public event BF3ServerEventHandler ServerUpdated;
        public event BF3ServerEventHandler ServerRemoved;

        private ZloClient _client;

        public new void Add(BF3ServerBase server)
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
        public new void Remove(BF3ServerBase server)
        {
            if (server != null)
            {
                base.Remove(server);
                ServerRemoved?.Invoke(server.ServerID , server);
            }
        }

        public void Remove(uint ServerID)
        {
            var serv = Find(ServerID);
            if (serv != null)
            {
                //remove it
                base.Remove(serv);
                ServerRemoved?.Invoke(ServerID , serv);
            }
        }
        public void SafeAdd(BF3ServerBase server)
        {
            base.Add(server);
            ServerAdded?.Invoke(server.ServerID , server);
        }

        public void UpdateServerInfo(uint ServerID , byte[] info)
        {
            var serv = Find(ServerID);
            if (serv == null)
            {
                //server doesn't exist,create a new one
                serv = new BF3ServerBase(ServerID);
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
        public void UpdateServerPlayers(uint ServerID , byte[] info)
        {
            var serv = Find(ServerID);
            if (serv == null)
            {
                //server doesn't exist,create a new one
                serv = new BF3ServerBase(ServerID);
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
        public BF3ServerBase Find(uint ServerID)
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

    public class BF4ServersList : List<BF4ServerBase>
    {
        public BF4ServersList(ZloClient client)
        {
            _client = client;
        }

        public event BF4ServerEventHandler ServerAdded;
        public event BF4ServerEventHandler ServerUpdated;
        public event BF4ServerEventHandler ServerRemoved;

        private ZloClient _client;

        public new void Add(BF4ServerBase server)
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
        public new void Remove(BF4ServerBase server)
        {
            if (server != null)
            {
                base.Remove(server);
                ServerRemoved?.Invoke(server.ServerID , server);
            }
        }

        public void Remove(uint ServerID)
        {
            var serv = Find(ServerID);
            if (serv != null)
            {
                //remove it
                base.Remove(serv);
                ServerRemoved?.Invoke(ServerID , serv);
            }
        }
        public void SafeAdd(BF4ServerBase server)
        {
            base.Add(server);
            ServerAdded?.Invoke(server.ServerID , server);
        }

        public void UpdateServerInfo(uint ServerID , byte[] info)
        {
            var serv = Find(ServerID);
            if (serv == null)
            {
                //server doesn't exist,create a new one
                serv = new BF4ServerBase(ServerID);
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
        public void UpdateServerPlayers(uint ServerID , byte[] info)
        {
            var serv = Find(ServerID);
            if (serv == null)
            {
                //server doesn't exist,create a new one
                serv = new BF4ServerBase(ServerID);
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
        public BF4ServerBase Find(uint ServerID)
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


    public class BFHServersList : List<BFHServerBase>
    {
        public BFHServersList(ZloClient client)
        {
            _client = client;
        }

        public event BFHServerEventHandler ServerAdded;
        public event BFHServerEventHandler ServerUpdated;
        public event BFHServerEventHandler ServerRemoved;

        private ZloClient _client;

        public new void Add(BFHServerBase server)
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
        public new void Remove(BFHServerBase server)
        {
            if (server != null)
            {
                base.Remove(server);
                ServerRemoved?.Invoke(server.ServerID , server);
            }
        }

        public void Remove(uint ServerID)
        {
            var serv = Find(ServerID);
            if (serv != null)
            {
                //remove it
                base.Remove(serv);
                ServerRemoved?.Invoke(ServerID , serv);
            }
        }
        public void SafeAdd(BFHServerBase server)
        {
            base.Add(server);
            ServerAdded?.Invoke(server.ServerID , server);
        }

        public void UpdateServerInfo(uint ServerID , byte[] info)
        {
            var serv = Find(ServerID);
            if (serv == null)
            {
                //server doesn't exist,create a new one
                serv = new BFHServerBase(ServerID);
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
        public void UpdateServerPlayers(uint ServerID , byte[] info)
        {
            var serv = Find(ServerID);
            if (serv == null)
            {
                //server doesn't exist,create a new one
                serv = new BFHServerBase(ServerID);
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
        public BFHServerBase Find(uint ServerID)
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
