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
        private void API_ZloClient_ItemsReceived(ZloGame Game , List<API_Item> List)
        {
            switch (Game)
            {               
                case ZloGame.BF_4:
                    {
                        var finaldef = BF4_stats_def.ToString();
                        foreach (var item in List)
                        {
                            finaldef = finaldef.Replace($@"""item.{item.FlagName}""" , item.ItemExists.ToString());
                        }
                        m_BF4_Stats = JObject.Parse(finaldef);

                        File.WriteAllText($".\\{CurrentPlayerName}_BF4_stats.json" , BF4_Stats.ToString());                        
                        File.WriteAllLines($".\\{CurrentPlayerName}_BF4_items_raw.txt" , List.Select(x => $"{x.FlagName} = {x.ItemExists.ToString()}"));
                        break;
                    }
                case ZloGame.BF_HardLine:
                    break;
                case ZloGame.None:
                    break;
                default:
                    break;
            }
        }

        private void API_ZloClient_StatsReceived(ZloGame Game , List<API_Stat> List)
        {
            switch (Game)
            {
                case ZloGame.BF_3:
                    {
                        var finaldef = BF3_stats_def.ToString();
                        foreach (var item in List)
                        {
                            finaldef = finaldef.Replace($@"""stat.{item.FlagName}""" , item.FlagValue.ToString());
                        }
                        m_BF3_Stats = JObject.Parse(finaldef);
                        BF3_Stats_Handler(List);
                        File.WriteAllText($".\\{CurrentPlayerName}_BF3_stats.json" , BF3_Stats.ToString());
                        break;
                    }
                case ZloGame.BF_4:
                    {
                        var finaldef = BF4_stats_def.ToString();
                        foreach (var item in List)
                        {
                            finaldef = finaldef.Replace($@"""stat.{item.FlagName}""" , item.FlagValue.ToString());                           
                        }
                        m_BF4_Stats = JObject.Parse(finaldef);
                        BF4_Stats_Handler(List);
                        var str = BF4_Stats.ToString();
                        File.WriteAllText($".\\{CurrentPlayerName}_BF4_stats.json" , str);
                        File.WriteAllLines($".\\{CurrentPlayerName}_BF4_stats_raw.txt" , List.Select(x => $"{x.FlagName} = {x.FlagValue}"));
                        break;
                    }
                case ZloGame.BF_HardLine:
                    break;
                case ZloGame.None:
                    break;
                default:
                    break;
            }

        }

        private void BF3_Stats_Handler(List<API_Stat> List)
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
        private void BF4_Stats_Handler(List<API_Stat> List)
        {
            //edit custom stats
            BF4_Stats["plat"] = "pc";
            BF4_Stats["id"] = CurrentPlayerID;
            BF4_Stats["name"] = CurrentPlayerName;
            BF4_Stats["last_update_date"] = DateTime.Now;
            //BF3_Stats["tag"] = ;

          
            

            int rank = BF4_Stats["stats"].Value<int>("rank");

            var rankdets = GetBF4RankDetails(rank);
            BF4_Stats["stats"]["rankname"] = rankdets["Rank Title"];
            BF4_Stats["stats"]["scores"]["maxxp"] = GetBF4RankDetails(rank + 1)["XP Min Relative"];
            BF4_Stats["stats"]["scores"]["shortxp"] = BF4_Stats["stats"]["scores"]["longxp"].ToObject<double>() - rankdets["XP Min Total"].ToObject<double>();          
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
