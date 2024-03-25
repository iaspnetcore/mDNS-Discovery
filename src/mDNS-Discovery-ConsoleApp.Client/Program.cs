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


            #region Advertising
            //https://github.com/richardschneider/net-mdns

            var mdns = new MulticastService();

            /*  Advertise
             *  在加入网络时都会发 IGMP 报文加入组 224.0.0.251
                        //Advertising
                        //Always broadcast the service("foo") running on local host with port 1024.

            https://iotespresso.com/how-to-discover-esp32-service-over-mdns/
            We will essentially make an ESP32 tell the other devices on the network: 
            “Hey, I’m <name>. My IP address is <ip>. I provide the following services: <service_name> + <protocol (tcp/udp)>. Let’s connect”.

            ServiceProfile =>(instanceName,  serviceName, ushort port)


            2.we broadcast a service offering to anyone who discovers this device.
             */

            var sd = new ServiceDiscovery(mdns);
            sd.Advertise(new ServiceProfile("ipfs1", "_mDNSClientipfs-discovery._udp", 5010));
            sd.Advertise(new ServiceProfile("x1", "_mDNSClientxservice._tcp", 5011));
            sd.Advertise(new ServiceProfile("_mDNS_Discovery_ConsoleApp_Client", "_mDNS_Discovery_ConsoleApp_Client._tcp", 666));
            var z1 = new ServiceProfile("z1", "_mDNSClientzservice._udp", 5012);
            z1.AddProperty("foo", "bar");
            // 开启广播
            sd.Advertise(z1);

            #endregion


            /* Respond to a query for the service
            //
            //Respond to a query for the service. Note that ServiceDiscovery.Advertise is much easier.
            所有收到请求的设备都会检查他们是否对应请求中的主机名。如果一个设备发现它对应了请求中的主机名，它就会响应请求，回复它的IP地址。
            */

            var service1 = "_mDNSClientipfs-discovery._udp";

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

            //接收所有的广播，代码如下：
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

