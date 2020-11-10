using DNSLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNS_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            DNSServer s = new DNSServer();
            s.Start();
            Console.ReadLine();
        }
    }
}
