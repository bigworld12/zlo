using System.Collections.Generic;
using System.IO;

namespace Zlo.Extras
{
    public interface IBFServerBase
    {
        Dictionary<string, string> Attributes { get; }
        ZloBFGame Game { get; }
        uint GameSet { get; }
        byte IGNO { get; }
        uint INIP { get; }
        ushort INPORT { get; }
        bool IsPasswordProtected { get; }
        API_MapRotationBase MapRotation { get; }
        byte MaxPlayers { get; }
        ulong NATT { get; }
        byte NRES { get; }
        byte NTOP { get; }
        string PGID { get; }
        API_PlayerListBase Players { get; }
        byte PRES { get; }
        uint SEED { get; }
        uint ServerID { get; }
        uint ServerIP { get; }
        string ServerName { get; }
        ushort ServerPort { get; }
        Dictionary<string, string> ServerSettings { get; }
        byte ServerState { get; }
        byte SlotCapacity { get; }
        string UUID { get; }
        byte VOIP { get; }
        string VSTR { get; }

        internal void Parse(byte[] info);        
        internal void ParsePlayers(byte[] playersbuffer);

        string ToString();
    }
}