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

        public delegate void LoggingHandler(string message);
        public event LoggingHandler VerboseLog;
        public event LoggingHandler ErrorLog;

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
            VerboseLog?.Invoke("Server has started listening for DNS requests.");
        }

        public void Stop()
        {
            Server.Close();            
        }

        private void Request(IAsyncResult result)
        {
            try
            {
                Server.EndReceiveFrom(result, ref Sender);
                bool success = DNSRequestHandler(Sender, Data);
                VerboseLog?.Invoke($"Received data from {((IPEndPoint)Sender).Address}:{((IPEndPoint)Sender).Port}. Response success: {success}.");
                Server.BeginReceiveFrom(Data, 0, 258, SocketFlags.None, ref Sender, new AsyncCallback(Request), null);
            }
            catch (SocketException sEx)
            {
                ErrorLog?.Invoke($"SocketException: '{sEx.Message}' for {((IPEndPoint)Sender).Address}.");
            }
            catch (DNSException dEx)
            {
                ErrorLog?.Invoke($"DNS Exception: '{dEx.Message}' for {((IPEndPoint)Sender).Address}.");
            }
            catch (Exception ex)
            {
                ErrorLog?.Invoke($"Unexpected Exception: '{ex.Message}'.");
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
                    string lookupData = Encoding.ASCII.GetString(packet.Data, 0, packet.Length);
                    switch (packet.Lookup)
                    {
                        case DNSPacket.LookupType.Name:
                            return SendResponse(endpoint, packet.Lookup, Processor.LookupByName(lookupData.Split('.').Reverse().ToArray()));
                        case DNSPacket.LookupType.IP:
                            return SendResponse(endpoint, packet.Lookup, Processor.LookupByIp(lookupData));
                    }
                }
                else
                {
                    throw new DNSException(packet, "Received a Response package while expecting only Request packages.");
                }
            }

            return false;
        }

        private bool SendResponse(EndPoint endpoint, DNSPacket.LookupType type, DNSRecord record)
        {
            if (record == null)
                return false;

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
                ErrorLog?.Invoke($"Error sending async response: '{ex.Message}'.");
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
                    VerboseLog?.Invoke($"Sent a response to {((IPEndPoint)ep).Address}:{((IPEndPoint)ep).Port}.");
                }
            }
            catch (Exception ex)
            {
                ErrorLog?.Invoke($"Error ending async response: '{ex.Message}'.");
            }
        }

        public void GenerateFileStructure()
        {
            Processor.CreateDNSStructureFile();
        }
    }
}
