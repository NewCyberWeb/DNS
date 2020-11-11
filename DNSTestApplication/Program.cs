using DNSLib;
using DNSLib.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSTestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            DNSClient c = new DNSClient("192.168.2.86");
            Console.WriteLine(c.RetrieveIpAddress("example.com"));
            Console.WriteLine(c.RetrieveDNSName("127.0.0.1"));
            Console.ReadLine();
        }
    }
}
