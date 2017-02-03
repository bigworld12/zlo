using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatsDefGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            //cur rank 55
            //XP Max Total = 3050000
            //current xp = 3131691

            Console.WriteLine("Started");
            Generate2();
            Console.WriteLine("Done");
            Console.ReadKey();
        }
        /// <summary>
        /// Rank Names,scores
        /// </summary>
        public static void Generate1()
        {
            string sourceRanksPath = @"D:\All Games\Battlefield 4\__Research\ranksnames_fixed.json";
            string fixedRanksPath = @"D:\All Games\Battlefield 4\__Research\ranksnames_fixed.json";
            var sourceRanks = JObject.Parse(File.ReadAllText(sourceRanksPath));
            
            foreach (JProperty item in sourceRanks.Properties())
            {
                var val = (JObject)item.Value;                
                if (val["Unlocks"].ToString() == "N/A")
                {
                    val["Unlocks"] = JArray.FromObject(new string[0]);
                }
                else
                {
                    val["Unlocks"] = JArray.FromObject(new string[] { val["Unlocks"].ToString() });
                }
            }
            File.WriteAllText(fixedRanksPath , sourceRanks.ToString());
        }
        /// <summary>
        /// understanding the pattern of scores
        /// </summary>
        public static void Generate2()
        {
            string example_stats_file_path = @"D:\All Games\Battlefield 4\__Research\bigworld_12_BF4_stats.txt";
            string final_cats_file_path = @"D:\All Games\Battlefield 4\__Research\bigworld_12_BF4_stats_cats_def.json";
            JObject final = new JObject();
            
            foreach (var unt in File.ReadAllLines(example_stats_file_path))
            {
                if (string.IsNullOrWhiteSpace(unt))
                {
                    continue;
                }
                var _ = unt.Split('=').Select(x => x.Trim()).ToArray();
                var propname = _[0];
                var value = double.Parse(_[1]);
                //c___sfw_g
                //{ "c", "", "", "sfw", "g" }
                var cats = propname.Split('_');              
                JProperty ActiveCat = null;                
                foreach (var cat in cats)
                {
                    if (ActiveCat == null)
                    {
                        //first time
                        var jcat = final.Property(cat);
                        if (jcat == null)
                        {
                            final[cat] = new JObject();
                        }
                        ActiveCat = final.Property(cat);
                    }
                    else
                    {      
                        var prop = ((JObject)ActiveCat.Value).Property(cat);                     
                        if (prop == null)
                        {
                            ActiveCat.Value[cat] = new JObject();                            
                        }
                        ActiveCat = ((JObject)ActiveCat.Value).Property(cat);
                    }                    
                }
                ActiveCat.Value["value"] = value;
            }
            File.WriteAllText(final_cats_file_path , final.ToString());
        }
    }
}
