using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zlo.Extras
{
    public static class API_Dictionaries
    {
        public static string GetMapName(ZloGame game,string key)
        {
            switch (game)
            {
                case ZloGame.BF_3:
                    if (API_BF3_Maps.ContainsKey(key))
                        return API_BF3_Maps[key];
                    break;
                case ZloGame.BF_4:
                    if (API_BF4_Maps.ContainsKey(key))
                        return API_BF4_Maps[key];
                    break;
                case ZloGame.BF_HardLine:
                    if (API_BFH_Maps.ContainsKey(key))
                        return API_BFH_Maps[key];
                    break;
                case ZloGame.None:
                    break;
                default:
                    break;
            }
            return key;
        }
        public static string GetGameModeName(ZloGame game, string key)
        {
            switch (game)
            {
                case ZloGame.BF_3:
                    if (API_BF3_GameModes.ContainsKey(key))
                        return API_BF3_GameModes[key];
                    break;
                case ZloGame.BF_4:
                    if (API_BF4_GameModes.ContainsKey(key))
                        return API_BF4_GameModes[key];
                    break;
                case ZloGame.BF_HardLine:
                    if (API_BFH_GameModes.ContainsKey(key))
                        return API_BFH_GameModes[key];
                    break;
                case ZloGame.None:
                    break;
                default:
                    break;
            }
            return key;
        }
        public static void GetItemDetails(ZloGame game,string key,out string name,out string desc)
        {
            switch (game)
            {
                case ZloGame.BF_4:
                    var translated = (JObject)GameData.BF4_items[key];
                    if (translated != null)
                    {
                        name = translated["rname"]?.ToObject<string>();
                        desc = translated["rdesc"]?.ToObject<string>();
                    }
                    else
                    {
                        name = key;
                        desc = string.Empty;
                    }
                    break;
                case ZloGame.BF_3:
                case ZloGame.BF_HardLine:
                case ZloGame.None:
                default:
                    name = key;
                    desc = string.Empty;
                    break;
            }
            
            
        }
        public static Dictionary<string, string> API_BF4_Maps { get; internal set; } = new Dictionary<string, string>
        {
            #region BF4_Maps
            { "MP_Abandoned","Zavod 311" },
            { "MP_Damage","Lancang Dam" },
            { "MP_Flooded","Flood Zone" },
            { "MP_Journey","Golmud Railway" },
            { "MP_Naval","Paracel Storm" },
            { "MP_Prison","Operation Locker" },
            { "MP_Resort","Hainan Resort" },
            { "MP_Siege","Siege of Shanghai" },
            { "MP_TheDish","Rogue Transmission" },
            { "MP_Tremors","Dawnbreaker" },
            { "XP1_001","Silk Road" },
            { "XP1_002","Altai Range" },
            { "XP1_003","Guilin Peaks" },
            { "XP1_004","Dragon Pass" },
            { "XP0_Caspian","Caspian Border 2014" },
            { "XP0_Firestorm","Operation Firestorm 2014" },
            { "XP0_Metro","Operation Metro 2014" },
            { "XP0_Oman","Gulf of Oman 2014" },
            { "XP2_001","Lost Islands" },
            { "XP2_002","Nansha Strike" },
            { "XP2_003","Wavebreaker" },
            { "XP2_004","Operation Mortar" },
            { "XP3_MarketPl","Pearl Market" },
            { "XP3_Prpganda","Propaganda" },
            { "XP3_UrbanGdn","Lumphini Garden" },
            { "XP3_WtrFront","Sunken Dragon" },
            { "XP4_Arctic","Operation Whiteout" },
            { "XP4_SubBase","Hammerhead" },
            { "XP4_Titan","Hangar 21" },
            { "XP4_WlkrFtry","Giants Of Karelia" },
            { "XP5_Night_01","Zavod:Graveyard Shift" },
            { "XP6_CMP","Operation Outbreak" },
            { "XP7_Valley","Dragon Valley 2015" }
	        #endregion            
        };


        public static Dictionary<string, string> API_BF4_GameModes { get; internal set; } = new Dictionary<string, string>
        {
            #region  BF4_GameModes
            { "ConquestLarge0","Conquest Large" },
            { "ConquestSmall0","Conquest Small" },
            { "Domination0","Domination" },
            { "Elimination0","Defuse" },
            { "Obliteration","Obliteration" },
            { "RushLarge0","Rush" },
            { "SquadDeathMatch0","Squad Deathmatch" },
            { "TeamDeathMatch0","Team Deathmatch" },
            { "AirSuperiority0","Air Superiority" },
            { "CaptureTheFlag0","CTF" },
            { "CarrierAssaultLarge0","Carrier Assault Large" },
            { "CarrierAssaultSmall0","Carrier Assault Small" },
            { "Chainlink0","Chain Link" },
            { "SquadObliteration0","Squad Obliteration" },
            { "GunMaster0","Gun Master" },
            { "GunMaster1","Gun Master" },
            { "SquadDeathMatch1","Squad Deathmatch" },
            { "TeamDeathMatch1","Team Deathmatch" },

            { "CQL0","Conquest Large" },
            { "CQS0","Conquest Small" },
            { "DOM0","Domination" },
            { "ELI0","Defuse" },
            { "OBL","Obliteration" },
            { "RL0","Rush" },
            { "SDM0","Squad Deathmatch" },
            { "TDM0","Team Deathmatch" },
            { "AS0","Air Superiority" },
            { "CTF0","CTF" },
            { "CAL0","Carrier Assault Large" },
            { "CAS0","Carrier Assault Small" },
            { "CHA0","Chain Link" },
            { "SQO0","Squad Obliteration" },
            { "GM0","Gun Master" },
            { "GM1","Gun Master" },
            { "SDM1","Squad Deathmatch" },
            { "TDM1","Team Deathmatch" }
            #endregion
    };



        public static Dictionary<string, string> API_BF3_Maps { get; internal set; } = new Dictionary<string, string>
        {
            #region BF3_Maps	
            { "MP_001","Grand Bazaar" },
            { "MP_003","Teheran Highway" },
            { "MP_007","Caspian Border" },
            { "MP_011","Seine Crossing" },
            { "MP_012","Operation Firestorm" },
            { "MP_013","Damavand Peak" },
            { "MP_017","Noshahr Canals" },
            { "MP_018","Kharg Island" },
            { "MP_Subway","Operation Metro" },
            { "XP1_002","Gulf of Oman" },
            { "XP3_Desert","Bandar Desert" },
            { "XP3_Alborz","Alborz Mountains" },
            { "XP3_Shield","Armored Shield" },
            { "XP3_Valley","Death Valley" },
            { "XP4_FD","Markaz Monolith" },
            { "XP4_Parl","Azadi Palace" },
            { "XP4_Quake","Epicenter" },
            { "XP5_001","Operation Riverside" },
            { "XP5_002","Nebandan Flats" },
            { "XP5_003","Kiasar Railroad" },
            { "XP5_004","Sabalan Pipeline" },
            { "XP1_001","Strike at Karkand" },
            { "XP1_003","Sharqi Peninsula" },
            { "XP1_004","Wake Island" },
            { "XP4_Rubble","Talah Market" },
            { "XP2_Factory","Scrapmetal" },
            { "XP2_Office","Operation 925" },
            { "XP2_Palace","Donya Fortress" },
            { "XP2_Skybar","Ziba Tower" }
            #endregion
        };

        public static Dictionary<string, string> API_BF3_GameModes { get; internal set; } = new Dictionary<string, string>
        {
            #region BF3_GameModes	
            { "ConquestLarge0","Conquest Large" },
            { "ConquestAssaultLarge0","Assault64" },
            { "ConquestSmall0","Conquest Small" },
            { "ConquestAssaultSmall0","Assault" },
            { "ConquestAssaultSmall1","Assault" },
            { "RushLarge0","Rush" },
            { "SquadRush0","Squad Rush" },
            { "SquadDeathMatch0","Squad Deathmatch" },
            { "TeamDeathMatch0","TDM" },
            { "TeamDeathMatchC0","TDM Close Quarters" },
            { "GunMaster0","Gun Master" },
            { "Domination0","Conquest Domination" },
            { "TankSuperiority0","Tank Superiority" },
            { "Scavenger0","Scavenger" },
            { "CaptureTheFlag0","CTF" },
            { "AirSuperiority0","Air Superiority" },


            { "CQL0","Conquest Large" },
            { "CQAL0","Assault64" },
            { "CQS0","Conquest Small" },
            { "CQAS0","Assault" },
            { "CQAS1","Assault" },
            { "RL0","Rush" },
            { "SR0","Squad Rush" },
            { "SDM0","Squad Deathmatch" },
            { "TDM0","TDM" },
            { "TDMC0","TDM Close Quarters" },
            { "GM0","Gun Master" },
            { "DOM0","Conquest Domination" },
            { "TS0","Tank Superiority" },
            { "SCV0","Scavenger" },
            { "CTF0","CTF" },
            { "AS0","Air Superiority" }
            #endregion
        };



        public static Dictionary<string, string> API_BFH_Maps { get; internal set; } = new Dictionary<string, string>()
        {
            #region BFH_Maps
            { "mp_bank" , "Bank Job" },
{ "mp_bloodout" , "The Block" },
{ "mp_desert05" , "Dust Bowl" },
{ "mp_downtown" , "Downtown" },
{ "mp_eastside" , "Derailed" },
{ "mp_glades" , "Everglades" },
{ "mp_growhouse" , "Growhouse" },
{ "mp_hills" , "Hollywood Heights" },
{ "mp_offshore" , "Riptide" },
{ "xp1_mallcops" , "Black Friday" },
{ "xp1_nights" , "Code Blue" },
{ "xp1_projects" , "The Beat" },
{ "xp1_sawmill" , "Backwoods" },
{ "xp25_bank" , "Night Job" },
{ "xp25_sawmill" , "Night Woods" },
{ "xp2_cargoship" , "The Docks" },
{ "xp2_coastal" , "Break Pointe" },
{ "xp2_museum02" , "Museum" },
{ "xp2_precinct7" , "Precinct 7" },
{ "xp3_border" , "Double Cross" },
{ "xp3_cistern02" , "Diversion" },
{ "xp3_highway" , "Pacific Highway" },
{ "xp3_traindodge" , "Train Dodge" },
{ "xp4_alcatraz" , "Alcatraz" },
{ "xp4_cemetery" , "Cemetery" },
{ "xp4_chinatown" , "Chinatown" },
{ "xp4_snowcrash" , "Thin Ice" }

	        #endregion            
        };

        public static Dictionary<string, string> API_BFH_GameModes { get; internal set; } = new Dictionary<string, string>()
        {

        };
    }
}
