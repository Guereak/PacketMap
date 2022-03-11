using System;
using SharpPcap;
using System.Web;
using System.IO;

namespace PacketMapCLI
{
    class Program
    {
        public static void Main()
        {
            // Print SharpPcap version
            string ver = Pcap.Version;
            Console.WriteLine("SharpPcap {0}, Example4.BasicCapNoCallback.cs", ver);

            ILiveDevice dev = IfaceSelector();
            StartCapture(dev);

            Console.ReadLine();
        }

        public static ILiveDevice IfaceSelector()
        {
            CaptureDeviceList devices = CaptureDeviceList.Instance;

            // If no devices were found print an error
            if (devices.Count < 1)
            {
                Console.WriteLine("No devices were found on this machine");
                return null;
            }

            Console.WriteLine();
            Console.WriteLine("The following devices are available on this machine:");
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine();

            int i = 0;

            // Print out the devices
            foreach (ILiveDevice dev in devices)
            {
                Console.WriteLine("{0}) {1} {2}", i, dev.Name, dev.Description);
                i++;
            }

            Console.WriteLine();
            Console.Write("-- Please choose a device to capture: ");
            i = int.Parse(Console.ReadLine());

            ILiveDevice device = devices[i];
            return device;
        }

        public static void StartCapture(ILiveDevice device)
        {
            // Open the device for capturing
            int readTimeoutMilliseconds = 1000;
            device.Open(DeviceModes.Promiscuous, readTimeoutMilliseconds);

            Console.WriteLine();
            Console.WriteLine("-- Listening on {0}...", device.Description);

            RawCapture packet;

            // Capture packets using GetNextPacket()
            PacketCapture e;

            GetPacketStatus retval;
            while ((retval = device.GetNextPacket(out e)) == GetPacketStatus.PacketRead)
            {
                packet = e.GetPacket();
                PacketHeader packetHeader = new PacketHeader(packet.Data);
                if (packetHeader.protocol == "Unknown")
                    Console.WriteLine(packetHeader.hexHeader);

                ExtractInfo(packetHeader);

                // Prints the time and length of each received packet
                //DateTime time = packet.Timeval.Date;

                //Console.WriteLine($"{time.Hour}:{time.Minute}:{time.Second},{time.Millisecond}   Source: {packetHeader.sourceAddrstr} 
                //Destination: {packetHeader.destAddrstr}  Protocol: {packetHeader.protocol}");
            }

            // Print out the device statistics
            Console.WriteLine(device.Statistics.ToString());

            Console.WriteLine("-- Timeout elapsed, capture stopped, device closed.");
            Console.Write("Hit 'Enter' to exit...");
            Console.ReadLine();
        }

        public static void ExtractInfo(PacketHeader header)
        {
            if(header.direction == PacketHeader.Direction.Incoming)
            {
                int index = -1;

                //Check if IP in database
                bool isKnown = false;
                using (StreamReader sr = File.OpenText(@"../../ipListing.pmap"))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null && isKnown != true)
                    {
                        index++;
                        if (s.Trim() == header.sourceAddrstr)
                            isKnown = true;
                    }
                }

