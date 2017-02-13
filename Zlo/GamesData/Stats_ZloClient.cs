using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zlo.Extras;
using static Zlo.GameData;
namespace Zlo
{
    public partial class API_ZloClient
    {
        private void API_ZloClient_ItemsReceived(ZloGame Game , Dictionary<string,API_Item> List)
        {

            switch (Game)
            {
                case ZloGame.BF_4:
                    {
                        //var finaldef = BF4_stats_def;
                        //foreach (var item in List)
                        //{
                        //    finaldef = finaldef.Replace($@"""item.{item.FlagName}""" , item.ItemExists.ToString());
                        //}
                        //m_BF4_Stats = JObject.Parse(finaldef);

                        //File.WriteAllText($".\\{CurrentPlayerName}_BF4_stats.json" , BF4_Stats.ToString());
                        File.WriteAllLines($".\\{CurrentPlayerName}_BF4_items_raw.txt" , List.Select(x => $"{x.Key} = {x.Value.ItemExists.ToString()}"));
                        break;
                    }
                case ZloGame.BF_HardLine:
                    break;
                case ZloGame.None:
                    break;
                default:
                    break;
            }
            RaiseItemsReceived(Game , List);
        }

        private void API_ZloClient_StatsReceived(ZloGame Game , Dictionary<string , float> List)
        {
            switch (Game)
            {
                case ZloGame.BF_3:
                    {
                        m_BF3_Stats = JObject.Parse(BF3_stats_def);
                        AssignStats(List , BF3_Stats);
                        BF3_Stats_Handler();
                        File.WriteAllText($".\\{CurrentPlayerName}_BF3_stats.json" , BF3_Stats.ToString());
                        break;
                    }
                case ZloGame.BF_4:
                    {
                        m_BF4_Stats = JObject.Parse(BF4_stats_def);
                        AssignStats(List , BF4_Stats);
                        BF4_Stats_Handler();
                        var str = BF4_Stats.ToString();
                        File.WriteAllText($".\\{CurrentPlayerName}_BF4_stats.json" , str);
                        File.WriteAllLines($".\\{CurrentPlayerName}_BF4_stats_raw.txt" , List.Select(x => x.ToString()));
                        break;
                    }
                case ZloGame.BF_HardLine:
                    break;
                case ZloGame.None:
                    break;
                default:
                    break;
            }
            RaiseStatsReceived(Game , List);
        }

        private void BF3_Stats_Handler()
        {
            //edit custom stats
            BF3_Stats["plat"] = "pc";
            BF3_Stats["id"] = CurrentPlayerID;
            BF3_Stats["name"] = CurrentPlayerName;
            BF3_Stats["last_update_date"] = DateTime.Now;
            //BF3_Stats["tag"] = ;

            var s = BF3_Stats["stats"];
            int rank = s.Value<int>("rank");
            s["rankname"] = GetBF3RankName(rank);

            var scores = s["scores"];

            var vehicles = SumIfNum(
               scores["vehicleaa"] ,
               scores["vehicleah"] ,
               scores["vehicleifv"] ,
               scores["vehiclejet"] ,
               scores["vehiclembt"] ,
               scores["vehiclesh"] ,
               scores["vehiclelbt"] ,
               scores["vehicleart"]);
            scores["vehicleall"] = vehicles;
            var combat = SumIfNum(
                scores["support"] ,
                scores["assault"] ,
                scores["engineer"] ,
                scores["recon"]) +
                vehicles;

            double allscore = 0;
            allscore = combat + SumIfNum(scores["unlock"] , scores["award"] , scores["special"]);

            scores["maxxp"] = GetRankMaxScore(rank);
            scores["shortxp"] = allscore - sumfrom0to(rank);
            scores["longxp"] = allscore;
        }


