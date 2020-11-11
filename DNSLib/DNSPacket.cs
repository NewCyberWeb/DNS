using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSLib
{
    internal class DNSPacket
    {
        public enum PacketType
        {
            Request = 0x1A,
            Response = 0x1B
        }

        public enum LookupType
        {
            Name = 0x2A,
            IP = 0x2B
        }

        public PacketType Type { get; set; }
        public LookupType Lookup { get; set; }
        public int Length { get; set; }
        public byte[] Data { get; set; }

        public DNSPacket()
        {
            Data = new byte[255];
        }

        public static DNSPacket RawToPacket(byte[] data)
        {
            return new DNSPacket
            {
                Type = (PacketType)data[0],
                Lookup = (LookupType)data[1],
                Length = (int)data[2],
                Data = data.SubArray(3, 255)
            };
        }

        public static byte[] PacketToRaw(DNSPacket packet)
        {
            byte[] responseArray = new byte[] { (byte)packet.Type, (byte)packet.Lookup, (byte)packet.Length };
            responseArray = responseArray.Concat(packet.Data).ToArray();         

            return responseArray;
        }
    }
}
