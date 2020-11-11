using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSLib
{
    public sealed class DNSException : Exception
    {
        public DNSPacket Packet;
        public DNSException(DNSPacket p)
        {
            Packet = p;
        }

        public DNSException(DNSPacket p, string message) : base(message)
        {
            Packet = p;
        }
    }
}
