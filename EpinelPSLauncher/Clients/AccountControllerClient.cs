using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EpinelPSLauncher.Models;
using EpinelPSLauncher.Utils;

namespace EpinelPSLauncher.Clients
{
    public class AccountControllerClient : BaseClient
    {
        public static AccountControllerClient Instance { get; } = new();

        public override string BaseUrl => "li-sg.intlgame.com";

        public async Task<ClientResult<LoginEndpoint2Res>> LoginAsync(string username, string password)
        {
            // POST https://li-sg.intlgame.com/account/login HTTP/1.1 application/json;charset=UTF-8 
            LoginEndpoint2Req request = new()
            {
                account = username,
                account_type = 1,
                device_info = GenerateDeviceInfo(),
                extra_json = "", // TODO weird encrypted thing
                password = CreateMD5(password)
            };

            return await DoRequest("/account/login?account_plat_type=131&app_id=09af79d65d6e4fdf2d2569f0d365739d&lang_type=en&os=5",
                request, SourceGenerationContext.Default.LoginEndpoint2Req,
                SourceGenerationContext.Default.LoginEndpoint2Res, false);
        }

        public async Task<ClientResult<AccountGetUserInfoResponse>> GetUserInfo(AccountEntry entry)
        {
            SimpleRequestAccount request = new()
            {
                uid = entry.Uid,
                token = entry.Token
            };

            return await DoRequest("/account/getuserinfo?account_plat_type=131&app_id=09af79d65d6e4fdf2d2569f0d365739d&lang_type=en&os=5",
              request, SourceGenerationContext.Default.SimpleRequestAccount,
              SourceGenerationContext.Default.AccountGetUserInfoResponse, false);
        }
    }
}
