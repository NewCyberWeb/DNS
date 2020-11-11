using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSLib.Server
{
    internal sealed class RequestProcessor
    {
        private readonly DNSRecord Root;
        public RequestProcessor()
        {
            Root = new DNSRecord();
        }

        public void LoadDomainNameTable()
        {
            //get the DNS records from a database and put them into a dictionary, start a seperate thread to keep updating this.
            //the database might as well be a file on disk and get an API to update this.
        }

        public bool IsDNSRequest(byte[] Data)
        {
            if (Data.Length <= 258)
                if (Enum.IsDefined(typeof(DNSPacket.PacketType), (DNSPacket.PacketType)Data[0]))
                    if (Enum.IsDefined(typeof(DNSPacket.LookupType), (DNSPacket.LookupType)Data[1]))
                        return true;
            return false;
        }

        public DNSRecord LookupByName(string[] args)
        {
            if (args[0] == "arpa") return null;
            DNSRecord _record = Root;
            for (int i = 0; i < args.Length; i++)
            {
                _record = _record.GetSubRecord(args[i]);
                if (_record == null) return null;
            }

            return _record;
        }

        public DNSRecord LookupByIp(string ip)
        {
            DNSRecord arpaIPv4 = Root.GetSubRecord("arpa").GetSubRecord("ipv4");
            string[] ipList = ip.Split('.');

            //return null if any of these values are not correct
            if (ipList.Count() != 4 || arpaIPv4 == null) return null;

            for (int i = 3; i > 0; i--)
            {
                arpaIPv4 = arpaIPv4.GetSubRecord(ipList[i]);
                if (arpaIPv4 == null) return null;
            }

            return arpaIPv4;
        }
    }
}
