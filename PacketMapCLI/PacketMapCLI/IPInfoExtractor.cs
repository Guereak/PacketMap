using System;
using SharpPcap;
using System.IO;
using System.Net;
using GsonReader;
using System.Collections.Generic;

namespace PacketMapCLI
{
    class IPInfoExtractor
    {
        public string companyName = "";
        public string companyDomain = "";
        public string country = "";
        public string city = "";
        public string latitude = "";
        public string longitude = "";

        public IPInfoExtractor(string ipAddr)
        {
            string fileName = @"../../../../apiKey.txt";
            string key = File.ReadAllText(fileName).Trim();

            Console.WriteLine(ipAddr);
            WebClient wc = new WebClient();
            string webData = wc.DownloadString($"https://api.ipregistry.co/{ipAddr}?key={key}");

            Gson mainData = new Gson(webData);
            Gson companyInfo = new Gson(mainData.subCategory["connection"]);
            Gson locationInfo = new Gson(mainData.subCategory["location"]);
            Gson countryInfo = new Gson(locationInfo.subCategory["country"]);

            companyName = companyInfo.subCategory["organization"];
            companyDomain = companyInfo.subCategory["domain"];
            country = countryInfo.subCategory["name"];
            city = locationInfo.subCategory["city"];
            latitude = locationInfo.subCategory["latitude"];
            longitude = locationInfo.subCategory["longitude"];
        }
    }
}
