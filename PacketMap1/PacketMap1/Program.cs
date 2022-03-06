using System;
using SharpPcap;

namespace PacketMap1
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

                // Prints the time and length of each received packet
                DateTime time = packet.Timeval.Date;

                //Console.WriteLine($"{time.Hour}:{time.Minute}:{time.Second},{time.Millisecond}   Source: {packetHeader.sourceAddrstr}  Destination: {packetHeader.destAddrstr}  Protocol: {packetHeader.protocol}");
            }

            // Print out the device statistics
            Console.WriteLine(device.Statistics.ToString());

            Console.WriteLine("-- Timeout elapsed, capture stopped, device closed.");
            Console.Write("Hit 'Enter' to exit...");
            Console.ReadLine();
        }
    }
}   