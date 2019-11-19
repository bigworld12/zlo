namespace Zlo.PacketInfo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Zlo.Extras;

    internal delegate void ReceivedResponseEventHandler(BaseRequestPacket request, BaseResponsePacket response);
    /// <summary>
    /// A sendable packet
    /// </summary>
    internal abstract class BaseRequestPacket : BasePacket
    {

        public static readonly Dictionary<ZloPacketId, Func<BaseRequestPacket, BaseResponsePacket>> PacketIdToResponseObjectMapper = new Dictionary<ZloPacketId, Func<BaseRequestPacket, BaseResponsePacket>>()
        {
            { ZloPacketId.Ping, (req) => new RESP.Empty(ZloPacketId.Ping) { From = req } },
            { ZloPacketId.User_Info, (req) => new RESP.UserInfo() { From = req } },
            { ZloPacketId.Player_Info, (req) =>  new RESP.PlayerInfo() { From = req } },
            { ZloPacketId.Stats,(req) =>  new RESP.Stats() { From = req } },
            { ZloPacketId.Items,(req) =>  new RESP.Items() { From = req } },
            { ZloPacketId.RunnableGameList,(req) =>  new RESP.RunnableGameList() { From = req } },
            { ZloPacketId.RunGame,(req) =>  new RESP.RunGame() { From = req } },
        };
        public byte[] Serialize()
        {
            var l = new List<byte>
            {
                (byte)PacketId,0,0,0,0
            };
            SerializeCustom(l);
            var c = ((uint)l.Count - 5).QBitConv();
            for (int i = 0; i < 4; i++)
            {
                l[1 + i] = c[i];
            }
            return l.ToArray();

        }
        public virtual void SerializeCustom(List<byte> bytes) { }


        public bool IsSent { get; set; }
        public TimeSpan WaitBeforePeriod { get; set; } = TimeSpan.Zero;


        public BaseResponsePacket Response { get; private set; }
        public virtual bool IsRespondable => true;
        public bool IsReceived { get; set; }


        public event ReceivedResponseEventHandler ReceivedResponse;
        public void RaiseResponse(byte[] respData)
        {            
            if (IsRespondable && respData != null)
            {
                Response = PacketIdToResponseObjectMapper[PacketId](this);
                Response.Deserialize(respData);
            }            
            IsReceived = true;
            ReceivedResponse?.Invoke(this, Response);
        }
    }

}