        private void BF4_Stats_Handler()
        {
            //edit custom stats
            BF4_Stats["player"]["id"] = CurrentPlayerID;
            BF4_Stats["player"]["name"] = CurrentPlayerName;
            BF4_Stats["player"]["last_update_date"] = DateTime.Now;

            //rank
            long score = BF4_Stats["player"]["score"].ToObject<long>();
            var todoRankObject = (JObject)BF4_Stats["player"]["rank"];
            int rank = todoRankObject.Value<int>("nr");

            var rankdets = GetBF4RankDetails(rank);
            var nextrankdets = GetBF4RankDetails(rank + 1);

            var curmin = rankdets["XP Min Total"].ToObject<long>();
            var relScore = score - curmin;
            var curmax = nextrankdets["XP Min Relative"].ToObject<long>();

            todoRankObject["imgLarge"] = todoRankObject.Value<string>("imgLarge").Replace("140" , rank.ToString());
            todoRankObject["img"] = $"r{rank}";
            todoRankObject["name"] = rankdets["Rank Title"];
            todoRankObject["Unlocks"] = rankdets["Unlocks"];
            todoRankObject["Short XP"] = relScore;
            todoRankObject["Long XP"] = score;
            todoRankObject["needed"] = curmax - relScore;
            todoRankObject["Max XP"] = curmax;

            var nextTodoRankObject = (JObject)(todoRankObject["next"] = new JObject());

            nextTodoRankObject["imgLarge"] = todoRankObject.Value<string>("imgLarge").Replace(rank.ToString() , (rank + 1).ToString());
            nextTodoRankObject["img"] = $"r{rank + 1}";
            nextTodoRankObject["name"] = nextrankdets["Rank Title"];
            nextTodoRankObject["Unlocks"] = nextrankdets["Unlocks"];

            //kits
            foreach (JProperty kitprop in ((JObject)BF4_Stats["stats"]["kits"]).Properties())
            {
                var kit = ((JObject)kitprop.Value);
                int starsC = kit["stars"]["count"].ToObject<int>();
                int starsCScore = kit["stars"]["curr"].ToObject<int>();
                int Max = kit["stars"]["Max"].ToObject<int>();
                int ShortCurr = starsCScore - starsC * Max;
                kit["stars"]["shortCurr"] = ShortCurr;
                kit["stars"]["progress"] = ShortCurr / Max * 100;
            }

            //weapons
            foreach (JObject weapon in (JArray)BF4_Stats["weapons"])
            {
                var ws = (JObject)weapon["stat"];

                double kills = ws.Value<int>("kills");
                double shots = ws.Value<int>("shots");
                double hits = ws.Value<int>("hits");
                double time = ws.Value<int>("time");
                double headshots = ws.Value<int>("headshots");

                var unlock = (JObject)weapon["detail"]["unlocks"];
                if (unlock != null)
                {
                    unlock["isUnlocked"] = unlock["nvalue"].ToObject<int>() > unlock["needed"].ToObject<int>();
                }

                var stars = (JObject)weapon["stars"];
                int starsCount = (int)(kills / 100);
                stars["curr"] = starsCount;
                stars["shortNValue"] = kills - starsCount * 100;

                var unlocks = (JArray)weapon["unlocks"];
                if (unlocks != null)
                {
                    int unlcount = 0;
                    foreach (var unl in unlocks)
                    {
                        bool isunl = kills > unl["needed"].ToObject<int>();
                        unl["isUnlocked"] = isunl;
                        if (isunl)
                        {
                            unlcount += 1;
                        }
                    }
                    weapon["unlocksCurr"] = unlcount;
                }




                var extra = new JObject();
                extra["accuracy"] = hits / shots;
                extra["kills per min"] = (kills) / (time / 60);
                extra["shots fired per min"] = (shots) / (time / 60);
                extra["headshot per kill"] = headshots / kills;
                weapon["extra"] = extra;
            }

            //weapon categories
            //get all the weapons with the same category name
            foreach (JObject cat in (JArray)BF4_Stats["weaponCategory"])
            {
                //weapon.detail.category
                var catname = cat["name"].ToObject<string>();

                double? catscore = 0;
                if (cat["stat"]["score"] != null)
                {
                    catscore = cat["stat"].Value<double?>("score");
                }


                double TOTtime = 0;
                double TOTkills = 0;
                double TOThs = 0;
                double TOThits = 0;
                double TOTshots = 0;
                foreach (JObject weapon in (JArray)BF4_Stats["weapons"])
                {
                    if (weapon["detail"]["category"]?.ToObject<string>() != catname)
                    {
                        continue;
                    }
                    var ws = (JObject)weapon["stat"];

                    double kills = ws.Value<double>("kills");
                    double shots = ws.Value<double>("shots");
                    double hits = ws.Value<double>("hits");
                    double time = ws.Value<double>("time");
                    double headshots = ws.Value<double>("headshots");

                    TOTkills += kills;
                    TOTtime += time;
                    TOTshots += shots;
                    TOThits += hits;
                    TOThs += headshots;
                }
                cat["stat"]["time"] = TOTtime;
                cat["stat"]["kills"] = TOTkills;
                cat["stat"]["shots"] = TOTshots;
                cat["stat"]["hits"] = TOThits;
                cat["stat"]["hs"] = TOThs;

                var extra = new JObject();
                extra["accuracy"] = TOThits / TOTshots;
                extra["kills per min"] = (TOTkills) / (TOTtime / 60);
                extra["shots fired per min"] = (TOTshots) / (TOTtime / 60);
                extra["headshot per kill"] = TOThs / TOTkills;
                extra["score per min"] = catscore.HasValue ? (catscore.Value / (TOTtime / 60)) : 0;

                cat["extra"] = extra;
            }

            //kititems
            foreach (JObject kititem in (JArray)BF4_Stats["kititems"])
            {
                string name = kititem["name"].ToObject<string>();
                kititem.Property("curr")?.Remove();
                kititem["equiWeapon"] = ((JArray)BF4_Stats["weapons"]).FirstOrDefault(x => x["name"].ToObject<string>() == name);


                if (kititem["detail"]["unlock"] != null && kititem["detail"]["unlock"].HasValues)
                {
                    var unl = (JObject)kititem["detail"]["unlock"];
                    unl["isUnlocked"] = unl["curr"].ToObject<int>() > unl["needed"].ToObject<int>();
                }

            }
        }       
        private void AssignStats(Dictionary<string , float> List , JObject parent)
        {
            foreach (var item in parent)
            {
                if (item.Value.Type == JTokenType.String)
                {
                    //attemp replace
                    var oldstr = item.Value.ToObject<string>();
                    if (oldstr.StartsWith("stat."))
                    {
                        float val = 0;
                        List.TryGetValue(oldstr.Substring(5) , out val);
                        parent[item.Key] = val;
                    }
                }
                else if (item.Value.Type == JTokenType.Object)
                {
                    AssignStats(List , (JObject)item.Value);
                }
                else if (item.Value.Type == JTokenType.Array)
                {
                    var JAr = (JArray)item.Value;
                    foreach (var JAri in JAr)
                    {
                        if (JAri.Type == JTokenType.Object)
                        {
                            AssignStats(List , (JObject)JAri);
                        }
                    }
                }
            }
        }
        private double sumfrom0to(int index)
        {
            float finalsum = 0;
            for (int i = 0; i < index; ++i)
            {
                finalsum += GetRankMaxScore(i);
            }
            return finalsum;
        }        
    }
}
