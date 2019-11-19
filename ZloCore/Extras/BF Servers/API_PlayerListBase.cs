using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Zlo.Extras
{
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
                for (byte i = 0; i < t; ++i)
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
}
