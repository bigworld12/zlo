using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Zlo.Extras;

namespace Zlo
{
    public static class GameDiscovery
    {
        internal static Dictionary<ZloBFGame, string> GamePaths => Settings.CurrentSettings.GamePaths;
        internal static HashSet<string> DiscoveryPaths => Settings.CurrentSettings.DiscoveryPaths;
        /// <summary>
        /// register a folder where games might exist
        /// </summary>
        /// <param name="path"></param>
        public static void RegisterDiscoveryFolder(string path)
        {
            DiscoveryPaths.Add(path);
            Settings.DoSaveSettings();
        }
        public static void RemoveDiscoveryFolder(string path)
        {
            DiscoveryPaths.Remove(path);
            Settings.DoSaveSettings();
        }
        private static Dictionary<ZloBFGame, string> GameFileNames { get; } = new Dictionary<ZloBFGame, string>()
        {
            { ZloBFGame.BF_3,"bf3.exe" },
            { ZloBFGame.BF_4,"bf3.exe" },
            { ZloBFGame.BF_HardLine,"bf3.exe" },
        };
        public static bool TryDiscoverGame(ZloBFGame game, out string path)
        {
            //first check cache
            if (GamePaths.TryGetValue(game, out var p))
            {
                if (File.Exists(p))
                {
                    path = p;
                    return true;
                }
            }


            //then check local folder
            //then check paths in 'DiscoveryPaths'
            //then check registry

            path = null;
            return false;
        }
    }
}
