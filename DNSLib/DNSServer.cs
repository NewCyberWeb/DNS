using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace DNSLib
{
    public class DNSServer
    {
        private readonly Socket Server;
        private EndPoint Sender;
        private byte[] Data;
        private RequestProcessor Processor;
        public DNSServer()
        {
            Server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Data = new byte[1024];

            Sender = new IPEndPoint(IPAddress.Any, 0);
            Processor = new RequestProcessor();
        }

        public void Start()
        {
            EndPoint EndPoint = new IPEndPoint(IPAddress.Any, 51);
            Server.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
            Server.Bind(EndPoint);
            //Server.BeginAccept(new AsyncCallback(AcceptConnection), Server);
            //Server.Listen(0);
            Processor.LoadDomainNameTable();
            Server.BeginReceiveFrom(Data, 0, 1024, SocketFlags.None, ref Sender, new AsyncCallback(Request), null);
        }

        private void Request(IAsyncResult result)
        {
            try
            {
                int bytesReceived = Server.EndReceiveFrom(result, ref Sender);
                Console.WriteLine($"Received {bytesReceived} bytes from {((IPEndPoint)Sender).Address}, string version of data: '{Encoding.Default.GetString(Data).Replace("\0", "")}'.");

                if(Processor.IsDNSRequest(Data))
                DNSRequestHandler(Sender, Data);

                Server.BeginReceiveFrom(Data, 0, 1024, SocketFlags.None, ref Sender, new AsyncCallback(Request), null);
            }
            catch (SocketException sEx)
            {
                Console.WriteLine("SocketException: {0} | {1}", sEx.Message, ((IPEndPoint)Sender).Address);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected Exception: {0}", ex.Message);
            }
        }

        private bool DNSRequestHandler(EndPoint endpoint, byte[] Data)
        {
            //take out the headers, and send it into the processor.

            return true;
        }

        private void AcceptConnection(IAsyncResult result)
        {

        }
    }
}
