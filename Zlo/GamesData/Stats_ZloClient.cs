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
        private void API_ZloClient_StatsReceived(ZloGame Game , List<API_Stat> List)
        {
            switch (Game)
            {
                case ZloGame.BF_3:
                    var finaldef = BF3_Stats.ToString();
                    foreach (var item in List)
                    {
                        if (!finaldef.Contains(item.FlagName)) WriteLog($"stats don't exist in BF3_stats.json detected, name : {item.FlagName},value : {item.FlagValue.ToString()}");
                        finaldef = finaldef.Replace($@"""stat.{item.FlagName}""" , item.FlagValue.ToString());
                    }
                    m_BF3_Stats = JObject.Parse(finaldef);
                    BF3_Stats_Handler(List);
                    File.WriteAllText($".\\{CurrentPlayerName}_BF3_stats.json", BF3_Stats.ToString());
                    break;
                case ZloGame.BF_4:
                    File.WriteAllLines($".\\{CurrentPlayerName}_BF4_stats.txt" , List.Select(x => $"{x.FlagName} = {x.FlagValue};"));
                    break;
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
            s["rankname"] = GetRankName(rank);

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
