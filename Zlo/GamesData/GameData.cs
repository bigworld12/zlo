using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Zlo
{
    internal static class GameData
    {
        public static string LoadResourcedFile(string file_name)
        {            
            var ass = typeof(GameData).Assembly;
            var allres = ass.GetManifestResourceNames();
            string rname = allres.FirstOrDefault(x => x.ToLower().Contains(file_name.ToLower()));
            if (!string.IsNullOrWhiteSpace(rname))
            {
                using (Stream stream = ass.GetManifestResourceStream(rname))
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            else
            {
                return string.Empty;
            }
        }

        private static JObject m_bf4_items;
        public static JObject BF4_items
        {
            get
            {
                if (m_bf4_items == null)
                {
                    m_bf4_items = JObject.Parse(LoadResourcedFile("BF4_Items.json"));
                }
                return m_bf4_items;
            }
        }

        public static JObject BF4_GetTranslatedItem(string flagname)
        {
            return (JObject)BF4_items[flagname];
        }
    }
}
