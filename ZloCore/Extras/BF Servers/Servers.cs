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

        internal override void Parse(byte[] serverbuffer)
        {
            using (var ms = new MemoryStream(serverbuffer))
            using (var br = new BinaryReader(ms, Encoding.ASCII))
            {
                ParseBinaryReader(br);

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
        internal API_BF4ServerBase(uint id) : base(id, ZloBFGame.BF_4)
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
            Game = ZloBFGame.BF_HardLine;
        }
    }
}
