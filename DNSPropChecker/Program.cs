using System;
using System.IO;
using System.Linq;
using System.Net;
using DnsClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;

// DNS Propagation Check

namespace DNSPropChecker
{
    class Program
    {
        static void Main(string[] args)
        {

            // Begin load app settings
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();
            // End load app settings

            // Preparing DNS List
            var dnsList = config.GetSection("dns").GetChildren().ToList();

            Console.WriteLine("Number of DNS Checking: " + dnsList.Count + " server(s)");

            // Type your domain name
            Console.WriteLine("Enter Domain:");

            string domainName = Console.ReadLine();

            // Type Query Type: A or CNAME
            Console.WriteLine("Enter Query Type [A or CNAME]:");

            string queryType = Console.ReadLine();            

            // Process
            foreach (var dns in dnsList)
            {
                Console.WriteLine("Provider: [" + dns.GetSection("Country").Value + "]" + dns.GetSection("Name").Value +  " = " + dns.GetSection("IP").Value);

                IPAddress ipaddress = IPAddress.Parse(dns.GetSection("IP").Value);

                var lookup = new LookupClient(ipaddress);
                lookup.Timeout = TimeSpan.FromSeconds(5);

                if (queryType.Equals("A"))
                {
                    try
                    {
                        var result = lookup.Query(domainName, QueryType.A);
                        var records = result.Answers.ARecords().ToList();
                        if(records.Count > 0)
                        {
                            foreach (var record in records)
                            {
                                var ip = record?.Address;
                                Console.WriteLine("  Answer IP: " + ip);
                            }
                        }
                        else
                        {
                            Console.WriteLine("  Not Answer");
                        }                        
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("  Answer IP: error");
                    }
                }
                else if (queryType.Equals("CNAME"))
                {
                    try
                    {
                        var result = lookup.Query(domainName, QueryType.CNAME);
                        var records = result.Answers.CnameRecords().ToList();
                        if (records.Count > 0)
                        {
                            foreach (var record in records)
                            {
                                var cn = record?.CanonicalName;
                                Console.WriteLine("  Answer Canonical Name: " + cn);
                            }
                        }
                        else
                        {
                            Console.WriteLine("  Not Answer");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("  Answer IP: error");
                    }
                }
            }
        }
    }
}
