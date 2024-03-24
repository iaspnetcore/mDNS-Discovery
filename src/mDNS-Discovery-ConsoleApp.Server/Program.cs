using Makaretu.Dns;
using System.Net;
using System.Net.Sockets;

// ptr aaa etc:https://github.com/karlredgate/mDNS-sharp/blob/master/mDNS.cs

// server 发起查询的主机   Start MDNS server.  Find Device 

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
        /// https://github.com/oddbear/Loupedeck.KeyLight.Plugin/blob/0981a12e4c5aba5bc2efec7e29f185df559b4b7a/KeyLightPlugin/KeyLightPlugin.cs#L36
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

            //https://www.cnblogs.com/xueyk/articles/mDNS.html
            try
            {
                //查看应答信息
                if (e.Message.AdditionalRecords.Count > 0)
                {
                    foreach (ResourceRecord rr in e.Message.AdditionalRecords)
                    {
                        Console.WriteLine($"DomainName:{rr.Name},Canonical:{rr.CanonicalName},Type:{rr.Type.ToString()}\r\n");
                        string resultStr = rr.ToString();
                        byte[] byteArray = rr.GetData();
                        Console.WriteLine($":{resultStr},[DataLength]{byteArray.Length}\r\n");
                    }
                }

                if (e.Message.Answers.Count > 0)
                {
                    Console.WriteLine($"\r\n***************Answers*****************\r\n");
                    foreach (ResourceRecord rr in e.Message.Answers)
                    {
                        Console.WriteLine($":{rr.ToString()}\r\n");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


        }


        /// <summary>
        /// https://github.com/oddbear/Loupedeck.KeyLight.Plugin/blob/0981a12e4c5aba5bc2efec7e29f185df559b4b7a/KeyLightPlugin/KeyLightPlugin.cs#L36
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MulticastServiceOnAnswerReceived(object sender, MessageEventArgs e)
        {
            try
            {
                if (e.Message.Answers.All(resourceRecord => resourceRecord.CanonicalName != "_elg._tcp.local"))
                    return;

                var address = e.RemoteEndPoint.Address.ToString();
                if (e.RemoteEndPoint.Address.AddressFamily == AddressFamily.InterNetworkV6)
                    address = $"[{address}]"; //[...%10]:1234, the [] is needed to be allowed to specify a port (IPv6 contains ':' chars), and '%' is to scope to a interface number.

                var dnsName = e.Message.AdditionalRecords?
                    .First(resourceRecord => resourceRecord.Type == DnsType.TXT)
                    .Name
                    .Labels
                    .First();

                if (string.IsNullOrWhiteSpace(dnsName))
                    return;

                ////Light found, check if it's a new one or existing one:
                //if (!KeyLights.TryGetValue(dnsName, out var keyLight))
                //    KeyLights[dnsName] = keyLight = new DiscoveredKeyLight { Id = dnsName };

                ////Updates address:
                //keyLight.Address = address;

                ////Updates states:
                //var cancellationToken = CancellationToken.None;

                ////Update lights state, and raise lights updated events:
                //keyLight.Lights = KeyLightService.GetLights(address, cancellationToken);
                //LightsUpdated(dnsName, keyLight.Lights);

                //keyLight.AccessoryInfo = KeyLightService.GetAccessoryInfo(address, cancellationToken);
                //keyLight.Settings = KeyLightService.GetLightsSettings(address, cancellationToken);

                //AdditionalRecords:
                //A: IPv4 address record
                //AAAA: IPv6 address record
                //SRV: Service locator
                //TXT: Text record
                //NSEC (A, AAAA): Next Secure record
                //NSEC (TXT, SRV): Next Secure record
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
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

