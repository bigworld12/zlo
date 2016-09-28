using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zlo.Extentions
{
    public static class Helpers
    {
        public static int FloorToHigher(this float tof)
        {
            var intportion = (int)tof;
            if (tof > intportion)
            {
                return intportion + 1;
            }
            else
            {
                return intportion;
            }
        }

        public static byte[] ReadReversedBytes(this BinaryReader br , int count)
        {
            var bytes = br.ReadBytes(count);
            Array.Reverse(bytes);
            return bytes;
        }
        public static string ReadZString(this BinaryReader br)
        {
            StringBuilder s = new StringBuilder();
            char t;

            while (br.PeekChar() != -1 && (t = br.ReadChar()) > 0)
                s.Append(t);
            return s.ToString();
        }
        public static uint ReadZUInt(this BinaryReader br)
        {
            return BitConverter.ToUInt32(br.ReadReversedBytes(4) , 0);
        }
        public static ushort ReadZUInt16(this BinaryReader br)
        {
            return BitConverter.ToUInt16(br.ReadReversedBytes(4) , 0);
        }
        public static float ReadZFloat(this BinaryReader br)
        {
            return BitConverter.ToSingle(br.ReadReversedBytes(4) , 0);
        }
    }
}
