using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using Zlo.Extras;

namespace Zlo
{
    public partial class API_ZloClient
    {
        public void JoinOnlineServer(OnlinePlayModes playmode, uint serverid = 0)
        {

            ProcessStartInfo rungame = null;
            switch (playmode)
            {
                case OnlinePlayModes.BF3_Multi_Player:
                    rungame = GetGameJoinID(ZloGame.BF_3, CurrentPlayerID, serverid, 1);
                    break;


                case OnlinePlayModes.BF4_Multi_Player:
                    rungame = GetGameJoinID(ZloGame.BF_4, CurrentPlayerID, serverid, 1);
                    break;
                case OnlinePlayModes.BF4_Spectator:
                    rungame = GetGameJoinID(ZloGame.BF_4, CurrentPlayerID, serverid, 3);
                    break;
                case OnlinePlayModes.BF4_Commander:
                    rungame = GetGameJoinID(ZloGame.BF_4, CurrentPlayerID, serverid, 2);
                    break;

                case OnlinePlayModes.BFH_Multi_Player:
                    rungame = GetGameJoinID(ZloGame.BF_HardLine, CurrentPlayerID, serverid, 1);
                    break;
                default:
                    return;
            }
            if (rungame == null)
            {
                return;
            }
            else
            {
                try
                {
                    if (rungame.FileName.StartsWith("origin2"))
                    {
                        Process.Start(rungame.FileName);
                    }
                    else
                    {
                        rungame.UseShellExecute = false;
                        rungame.WorkingDirectory = Path.GetDirectoryName(rungame.FileName);
                        Process.Start(rungame);
                    }
                }
                catch (Exception ex)
                {
                    RaiseError(ex, $"Error occured when Starting the game with:\nfile name : {rungame.FileName}\nand arguments : {rungame.Arguments}\nuse shell execute ? {rungame.UseShellExecute}");
                }
            }
        }
        public void JoinOfflineGame(OfflinePlayModes playmode)
        {
            ProcessStartInfo rungame = null;
            switch (playmode)
            {
                case OfflinePlayModes.BF3_Single_Player:
                    rungame = GetGameJoinID(ZloGame.BF_3, CurrentPlayerID, 0, 0);
                    break;

                case OfflinePlayModes.BF4_Single_Player:
                    rungame = GetGameJoinID(ZloGame.BF_4, CurrentPlayerID, 0, 0);
                    break;
                case OfflinePlayModes.BF4_Test_Range:
                    rungame = GetGameJoinID(ZloGame.BF_4, CurrentPlayerID, 0, 4);
                    break;


                case OfflinePlayModes.BFH_Single_Player:
                    rungame = GetGameJoinID(ZloGame.BF_HardLine, CurrentPlayerID, 0, 0);
                    break;

                default:
                    return;
            }

            try
            {
                if (rungame == null)
                {
                    return;
                }
                else
                {
                    if (rungame.FileName.StartsWith("origin2"))
                    {
                        Process.Start(rungame.FileName);
                    }
                    else
                    {
                        rungame.UseShellExecute = false;
                        rungame.WorkingDirectory = Path.GetDirectoryName(rungame.FileName);
                        Process.Start(rungame);
                    }
                }
            }
            catch (Exception ex)
            {
                RaiseError(ex, $"Error occured when Starting the game with:\nfile name : {rungame.FileName}\nand arguments : {rungame.Arguments}\nuse shell execute ? {rungame.UseShellExecute}");
            }
        }
        public void JoinOnlineGameWithPassWord(OnlinePlayModes playmode, uint serverid, string password)
        {
            ProcessStartInfo rungame = null;
            switch (playmode)
            {
                case OnlinePlayModes.BF3_Multi_Player:
                    rungame = GetGameJoinID(ZloGame.BF_3, CurrentPlayerID, serverid, 1, password);
                    break;

                case OnlinePlayModes.BF4_Multi_Player:
                    rungame = GetGameJoinID(ZloGame.BF_4, CurrentPlayerID, serverid, 1, password);
                    break;
                case OnlinePlayModes.BF4_Spectator:
                    rungame = GetGameJoinID(ZloGame.BF_4, CurrentPlayerID, serverid, 3, password);
                    break;
                case OnlinePlayModes.BF4_Commander:
                    rungame = GetGameJoinID(ZloGame.BF_4, CurrentPlayerID, serverid, 2, password);
                    break;


                case OnlinePlayModes.BFH_Multi_Player:
                    rungame = GetGameJoinID(ZloGame.BF_HardLine, CurrentPlayerID, serverid, 1, password);
                    break;
                case OnlinePlayModes.BFH_Spectator:
                    rungame = GetGameJoinID(ZloGame.BF_HardLine, CurrentPlayerID, serverid, 3, password);
                    break;
                case OnlinePlayModes.BFH_Commander:
                    rungame = GetGameJoinID(ZloGame.BF_HardLine, CurrentPlayerID, serverid, 2, password);
                    break;
                default:
                    return;
            }
            if (rungame == null)
            {
                return;
            }
            else
            {
                try
                {
                    if (rungame.FileName.StartsWith("origin2"))
                    {
                        Process.Start(rungame.FileName);
                    }
                    else
                    {
                        rungame.UseShellExecute = false;
                        rungame.WorkingDirectory = Path.GetDirectoryName(rungame.FileName);
                        Process.Start(rungame);
                    }
                }
                catch (Exception ex)
                {
                    RaiseError(ex, $"Error occured when Starting the game with:\nfile name : {rungame.FileName}\nand arguments : {rungame.Arguments}\nuse shell execute ? {rungame.UseShellExecute}");
                }
            }
        }
        private ProcessStartInfo PrepareBF3(string ps)
        {
            var title = "Battlefield3";
            string bf3offers = "70619,71067,DGR01609244,DGR01609245";
            ProcessStartInfo final = null;
            int state = 2;
            if (File.Exists("bf3.exe"))
            {
                final = new ProcessStartInfo(Path.GetFullPath("bf3.exe"));
                state = 1;
            }
            else
            {
                try
                {
                    //check registry 
                    using (var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\EA Games\Battlefield 3"))
                    {
                        if (reg != null)
                        {
                            var val = reg.GetValue("Install Dir", null) as string;
                            if (string.IsNullOrWhiteSpace(val))
                            {
                                state = 2;
                                //doesn't exist in registry                                            
                            }
                            else
                            {
                                string bf3Path = Path.Combine(val, "bf3.exe");
                                if (File.Exists(bf3Path))
                                {
                                    state = 1;
                                    final = new ProcessStartInfo(bf3Path);
                                }
                                else
                                {
                                    state = 2;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    state = 2;
                }
            }

            if (state == 2)
            {
                final = new ProcessStartInfo($@"origin2://game/launch/?offerIds={bf3offers}&title={title}&cmdParams={ps}");
            }
            else if (state == 1)
            {
                final.Arguments = Uri.UnescapeDataString(ps);
            }
            return final;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="PlayerID"></param>
        /// <param name="ServerID"></param>
        /// <param name="playmode">0 = single player
        /// 1 = multi player
        /// 2 = commander
        /// 3 = spectator
        /// 4 = test range
        /// 5 = co-op</param>
        /// <param name="pw">The server password</param>
        /// <returns></returns>
        private ProcessStartInfo GetGameJoinID(ZloGame game, uint PlayerID, uint ServerID, int playmode, string pw = "")
        {
            /*
             play mode : 
             0 = single player
             1 = multi player
             2 = commander
             3 = spectator
             4 = test range
             5 = co-op
             */
            string q = "\\" + "\"";
            ProcessStartInfo final = null;
            var title = string.Empty;
            var pwExpression = playmode == 1 && !string.IsNullOrWhiteSpace(pw) ? $@"password={q}{pw}{q}" : string.Empty;
            switch (game)
            {
                //%20password%3D%5C%22{pw}%5C%22%20
                case ZloGame.BF_3:
                    {
                        title = "Battlefield3";
                        string bf3offers = "70619,71067,DGR01609244,DGR01609245";
                        string requestState = playmode == 1 ? "State_ClaimReservation" : "State_ResumeCampaign";
                        string levelmode = playmode == 1 ? "mp" : "sp";
                        string gameIDstr = playmode == 1 ? $@"putinsquad={q}true{q} gameid={q}{ServerID}{q}" : string.Empty;
                        string ps = Uri.EscapeDataString($@"-webMode {levelmode.ToUpper()} -Origin_NoAppFocus -loginToken WAHAHA_IMMA_ZLO_TOKEN -requestState {requestState} -requestStateParams ""<data {pwExpression} {gameIDstr} role={q}soldier{q} personaref={q}{PlayerID}{q} levelmode={q}{levelmode}{q} logintoken={q}WAHAHA_IMMA_ZLO_TOKEN{q}></data>""");
                        //state 1 = from path
                        //state 2 = from origin2
                        return final = PrepareBF3(ps);
                    }
                case ZloGame.BF_4:
                    {
                        title = "Battlefield4";
                        string bf4offers = "1007968,1011575,1011576,1011577,1010268,1010269,1010270,1010271,1010958,1010959,1010960,1010961,1007077,1016751,1016757,1016754,1015365,1015364,1015363,1015362";
                        switch (playmode)
                        {
                            case 0:
                                //single
                                return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20SP%20-Origin_NoAppFocus%20-requestState%20State_ResumeCampaign%20-requestStateParams%20%22%3Cdata%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22sp%5C%22%3E%3C/data%3E%22");
                            case 1:
                                //multi
                                if (pw != "")
                                {
                                    return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20password%3D%5C%22{pw}%5C%22%20putinsquad%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22soldier%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                                }
                                else
                                {
                                    return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20putinsquad%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22soldier%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                                }
                            case 2:
                                //commander
                                if (pw != "")
                                {
                                    return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20password%3D%5C%22{pw}%5C%22%20putinsquad%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22commander%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                                }
                                else
                                {
                                    return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20putinsquad%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22commander%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                                }
                            case 3:
                                //spectator
                                if (pw != "")
                                {
                                    return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20password%3D%5C%22{pw}%5C%22%20putinsquad%3D%5C%22true%5C%22%20isspectator%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22soldier%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                                }
                                else
                                {

                                    return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20putinsquad%3D%5C%22true%5C%22%20isspectator%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22soldier%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                                }
                            case 4:
                                //test range
                                return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20SP%20-Origin_NoAppFocus%20-requestState%20State_LaunchPlayground%20-requestStateParams%20%22%3Cdata%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                            case 5:
                                //co-op
                                //currently returns single player
                                return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20SP%20-Origin_NoAppFocus%20-requestState%20State_ResumeCampaign%20-requestStateParams%20%22%3Cdata%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22sp%5C%22%3E%3C/data%3E%22");
                            default:
                                //default is single player
                                return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bf4offers}&title={title}&cmdParams=-webMode%20SP%20-Origin_NoAppFocus%20-requestState%20State_ResumeCampaign%20-requestStateParams%20%22%3Cdata%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22sp%5C%22%3E%3C/data%3E%22");
                        }
                    }
                case ZloGame.BF_HardLine:
                    {
                        //title = BattlefieldHardline
                        //1013920
                        title = "BattlefieldHardline";
                        string bfhoffers = "1013920";
                        switch (playmode)
                        {
                            case 0:
                                //single
                                return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bfhoffers}&title={title}&cmdParams=-webMode%20SP%20-Origin_NoAppFocus%20-requestState%20State_ResumeCampaign%20-requestStateParams%20%22%3Cdata%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22sp%5C%22%3E%3C/data%3E%22");
                            case 1:
                                //multi
                                if (pw != "")
                                {
                                    return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bfhoffers }&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20password%3D%5C%22{pw}%5C%22%20putinsquad%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22soldier%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                                }
                                else
                                {
                                    return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bfhoffers }&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20putinsquad%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22soldier%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                                }
                            case 2:
                                //commander
                                if (pw != "")
                                {
                                    return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bfhoffers }&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20password%3D%5C%22{pw}%5C%22%20putinsquad%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22commander%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                                }
                                else
                                {
                                    return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bfhoffers }&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20putinsquad%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22commander%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                                }
                            case 3:
                                //spectator
                                if (pw != "")
                                {
                                    return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bfhoffers }&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20password%3D%5C%22{pw}%5C%22%20putinsquad%3D%5C%22true%5C%22%20isspectator%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22soldier%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                                }
                                else
                                {

                                    return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bfhoffers }&title={title}&cmdParams=-webMode%20MP%20-Origin_NoAppFocus%20-requestState%20State_ClaimReservation%20-requestStateParams%20%22%3Cdata%20putinsquad%3D%5C%22true%5C%22%20isspectator%3D%5C%22true%5C%22%20gameid%3D%5C%22{ServerID}%5C%22%20role%3D%5C%22soldier%5C%22%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                                }
                            case 4:
                                //test range
                                return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bfhoffers }&title={title}&cmdParams=-webMode%20SP%20-Origin_NoAppFocus%20-requestState%20State_LaunchPlayground%20-requestStateParams%20%22%3Cdata%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22mp%5C%22%3E%3C/data%3E%22");
                            case 5:
                                //co-op
                                //currently returns single player
                                return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bfhoffers }&title={title}&cmdParams=-webMode%20SP%20-Origin_NoAppFocus%20-requestState%20State_ResumeCampaign%20-requestStateParams%20%22%3Cdata%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22sp%5C%22%3E%3C/data%3E%22");
                            default:
                                //default is single player
                                return new ProcessStartInfo($@"origin2://game/launch/?offerIds={bfhoffers }&title={title}&cmdParams=-webMode%20SP%20-Origin_NoAppFocus%20-requestState%20State_ResumeCampaign%20-requestStateParams%20%22%3Cdata%20personaref%3D%5C%22{PlayerID}%5C%22%20levelmode%3D%5C%22sp%5C%22%3E%3C/data%3E%22");
                        }
                    }
                case ZloGame.None:
                    return null;
                default:
                    return null;
            }
        }


     

        /*
             coop client params
-webMode COOP -Origin_NoAppFocus -AuthCode WAHAHA_IMMA_ZLO_TOKEN -requestState State_ConnectToUserId -requestStateParams "<data friendpersonaid=\"3\" personaref=\"2\" levelmode=\"coop\" logintoken=\"WAHAHA_IMMA_ZLO_TOKEN\"></data>"
personaref - own user, friendpersona - to which user connect, dunno how make it for now, because no server browser for coop
coop server
-webMode COOP -Origin_NoAppFocus -AuthCode WAHAHA_IMMA_ZLO_TOKEN -requestState State_CreateCoOpPeer -requestStateParams "<data level=\"Levels/COOP_007/COOP_007\" difficulty=\"NORMAL\" personaref=\"2\" levelmode=\"coop\" logintoken=\"WAHAHA_IMMA_ZLO_TOKEN\"></data>"
persona - own, level - any coop level
*/

        public ProcessStartInfo HostBf3Coop(BF3_COOP_LEVELS level, COOP_Difficulty difficulty)
        {            
            string q = "\\" + "\"";
            var personaId = CurrentPlayerID;
            string ps = Uri.EscapeDataString($@"-webMode COOP -Origin_NoAppFocus -AuthCode WAHAHA_IMMA_ZLO_TOKEN -requestState State_CreateCoOpPeer -requestStateParams ""<data level={q}Levels/{level.ToString().ToUpper()}/{level.ToString().ToUpper()}{q} difficulty={q}{difficulty.ToString().ToUpper()}{q} personaref={q}{personaId}{q} levelmode={q}coop{q} logintoken={q}WAHAHA_IMMA_ZLO_TOKEN{q}></data>""");
            var pinfo = PrepareBF3(ps);
            return pinfo;
        }
        public ProcessStartInfo JoinBf3Coop(uint FriendId)
        {
            string q = "\\" + "\"";
            var personaId = CurrentPlayerID;
            string ps = Uri.EscapeDataString($@"-webMode COOP -Origin_NoAppFocus -AuthCode WAHAHA_IMMA_ZLO_TOKEN -requestState State_ConnectToUserId -requestStateParams ""<data friendpersonaid={q}{FriendId}{q} personaref={q}{personaId}{q} levelmode={q}coop{q} logintoken={q}WAHAHA_IMMA_ZLO_TOKEN{q}></data>""");
            var pinfo = PrepareBF3(ps);
            return pinfo;
        }
    }
}
