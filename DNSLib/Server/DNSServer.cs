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
        public readonly RequestProcessor Processor;
        private const int PACKET_LENGTH = 258;

        public delegate void LoggingHandler(string message);
        public event LoggingHandler VerboseLog;
        public event LoggingHandler ErrorLog;

        public DNSServer()
        {
            Server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Data = new byte[PACKET_LENGTH];

            Sender = new IPEndPoint(IPAddress.Any, 0);
            Processor = new RequestProcessor();
        }

        /// <summary>
        /// Initiates the DNS Server's protocols and starts listening for requests.
        /// </summary>
        public void Start()
        {
            EndPoint EndPoint = new IPEndPoint(IPAddress.Any, 51);
            Server.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
            Server.Bind(EndPoint);
            Processor.LoadDomainNameTable(); //do database stuff.
            Server.BeginReceiveFrom(Data, 0, PACKET_LENGTH, SocketFlags.None, ref Sender, new AsyncCallback(Request), null);
            VerboseLog?.Invoke("Server has started listening for DNS requests.");
        }

        /// <summary>
        /// Stops the DNS Server
        /// </summary>
        public void Stop()
        {
            Server.Close();
        }

        /// <summary>
        /// Callback function for the Data recieve function.
        /// </summary>
        /// <param name="result"></param>
        private void Request(IAsyncResult result)
        {
            try
            {
                Server.EndReceiveFrom(result, ref Sender);
                bool success = DNSRequestHandler(Sender, Data);
                VerboseLog?.Invoke($"Received data from {((IPEndPoint)Sender).Address}:{((IPEndPoint)Sender).Port}. Response success: {success}.");
                Server.BeginReceiveFrom(Data, 0, PACKET_LENGTH, SocketFlags.None, ref Sender, new AsyncCallback(Request), null);
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

        /// <summary>
        /// Function that processes the incoming data after an async callback returned.
        /// </summary>
        /// <param name="endpoint">The endpoint the data came from</param>
        /// <param name="Data">The dns packet data</param>
        /// <returns></returns>
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
                        case DNSPacket.LookupType.ARPA:
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

        /// <summary>
        /// Function that sends the async responses to the client.
        /// </summary>
        /// <param name="endpoint">The endpoint to send the response to</param>
        /// <param name="type">Packet type</param>
        /// <param name="record">The DNSRecord to send back</param>
        /// <returns></returns>
        private bool SendResponse(EndPoint endpoint, DNSPacket.LookupType type, DNSRecord record)
        {
            if (record == null)
                return false;

            DNSPacket packet = new DNSPacket
            {
                Type = DNSPacket.PacketType.Response,
                Lookup = type,
                Data = (type == DNSPacket.LookupType.Name)
                ? Encoding.ASCII.GetBytes(record.Address)
                : Encoding.ASCII.GetBytes(string.Join(",", record.SubDomains.Select(n => n.Domain)).ToCharArray(), 0, 254)
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

        /// <summary>
        /// Callback function for sending async DNS responses.
        /// </summary>
        /// <param name="result"></param>
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

        /// <summary>
        /// Function that can generate a DNS file structure file with the default DNS entry's.
        /// </summary>
        public void GenerateFileStructure()
        {
            Processor.CreateDNSStructureFile();
        }
    }
}
