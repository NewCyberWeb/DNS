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
        private readonly string RootDNSFileStorageName = "DataRecords.jDNS";
        public RequestProcessor()
        {
            Root = new DNSRecord();
        }

        public void CreateDNSStructureFile()
        {
            DNSRecord record = new DNSRecord
            {
                Domain = "Root"
            };

            DNSRecord com = new DNSRecord
            {
                Domain = "com"
            };

            com.SubDomains.Add(new DNSRecord
            {
                Address = "127.0.0.1",
                Domain = "example"
            });

            DNSRecord arpa = new DNSRecord
            {
                Domain = "arpa"
            };

            DNSRecord arpaIpv4 = new DNSRecord
            {
                Domain = "ipv4"
            };

            arpaIpv4.SubDomains.Add(new DNSRecord
            {
                Domain = "127",
                SubDomains = new List<DNSRecord>
                { new DNSRecord
                    {
                        Domain = "0",
                        SubDomains = new List<DNSRecord>
                        { new DNSRecord
                            {
                                Domain = "0",
                                SubDomains = new List<DNSRecord>
                                { new DNSRecord
                                    {
                                        Domain = "1",
                                        Address = "example.com"
                                    }
                                }
                            }
                        }
                    }
                }
            });

            arpa.SubDomains.Add(arpaIpv4);
            record.SubDomains.Add(com);
            record.SubDomains.Add(arpa);

            JsonStorageHelper.SaveStorageFile(record, RootDNSFileStorageName);
        }

        public void LoadDomainNameTable()
        {
            //get the DNS records from a database and put them into a dictionary, start a seperate thread to keep updating this.
            //the database might as well be a file on disk and get an API to update this.
            DNSRecord _root = JsonStorageHelper.GetStorageFileContents<DNSRecord>(RootDNSFileStorageName);
            if(_root != null)
            {
                Root.SubDomains = _root.SubDomains;
            }
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

            for (int i = 0; i < 4; i++)
            {
                arpaIPv4 = arpaIPv4.GetSubRecord(ipList[i]);
                if (arpaIPv4 == null) return null;
            }

            return arpaIPv4;
        }
    }
}
