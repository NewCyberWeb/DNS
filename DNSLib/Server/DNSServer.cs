using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace DNSLib.Server
{
    public sealed class DNSServer
    {
        private readonly Socket Server;
        private EndPoint Sender;
        private readonly byte[] Data;
        private readonly RequestProcessor Processor;

        public DNSServer()
        {
            Server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Data = new byte[258];

            Sender = new IPEndPoint(IPAddress.Any, 0);
            Processor = new RequestProcessor();
        }

        public void Start()
        {
            EndPoint EndPoint = new IPEndPoint(IPAddress.Any, 51);
            Server.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
            Server.Bind(EndPoint);
            Processor.LoadDomainNameTable(); //do database stuff.
            Server.BeginReceiveFrom(Data, 0, 258, SocketFlags.None, ref Sender, new AsyncCallback(Request), null);
        }

        private void Request(IAsyncResult result)
        {
            try
            {
                int bytesReceived = Server.EndReceiveFrom(result, ref Sender);
                Console.WriteLine($"Received {bytesReceived} bytes from {((IPEndPoint)Sender).Address}, string version of data: '{Encoding.Default.GetString(Data).Replace("\0", "")}'.");

                //check if this request is a dns request, if not, absorb this message and continue
                if (Processor.IsDNSRequest(Data))
                    Console.WriteLine(DNSRequestHandler(Sender, Data) ? "Responded" : "Problem with request, ignored");

                Server.BeginReceiveFrom(Data, 0, 258, SocketFlags.None, ref Sender, new AsyncCallback(Request), null);
            }
            catch (SocketException sEx)
            {
                Console.WriteLine("SocketException: '{0}' for {1}", sEx.Message, ((IPEndPoint)Sender).Address);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected Exception: {0}", ex.Message);
            }
        }

        private bool DNSRequestHandler(EndPoint endpoint, byte[] Data)
        {
            //take out the headers, decide where to send it into the processor.
            if (Processor.IsDNSRequest(Data))
            {
                DNSPacket packet = DNSPacket.RawToPacket(Data);
                if (packet.Type == DNSPacket.PacketType.Request)
                {
                    string lookupData = Encoding.ASCII.GetString(packet.Data).Replace("\0", "");
                    switch (packet.Lookup)
                    {
                        case DNSPacket.LookupType.Name:
                            return SendResponse(endpoint, packet.Lookup, Processor.LookupByName(lookupData.Split('.').Reverse().ToArray()));
                        case DNSPacket.LookupType.IP:
                            return SendResponse(endpoint, packet.Lookup, Processor.LookupByIp(lookupData));
                    }
                }
            }

            return false;
        }

        private bool SendResponse(EndPoint endpoint, DNSPacket.LookupType type, DNSRecord record)
        {
            DNSPacket packet = new DNSPacket
            {
                Type = DNSPacket.PacketType.Response,
                Lookup = type,
                Data = Encoding.ASCII.GetBytes(record.Address)
            };

            packet.Length = packet.Data.Length;
            byte[] sendBuffer = DNSPacket.PacketToRaw(packet);

            try
            {
                Server.BeginSendTo(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, endpoint, new AsyncCallback(ResponseCompleted), endpoint);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending async response: " + ex.Message);
                return false;
            }
        }

        private void ResponseCompleted(IAsyncResult result)
        {
            try
            {
                EndPoint ep = (EndPoint)result.AsyncState;
                int bytesSend = Server.EndSendTo(result);

                if (bytesSend > 0)
                {
                    Console.WriteLine($"Sent a response to {((IPEndPoint)ep).Address} on port {((IPEndPoint)ep).Port}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error ending async response: " + ex.Message);
            }
        }

        public void GenerateFileStructure()
        {
            Processor.CreateDNSStructureFile();
        }
    }
}
