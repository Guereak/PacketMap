using System;
using GsonReader;
using System.Net;
using System.IO;

namespace PacketMap1
{
    class IPInfoExtractor
    {
        public string companyName = "";
        public string companyDomain = "";
        public string country = "";
        public string city = "";
        public float latitude = 0f;
        public float longitude = 0f;

        public IPInfoExtractor(string ipAddr)
        {
            string fileName = @"../../../apiKey.txt";
            string key = File.ReadAllText(fileName).Trim();
            

            WebClient wc = new WebClient();
            string webData = wc.DownloadString($"https://api.ipregistry.co/{ipAddr}?key={key}");
            Console.WriteLine(webData);
            Gson mainData = new Gson(webData);
            /*Gson companyInfo = new Gson(mainData.subCategory["company"]);
            Console.WriteLine(mainData.subCategory["location"]);
            Gson locationInfo = new Gson(mainData.subCategory["location"]);
            companyName = companyInfo.subCategory["organisation"];
            companyDomain = companyInfo.subCategory["domain"];

            Console.WriteLine(companyName, companyInfo);*/
            Console.WriteLine("hey");
        }
    }
}
