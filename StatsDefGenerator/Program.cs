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
            GenerateFromBF4StatsAPI();
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
        /// <summary>
        /// Generates GUIDs as json
        /// </summary>
        public static void Generate3()
        {
            JObject final = new JObject();
            string[] raw = File.ReadAllLines(@"D:\All Games\Battlefield 4\__Research\Raw JSONs\all GUIDs.txt");
            foreach (var item in raw)
            {
                var arr = item.TrimEnd(' ' , ';').Split('=');
                arr[0] = arr[0].Remove(0 , 3).TrimEnd('\'' , ']');
                arr[1] = arr[1].Trim('"');

                final[arr[0]] = arr[1];
            }
            File.WriteAllText(@"D:\All Games\Battlefield 4\__Research\Final\all GUIDs.json" , final.ToString());
        }


        public static void GenerateStatsConnectors()
        {
            //what to do ?
            //connect every id to its GUID
            //ids are from
            JObject GUIDs = JObject.Parse(File.ReadAllText(@"D:\All Games\Battlefield 4\__Research\Final\all GUIDs.json"));


            JObject WeaponsJson = JObject.Parse(File.ReadAllText(@"D:\All Games\Battlefield 4\__Research\Raw JSONs\loadouts def.json"));
            string WeaponsJsonFixedSavePath = @"D:\All Games\Battlefield 4\__Research\Raw JSONs\loadouts def_fixed.json";

            JObject weaponsFinal = new JObject();
            int wpcount = 0;
            foreach (var weapon in ((JObject)WeaponsJson["compact"]["weapons"]).Properties())
            {
                /*
                "94493788": {
        "category": "SHOTGUN",
        "name": "DBV-12",
        "weaponData": {
          "statHandling": 0.0935,
          "fireModeBurst": false,
          "statDamage": 0.9333,
          "statMobility": 0.1301,
          "fireModeSingle": true,
          "fireModeAuto": false,
          "statRange": 0.2831,
          "range": "SHORT",
          "rateOfFire": "SEMIAUTO",
          "altAmmoName": null,
          "ammoType": "12 gauge",
          "statAccuracy": 0.3175,
          "ammo": "10"
        },
        "Requirements": [
          {
            "nname": "sc_shotguns",
            "nvalue": 59000
          }
        ]
      }
      }
                 */
                string weaponNumericID = weapon.Name;
                //if it starts with :
                /*
                 * wp = weapon definition
                 * ic = image config
                 * uic = unlock image config                 
                 */
                if (weaponNumericID.StartsWith("wp"))
                {
                    //weapon data
                }
                else if (weaponNumericID.StartsWith("ic"))
                {
                    //image configs for the weapon
                }
                else if (weaponNumericID.StartsWith("uic"))
                {
                    //image configs for the weapon unlock
                }
                else
                {
                    wpcount += 1;
                    var w = (JObject)weapon.Value;
                    var toadd = new JObject();
                    //weapon generals

                    //category
                    if (w["category"] != null)
                    {
                        toadd["category"] = GUIDs[w.Value<string>("category")];
                    }
                    //name
                    if (w["name"] != null)
                    {
                        toadd["name"] = GUIDs[w.Value<string>("name")];
                    }
                    //desc
                    if (w["desc"] != null)
                    {
                        toadd["desc"] = GUIDs[w.Value<string>("desc")];
                    }
                    else
                    {

                    }


                    var see = w["see"];
                    if (see != null)
                    {
                        toadd["weaponData"] = null;
                        foreach (string to_see in (JArray)see)
                        {
                            if (to_see.StartsWith("wp"))
                            {
                                var data = (JObject)WeaponsJson["compact"]["weapons"][to_see]["weaponData"];

                                foreach (var d in data.Properties())
                                {
                                    if (GUIDs.Properties().Any(x => x.Name == d.Value.ToObject<string>()))
                                    {
                                        d.Value = GUIDs[d.Value.ToObject<string>()];
                                    }
                                }
                                toadd["weaponData"] = data;
                            }
                            if (to_see.StartsWith("ic"))
                            {
                                //get the stats name
                            }
                        }
                    }

                    var req = w["req"];
                    if (req != null)
                    {
                        var finalJArray = new JArray();
                        foreach (JObject to_req in (JArray)req)
                        {
                            if (to_req["c"] != null)
                            {
                                var new_req = new JObject();
                                new_req["nname"] = to_req["c"];
                                new_req["nvalue"] = to_req["v"];

                                finalJArray.Add(new_req);
                            }
                        }
                        toadd["Requirements"] = finalJArray;
                    }

                    weaponsFinal[weaponNumericID] = w;
                }

            }
            WeaponsJson["compact"]["weapons"] = weaponsFinal;
            Console.WriteLine($"Found {wpcount} Weapons");
            File.WriteAllText(WeaponsJsonFixedSavePath , WeaponsJson.ToString());

        }


        public static void Generate4()
        {
            string[] AllWeapons = File.ReadAllLines(@"D:\All Games\Battlefield 4\__Research\Raw JSONs\All Weapons.txt").Select(x => x.Split('=').Select(z => z.Trim()).ElementAt(0)).ToArray();

            JObject Final = new JObject();
            for (int i1 = 0; i1 < AllWeapons.Length; i1 += 5)
            {
                JObject weapon = new JObject();
                weapon["shots"] = AllWeapons[i1 + 0];
                weapon["hits"] = AllWeapons[i1 + 1];
                weapon["time"] = AllWeapons[i1 + 2];
                weapon["kills"] = AllWeapons[i1 + 3];
                weapon["headshots"] = AllWeapons[i1 + 4];

                var estimatedName = AllWeapons[i1].Split('_')[1];
                Final[estimatedName] = weapon;
            }
            File.WriteAllText(@"D:\All Games\Battlefield 4\__Research\Final\All Weapons.json" , Final.ToString());
        }
        public static void GenerateFromBF4StatsAPI()
        {
            JObject PlayerStats = JObject.Parse(File.ReadAllText(@"D:\All Games\Battlefield 4\__Research\Raw JSONs\BF4 stats API example.json"));
            string PlayerStatsFixedSavePath = @"D:\All Games\Battlefield 4\__Research\Raw JSONs\BF4 stats API example_fixed.json";



            PlayerStats["player"]["score"] = "stat.sc_rank";
            PlayerStats["player"]["timePlayed"] = "stat.c___sa_g";
            PlayerStats["player"]["rank"]["nr"] = "stat.rank";

            var stats = (JObject)PlayerStats["stats"];
            var scores = (JObject)stats["scores"];

            scores.Property("objective").Remove();

            scores["score"] = "stat.sc_rank";
            scores["award"] = "stat.sc_award";
            scores["bonus"] = "stat.sc_bonus";
            scores["unlock"] = "stat.sc_unlock";
            scores["vehicle"] = "stat.sc_vehicle";
            scores["team"] = "stat.sc_team";
            scores["squad"] = "stat.sc_squad";
            scores["general"] = "stat.sc_general";
            scores["award"] = "stat.sc_award";


            stats["skill"] = "stat.skill";
            stats["rank"] = "stat.rank";
            stats["kills"] = "stat.c___k_g";
            stats["deaths"] = "stat.c___d_g";
            stats["headshots"] = "stat.c___hsh_g";
            stats["shotsFired"] = "stat.c___sfw_g";
            stats["shotsHit"] = "stat.c___shw_g";
            stats["suppressionAssists"] = "stat.c___sua_g";
            stats["avengerKills"] = "stat.c___ak_g";
            stats["saviorKills"] = "stat.c___sk_g";
            stats["nemesisKills"] = "stat.c___nx_g";
            stats["numRounds"] = "stat.c___ro_g";
            stats["numLosses"] = "stat.c_mlos__roo_g";
            stats["numWins"] = "stat.c_mwin__roo_g";
            stats["killStreakBonus"] = "stat.c___k_ghvs";
            stats["nemesisStreak"] = "stat.c___nk_ghva";
            stats["mcomDefendKills"] = "stat.c___cdk_g";
            stats["resupplies"] = "stat.c___rs_g";
            stats["repairs"] = "stat.c___r_g";
            stats["heals"] = "stat.c___h_g";
            stats["revives"] = "stat.c___re_g";
            stats["longestHeadshot"] = "stat.c___hsd_ghva";
            stats["flagDefend"] = "stat.c___ccp_g";
            stats["flagCaptures"] = "stat.c___cpd_g";
            stats["killAssists"] = "stat.c___ka_g";
            stats["vehiclesDestroyed"] = "stat.c_vA__de_g";
            stats["vehicleDamage"] = "stat.c___vd_g";
            stats["dogtagsTaken"] = "stat.c___dt_g";

            var modes = (JArray)stats["modes"];
            /*
             Conquest = sc_conquest
Rush = sc_rush
Deathmatch = sc_deathmatch
Domination = sc_domination
Obliteration = sc_obliteration
Defuse = sc_bomber
Capture the Flag = sc_capturetheflag
Air Superiority = sc_airsuperiority
Carrier Assault = sc_carrierassault
Chain Link = sc_chainlink
Gun Master = sc_elimination
             */
            foreach (JObject m in modes)
            {
                string name = m["name"].ToObject<string>();
                string sc_name = "";
                switch (name)
                {
                    case "Conquest":
                        sc_name = "sc_conquest";
                        break;
                    case "Rush":
                        sc_name = "sc_rush";
                        break;
                    case "Deathmatch":
                        sc_name = "sc_deathmatch";
                        break;
                    case "Domination":
                        sc_name = "sc_domination";
                        break;
                    case "Capture the Flag":
                        sc_name = "sc_capturetheflag";
                        break;
                    case "Obliteration":
                        sc_name = "sc_obliteration";
                        break;
                    case "Air Superiority":
                        sc_name = "sc_airsuperiority";
                        break;
                    case "Defuse":
                        sc_name = "sc_bomber";
                        break;
                    case "Carrier Assault":
                        sc_name = "sc_carrierassault";
                        break;
                    case "Chain Link":
                        sc_name = "sc_chainlink";
                        break;
                    case "Gun Master":
                        sc_name = "sc_elimination";
                        break;
                    default:
                        break;
                }
                m["score"] = sc_name;
            }

            foreach (JObject item in (JArray)PlayerStats["weapons"])
            {
                string stats_code = item["detail"]["code"].ToObject<string>();
                item["stat"]["shots"] = $"stat.c_{stats_code}__sfw_g";
                item["stat"]["hits"] = $"stat.c_{stats_code}__shw_g";
                item["stat"]["time"] = $"stat.c_{stats_code}__sw_g";
                item["stat"]["kills"] = $"stat.c_{stats_code}__kwa_g";
                item["stat"]["headshots"] = $"stat.c_kany_{stats_code}_hsh_g";

                ((JObject)item["stat"]).Property("hs").Remove();

                item.Property("extra").Remove();

                var stars = ((JObject)item["stars"]);
                stars["curr"] = $"stat.ssha{stats_code}_00";
                stars["date"] = $"stat.ssha{stats_code}_01";
                stars["nname"] = "Kills";
                stars["nvalue"] = $"stat.c_{stats_code}__kwa_g";
                stars["relNeeded"] = 100;

                stars.Property("count").Remove();
                stars.Property("needed").Remove();
                stars.Property("relCurr").Remove();
                stars.Property("relProg").Remove();


                item["unlocksCurr"] = 0;

                var unlock = item["detail"]["unlock"];
                if (unlock != null && unlock.HasValues)
                {
                    unlock["nvalue"] = "stat." + unlock["code"]?.ToObject<string>();
                    ((JObject)unlock).Property("code").Remove();

                    unlock["nname"] = unlock["name"]?.ToObject<string>();
                    ((JObject)unlock).Property("name").Remove();
                }


            }
            //done weapons

            /*
             * Weapon category stats :
             * score = sc_stuff
             * kills = sum of contents
             * time = sum of contents
             * hs = sum of contents
             * hits = sum of contents
             * shots = sum of contents
             * 
             */

            var wepcat = (JArray)PlayerStats["weaponCategory"];

            foreach (JObject cat in wepcat)
            {
                string name = cat["name"].ToObject<string>();
                switch (name)
                {
                    case "SNIPER RIFLE":
                        cat["stat"]["score"] = "stat.sc_sniperrifles";
                        break;
                    case "PDW":
                        cat["stat"]["score"] = "stat.sc_pdws";
                        break;
                    case "CARBINE":
                        cat["stat"]["score"] = "stat.sc_carbines";
                        break;
                    case "LMG":
                        cat["stat"]["score"] = "stat.sc_lmgs";
                        break;
                    case "GRENADE":
                        cat["stat"]["score"] = "stat.sc_handgrenades";
                        break;
                    case "DMR":
                        cat["stat"]["score"] = "stat.sc_dmrs";
                        break;
                    case "ASSAULT RIFLE":
                        cat["stat"]["score"] = "stat.sc_assaultrifles";
                        break;
                    case "SHOTGUN":
                        cat["stat"]["score"] = "stat.sc_shotguns";
                        break;
                    case "SIDEARM":
                        cat["stat"]["score"] = "stat.sc_handguns";
                        break;
                    default:
                        break;
                }
            }
            //done weapon categories

            foreach (JObject k in (JArray)PlayerStats["kititems"])
            {
                k.Property("stat")?.Remove();
                ((JObject)k["detail"])?.Property("stat")?.Remove();
                k["detail"]["special"] = k["detail"]["ranked"];
                ((JObject)k["detail"])?.Property("ranked")?.Remove();

                var unl = k["detail"]["unlock"];
                if (unl != null && unl.HasValues)
                {
                    unl["curr"] = "stat." + unl["code"];
                    ((JObject)unl).Property("code").Remove();
                }
            }



            File.WriteAllText(PlayerStatsFixedSavePath , PlayerStats.ToString());
        }
        public static void GenerateFromBF4StatsAPI2()
        {

        }
    }
}
