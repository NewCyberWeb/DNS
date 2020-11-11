using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSLib.Server
{
    internal sealed class DNSRecord
    {
        public string Domain { get; set; }
        public string Address { get; set; }
        public List<DNSRecord> SubDomains { get; set; }

        public DNSRecord()
        {
            SubDomains = new List<DNSRecord>();
        }

        //Checks if the current domain has sub records.
        public bool HasSubDomains()
        {
            return SubDomains.Count != 0;
        }

        /// <summary>
        /// Gets the subrecord based on the provided key
        /// </summary>
        /// <param name="key">part of a domain name between the dots</param>
        /// <returns>returns the found DNS record</returns>
        public DNSRecord GetSubRecord(string key)
        {
            return SubDomains.FirstOrDefault(n => n.Domain == key);
        }
    }
}
