using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Zlo
{  
    public static class ModuleInitializer
    {
        public static void Initialize()
        {
            //Init code
            if (Settings.TryLoad())
            {
                Log.WriteLog($"Loaded Settings from : ({Path.GetFullPath(Settings.SavePath)})");
            }
            else
            {
                Log.WriteLog($"Couldn't load settings from : ({Path.GetFullPath(Settings.SavePath)}), using default settings instead");
            }
        }
    }
}
