using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSLib
{
    public sealed class DNSPacket
    {
        /// <summary>
        /// Enum for identifying the packets type.
        /// </summary>
        public enum PacketType
        {
            Request = 0x1A,
            Response = 0x1B
        }

        /// <summary>
        /// Enum for identifying the retrieval type.
        /// </summary>
        public enum LookupType
        {
            Name = 0x2A,
            ARPA = 0x2B
        }

        /// <summary>
        /// This packets type.
        /// </summary>
        public PacketType Type { get; set; }

        /// <summary>
        /// This packets lookup type.
        /// </summary>
        public LookupType Lookup { get; set; }

        /// <summary>
        /// The length of the embedded data message.
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// The embedded data message.
        /// </summary>
        public byte[] Data { get; set; }

        public DNSPacket()
        {
            Data = new byte[255];
        }

        /// <summary>
        /// Converts a byte array into a <see cref="DNSPacket"/> object.
        /// </summary>
        /// <param name="data">The byte array with a max length of 258</param>
        /// <returns></returns>
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

        /// <summary>
        /// Converts a <see cref="DNSPacket"/> into a byte array.
        /// </summary>
        /// <param name="packet">The DNSPacket to convert</param>
        /// <returns>A byte Array of max length 258 </returns>
        public static byte[] PacketToRaw(DNSPacket packet)
        {
            byte[] responseArray = new byte[] { (byte)packet.Type, (byte)packet.Lookup, (byte)packet.Length };
            responseArray = responseArray.Concat(packet.Data).ToArray();         

            return responseArray;
        }
    }
}
