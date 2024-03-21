using Makaretu.Dns;
using Makaretu.Dns.Resolving;
using System.Net;
using System.Net.Sockets;

namespace mDNS_Discovery_ConsoleApp.Client
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var mdns = new MulticastService();


            //Advertising
            //Always broadcast the service("foo") running on local host with port 1024.



            var sd = new ServiceDiscovery(mdns);
            sd.Advertise(new ServiceProfile("ipfs1", "_mDNSClientipfs-discovery._udp", 5010));
            sd.Advertise(new ServiceProfile("x1", "_mDNSClientxservice._tcp", 5011));
            sd.Advertise(new ServiceProfile("x2", "_mDNSClientxservice._tcp", 666));
            var z1 = new ServiceProfile("z1", "_mDNSClientzservice._udp", 5012);
            z1.AddProperty("foo", "bar");
            sd.Advertise(z1);




            //Broadcasting
            //Respond to a query for the service. Note that ServiceDiscovery.Advertise is much easier.

            var service1 = "...";

            mdns.QueryReceived += (s, e) =>
            {
                var msg = e.Message;
                Console.WriteLine($"{DateTime.Now} Hello, World! Query Received :{msg.Questions[0].Name.ToString()}");

                if (msg.Questions.Any(q => q.Name == service1))
                {
                    var res = msg.CreateResponse();
                    var addresses = MulticastService.GetIPAddresses()
                        .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                    foreach (var address in addresses)
                    {
                        res.Answers.Add(new ARecord
                        {
                            Name = service1,
                            Address = address
                        });
                    }
                    mdns.SendAnswer(res);
                }
            };
            mdns.Start();

            Console.WriteLine("Hello, World!");

            Console.ReadKey();

        }
    }
}

