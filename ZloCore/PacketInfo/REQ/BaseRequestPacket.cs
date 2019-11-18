namespace Zlo.PacketInfo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Zlo.Extras;

    internal delegate void ReceivedResponseEventHandler(BaseRequestPacket request, BasePacket response);
    /// <summary>
    /// A sendable packet
    /// </summary>
    internal abstract class BaseRequestPacket : BasePacket
    {
        public static readonly Dictionary<ZloPacketId, Func<BaseRequestPacket, BaseResponsePacket>> PacketIdToResponseObjectMapper = new Dictionary<ZloPacketId, Func<BaseRequestPacket, BaseResponsePacket>>()
        {
            { ZloPacketId.Ping, (_) => new RESP.Empty(ZloPacketId.Ping) },
            { ZloPacketId.User_Info, (_) => new RESP.UserInfo() },
            //{ ZloPacketId.Server_List, (_)=> }
        };
        public List<byte> Serialize()
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
            return l;

        }
        public virtual void SerializeCustom(List<byte> bytes) { }


        public bool IsSent { get; set; }


        public BaseResponsePacket Response { get; private set; }
        public virtual bool IsRespondable => true;
        public bool IsReceived { get; set; }


        public event ReceivedResponseEventHandler ReceivedResponse;
        public void RaiseResponse(ZloPacketId respId, byte[] respData)
        {
            if (!IsRespondable)
                return;
            Response = PacketIdToResponseObjectMapper[respId](this);
            Response.Deserialize(respData);
            IsReceived = true;
            ReceivedResponse?.Invoke(this, Response);
        }
    }

}
