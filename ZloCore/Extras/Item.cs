using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zlo.Extras
{
    /// <summary>
    /// Represents an items description
    /// </summary>
    public struct API_Item
    {
        /// <summary>
        /// flag name
        /// </summary>
        public string FlagName { get; internal set; }

        /// <summary>
        /// what the item's actual name is
        /// </summary>
        public string ItemName { get; internal set; }
        
        /// <summary>
        /// the item's description
        /// </summary>
        public string ItemDescription { get; internal set; }

        /// <summary>
        /// wether you unlocked it or not
        /// </summary>
        public bool ItemExists { get; internal set; }

        internal API_Item(string flag , bool exists,ZloGame game)
        {
            FlagName = flag;
            ItemExists = exists;


            API_Dictionaries.GetItemDetails(game, flag, out var name, out var desc);
            ItemName = name;
            ItemDescription = desc;
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
