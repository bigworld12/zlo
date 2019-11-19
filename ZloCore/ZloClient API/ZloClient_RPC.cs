using DiscordRPC;
using DiscordRPC.Logging;
using DiscordRPC.Message;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zlo.Extras;

namespace Zlo
{
    public class RPC_Settings
    {
        [JsonIgnore]
        public string Token { get; set; }
        public bool UseDefaultRPC { get; set; } = true;
        [JsonIgnore]
        public GetPresenceFunc GetPresence { get; set; }
    }
    public partial class API_ZloClient
    {
        private ZloBFGame? LatestGameState_Game;
        private string LatestGameState_Type;
        private string LatestGameState_Message;
        private DateTime? latestDate;
        /// <summary>
        /// True -> will enable DiscordRPC
        /// False -> will disable DiscordRPC        
        /// </summary>
        public bool IsEnableDiscordRPC
        {
            get => Settings.CurrentSettings.IsEnableDiscordRPC;
            set
            {
                if (Settings.CurrentSettings.IsEnableDiscordRPC == false && value == true)
                {
                    Settings.CurrentSettings.IsEnableDiscordRPC = true;
                    //enable 
                    StartDiscordRPC();
                }
                else if (Settings.CurrentSettings.IsEnableDiscordRPC == true && value == false)
                {
                    Settings.CurrentSettings.IsEnableDiscordRPC = false;
                    //enable 
                    StopDiscordRPC();
                }
                else
                {
                    return;
                }
            }
        }
        RPC_Settings m_RPC_Settings;
        public RPC_Settings RPC_Settings
        {
            get => m_RPC_Settings ?? (m_RPC_Settings = new RPC_Settings());
        }


        private RichPresence DiscordRPCState = new RichPresence()
        {
            Details = "Zlo Launcher"
        };
        private DiscordRpcClient RpcClient;
        private void StartDiscordRPC()
        {

            var token = RPC_Settings.UseDefaultRPC ? "464170401472446465" : RPC_Settings.Token;
            RpcClient = new DiscordRpcClient(token)
            {
                //Logger = new ConsoleLogger() { Coloured = true, Level = LogLevel.Info }
            };
            RpcClient.OnReady += DiscordRPC_Ready;
            RpcClient.OnClose += RpcClient_OnClose;
            RpcClient.OnError += RpcClient_OnError;
            RpcClient.OnPresenceUpdate += RpcClient_OnPresenceUpdate;

            if (!RPC_Settings.UseDefaultRPC)
            {
                DiscordRPCState = GetCurrentPresence();
            }
            RpcClient.SetPresence(DiscordRPCState);
            RpcClient.Initialize();

            Task.Run(() => MainLoop());
        }
        private void MainLoop()
        {
            while (true)
            {
                RpcClient.Invoke();
                Thread.Sleep(1000);
                if (DiscordRPCState == null || !IsEnableDiscordRPC)
                {
                    return;
                }
                UpdateCurrentPresence();
            }
        }
        private void RpcClient_OnPresenceUpdate(object sender, PresenceMessage args)
        {
            Log.WriteLog($"Discord RPC Updated: {args.Name}");
        }

        private void RpcClient_OnError(object sender, ErrorMessage args)
        {
            Log.WriteLog($"Discord RPC Error: {args.Code}\nMessage: {args.Message}\nMessage Type : {args.Type}\n");
        }

        private void RpcClient_OnClose(object sender, CloseMessage args)
        {
            Log.WriteLog($"Discord RPC Closed: {args.Code}\nReason: {args.Reason}\nMessage Type : {args.Type}");
        }

        private void StopDiscordRPC()
        {
            RpcClient.Dispose();
        }

