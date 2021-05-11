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
        public void JoinOnlineServer(OnlinePlayModes playmode, uint serverid, string password = null)
        {
            //Z.BF3 Z.BF4x32 Z.BF4x64 z.BFHL
            const string State_ClaimReservation = "State_ClaimReservation";
            const string MP = "mp";
            string runName;
            string cmd;
            switch (playmode)
            {
                case OnlinePlayModes.BF3_Multi_Player:
                    runName = "Z.BF3";
                    cmd = GetBfCmd(mode: MP,
                                   requestState: State_ClaimReservation,
                                   ServerID: serverid,
                                   putInSquad: true,
                                   role: "soldier",
                                   pw: password);
                    break;
                case OnlinePlayModes.BF4_Multi_Player:
                    runName = RunnableGameList.IsOSx64 ? "Z.BF4x64" : "Z.BF4x32";
                    cmd = GetBfCmd(mode: MP,
                                   requestState: State_ClaimReservation,
                                   ServerID: serverid,
                                   putInSquad: true,
                                   role: "soldier",
                                   pw: password);
                    break;
                case OnlinePlayModes.BF4_Spectator:
                    runName = RunnableGameList.IsOSx64 ? "Z.BF4x64" : "Z.BF4x32";
                    cmd = GetBfCmd(mode: MP,
                                   requestState: State_ClaimReservation,
                                   ServerID: serverid,
                                   putInSquad: true,
                                   isSpectator: true,
                                   role: "soldier",
                                   pw: password);
                    break;
                case OnlinePlayModes.BF4_Commander:
                    runName = RunnableGameList.IsOSx64 ? "Z.BF4x64" : "Z.BF4x32";
                    cmd = GetBfCmd(mode: MP,
                                   requestState: State_ClaimReservation,
                                   ServerID: serverid,
                                   putInSquad: true,
                                   role: "commander",
                                   pw: password);
                    break;
                case OnlinePlayModes.BFH_Multi_Player:
                    runName = "Z.BFHL";
                    cmd = GetBfCmd(mode: MP,
                                   requestState: State_ClaimReservation,
                                   ServerID: serverid,
                                   putInSquad: true,
                                   role: "soldier",
                                   pw: password);
                    break;
                default:
                    return;
            }
            SendRunGameRequest(runName, cmd);
        }
        public void JoinOfflineGame(OfflinePlayModes playmode)
        {
            const string State_ResumeCampaign = "State_ResumeCampaign";
            const string State_LaunchPlayground = "State_LaunchPlayground";
            const string SP = "SP";
            const string MP = "MP";
            string runName;
            string cmd;
            switch (playmode)
            {
                case OfflinePlayModes.BF3_Single_Player:
                    runName = "Z.BF3";
                    cmd = GetBfCmd(mode: SP,
                                   requestState: State_ResumeCampaign,
                                   role: "soldier");
                    break;
                case OfflinePlayModes.BF4_Single_Player:
                    runName = RunnableGameList.IsOSx64 ? "Z.BF4x64" : "Z.BF4x32";
                    cmd = GetBfCmd(mode: SP,
                                   requestState: State_ResumeCampaign,
                                   role: "soldier");
                    break;
                case OfflinePlayModes.BF4_Test_Range:
                    runName = RunnableGameList.IsOSx64 ? "Z.BF4x64" : "Z.BF4x32";
                    cmd = GetBfCmd(mode: MP, //check if valid
                                   requestState: State_LaunchPlayground,
                                   role: "soldier");
                    break;
                case OfflinePlayModes.BFH_Single_Player:
                    runName = "Z.BFHL";
                    cmd = GetBfCmd(mode: SP,
                                   requestState: State_ResumeCampaign,
                                   role: "soldier");
                    break;
                default:
                    return;
            }
            SendRunGameRequest(runName, cmd);
        }
        private string GetBfCmd(string mode,
                                string requestState,
                                uint? ServerID = null,
                                bool? putInSquad = null,
                                string role = null,
                                string pw = null,
                                bool? isSpectator = null,
                                uint? friendpersonaid = null,
                                string level = null,
                                string difficulty = null,
                                Dictionary<string, string> extraRequestStateParams = null)
        {
            const string loginToken = "WAHAHA_IMMA_ZLO_TOKEN";
            string formatIfNotNull(string key, string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return string.Empty;
                }
                else
                {
                    return $@"{key}=\""{value}\""";
                }
            }
            var requestStateParamsSubSb = new StringBuilder();
            void appendSpace(string toAdd)
            {
                requestStateParamsSubSb.Append($"{toAdd} ");
            }
            appendSpace(formatIfNotNull("password", pw));
            appendSpace(formatIfNotNull("putinsquad", putInSquad?.ToString()?.ToLower()));
            appendSpace(formatIfNotNull("gameid", ServerID?.ToString()));
            appendSpace(formatIfNotNull("role", role));
            appendSpace(formatIfNotNull("personaref", "%ZID%"));
            appendSpace(formatIfNotNull("levelmode", mode.ToLower()));
            appendSpace(formatIfNotNull("logintoken", loginToken));
            appendSpace(formatIfNotNull("isspectator", isSpectator?.ToString()?.ToLower()));
            appendSpace(formatIfNotNull("friendpersonaid", friendpersonaid?.ToString()));
            appendSpace(formatIfNotNull("level", level));
            appendSpace(formatIfNotNull("difficulty", difficulty?.ToString()));
            if (extraRequestStateParams != null)
            {
                foreach (var item in extraRequestStateParams)
                {
                    appendSpace(formatIfNotNull(item.Key, item.Value));
                }
            }
            string requestStateParams = $@"""<data {requestStateParamsSubSb}></data>""";
            return $@"-webMode {mode.ToUpper()} -Origin_NoAppFocus -loginToken {loginToken} -requestState {requestState} -requestStateParams {requestStateParams}";
        }

        /*
             coop client params
-webMode COOP -Origin_NoAppFocus -AuthCode WAHAHA_IMMA_ZLO_TOKEN -requestState State_ConnectToUserId -requestStateParams "<data friendpersonaid=\"3\" personaref=\"2\" levelmode=\"coop\" logintoken=\"WAHAHA_IMMA_ZLO_TOKEN\"></data>"
personaref - own user, friendpersona - to which user connect, dunno how make it for now, because no server browser for coop
coop server
-webMode COOP -Origin_NoAppFocus -AuthCode WAHAHA_IMMA_ZLO_TOKEN -requestState State_CreateCoOpPeer -requestStateParams "<data level=\"Levels/COOP_007/COOP_007\" difficulty=\"NORMAL\" personaref=\"2\" levelmode=\"coop\" logintoken=\"WAHAHA_IMMA_ZLO_TOKEN\"></data>"
persona - own, level - any coop level
*/

        public void HostBf3Coop(BF3_COOP_LEVELS level, COOP_Difficulty difficulty)
        {
            string runName = "Z.BF3";
            string cmd = GetBfCmd(mode: "coop",
                                  requestState: "State_CreateCoOpPeer",
                                  level: $"Levels/{level.ToString().ToUpper()}/{level.ToString().ToUpper()}",
                                  difficulty: difficulty.ToString().ToUpper());
            SendRunGameRequest(runName, cmd);
        }
        public void JoinBf3Coop(uint FriendId)
        {
            string runName = "Z.BF3";
            string cmd = GetBfCmd(mode: "coop",
                                  requestState: "State_ConnectToUserId",
                                  friendpersonaid: FriendId);
            SendRunGameRequest(runName, cmd);
        }
    }
}
