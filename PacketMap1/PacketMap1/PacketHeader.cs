using System;

namespace PacketMap1
{
    class PacketHeader
    {
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
            if(totalLength < 34)
            {
                throw new InvalidHeaderException();
            }
            else
            {
                hexHeader = ByteToHex(header, true);
                if(header[14] == 69)
                {
                    protocol = DetermineProtocol(header[23]);
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
                }
                else
                {
                    if (header[14] == 96)
                        Console.WriteLine("IPv6 Packet (not yet implemented)");
                    else
                        Console.WriteLine(hexHeader);
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

    }
}
