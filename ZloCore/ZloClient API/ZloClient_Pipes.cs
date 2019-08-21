using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using Zlo.Extras;
using Zlo.Extentions;
using static Zlo.Extentions.Helpers;

namespace Zlo
{
    public partial class API_ZloClient
    {

        private void InitializePipes()
        {
            BF3_Pipe = new NamedPipeClientStream(".", "venice_snowroller");
            BF4_Pipe = new NamedPipeClientStream(".", "warsaw_snowroller");
            BFH_Pipe = new NamedPipeClientStream(".", "omaha_snowroller");

            BF3_Pipe_Listener = new Thread(() => BF_Pipe_Loop(ZloGame.BF_3)) { IsBackground = true };
            BF4_Pipe_Listener = new Thread(() => BF_Pipe_Loop(ZloGame.BF_4)) { IsBackground = true };
            BFH_Pipe_Listener = new Thread(() => BF_Pipe_Loop(ZloGame.BF_HardLine)) { IsBackground = true };
        }
        private bool ConnectPipes()
        {
            try
            {
                BF3_Pipe_Listener.Start();
                BF4_Pipe_Listener.Start();
                BFH_Pipe_Listener.Start();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private NamedPipeClientStream BF3_Pipe { get; set; }
        private NamedPipeClientStream BF4_Pipe { get; set; }
        private NamedPipeClientStream BFH_Pipe { get; set; }


        Thread BF3_Pipe_Listener;
        Thread BF4_Pipe_Listener;
        Thread BFH_Pipe_Listener;

        private void BF_Pipe_Loop(ZloGame game)
        {
            NamedPipeClientStream stream = null;
            string pipeName = null;
            switch (game)
            {
                case ZloGame.BF_3:
                    stream = BF3_Pipe;
                    pipeName = "venice_snowroller";
                    break;
                case ZloGame.BF_4:
                    stream = BF4_Pipe;
                    pipeName = "warsaw_snowroller";
                    break;
                case ZloGame.BF_HardLine:
                    stream = BFH_Pipe;
                    pipeName = "omaha_snowroller";
                    break;
                case ZloGame.None:
                default:
                    break;
            }
            while (true)
            {
                try
                {
                    if (!stream.IsConnected && NamedPipeExists(pipeName))
                    {
                        stream.Connect();
                    }
                    else { Thread.Sleep(200); }

                    while (stream.IsConnected)
                    {
                        byte[] buffer = new byte[1024];
                        int read = stream.Read(buffer, 0, buffer.Length);
                        if (read > 0)
                        {
                            byte[] final = new byte[read + 1];
                            Buffer.BlockCopy(buffer, 0, final, 0, read + 1);
                            ProcessPipeMessage(game, final, read);
                        }
                        Thread.Sleep(50);
                    }
                }
                catch
                { }
            }
        }
        private void ParsePipeMessage(ZloGame game, string type, string message, out ServerBase Server, out bool IsInGame)
        {
            //StateChanging;State_Connecting State_ClaimReservation 14
            //StateChanging;State_Connecting State_Game 14
            //GameWaiting;14
            //StateChanged;State_Game State_NA 14
            if (message == $"State_Game State_NA {CurrentPlayerID}" || message == $"State_Game State_ClaimReservation {CurrentPlayerID}")
            {
                IsInGame = true;

                switch (game)
                {
                    case ZloGame.BF_3:
                        Server = BF3Servers.FirstOrDefault(x => x.Players.Any(z => z.Name == CurrentPlayerName));
                        IsInGame = true;
                        break;
                    case ZloGame.BF_4:
                        Server = BF4Servers.FirstOrDefault(x => x.Players.Any(z => z.Name == CurrentPlayerName));
                        IsInGame = true;
                        break;
                    case ZloGame.BF_HardLine:
                        Server = BFHServers.FirstOrDefault(x => x.Players.Any(z => z.Name == CurrentPlayerName));
                        IsInGame = true;
                        break;
                    case ZloGame.None:
                    default:
                        Server = null;
                        IsInGame = false;
                        break;
                }
            }
            else
            {
                IsInGame = false;
                Server = null;
            }
        }

    }
}
