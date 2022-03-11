using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketMapCLI
{
    public class StreamInfo
    {
        public string sourceAddrstr;
        public string destAddrstr;
        public enum IpType { IPv4, IPv6, Ethernet };
        public IpType ipType = IpType.Ethernet;
        public enum Direction { Incoming, Outcoming, Other }
        public Direction direction = Direction.Other;

        public string companyName;
        public string companyDomain;
        public string country;
        public string city;
        public double latitude;
        public double longitude;
        public string protocol;


        public StreamInfo FetchPacketInfo(PacketHeader packetHeader)
        {
            StreamInfo streamInfo = new StreamInfo();
            sourceAddrstr = packetHeader.sourceAddrstr;
            destAddrstr = packetHeader.destAddrstr;

            string tempIpType = packetHeader.ipType.ToString();
            if (tempIpType == "IPv4")
                ipType = IpType.IPv4;
            else if (tempIpType == "IPv6")
                ipType = IpType.IPv6;

            string tempDirection = packetHeader.direction.ToString();
            if (tempDirection == "Incoming")
                direction = Direction.Incoming;
            else if (tempDirection == "Outcoming")
                direction = Direction.Outcoming;

            //DOES NOT FETCH InfoExtractor stuff;

            return streamInfo;
        }
    }
}
