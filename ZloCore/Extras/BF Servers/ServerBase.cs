﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Zlo.Extras
{
    /// <summary>
    /// used internally to represent a server between all bf games [don't use this]
    /// </summary>
    public abstract class ServerBase : IBFServerBase
    {
        internal ServerBase(uint id, ZloBFGame game)
        {
            ServerID = id;
            Game = game;
        }

        public ZloBFGame Game { get; protected set; }

        #region Props

        /// <summary>
        /// server id on zloemu
        /// </summary>
        public uint ServerID { get; }



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


        /// <summary>
        /// Attributes
        /// </summary>
        public Dictionary<string, string> Attributes { get; } = new Dictionary<string, string>();


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


        public API_PlayerListBase Players { get; } = new API_PlayerListBase();

        /// <summary>
        /// server settings
        /// </summary>
        public Dictionary<string, string> ServerSettings { get; } = new Dictionary<string, string>();


        public API_MapRotationBase MapRotation { get; } = new API_MapRotationBase();

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

        readonly char[] numbs = new char[]
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

        internal virtual void ParsePlayers(byte[] playersbuffer)
        {
            if (playersbuffer.Length == 0)
            {
                return;
            }
            Players.Parse(playersbuffer);
        }

        public override string ToString()
        {
            return !string.IsNullOrWhiteSpace(ServerName) ? ServerName : string.Empty;
        }

        public virtual void ParseBinaryReader(BinaryReader br)
        {
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
        void IBFServerBase.Parse(byte[] info)
        {

        }

        void IBFServerBase.ParsePlayers(byte[] playersbuffer)
        {
            ParsePlayers(playersbuffer);
        }
        string IBFServerBase.ToString() => ToString();
    }
}
