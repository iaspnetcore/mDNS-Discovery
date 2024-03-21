using Makaretu.Dns;
using System.Net;
using System.Net.Sockets;

// server 发起查询的主机   Start MDNS server.
namespace mDNS_Discovery_ConsoleApp.Server
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var mdns = new MulticastService();

            foreach (var a in MulticastService.GetIPAddresses())
            {
                Console.WriteLine($"IP address {a}");
            }

            //   Find all services running on the local link.

            var sd1 = new ServiceDiscovery();
            sd1.ServiceDiscovered += (s, serviceName) =>
            {
                // Do something

                Console.WriteLine($"all services running on the local link {serviceName} \n");

            };



            mdns.QueryReceived += (s, e) =>
            {
                var names = e.Message.Questions
                    .Select(q => q.Name + " " + q.Type);
                Console.WriteLine($"got a query for {String.Join(", ", names)} \n");
            };

            mdns.AnswerReceived += (s, e) =>
            {
                var names = e.Message.Answers
                    .Select(q => q.Name + " " + q.Type)
                    .Distinct();
                Console.WriteLine($"got answer for {String.Join(", ", names)} \n");
            };

            mdns.NetworkInterfaceDiscovered += (s, e) =>
            {
                foreach (var nic in e.NetworkInterfaces)
                {
                    Console.WriteLine($"discovered NIC '{nic.Name}\n'");
                }


            };

            mdns.NetworkInterfaceDiscovered += (s, e)
               => mdns.SendQuery(ServiceDiscovery.ServiceName, type: DnsType.PTR);



            mdns.Start();
            Console.ReadKey();

        }
    }
}

