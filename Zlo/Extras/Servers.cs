using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using static Zlo.Extentions.Helpers;
using System.Collections;

namespace Zlo.Extras
{
    #region Server Bases   
    /// <summary>
    /// used internally to represent a server between all bf games [don't use this]
    /// </summary>
    public abstract class ServerBase
    {
        internal ServerBase(uint id,ZloGame game)
        {
            m_ServerID = id;
            Game = game;
        }

        public ZloGame Game { get; protected set; }

        #region Props
        private uint m_ServerID;

        /// <summary>
        /// server id on zloemu
        /// </summary>
        public uint ServerID
        {
            get { return m_ServerID; }
        }



        /// <summary>
        /// whether  the server is protected by a password or not
        /// </summary>
        public bool IsPasswordProtected
        {
            get { return false; }
            private set { /*do noting , yet*/}
        }

        /// <summary>
        /// Actual server ip
        /// </summary>
        public uint ServerIP { get; internal set; }

        /// <summary>
        /// Actual port
        /// </summary>
        public ushort ServerPort { get; internal set; }

        public uint INIP { get; internal set; }
        public ushort INPORT { get; internal set; }


        private Dictionary<string, string> m_ATTRS;
        /// <summary>
        /// Attributes
        /// </summary>
        public Dictionary<string, string> Attributes
        {
            get
            {
                if (m_ATTRS == null)
                {
                    m_ATTRS = new Dictionary<string, string>();
                }
                return m_ATTRS;
            }
        }

        /// <summary>
        /// Server name
        /// </summary>
        public string ServerName { get; internal set; }

        /// <summary>
        /// game set
        /// </summary>
        public uint GameSet { get; internal set; }

        /// <summary>
        /// server state [only run if it's 131]
        /// </summary>
        public byte ServerState { get; internal set; }

        public byte IGNO { get; internal set; }

        /// <summary>
        /// total player max
        /// </summary>
        public byte MaxPlayers { get; internal set; }

        public ulong NATT { get; internal set; }
        public byte NRES { get; internal set; }
        public byte NTOP { get; internal set; }
        public string PGID { get; internal set; }
        public byte PRES { get; internal set; }

