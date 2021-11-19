using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP.DNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MishaNetworkDemoOS
{
    public static class Utils
    {
        public static DnsClient dnsClient = new DnsClient();

        /// <summary>
        /// Gets IP address of a domain. The parameter can be also an IP address
        /// </summary>
        /// <param name="s">A domain or an IP. For example: google.com or 8.8.8.8</param>
        /// <returns>An IP address</returns>
        public static Address GetAddressFromName(string s)
        {
            Address a = null;
            try
            {
                a = Address.Parse(s);
            }
            catch
            {
            }
            if (a != null)
            {
                return a;
            }

            dnsClient.Connect(new Address(8, 8, 8, 8)); //DNS Server address
            /** Send DNS ask for a single domain name **/
            dnsClient.SendAsk(s);
            /** Receive DNS Response **/
            Address destination = dnsClient.Receive(); //can set a timeout value
            return destination;
        }
    }
}
