using Makaretu.Dns;
using Makaretu.Dns.Resolving;
using System.Net;
using System.Net.Sockets;

//被查询的mDNS主机
namespace mDNS_Discovery_ConsoleApp.Client
{
    internal class Program
    {
        static readonly object ttyLock = new object();

        static void Main(string[] args)
        {

            var mdns = new MulticastService();

            /*  Broadcasting
             *  在加入网络时都会发 IGMP 报文加入组 224.0.0.251
                        //Advertising
                        //Always broadcast the service("foo") running on local host with port 1024.
             */

            var sd = new ServiceDiscovery(mdns);
            sd.Advertise(new ServiceProfile("ipfs1", "_mDNSClientipfs-discovery._udp", 5010));
            sd.Advertise(new ServiceProfile("x1", "_mDNSClientxservice._tcp", 5011));
            sd.Advertise(new ServiceProfile("x2", "_mDNSClientxservice._tcp", 666));
            var z1 = new ServiceProfile("z1", "_mDNSClientzservice._udp", 5012);
            z1.AddProperty("foo", "bar");
            sd.Advertise(z1);



            /* Respond to a query for the service
            //
            //Respond to a query for the service. Note that ServiceDiscovery.Advertise is much easier.
            所有收到请求的设备都会检查他们是否对应请求中的主机名。如果一个设备发现它对应了请求中的主机名，它就会响应请求，回复它的IP地址。
            */

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

            mdns.QueryReceived += QueryReceived;

            mdns.Start();

            Console.WriteLine("Hello, World!");

            Console.ReadKey();

        }


        /// <summary>
        /// 收到mDNS查询请求
        /// https://github.com/richardschneider/net-mdns/blob/b9f2f8158052568a19d09536179ceaf5cae9b23e/traffic/Program.cs#L12
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void QueryReceived(object sender, MessageEventArgs e)
        {


            lock (ttyLock)
            {
                var names = e.Message.Questions
                   .Select(q => q.Name + " " + q.Type);
                Console.WriteLine($"Query Received for {String.Join(", ", names)} \n");

                Console.WriteLine("detail === {0:O} ===", DateTime.Now);
                Console.WriteLine(e.Message.ToString());


            }
        }
    }
}

