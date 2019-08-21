using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zlo.Extras;

namespace Zlo
{
    [Serializable]
    public sealed class Settings
    {
        public static Settings CurrentSettings { get; } = new Settings();
        /// <summary>
        /// Default is Zlo_settings.json
        /// </summary>
        public static readonly string SavePath = "Zlo_settings.json";
        internal static void DoSaveSettings()
        {
            if (Save())
            {
                Log.WriteLog($"Saved settings to ({Path.GetFullPath(SavePath)})");
            }
            else
            {
                Log.WriteLog($"Couldn't save settings to ({Path.GetFullPath(SavePath)}), please select a different path and try again");
            }
        }
        [JsonProperty("last_game")]
        public ZloGame ActiveServerListener { get; set; } = ZloGame.None;

        public bool IsEnableDiscordRPC { get; set; } = true;

        /// <summary>
        /// Contains paths to injected dlls, please call <see cref="Save"/> after editing the lists
        /// </summary>
        [JsonProperty("dlls")]
        public DllInjectionSettings InjectedDlls { get; } = new DllInjectionSettings();




        [JsonProperty("customSettings")]
        private Dictionary<string, object> _customSettings = new Dictionary<string, object>();

        [JsonProperty("gamePaths")]
        internal Dictionary<ZloGame, string> GamePaths { get; } = new Dictionary<ZloGame, string>();
        [JsonProperty("discoveryPaths")]
        internal HashSet<string> DiscoveryPaths { get; } = new HashSet<string>();

        /// <summary>
        /// gets a custom setting value, if it doesn't exist it's created and saved
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetOrCreateCustomSetting<T>(string name, T defaultValue = default)
        {
            if (_customSettings.TryGetValue(name, out var res))
            {
                return (T)res;
            }
            _customSettings[name] = defaultValue;
            return defaultValue;
        }
        public void SetCustomSetting<T>(string name, T value)
        {
            _customSettings[name] = value;
        }


        public static bool TryLoad()
        {
            string res;
            try
            {
                if (File.Exists(SavePath))
                    res = File.ReadAllText(SavePath);
                else
                    return false;
            }
            catch
            {
                return false;
            }
            try
            {
                CurrentSettings.InjectedDlls.Bf3.Clear();
                CurrentSettings.InjectedDlls.Bf4.Clear();
                CurrentSettings.InjectedDlls.BfH.Clear();
                CurrentSettings.GamePaths.Clear();
                CurrentSettings.DiscoveryPaths.Clear();
                JsonConvert.PopulateObject(res, CurrentSettings);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool Save()
        {
            return SaveAsync().Result;
        }
        /// <summary>
        /// tries to save the file 5 times, then returns false if it fails
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> SaveAsync()
        {
            var toSave = JsonConvert.SerializeObject(CurrentSettings);
            int saveTries = 0;
            startSave:
            try
            {
                using (var sw = new StreamWriter(SavePath))
                {
                    await sw.WriteAsync(toSave);
                }
                return true;
            }
            catch
            {
                saveTries++;
                if (saveTries <= 5)
                {
                    await Task.Delay(200);
                    goto startSave;
                }
                else
                {
                    saveTries = 0;
                    return false;
                }
            }
        }
    }
    [Serializable]
    public sealed class DllInjectionSettings
    {
        [JsonProperty("bf3")]
        public List<string> Bf3 { get; } = new List<string>();
        [JsonProperty("bf4")]
        public List<string> Bf4 { get; } = new List<string>();
        [JsonProperty("bfh")]
        public List<string> BfH { get; } = new List<string>();

        public List<string> GetDllsList(ZloGame game)
        {
            switch (game)
            {
                case ZloGame.BF_3:
                    return Bf3;
                case ZloGame.BF_4:
                    return Bf4;
                case ZloGame.BF_HardLine:
                    return BfH;
                case ZloGame.None:
                default:
                    return null;
            }
        }
    }
}