                if(isKnown)
                {
                    using(StreamReader sr = File.OpenText(@"../../ipInfo.pmap"))
                    {
                        int temp = 0;
                        StreamInfo streamInfo = new StreamInfo();
                        Console.WriteLine("From log:");
                        for(int i = 0; i < index; i++)
                        {
                            sr.ReadLine();
                        }
                        string s = sr.ReadLine();

                        string lat = "";
                        string lon = "";
                        foreach(char c in s)
                        {
                            if (c == '|')
                                temp++;
                            else if(temp < 7)
                            {
                                switch (temp)
                                {
                                    case 0:
                                        streamInfo.sourceAddrstr += c;
                                        break;
                                    case 1:
                                        streamInfo.companyName += c;
                                        break;
                                    case 2:
                                        streamInfo.companyDomain += c;
                                        break;
                                    case 3:
                                        streamInfo.country += c;
                                        break;
                                    case 4:
                                        streamInfo.city += c;
                                        break;
                                    case 5:
                                        if (c == '.')
                                            lat += ",";
                                        else
                                            lat += c;
                                        break;
                                    case 6:
                                        if (c == '.')
                                            lon += ',';
                                        else
                                            lon += c;
                                        break;
                                }
                            }
                        }
                        if (lat != "")
                            streamInfo.latitude = Convert.ToDouble(lat);
                        else
                            Console.WriteLine("NULL LATITUDE");
                        if (lon != "")
                            streamInfo.longitude = Convert.ToDouble(lon);
                        else
                            Console.WriteLine("NULL LONGITUDE");

                        streamInfo.FetchPacketInfo(header);
                    }
                }
                else
                {
                    IPInfoExtractor extractor = new IPInfoExtractor(header.sourceAddrstr);

                    using (StreamWriter sw = File.AppendText(@"../../ipListing.pmap"))
                    {
                        sw.WriteLine(header.sourceAddrstr);
                    }
                    using(StreamWriter sw = File.AppendText(@"../../ipInfo.pmap"))
                    {
                        sw.WriteLine($"{header.sourceAddrstr}|{extractor.companyName}|{extractor.companyDomain}|{extractor.country}|{extractor.city}|{extractor.latitude}|{extractor.longitude}|");
                    }

                    Console.WriteLine($"Company name: {extractor.companyName}, domain: {extractor.companyDomain}, country: {extractor.country}, city: {extractor.city}, lat: {extractor.latitude}, long: {extractor.longitude}");
                }
            }
            else if (header.direction == PacketHeader.Direction.Outcoming)
            {

                int index = -1;

                //Check if IP in database
                bool isKnown = false;
                using (StreamReader sr = File.OpenText(@"../../ipListing.pmap"))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null && isKnown != true)
                    {
                        index++;
                        if (s.Trim() == header.destAddrstr)
                            isKnown = true;
                    }
                }

                if (isKnown)
                {
                    using (StreamReader sr = File.OpenText(@"../../ipInfo.pmap"))
                    {
                        int temp = 0;
                        StreamInfo streamInfo = new StreamInfo();
                        Console.WriteLine("From log:");
                        for (int i = 0; i < index; i++)
                        {
                            sr.ReadLine();
                        }
                        string s = sr.ReadLine();

                        string lat = "";
                        string lon = "";
                        foreach (char c in s)
                        {
                            if (c == '|')
                                temp++;
                            else if (temp < 7)
                            {
                                switch (temp)
                                {
                                    case 0:
                                        streamInfo.destAddrstr += c;
                                        break;
                                    case 1:
                                        streamInfo.companyName += c;
                                        break;
                                    case 2:
                                        streamInfo.companyDomain += c;
                                        break;
                                    case 3:
                                        streamInfo.country += c;
                                        break;
                                    case 4:
                                        streamInfo.city += c;
                                        break;
                                    case 5:
                                        if (c == '.')
                                            lat += ",";
                                        else
                                            lat += c;
                                        break;
                                    case 6:
                                        if (c == '.')
                                            lon += ',';
                                        else
                                            lon += c;
                                        break;
                                }
                            }
                        }
                        if (lat != "")
                            streamInfo.latitude = Convert.ToDouble(lat);
                        else
                            Console.WriteLine("NULL LATITUDE");
                        if (lon != "")
                            streamInfo.longitude = Convert.ToDouble(lon);
                        else
                            Console.WriteLine("NULL LONGITUDE");

                        streamInfo.FetchPacketInfo(header);
                    }
                }
                else
                {
                    IPInfoExtractor extractor = new IPInfoExtractor(header.destAddrstr);

                    using (StreamWriter sw = File.AppendText(@"../../ipListing.pmap"))
                    {
                        sw.WriteLine(header.destAddrstr);
                    }
                    using (StreamWriter sw = File.AppendText(@"../../ipInfo.pmap"))
                    {
                        sw.WriteLine($"{header.destAddrstr}|{extractor.companyName}|{extractor.companyDomain}|{extractor.country}|{extractor.city}|{extractor.latitude}|{extractor.longitude}|");
                    }

                    Console.WriteLine($"Company name: {extractor.companyName}, domain: {extractor.companyDomain}, country: {extractor.country}, city: {extractor.city}, lat: {extractor.latitude}, long: {extractor.longitude}");
                }

            }
        }
    }
}
