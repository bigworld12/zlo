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

        private static JObject m_BF3_stats_def;
        public static JObject BF3_stats_def
        {
            get
            {
                if (m_BF3_stats_def == null)
                {
                    m_BF3_stats_def = JObject.Parse(LoadResourcedFile("BF3_stats.json"));
                }
                return (JObject)m_BF3_stats_def.DeepClone();
            }
        }

        private static JObject m_BF4_stats_def;
        public static JObject BF4_stats_def
        {
            get
            {
                if (m_BF4_stats_def == null)
                {
                    m_BF4_stats_def = JObject.Parse(LoadResourcedFile("BF4_Stats.json"));
                }
                return (JObject)m_BF4_stats_def.DeepClone();
            }
        }

        private static JObject m_bf4_ranks;
        public static JObject bf4_ranks
        {
            get
            {
                if (m_bf4_ranks == null)
                {
                    m_bf4_ranks = JObject.Parse(LoadResourcedFile("BF4_RanksDetails.json"));
                }
                return m_bf4_ranks;
            }
        }


        private static string[] ranknames { get; set; } = new string[]
              {
               "Noob",
                "Private First Class","Private First Class 1 Star","Private First Class 2 Stars","Private First Class 3 Stars",
                "Lance Corporal","Lance Corporal 1 Star","Lance Corporal 2 Stars", "Lance Corporal 3 Stars",
                "Corporal","Corporal 1 Star", "Corporal 2 Stars","Corporal 3 Stars",
                "Sergeant","Sergeant 1 Star","Sergeant 2 Stars","Sergeant 3 Stars",
                "Staff Sergeant","Staff Sergeant 1 Star","Staff Sergeant 2 Stars",
                "Gunnery Sergeant","Gunnery Sergeant 1 Star","Gunnery Sergeant 2 Stars",
                "Master Sergeant","Master Sergeant 1 Star","Master Sergeant 2 Stars",
                "First Sergeant","First Sergeant 1 Star","First Sergeant 2 Stars",
                "Master Gunnery Sergeant","Master Gunnery Sergeant 1 Star","Master Gunnery Sergeant 2 Stars",
                "Sergeant Major","Sergeant Major 1 Star","Sergeant Major 2 Star",
                "Warrant Officer 1","Chief Warrant Officer 2","Chief Warrant Officer 3","Chief Warrant Officer 4","Chief Warrant Officer 5",
                "Second Lieutenant","First Lieutenant",
                "Captain","Major","Lt. Colonel","Colonel"
              };
        private static int[] maxranks { get; set; } = new int[]
            {
                1000,
                7000,
                10000,
                11000,
                12000,
                13000,13000,
                14000,
                15000,15000,
                19000,
                20000,20000,20000,
                30000,30000,30000,30000,30000,30000,30000,30000,
                40000,40000,40000,40000,40000,40000,40000,
                50000,50000,50000,50000,50000,50000,50000,50000,
                55000,55000,
                60000,60000,60000,60000,60000,
                80000,
                230000
            };
        public static string GetBF3RankName(int rank)
        {
            if (rank <= 45)
            {
                return ranknames[rank];
            }
            else
            {
                return $"Colonel Service Star {rank - 45}";
            }
        }
        public static JObject GetBF4RankDetails(int rank)
        {
            return (JObject)bf4_ranks[rank.ToString()];
        }
        public static int GetRankMaxScore(int rank)
        {
            if (rank <= 45)
            {
                return maxranks[rank];
            }
            else
            {
                if (rank == 145)
                {
                    return 0;
                }
                return 230000;
            }
        }
        public static double SumIfNum(params JToken[] objects)
        {
            double final = 0;
            foreach (var item in objects)
            {
                if (IsNum(item))
                {
                    final += (double)item;
                }
            }
            return final;
        }
        public static bool IsNum(object obj)
        {
            double baseo;
            return double.TryParse(obj.ToString() , out baseo);
        }
    }
}
