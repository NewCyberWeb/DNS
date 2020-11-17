using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSLib.Server
{
    public sealed class RequestProcessor
    {
        internal readonly DNSRecord Root;
        private readonly string RootDNSFileStorageName = "DataRecords.jDNS";
        public RequestProcessor()
        {
            Root = new DNSRecord();
        }

        /// <summary>
        /// Generates the DNS File structure and stores it in the JSON file.
        /// </summary>
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
                { 
                    new DNSRecord
                    {
                        Domain = "0",
                        SubDomains = new List<DNSRecord>
                        { 
                            new DNSRecord
                            {
                                Domain = "0",
                                SubDomains = new List<DNSRecord>
                                { 
                                    new DNSRecord
                                    {
                                        Domain = "1",
                                        SubDomains = new List<DNSRecord>
                                        { 
                                            new DNSRecord
                                            {
                                                Domain = "example.com"
                                            }
                                        }
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

        /// <summary>
        /// Reads the DNS entry's from the JSON stored file.
        /// </summary>
        public void LoadDomainNameTable()
        {
            //get the DNS records from a JSON file and put the data into the local DNS root record.
            DNSRecord _root = JsonStorageHelper.GetStorageFileContents<DNSRecord>(RootDNSFileStorageName);
            if (_root == null)
            {
                CreateDNSStructureFile();
                LoadDomainNameTable();
                return;
            }
            else if (_root.HasSubDomains())
            {
                Root.SubDomains = _root.SubDomains;
            }
        }

        /// <summary>
        /// Saves the current DNS table into the JSON file.
        /// </summary>
        public void SaveDomainNameTable()
        {
            JsonStorageHelper.SaveStorageFile(Root, RootDNSFileStorageName);
        }

        /// <summary>
        /// Does a set of checks to confirm that the byte array is actually a DNS packet.
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public bool IsDNSRequest(byte[] Data)
        {
            if (Data.Length <= 258)
                if (Enum.IsDefined(typeof(DNSPacket.PacketType), (DNSPacket.PacketType)Data[0]))
                    if (Enum.IsDefined(typeof(DNSPacket.LookupType), (DNSPacket.LookupType)Data[1]))
                        return true;
            return false;
        }

        /// <summary>
        /// Do a DNS Lookup by domain name, this function searches through all entry's except the ARPA ipv4 entry's
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Do a DNS Lookup by ip, this function searches through the ARPA ipv4 entry's
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Edits the DNS entry specified with the flag parameter
        /// </summary>
        /// <param name="fDomainIp"></param>
        /// <param name="currentDomain"></param>
        /// <param name="newField"></param>
        /// <returns></returns>
        public string EditDNSEntry(bool fDomainIp, string currentDomain, string newField)
        {
            string[] domainSplit = currentDomain.Split('.').Reverse().ToArray();
            DNSRecord recordDNS = LookupByName(domainSplit);
            if (recordDNS == null) return $"Domain {currentDomain} not found";
            string ip = recordDNS.Address;
            if (fDomainIp) //search by domain, change the domain
            {
                recordDNS.Domain = newField;
                DNSRecord recordARP = LookupByIp(ip);
                recordARP = recordARP.GetSubRecord(currentDomain);
                if (recordARP != null)
                {
                    string[] newValueSplit = newField.Split('.').Reverse().ToArray();
                    recordARP.Domain = newValueSplit[0];
                }
            }
            else //search by domain name, change the ip
            {
                recordDNS.Address = newField;
                DNSRecord recordARP = LookupByIp(ip);
                if (recordARP.GetSubRecord(currentDomain) != null)
                {
                    //this record exists, delete it
                    recordARP.SubDomains.Remove(recordARP.GetSubRecord(currentDomain));
                    AddDNSIpEntry(currentDomain, newField);
                }
            }

            SaveDomainNameTable();
            return recordDNS.ToString();
        }

        /// <summary>
        /// Lets you search for DNS entry's
        /// </summary>
        /// <param name="fDomainIp"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string ShowDNSEntry(bool fDomainIp, string value)
        {
            if (fDomainIp)
            {
                DNSRecord recordDNS = LookupByName(value.Split('.').Reverse().ToArray());
                if (recordDNS == null) return $"Domain {value} not found";
                return recordDNS.ToString();
            }
            else
            {
                DNSRecord recordARP = LookupByIp(value);
                if (recordARP == null) return $"ARPA for {value} not found";
                return recordARP.ToString();
            }
        }

        /// <summary>
        /// Adds both ARPA and regular entry's into the DNS cache
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public string AddDNSEntry(string domain, string ip)
        {
            string name = AddDNSNameEntry(domain, ip);
            string _ip = AddDNSIpEntry(domain, ip);
            return $"Adding DNS and ReverseDNS records: \r\nDNS: {name}\r\nARPA: {_ip}";
        }

        /// <summary>
        /// Deletes the DNS entry from the cachce and stores it in the JSON
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public string DeleteDNSEntry(string domain)
        {
            //delete DNS record
            string[] domainSplit = domain.Split('.').Reverse().ToArray();
            DNSRecord recordToDelete = LookupByName(domainSplit);
            DNSRecord recordToDeleteFrom = LookupByName(domainSplit.SubArray(0, domainSplit.Length - 1));
            if (recordToDelete == null) return $"Domain {domain} not found";
            string ip = recordToDelete.Address;

            if (recordToDeleteFrom.SubDomains.Contains(recordToDelete))
            {
                recordToDeleteFrom.SubDomains.Remove(recordToDelete);
            }

            //delete ARPA record
            DNSRecord recordARP = LookupByIp(ip);
            if (recordARP.GetSubRecord(domain) != null)
            {
                //this record exists, delete it
                recordARP.SubDomains.Remove(recordARP.GetSubRecord(domain));
            }
            else
            {
                return $"No ARPA entry found for {domain} by ip {ip}";
            }

            SaveDomainNameTable();
            return $"Removed the entry's for domain {domain}";
        }

        /// <summary>
        /// Adds a standard DNS entry into the dns cache
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        private string AddDNSNameEntry(string domain, string ip)
        {
            //go to the deepest level possible:
            string[] domainLevels = domain.Split('.').Reverse().ToArray();
            DNSRecord deepestRecord = null;
            for (int i = 1; i <= domainLevels.Length; i++)
            {
                deepestRecord = LookupByName(domainLevels.SubArray(0, i));
                if (deepestRecord == null)
                {
                    //get the previous record and then break.
                    deepestRecord = LookupByName(domainLevels.SubArray(0, i - 1));
                    break;
                }

                if (!deepestRecord.HasSubDomains()) break;
            }

            // start creating from the latest deepestRecord
            if (deepestRecord != null)
            {
                int depthStartingIndex = Array.IndexOf(domainLevels, deepestRecord.Domain) + 1;

                if (depthStartingIndex == domainLevels.Length - 1) //means the last item in the array
                {
                    DNSRecord r = new DNSRecord
                    {
                        Domain = domainLevels[depthStartingIndex],
                        Address = ip
                    };

                    deepestRecord.SubDomains.Add(r);

                    SaveDomainNameTable();
                    return r.ToString();
                }
                else if (depthStartingIndex == domainLevels.Length)
                {
                    return $"Sub Domain {domainLevels[depthStartingIndex - 1]} already exists, please try editing instead of creating.";
                }
                else
                    return $"Missing subdomain record for {domainLevels[depthStartingIndex - 1]}.";
            }
            else
                return "No proper domain name specified, please try again.";
        }

        /// <summary>
        /// Adds an ARPA DNS entry into the dns cache
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        private string AddDNSIpEntry(string domain, string ip)
        {
            string[] ipSection = ip.Split('.');
            DNSRecord arpaRecord = Root.GetSubRecord("arpa").GetSubRecord("ipv4");

            if (arpaRecord == null || !ip.IsValidIpAddress()) //check if the arpa exists and the ip is valid
                throw new Exception($"Error ARPA search for ip: {ip}");

            //find or create the third ip segment
            for (int i = 0; i < 4; i++)
            {
                DNSRecord subDomain = arpaRecord.GetSubRecord(ipSection[i]);
                if (subDomain == null)
                {
                    DNSRecord r = new DNSRecord
                    {
                        Domain = ipSection[i]
                    };

                    arpaRecord.SubDomains.Add(r);
                    subDomain = arpaRecord.GetSubRecord(ipSection[i]);
                }

                arpaRecord = subDomain;
            }

            // now start adding the final arpa record        
            if (arpaRecord.GetSubRecord(domain) == null) //means the arpa record doesnt exists yet
            {
                DNSRecord r = new DNSRecord
                {
                    Domain = domain
                };

                arpaRecord.SubDomains.Add(r);

                SaveDomainNameTable();
                return r.ToString();
            }
            else
            {
                return $"Arpa record for {domain} already exists, please try editing instead of creating.";
            }
        }
    }
}
