using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Zlo.Extras.BF_Servers;

namespace Zlo.Extras
{
    public class API_BF3ServerBase : ServerBase
    {
        internal API_BF3ServerBase(uint id) : base(id, ZloBFGame.BF_3)
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
        internal override void ParseBinaryReader(BinaryReader br)
        {
            base.ParseBinaryReader(br);
            PlayerCapacity = br.ReadByte();
            TotalCapacity = br.ReadZUInt32();
        }
        protected override void FixAttrs()
        {
            base.FixAttrs();

            if (Attributes.TryGetValue("mode", out var mode))
            {
                MapRotation.CurrentActualMap.GameModeName = API_Dictionaries.GetGameModeName(Game, mode);
                Attributes.Remove("mode");
            }
        }
    }
    public class API_BF4ServerBase : ServerBase
    {
        internal API_BF4ServerBase(uint id) : base(id, ZloBFGame.BF_4)
        {
        }

        internal API_BF4ServerBase(uint id, ZloBFGame subGame) : base(id, subGame)
        {
        }
        public class tRNFO : Dictionary<(string, uint), Dictionary<string, string>>
        {
            public tRNFO()
            {

            }
            public tRNFO(int cap) : base(cap)
            {
            }
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
        internal override void ParseBinaryReader(BinaryReader br)
        {
            base.ParseBinaryReader(br);
            RNFO = new tRNFO();

            //uint8 t, t1; string ts;
            MACI = br.ReadZUInt32();

            PlayerCapacities[0] = br.ReadByte();
            PlayerCapacities[1] = br.ReadByte();
            PlayerCapacities[2] = br.ReadByte();
            PlayerCapacities[3] = br.ReadByte();

            GMRG = br.ReadByte();

            byte t = br.ReadByte();
            RNFO = new tRNFO(t);
            for (byte i = 0; i < t; ++i)
            {
                string first_key = br.ReadZString();
                uint first = br.ReadZUInt32();

                var t1 = br.ReadByte();
                var second_dict = new Dictionary<string, string>(t1);
                for (byte j = 0; j < t1; ++j)
                {
                    //server attributes
                    string key = br.ReadZString();
                    string value = br.ReadZString();
                    second_dict[key] = value;
                }
                RNFO[(first_key, first)] = second_dict;
            }
            SCID = br.ReadZString();
        }
        protected override void FixAttrs()
        {
            base.FixAttrs();
            if (Attributes.ContainsKey("levellocation"))
            {
                MapRotation.CurrentActualMap.GameModeName = API_Dictionaries.GetGameModeName(Game, Attributes["levellocation"]);
                Attributes.Remove("levellocation");
            }
        }
    }
    public class API_BFHServerBase : API_BF4ServerBase
    {
        internal API_BFHServerBase(uint id) : base(id, ZloBFGame.BF_HardLine)
        {
        }
    }
}
