using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zlo.Extras
{
    public struct API_Stat
    {
        public string FlagName { get; internal set; }
        public float FlagValue { get; internal set; }

        internal API_Stat(string n,float v)
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
