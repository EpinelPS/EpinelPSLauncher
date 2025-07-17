using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DnsClient;

namespace EpinelPSLauncher.Utils
{
    public static class DNS
    {
        public static async Task<string> GetIpAsync(string query)
        {
            var lookup = new LookupClient();
            var result = await lookup.QueryAsync(query, QueryType.A);

            var record = result.Answers.ARecords().FirstOrDefault();
            var ip = record?.Address ?? throw new Exception($"Failed to find IP address of {query}, check your internet connection.");

            return ip.ToString();
        }
    }
}
