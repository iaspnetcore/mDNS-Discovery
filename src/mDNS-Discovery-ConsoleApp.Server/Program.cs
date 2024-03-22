using Makaretu.Dns;
using System.Net;
using System.Net.Sockets;

// ptr aaa etc:https://github.com/karlredgate/mDNS-sharp/blob/master/mDNS.cs

// server 发起查询的主机   Start MDNS server.
namespace mDNS_Discovery_ConsoleApp.Server
{
    internal class Program
    {
        static readonly object ttyLock = new object();
        static void Main(string[] args)
        {

            var mdns = new MulticastService();

            foreach (var a in MulticastService.GetIPAddresses())
            {
                Console.WriteLine($"Program.cs ->IP address {a}");
            }

            //   Find all services running on the local link.

            var sd1 = new ServiceDiscovery();
            sd1.ServiceDiscovered += (s, serviceName) =>
            {
                // Do something

                Console.WriteLine($"all services running on the local link {serviceName} \n");

            };

            //Find all service instances running on the local link.
            //https://github.com/richardschneider/net-mdns
            sd1.ServiceInstanceDiscovered += (s, e) =>
            {
                //if (e.Message.Answers.All(w => !w.Name.ToString().Contains("ipfs1"))) return;
                Console.WriteLine($"Find all service instances running on the local link '{e.ServiceInstanceName}'");

                // Ask for the service instance details.
                mdns.SendQuery(e.ServiceInstanceName, type: DnsType.SRV);
            };



          

            mdns.AnswerReceived += (s, e) =>
            {
                var names = e.Message.Answers
                    .Select(q => q.Name + " " + q.Type)
                    .Distinct();
                Console.WriteLine($"got answer for {String.Join(", ", names)} \n");
            };

            mdns.QueryReceived += AnswerReceived;


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

            MulticastService_GetIPAddresses();

            Console.ReadKey();

        }

        /// <summary>
        /// 收到mDNS查询请求后的回答
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void AnswerReceived(object sender, MessageEventArgs e)
        {


            lock (ttyLock)
            {
                var names = e.Message.Answers
                   .Select(q => q.Name + " " + q.Type);
                Console.WriteLine($"Answer Received for {String.Join(", ", names)} \n");

                Console.WriteLine("detail === {0:O} ===", DateTime.Now);
                Console.WriteLine(e.Message.ToString());


            }
        }


        #region test MulticastService

        /// <summary>
        /// https://github.com/SteeBono/airplayreceiver/blob/806fd39ef263a2b38bdd7c8e636a9fd804a94c4e/AirPlay/AirPlayReceiver.cs#L66
        /// </summary>
        public static void MulticastService_GetIPAddresses()
        {
            foreach (var ip in MulticastService.GetIPAddresses())
            {
                Console.WriteLine($"MulticastService_GetIPAddresses() ->IP address {ip}");
            }
        }

        #endregion

    }
}

