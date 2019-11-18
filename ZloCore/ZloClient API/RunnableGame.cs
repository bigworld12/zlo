using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Zlo
{
    public struct RunnableGame
    {
        internal RunnableGame(string friendlyName, string runName, string zName)
        {
            FriendlyName = friendlyName;
            RunName = runName;
            ZName = zName;
        }

        /// <summary>
        /// Name to display
        /// </summary>
        public string FriendlyName { get; internal set; }
    
        /// <summary>
        /// pass this to api to run the game with the specific version
        /// </summary>
        public string RunName { get; internal set; }

        /// <summary>
        /// Name unique to each game without caring about x64 or x32
        /// </summary>
        public string ZName { get; internal set; }
    }

}
