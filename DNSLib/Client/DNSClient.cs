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
        private UdpClient Client;
        private readonly string Address;
        public DNSClient(string address)
        {           
            Client = new UdpClient();
            Address = address;
        }

        private void Connect()
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(Address), 51);
            if(Client.Client == null) Client = new UdpClient();
            Client.Connect(ipep);
        }

        private void Disconnect()
        {
            Client.Close();
        }

        public string RetrieveIpAddress(string DomainName)
        {
            Connect();
            DNSPacket packet = new DNSPacket
            {
                Type = DNSPacket.PacketType.Request,
                Lookup = DNSPacket.LookupType.Name,
                Data = Encoding.ASCII.GetBytes(DomainName)
            };

            packet.Length = packet.Data.Length;
            Client.Send(DNSPacket.PacketToRaw(packet), packet.Length + 3);

            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(Address), 51);
            byte[] answer = Client.Receive(ref ipep);

            string responseData;
            if (answer.Length != 0 && ipep.Address.ToString().Equals(Address))
            {
                //checks if the response came from the DNS server
                DNSPacket response = DNSPacket.RawToPacket(answer);
                if (response.Type == DNSPacket.PacketType.Response && response.Lookup == DNSPacket.LookupType.Name)
                {
                    responseData = Encoding.ASCII.GetString(response.Data, 0, response.Length);
                }
                else
                {
                    throw new DNSException(response, "Expected a Response and Name type, got something else.");
                }
            }
            else
            {
                throw new DNSException(null, "Received empty response or the response came from the wrong host.");
            }
            Disconnect();
            return responseData;
        }

        public string RetrieveDNSName(string ipadd)
        {
            Connect();
            DNSPacket packet = new DNSPacket
            {
                Type = DNSPacket.PacketType.Request,
                Lookup = DNSPacket.LookupType.IP,
                Data = Encoding.ASCII.GetBytes(ipadd)
            };

            packet.Length = packet.Data.Length;

            byte[] raw = DNSPacket.PacketToRaw(packet);

            Client.Send(raw, packet.Length + 3);

            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(Address), 51);
            byte[] answer = Client.Receive(ref ipep);

            string responseData;
            if (answer.Length != 0 && ipep.Address.ToString().Equals(Address))
            {
                //checks if the response came from the DNS server
                DNSPacket response = DNSPacket.RawToPacket(answer);
                if (response.Type == DNSPacket.PacketType.Response && response.Lookup == DNSPacket.LookupType.IP)
                {
                    responseData = Encoding.ASCII.GetString(response.Data, 0, response.Length);
                }
                else
                {
                    throw new DNSException(response, "Expected a Response and Name type, got something else.");
                }
            }
            else
            {
                throw new DNSException(null, "Received empty response or the response came from the wrong host.");
            }
            Disconnect();
            return responseData;
        }
    }
}
