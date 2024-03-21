using Makaretu.Dns;
using Makaretu.Dns.Resolving;
using System.Net.Sockets;

namespace mDNS_Discovery_ConsoleApp.Clent
{
    internal class Program
    {
        static void Main(string[] args)
        {
           
            var mdns1 = new MulticastService();

            var sd1 = new ServiceDiscovery(mdns1);

            sd1.Advertise(new ServiceProfile("ipfs1", "_ipfs-discovery._udp", 5010));

            //Advertising
            //Always broadcast the service("foo") running on local host with port 1024.
            var service = new ServiceProfile("x", "_foo._tcp", 5353);
            var sd = new ServiceDiscovery();
            sd.Advertise(service);


            //Broadcasting
            //Respond to a query for the service. Note that ServiceDiscovery.Advertise is much easier.

                        var service1 = "...";
            var mdns = new MulticastService();
            mdns.QueryReceived += (s, e) =>
            {
                var msg = e.Message;
                Console.WriteLine($"Hello, World!{msg.Questions[0].Name.ToString()}");

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
        }
    }
}
