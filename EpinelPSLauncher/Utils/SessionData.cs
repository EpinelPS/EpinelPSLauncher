using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EpinelPSLauncher.Models;

namespace EpinelPSLauncher.Utils
{
    internal class SessionData
    {
        public static LoginEndpoint1Res AuthResponse = null!;
        public static GetProfileResponse UserProfileResponse = null!;
        public static AccountGetUserInfoResponse UserProfileResponseAccount = null!;
        public static AccountEntry CurrentAccount = null!;
    }
}
