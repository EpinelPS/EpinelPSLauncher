using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPSLauncher.Models
{
    public record AccountEntry(string? ServerIP, string Email, string Token, string Uid, long TokenExpires)
    {
        public string FormattedName
        {
            get => ServerIP == null ? "Official Server" : "EpinelPS - " + ServerIP;
        }
    };
}