        /// <summary>
        /// slot capacity
        /// </summary>
        public byte SlotCapacity { get; internal set; }

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
                    BindingOperations.EnableCollectionSynchronization(m_Players, new object());
                }
                return m_Players;
            }
        }

        private Dictionary<string, string> m_ATTRS_Settings;
        /// <summary>
        /// server settings
        /// </summary>
        public Dictionary<string, string> ServerSettings
        {
            get
            {
                if (m_ATTRS_Settings == null)
                {
                    m_ATTRS_Settings = new Dictionary<string, string>();
                }
                return m_ATTRS_Settings;
            }
        }


        private API_MapRotationBase m_ATTRS_MapRotation;
        public API_MapRotationBase MapRotation
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
        internal virtual void Parse(byte[] serverbuffer)
        {
            Attributes.Clear();
            using (var ms = new MemoryStream(serverbuffer))
            using (var br = new BinaryReader(ms, Encoding.ASCII))
            {

                ServerIP = br.ReadZUInt32();
                ServerPort = br.ReadZUInt16();
                INIP = br.ReadZUInt32();
                INPORT = br.ReadZUInt16();
                byte t = br.ReadByte();

                for (byte i = 0; i < t; ++i)
                {
                    string key = br.ReadZString();
                    string value = br.ReadZString();
                    Attributes.Add(key, value);
                }


                ServerName = br.ReadZString();
                GameSet = br.ReadZUInt32();
                ServerState = br.ReadByte();
                IGNO = br.ReadByte();
                MaxPlayers = br.ReadByte();
                //was 64
                NATT = br.ReadZUInt64();
                //=========
                NRES = br.ReadByte();
                NTOP = br.ReadByte();
                PGID = br.ReadZString();
                PRES = br.ReadByte();
                SlotCapacity = br.ReadByte();
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
            ServerSettings.Clear();
            List<string> KeysToRemove = new List<string>();
            for (int i = 0; i < Attributes.Count; i++)
            {
                string key = Attributes.Keys.ElementAt(i);
                if (KeysToRemove.Contains(key))
                {
                    continue;
                }
                string value = Attributes[key];
                if (key.IndexOfAny(numbs) > -1)
                {
                    //it contains a number
                    //remove the number and call it CleanKey
                    string cleankey = new string(key.Where(x => !numbs.Contains(x)).ToArray());

                    var allkeys = Attributes.Keys.Where(x => new string(x.Where(z => !numbs.Contains(z)).ToArray()) == cleankey).OrderBy(q => q).ToList();
                    var allvalues = new List<string>();
                    foreach (var item in allkeys)
                    {
                        allvalues.Add(Attributes[item]);
                    }
                    string finalvalues = string.Join(string.Empty, allvalues);
                    Attributes.Add(cleankey, finalvalues);
                    KeysToRemove.AddRange(allkeys);
                }
            }

            foreach (var item in KeysToRemove)
            {
                Attributes.Remove(item);
            }
            if (Attributes.ContainsKey("settings"))
            {
                var pairset = Attributes["settings"].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in pairset)
                {
                    var splitpair = item.Split('=');
                    ServerSettings.Add(splitpair[0], splitpair[1]);
                }
                Attributes.Remove("settings");
            }
            if (ServerSettings.ContainsKey("vmsp"))
            {
                if (Attributes.ContainsKey("servertype"))
                {
                    if (Attributes["servertype"] == "PRIVATE")
                    {
                        IsPasswordProtected = bool.Parse(ServerSettings["vmsp"]);
                    }
                    else
                    {
                        IsPasswordProtected = false;
                    }
                }
                else
                {
                    IsPasswordProtected = bool.Parse(ServerSettings["vmsp"]);
                }
                ServerSettings.Remove("vmsp");
            }
            else
            {
                IsPasswordProtected = false;
            }
        }
        internal virtual void Parse(BinaryReader br)
        {
            if (br == null)
            {
                return;
            }
            Attributes.Clear();

            ServerIP = br.ReadZUInt32();
            ServerPort = br.ReadZUInt16();
            INIP = br.ReadZUInt32();
            INPORT = br.ReadZUInt16();
            byte t = br.ReadByte();

            for (byte i = 0; i < t; ++i)
            {
                string key = br.ReadZString();
                string value = br.ReadZString();
                Attributes.Add(key, value);
            }

            ServerName = br.ReadZString();
            GameSet = br.ReadZUInt32();
            ServerState = br.ReadByte();
            IGNO = br.ReadByte();
            MaxPlayers = br.ReadByte();
            //was 64
            NATT = br.ReadZUInt64();
            //=========
            NRES = br.ReadByte();
            NTOP = br.ReadByte();
            PGID = br.ReadZString();
            PRES = br.ReadByte();
            SlotCapacity = br.ReadByte();
            SEED = br.ReadZUInt32();
            UUID = br.ReadZString();
            VOIP = br.ReadByte();
            VSTR = br.ReadZString();

            FixAttrs();
        }
        internal virtual void ParsePlayers(byte[] playersbuffer)
        {
            if (playersbuffer.Length < 2)
            {
                return;
            }
            Players.Parse(playersbuffer);
        }

        public override string ToString()
        {
            return !string.IsNullOrWhiteSpace(ServerName) ? ServerName : string.Empty;
        }
    }

    public class API_BF3ServerBase : ServerBase
    {
        internal API_BF3ServerBase(uint id) : base(id, ZloGame.BF_3)
        {
        }

        /// <summary>
        /// player cap
        /// </summary>
        public byte PlayerCapacity { get; internal set; }

        /// <summary>
        /// total cap
        /// </summary>
        public uint TotalCapacity { get; internal set; }

        internal override void Parse(byte[] serverbuffer)
        {
            using (var ms = new MemoryStream(serverbuffer))
            using (var br = new BinaryReader(ms, Encoding.ASCII))
            {
                Parse(br);

                PlayerCapacity = br.ReadByte();
                TotalCapacity = br.ReadZUInt32();
            }

            if (Attributes.ContainsKey("maps") && Attributes.ContainsKey("mapsinfo"))
            {
                MapRotation.Parse(Attributes["mapsinfo"], Attributes["maps"], Game);
                Attributes.Remove("maps");
                Attributes.Remove("mapsinfo");
            }
            if (Attributes.ContainsKey("level"))
            {
                MapRotation.CurrentActualMap.MapName = API_Dictionaries.GetMapName(Game, Attributes["level"]);
                Attributes.Remove("level");
            }
            if (Attributes.ContainsKey("mode"))
            {
                MapRotation.CurrentActualMap.GameModeName = API_Dictionaries.GetGameModeName(Game, Attributes["mode"]);
                Attributes.Remove("mode");
            }
        }
    }
    public class API_BF4ServerBase : ServerBase
    {
        internal API_BF4ServerBase(uint id) : base(id, ZloGame.BF_4)
        {
        }

        public class tRNFO :
            Dictionary<
                string,
                Tuple<
                    uint,
                    Dictionary<
                        string,
                        string>>>
        {

        }
        public uint MACI { get; internal set; }

        /// <summary>
        /// [0] : public slots;
        /// [1] : private slots;
        /// [2] : public spect;
        /// [3] : private spect;
        /// </summary>
        public byte[] PlayerCapacities { get; internal set; } = new byte[4];
        public uint GMRG { get; internal set; }
        public tRNFO RNFO { get; internal set; }
        public string SCID { get; internal set; }
        internal override void Parse(byte[] serverbuffer)
        {
            try
            {
                using (var ms = new MemoryStream(serverbuffer))
                using (var br = new BinaryReader(ms, Encoding.ASCII))
                {
                    Parse(br);
                    var z = this;
                    RNFO = new tRNFO();



                    //uint8 t, t1; string ts;
                    MACI = br.ReadZUInt32();

                    PlayerCapacities[0] = br.ReadByte();
                    PlayerCapacities[1] = br.ReadByte();
                    PlayerCapacities[2] = br.ReadByte();
                    PlayerCapacities[3] = br.ReadByte();

                    GMRG = br.ReadByte();

                    byte t = br.ReadByte();
                    for (byte i = 0; i < t; ++i)
                    {
                        string first_key = br.ReadZString();

                        var second_dict = new Dictionary<string, string>();
                        uint first = 0;

                        first = br.ReadZUInt32();
                        var value_dict = new Tuple<uint, Dictionary<string, string>>
                            (
                             first, //first
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
                                second_dict.Add(key, value);
                            }
                        }

                        if (RNFO.ContainsKey(first_key))
                        {
                            RNFO[first_key] = value_dict;
                        }
                        else
                        {
                            RNFO.Add(first_key, value_dict);
                        }
                    }

                    SCID = br.ReadZString();
                }
                /*
                  Map       raw.ATTRS?["level"]
                  GameMode  raw.ATTRS?["levellocation"]*/
                if (Attributes.ContainsKey("maps") && Attributes.ContainsKey("mapsinfo"))
                {
                    MapRotation.Parse(Attributes["mapsinfo"], Attributes["maps"], Game);
                    Attributes.Remove("maps");
                    Attributes.Remove("mapsinfo");
                }

                if (Attributes.ContainsKey("level"))
                {
                    MapRotation.CurrentActualMap.MapName = API_Dictionaries.GetMapName(Game, Attributes["level"]);
                    Attributes.Remove("level");
                }

                if (Attributes.ContainsKey("levellocation"))
                {
                    MapRotation.CurrentActualMap.GameModeName = API_Dictionaries.GetGameModeName(Game, Attributes["levellocation"]);
                    Attributes.Remove("levellocation");
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
            Game = ZloGame.BF_HardLine;
        }
    }
    #endregion

    #region Player Bases
    public class API_PlayerListBase : List<API_PlayerBase>
    {
        /// <summary>
        /// gets a player by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public API_PlayerBase GetPlayer(uint id)
        {
            return Find(x => x.ID == id);
        }
        internal void Parse(byte[] buffer)
        {
            var old = ToArray();
            Clear();
            using (var ms = new MemoryStream(buffer))
            using (var br = new BinaryReader(ms, Encoding.ASCII))
            {
                byte t = br.ReadByte();
                for (int i = 0; i < t; ++i)
                {
                    byte slot = br.ReadByte();
                    uint id = br.ReadZUInt32();
                    string name = br.ReadZString();

                    var oldinst = old.FirstOrDefault(x => x.ID == id);
                    if (oldinst == null)
                    {
                        oldinst = new API_PlayerBase()
                        {
                            ID = id
                        };
                    }
                    oldinst.Slot = slot;
                    oldinst.Name = name;
                    Add(oldinst);
                }
                old = null;
            }
        }
    }
    public class API_PlayerBase
    {
        /// <summary>
        /// the in-game slot (don't know how to use yet)
        /// </summary>
        public byte Slot
        {
            get;
            internal set;
        }

        /// <summary>
        /// Player id on zloemu
        /// </summary>
        public uint ID
        {
            get;
            internal set;
        }

        /// <summary>
        /// Player name
        /// </summary>
        public string Name
        {
            get;
            internal set;
        }

        /// <summary>
        /// returns the player name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }
    #endregion

    #region ServersList
    /// <summary>
    /// represents the base server list for any bf3 server list that will be created
    /// </summary>
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
                ServerRemoved?.Invoke(server.ServerID, server);
            }
        }

        internal void Remove(uint ServerID)
        {
            var serv = Find(ServerID);
            if (serv != null)
            {
                //remove it
                base.Remove(serv);
                ServerRemoved?.Invoke(ServerID, serv);
            }
        }
        internal void SafeAdd(API_BF3ServerBase server)
        {
            base.Add(server);
            ServerAdded?.Invoke(server.ServerID, server);
        }

        internal void UpdateServerInfo(uint ServerID, byte[] info)
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
                ServerUpdated?.Invoke(ServerID, serv);
            }
        }
        internal void UpdateServerPlayers(uint ServerID, byte[] info)
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
                ServerUpdated?.Invoke(ServerID, serv);
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

    /// <summary>
    /// represents the base server list for any bf4 server list that will be created
    /// </summary>
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
                ServerRemoved?.Invoke(server.ServerID, server);
            }
        }

        internal void Remove(uint ServerID)
        {
            var serv = Find(ServerID);
            if (serv != null)
            {
                //remove it
                base.Remove(serv);
                ServerRemoved?.Invoke(ServerID, serv);
            }
        }
        internal void SafeAdd(API_BF4ServerBase server)
        {
            base.Add(server);
            ServerAdded?.Invoke(server.ServerID, server);
        }

        internal void UpdateServerInfo(uint ServerID, byte[] info)
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
                ServerUpdated?.Invoke(ServerID, serv);
            }
        }
        internal void UpdateServerPlayers(uint ServerID, byte[] info)
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
                ServerUpdated?.Invoke(ServerID, serv);
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

    /// <summary>
    /// represents the base server list for any bfh server list that will be created
    /// </summary>
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
                ServerRemoved?.Invoke(server.ServerID, server);
            }
        }

        internal void Remove(uint ServerID)
        {
            var serv = Find(ServerID);
            if (serv != null)
            {
                //remove it
                base.Remove(serv);
                ServerRemoved?.Invoke(ServerID, serv);
            }
        }
        internal void SafeAdd(API_BFHServerBase server)
        {
            base.Add(server);
            ServerAdded?.Invoke(server.ServerID, server);
        }

        internal void UpdateServerInfo(uint ServerID, byte[] info)
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
                ServerUpdated?.Invoke(ServerID, serv);
            }
        }
        internal void UpdateServerPlayers(uint ServerID, byte[] info)
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
                ServerUpdated?.Invoke(ServerID, serv);
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
