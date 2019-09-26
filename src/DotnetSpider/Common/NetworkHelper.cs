using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace DotnetSpider.Common
{
    public static class NetworkHelper
    {
        public static string CurrentIpAddress()
        {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(i => i.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                    i.NetworkInterfaceType == NetworkInterfaceType.Wireless80211);
            var unicastAddresses = networkInterfaces
                .Select(i => i.GetIPProperties())
                .SelectMany(p => p.UnicastAddresses);
            var addressInfo = unicastAddresses
                .Where(a => a.IPv4Mask.ToString() != "255.255.255.255" &&
                    a.Address.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(a.Address))
                .FirstOrDefault();
            var ipAddress = addressInfo?.Address.ToString() ?? "127.0.0.1";
            return ipAddress;
        }
    }
}