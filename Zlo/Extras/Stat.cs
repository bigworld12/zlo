using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zlo.Extras
{
    public struct Stat
    {
        public string FlagName { get; set; }
        public float FlagValue { get; set; }

        public Stat(string n,float v)
        {
            FlagName = n;
            FlagValue = v;
        }
   
        /// <summary>
        /// Represents the stat
        /// </summary>
        /// <returns>{FlagName} = {FlagValue}</returns>
        public override string ToString()
        {
            return $"{FlagName} = {FlagValue}";
        }
    }
}
