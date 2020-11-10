using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSLib
{
    public class RequestProcessor
    {
        public RequestProcessor()
        {
            
        }

        public void LoadDomainNameTable()
        {
            //get the DNS records from a database and put them into a dictionary, start a seperate thread to keep updating this.
        }

        public bool IsDNSRequest(byte[] Data)
        {
            //check the Data for dns specific headers.
            return false;
        }
    }
}
