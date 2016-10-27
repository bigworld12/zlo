using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Zlo.Extentions.Helpers;
namespace Zlo.Extras
{

    #region Server Bases
    public interface IServerBase
    {
        uint ServerID { get; set; }
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
            Players = new PlayerListBase();
            ServerID = id;
        }

        #region Props
        public uint ServerID { get; set; }

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


        public PlayerListBase Players { get; set; }

        #endregion
        public void Parse(byte[] serverbuffer)
        {
            ATTRS = new Dictionary<string , string>();
            using (var ms = new MemoryStream(serverbuffer))
            using (var br = new BinaryReader(ms))
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
                NATT = br.ReadZUInt64();
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
        }
        public void ParsePlayers(byte[] playersbuffer)
        {
            if (Players == null)
            {
                Players = new PlayerListBase();
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
        byte PCAP { get; set; }

        /// <summary>
        /// total cap
        /// </summary>
        uint TCAP { get; set; }

        public new void Parse(byte[] serverbuffer)
        {
            base.Parse(serverbuffer);
            using (var ms = new MemoryStream(serverbuffer))
            using (var br = new BinaryReader(ms))
            {
                PCAP = br.ReadByte();
                TCAP = br.ReadZUInt32();
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
        uint MACI { get; set; }

        /// <summary>
        /// first : public slots;
        /// second : private slots;
        /// third : public spect;
        /// fourth : private spect;
        /// </summary>
        byte[] PCAP { get; set; } = new byte[4];
        uint GMRG { get; set; }
        tRNFO RNFO { get; set; }
        string SCID { get; set; }
        public new void Parse(byte[] serverbuffer)
        {
            base.Parse(serverbuffer);

            RNFO = new tRNFO();

            using (var ms = new MemoryStream(serverbuffer))
            using (var br = new BinaryReader(ms))
            {
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
                    var value_dict = new Tuple<uint , Dictionary<string , string>>
                        (
                        br.ReadZUInt32() , //first
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
                    RNFO.Add(first_key , value_dict);
                }

                SCID = br.ReadZString();
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

    public class PlayerListBase : List<PlayerBase>
    {
        public PlayerBase GetPlayer(uint id)
        {
            return Find(x => x.ID == id);
        }
        public void Parse(byte[] buffer)
        {
            Clear();
            using (var ms = new MemoryStream(buffer))
            using (var br = new BinaryReader(ms))
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
        }
    }
    public class PlayerBase : IPlayerBase
    {
        public byte Slot { get; set; }
        public uint ID { get; set; }
        public string Name { get; set; }
    }
    #endregion
}