        //start update pesence timer
        private void DiscordRPC_Ready(object sender, ReadyMessage args)
        {
            if (IsEnableDiscordRPC)
                Log.WriteLog("Discord RPC Ready!");
        }
        private string PartyID = Secrets.CreateFriendlySecret(new Random());
        private void UpdateCurrentPresence()
        {
            var backup = DiscordRPCState?.Clone();
            var newRPC = GetCurrentPresence();
            var bjson = JObject.FromObject(backup);
            var njson = JObject.FromObject(newRPC);
            if (JToken.DeepEquals(bjson, njson))
            {
                return;
            }
            DiscordRPCState = newRPC;
            if (!RpcClient.Disposed)
                RpcClient.SetPresence(newRPC);
        }
        uint? latestServerID;
        private RichPresence GetCurrentPresence()
        {
            string gamelogo = "", gameName = "", shortGameName = "", state = "", detail = "", imgDesc = "";
            int current = 0, maxsize = 0;
            ZloBFGame Choice;
            void GetGameName()
            {
                switch (Choice)
                {
                    case ZloBFGame.BF_3:
                        gamelogo = "bf3";
                        gameName = "BATTLEFIELD 3";
                        shortGameName = "BF3";
                        break;
                    case ZloBFGame.BF_4:
                        gamelogo = "bf4";
                        gameName = "BATTLEFIELD 4";
                        shortGameName = "BF4";
                        break;
                    case ZloBFGame.BF_HardLine:
                        gamelogo = "bfh";
                        gameName = "BATTLEFIELD HARDLINE";
                        shortGameName = "BFH";
                        break;
                    case ZloBFGame.None:
                    default:
                        break;
                }
            }
            ServerBase s = null;
            bool IsInGame = false;
            string map = null, gameMode = null;
            if (!LatestGameState_Game.HasValue)
            {
                Choice = ActiveServerListener;
                GetGameName();
                //state = "Browsing Servers";
                detail = $"Browsing {shortGameName} Servers";
                imgDesc = detail;
                latestDate = null;
            }
            else
            {
                Choice = LatestGameState_Game.Value;
                GetGameName();
                ParsePipeMessage(Choice, LatestGameState_Type, LatestGameState_Message, out s, out IsInGame);
                if (IsInGame)
                {
                    if (!latestDate.HasValue)
                        latestDate = DateTime.UtcNow;
                    if (s != null)
                    {
                        if (!latestServerID.HasValue)
                        {
                            latestServerID = s.ServerID;
                        }
                        else
                        {
                            if (latestServerID != s.ServerID)
                            {
                                //server changed
                                latestDate = DateTime.UtcNow;
                                latestServerID = s.ServerID;
                            }
                        }
                        current = s.Players.Count;
                        maxsize = s.MaxPlayers;
                        map = s.MapRotation.CurrentActualMap.MapName;
                        gameMode = s.MapRotation.CurrentActualMap.GameModeName;

                        detail = s.ServerName;
                        state = map;
                        imgDesc = $"[{shortGameName}] {s.ServerName} [{gameMode} - {map}]";
                    }
                    else
                    {
                        detail = "Unknown server";
                        state = "IN-GAME";
                        imgDesc = "IN-GAME";
                    }
                }
                else
                {
                    latestDate = null;
                    latestServerID = null;
                    state = string.Empty;
                    detail = $"Browsing {gameName} Servers";
                    imgDesc = gameName;
                }
            }

            var res = RPC_Settings.UseDefaultRPC ? new RichPresence()
            {
                Assets = new Assets()
                {
                    LargeImageKey = gamelogo,
                    LargeImageText = imgDesc,
                    SmallImageKey = "zlo_s",
                    SmallImageText = "zloemu.net"
                },
                Details = detail,
                State = state
            } :
            RPC_Settings.GetPresence?.Invoke(s, IsInGame, Choice, map, gameMode);

            if (RPC_Settings.UseDefaultRPC)
            {
                if (maxsize != 0)
                    res.Party = new Party() { ID = PartyID, Size = current, Max = maxsize };
                if (latestDate.HasValue)
                    res.Timestamps = new Timestamps() { Start = latestDate };
            }
            return res;
        }

    }
    public delegate RichPresence GetPresenceFunc(ServerBase currentServer, bool isInGame, ZloBFGame game, string map, string gameMode);
}
