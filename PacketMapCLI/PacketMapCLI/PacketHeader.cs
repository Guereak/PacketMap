using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                if (header[14] == 69 || header[14] == 70)        //IPv4 handeled packets
                {
                    protocol = DetermineProtocol(header[23]);
                    if (protocol == "Unknown")
                    {
                        Console.WriteLine(header[23]);
                        Console.WriteLine("^^ THAT");
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
                else if (header[14] == 96)      //IPv6 Packets
                {
                    //Console.WriteLine("IPv6 Packet (not yet implemented)");
                    ipType = IpType.IPv6;
                }
                else if (header[14] == 0)       //ARP Packets
                {
                    //Console.WriteLine("ARP Packet");
                    Console.Write("");
                }
                else
                {
                    Console.WriteLine(hexHeader);
                    Console.WriteLine("Unknown Packet header: ^^");
                    //Ethernet packets?
                    //Find a way to intercept Ethernet packets I guess
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
            {
                //Class A private IP
                scope = Scope.Private;
            }
            else if (addr[0] == 192 && addr[1] == 168)
            {
                //Class C private IP
                scope = Scope.Private;
            }
            else if (addr[0] == 172 && addr[1] >= 16 && addr[1] <= 31)
            {
                //Class B private IP
                scope = Scope.Private;
            }
            else if(addr[0] >= 224 && addr[0] <= 239)
            {
                //IP Multicast (no idea what that is)
                scope = Scope.Private;
            }
            else
            {
                //Source ip is likely public
                //Does not work for public local adress: fix
                scope = Scope.Public;
            }
            return scope;
        }

    }
}
