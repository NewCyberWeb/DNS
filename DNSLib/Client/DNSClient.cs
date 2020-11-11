using DNSLib.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DNSLib.Client
{
    public sealed class DNSClient
    {
        public DNSClient()
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 51);
            UdpClient client = new UdpClient(ipep);
            byte[] bytes = Encoding.Default.GetBytes("test");
            client.Connect(ipep);
            client.Send(bytes, bytes.Length);
        }
    }
}
