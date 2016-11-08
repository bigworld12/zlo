using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zlo.Extras
{
    public struct Item
    {
        public string FlagName { get; set; }
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public bool ItemExists { get; set; }

        public Item(string flag , bool exists)
        {
            FlagName = flag;
            ItemExists = exists;
            var translated = GameData.BF4_GetTranslatedItem(flag);
            ItemName = translated["rname"].ToObject<string>();
            ItemDescription = translated["rdesc"].ToObject<string>();            
        }

        /// <summary>
        /// Represents the item
        /// </summary>
        /// <returns>{ItemName} = {ItemExists}</returns>
        public override string ToString()
        {
            return $"{ItemName} = {ItemExists}";
        }

    }
}
