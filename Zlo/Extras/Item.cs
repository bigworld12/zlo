using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zlo.Extras
{
    public struct Item
    {
        public string ItemName { get; set; }
        public bool ItemExists { get; set; }

        public Item(string n , bool v)
        {
            ItemName = n;
            ItemExists = v;
        }

        /// <summary>
        /// Represents the stat
        /// </summary>
        /// <returns>{ItemName} = {ItemExists}</returns>
        public override string ToString()
        {
            return $"{ItemName} = {ItemExists}";
        }

    }
}
