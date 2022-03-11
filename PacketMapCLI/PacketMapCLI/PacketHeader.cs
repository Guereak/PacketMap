using System;
using System.IO;

namespace PacketMapCLI
{
    public class PacketHeader
    {
        public enum IpType { IPv4, IPv6, Ethernet};
        public IpType ipType = IpType.Ethernet;
        public enum Direction { Incoming, Outcoming, Other}
        public Direction direction = Direction.Other;
        public enum Scope { Private, Public }
        public Scope source;
        public Scope destination;


        public string hexHeader = "";
        public int totalLength = 0;     //Packet Length
        public Byte[] sourceAddr = new Byte[4];
        public Byte[] destAddr = new Byte[4];
        public string sourceAddrstr = "";
        public string destAddrstr = "";
        public string protocol = "";

        public PacketHeader(Byte[] header)
        {
            totalLength = header.Length;
            if (totalLength < 34)
            {
                throw new Exception("Invalid Header Exception: too short");
            }
            else
            {
                hexHeader = ByteToHex(header, true);
                if (header[12] == 8 && header[13] == 0)        //IPv4 handeled packets
                {
                    protocol = DetermineProtocol(header[23]);
                    if (protocol == "Unknown")
                    {
                        using(StreamWriter sw = File.AppendText(@"../../debug.log"))
                        {
                            sw.WriteLine("Could not determine the protocol for the following:");
                            sw.WriteLine(header[23]);
                        }
                    }

                    ipType = IpType.IPv4;

                    sourceAddr[0] = header[26];
                    sourceAddr[1] = header[27];
                    sourceAddr[2] = header[28];
                    sourceAddr[3] = header[29];
                    sourceAddrstr = sourceAddr[0].ToString() + "." + sourceAddr[1].ToString() + "." + sourceAddr[2].ToString() + "." + sourceAddr[3].ToString();
                    destAddr[0] = header[30];
                    destAddr[1] = header[31];
                    destAddr[2] = header[32];
                    destAddr[3] = header[33];
                    destAddrstr = destAddr[0].ToString() + "." + destAddr[1].ToString() + "." + destAddr[2].ToString() + "." + destAddr[3].ToString();
                    Console.WriteLine($"source: {sourceAddrstr}, destination: {destAddrstr}, protocol: {protocol}");

                    source = DetermineScopeV4(sourceAddr);
                    destination = DetermineScopeV4(destAddr);

                    if (source == Scope.Public && destination == Scope.Private)
                        direction = Direction.Incoming;
                    else if (source == Scope.Private && destination == Scope.Public)
                        direction = Direction.Outcoming;
                    else if(source == Scope.Public && destination == Scope.Public)
                    {
                        Console.WriteLine("/!\\ Public to public connection: users with a public IP address are not currently supported");
                    }
                    //else: packets with reserved shit

                    Console.WriteLine(direction);
                }
                else if (header[12] == 134 && header[13] == 221)      //IPv6 Packets
                {
                    //IPv6 Packet / MAKE
                    ipType = IpType.IPv6;
                }
                else if(header[12] != 8 && header[12] != 136) using (StreamWriter sw = File.AppendText(@"../../debug.log"))
                {
                    //Are not taken into account ARP and ethernet bullshit
                    sw.WriteLine("Packet is not an IP packet:");
                    sw.WriteLine(hexHeader);
                }

            }
        }

        private string ByteToHex(Byte[] bi, bool spaces)
        {
            string s = "0123456789ABCDEF";
            string ret = "";
            foreach (Byte b in bi)
            {
                int i = Convert.ToInt32(b);
                ret += s[i / 16];
                ret += s[i % 16];
                if (spaces)
                    s += " ";
            }
            return ret;
        }

        private string DetermineProtocol(Byte b)
        {
            int i = Convert.ToInt32(b);
            string s = "";

            switch (i)
            {
                case 1:
                    s = "ICMP";
                    break;
                case 2:
                    s = "IGMP";
                    break;
                case 6:
                    s = "TCP";
                    break;
                case 17:
                    s = "UDP";
                    break;
                default:
                    s = "Unknown";
                    break;
            }

            return s;
        }

        private Scope DetermineScopeV4(Byte[] addr)
        {
            Scope scope;
            //Check if destination IP address is private or public
            if (addr[0] == 10)
                scope = Scope.Private;                //Class A private IP
            else if (addr[0] == 192 && addr[1] == 168)
                scope = Scope.Private;                //Class C private IP
            else if (addr[0] == 172 && addr[1] >= 16 && addr[1] <= 31)
                scope = Scope.Private;                //Class B private IP
            else if (addr[0] >= 224 && addr[0] <= 239)
                scope = Scope.Private;                //IP Multicast (no idea what that is)
            else if (addr[0] >= 240)
                scope = Scope.Private;                //Future use + subnet
            else if (addr[0] == 169 && addr[1] == 254)
                scope = Scope.Private;
            else                                      //Source ip is public
                scope = Scope.Public;                

            return scope;
        }

    }
}
