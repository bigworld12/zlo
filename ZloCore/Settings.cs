using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zlo;
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

        private readonly static JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            ContractResolver = UntypedToTypedValueContractResolver.Instance,
            Converters = new[] { new StringEnumConverter() }, // If you prefer
        };

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
            DoSaveSettings();
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
                JsonConvert.PopulateObject(res, CurrentSettings, settings);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> TryLoadAsync()
        {
            return await Task.Run(() =>
            {
                return TryLoad();
            });
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
            var toSave = JsonConvert.SerializeObject(CurrentSettings, settings);
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
public class UntypedToTypedValueContractResolver : DefaultContractResolver
{
    static UntypedToTypedValueContractResolver() { Instance = new UntypedToTypedValueContractResolver(); }

    public static UntypedToTypedValueContractResolver Instance { get; private set; }

    protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
    {
        var contract = base.CreateDictionaryContract(objectType);

        if (contract.DictionaryValueType == typeof(object) && contract.ItemConverter == null)
        {
            contract.ItemConverter = new UntypedToTypedValueConverter();
        }

        return contract;
    }
}

class UntypedToTypedValueConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        throw new NotImplementedException("This converter should only be applied directly via ItemConverterType, not added to JsonSerializer.Converters");
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;
        var value = serializer.Deserialize(reader, objectType);
        if (value is TypeWrapper)
        {
            return ((TypeWrapper)value).ObjectValue;
        }
        return value;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (serializer.TypeNameHandling == TypeNameHandling.None)
        {
            Log.WriteLog("ObjectItemConverter used when serializer.TypeNameHandling == TypeNameHandling.None");
            serializer.Serialize(writer, value);
        }
        // Handle a couple of simple primitive cases where a type wrapper is not needed
        else if (value is string)
        {
            writer.WriteValue((string)value);
        }
        else if (value is bool)
        {
            writer.WriteValue((bool)value);
        }
        else
        {
            var contract = serializer.ContractResolver.ResolveContract(value.GetType());
            if (contract is JsonPrimitiveContract)
            {
                var wrapper = TypeWrapper.CreateWrapper(value);
                serializer.Serialize(writer, wrapper, typeof(object));
            }
            else
            {
                serializer.Serialize(writer, value);
            }
        }
    }
}

public abstract class TypeWrapper
{
    protected TypeWrapper() { }

    [JsonIgnore]
    public abstract object ObjectValue { get; }

    public static TypeWrapper CreateWrapper<T>(T value)
    {
        if (value == null)
            return new TypeWrapper<T>();
        var type = value.GetType();
        if (type == typeof(T))
            return new TypeWrapper<T>(value);
        // Return actual type of subclass
        return (TypeWrapper)Activator.CreateInstance(typeof(TypeWrapper<>).MakeGenericType(type), value);
    }
}

public sealed class TypeWrapper<T> : TypeWrapper
{
    public TypeWrapper() : base() { }

    public TypeWrapper(T value)
        : base()
    {
        this.Value = value;
    }

    public override object ObjectValue { get { return Value; } }

    public T Value { get; set; }
}
