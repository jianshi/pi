using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Management;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class IPAddressInfo
    {
        public IPAddress IPv4Address { get; set; }
        public IPAddress IPv6Address { get; set; }
        public ManagementObject EnabledNetworkAdapterConfiguration { get; set; }
        public string Caption { get; set; }
        public string MACAddress { get; set; }
    }

    class Program
    {
        private const string SERVER_IP = @"192.168.0.7";
        private const int PORT_NO = 5556;
        static void Main(string[] args)
        {
            List<IPAddressInfo> ipAddressInfoList = ParseIP();

            // If have multiple adaptor, find the one with "Wireless" in description
            Debug.Assert(ipAddressInfoList.Count > 0, "Must have at least one network adaptor!");
            IPAddress ipAddressWireless = ipAddressInfoList[0].IPv4Address;

            foreach (IPAddressInfo ipAddressInfo in ipAddressInfoList)
            {
                if (ipAddressInfo.Caption.IndexOf("wireless", StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    ipAddressWireless = ipAddressInfo.IPv4Address;
                }
            }
            Debug.Assert(ipAddressWireless != null, "Cannot find a wireless adapter");

            TcpListener listener = new TcpListener(ipAddressWireless, PORT_NO);
            Console.WriteLine("Krenuo sa radom...");
            listener.Start();

            while (true)
            {

                TcpClient client = listener.AcceptTcpClient();


                NetworkStream nwStream = client.GetStream();
                byte[] buffer = new byte[client.ReceiveBufferSize];

                bool bTerminateConnection = false;

                do
                {
                    int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);


                    string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    dataReceived = dataReceived.Trim();
                    Console.WriteLine("Primljeno : " + dataReceived);


                    Console.WriteLine("Dobijena poruka na serveru : " + dataReceived);
                    nwStream.Write(buffer, 0, bytesRead);

                    Console.WriteLine("\n");

                    if (string.Compare(dataReceived, "END", true) == 0)
                    {
                        bTerminateConnection = true;
                    }
                } while (!bTerminateConnection);
                client.Close();
            }
            listener.Stop();
        }

        /// <summary>
        /// This seems to return 127.0.0.1 on my work laptop
        /// </summary>
        /// <returns></returns>
        private static string GetLocalIP()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }

        /// <summary>
        /// Running this script will get
        /// ipAddresses
        /// {string[2]}
        ///     [0]: "10.45.40.15"
        ///     [1]: "fe80::c0df:14b4:d950:37fb"
        /// Which is what I get from command line's ipconfig/all
        /// Link-local IPv6 Address . . . . . : fe80::c0df:14b4:d950:37fb%11(Preferred)
        /// IPv4 Address. . . . . . . . . . . : 10.45.40.15(Preferred)
        /// 
        /// </summary>
        private static List<IPAddressInfo> ParseIP()
        {
            List<IPAddressInfo> ipAddressInfoList = new List<IPAddressInfo>();

            using (var mc = new ManagementClass("Win32_NetworkAdapterConfiguration"))
            using (var instances = mc.GetInstances())
            {
                foreach (ManagementObject instance in instances)
                {
                    if (!(bool)instance["ipEnabled"])
                    {
                        continue;
                    }

                    IPAddressInfo ipAddressInfo = new IPAddressInfo();
                    ipAddressInfo.Caption = instance["Caption"].ToString();
                    ipAddressInfo.MACAddress = instance["MACAddress"].ToString();
                    ipAddressInfo.EnabledNetworkAdapterConfiguration = instance;
                    Console.WriteLine("{0}, {1}, {2}", instance["Caption"], instance["ServiceName"], instance["MACAddress"]);

                    string[] ipAddresses = (string[])instance["IPAddress"];
                    foreach (string strIPAddress in ipAddresses)
                    {
                        IPAddress address;
                        if (IPAddress.TryParse(strIPAddress, out address))
                        {
                            switch (address.AddressFamily)
                            {
                                case System.Net.Sockets.AddressFamily.InterNetwork:
                                    {
                                        // we have IPv4
                                        ipAddressInfo.IPv4Address = address;
                                        break;
                                    }
                                case System.Net.Sockets.AddressFamily.InterNetworkV6:
                                    {
                                        // we have IPv6
                                        ipAddressInfo.IPv6Address = address;
                                        break;
                                    }
                                default:
                                    {
                                        // umm... yeah... I'm going to need to take your red packet and...
                                        break;
                                    }
                            }
                        }
                    }

                    string[] subnets = (string[])instance["IPSubnet"];
                    string[] gateways = (string[])instance["DefaultIPGateway"];
                    string domains = (string)instance["DNSDomain"];
                    string description = (string)instance["Description"];
                    bool dhcp = (bool)instance["DHCPEnabled"];
                    string[] dnses = (string[])instance["DNSServerSearchOrder"];

                    ipAddressInfoList.Add(ipAddressInfo);
                }
            }

            return ipAddressInfoList;
        }
    }
}
