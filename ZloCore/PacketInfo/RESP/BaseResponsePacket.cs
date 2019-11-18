using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Zlo.Extras;

namespace Zlo.PacketInfo
{
    internal abstract class BaseResponsePacket : BasePacket
    {
        public void Deserialize(byte[] packetData)
        {
            using var ms = new MemoryStream(packetData);
            using var br = new BinaryReader(ms);
            DeserializeCustom(br);
        }
        public virtual void DeserializeCustom(BinaryReader from) { }
    }
}
