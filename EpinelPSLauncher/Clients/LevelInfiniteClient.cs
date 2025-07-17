using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using EpinelPSLauncher.Models;
using EpinelPSLauncher.Utils;

namespace EpinelPSLauncher.Clients
{
    public class LevelInfiniteClient : BaseClient
    {
        public static LevelInfiniteClient Instance { get; } =
        new();

        public override string BaseUrl => "aws-na.intlgame.com";


        public async Task<ClientResult<LoginEndpoint1Res>> AuthenticateAsync(AccountEntry entry)
        {
            LoginEndpoint1Req request = new()
            {
                channel_dis = "Windows",
                login_extra_info = "{}",
                lang_type = "en",
                device_info = GenerateDeviceInfo(),
                channel_info = new()
                {
                    openid = entry.Uid,
                    token = entry.Token,
                    account_type = 1,
                    account = entry.Email,
                    account_plat_type = 131,
                    lang_type = "en",
                    is_login = true,
                    account_uid = entry.Uid,
                    account_token = entry.Token,
                }
            };

            return await DoRequest("/v2/auth/login?channelid=131&gameid=29080&os=5",
                request, SourceGenerationContext.Default.LoginEndpoint1Req,
                SourceGenerationContext.Default.LoginEndpoint1Res, true);
        }

        public async Task<ClientResult<GetMinorcerStatusRes>> GetMinorcerStatus(AccountEntry entry)
        {
            GetMinorcerStatusReq request = new()
            {
                openid = entry.Uid
            };

            return await DoRequest("/v2/minorcer/get_status?channelid=131&gameid=29080&os=5",
                request, SourceGenerationContext.Default.GetMinorcerStatusReq,
                SourceGenerationContext.Default.GetMinorcerStatusRes, true);
        }

        public async Task<ClientResult<GetProfileResponse>> GetUserInfo()
        {
            SimpleRequest request = new()
            {
                openid = SessionData.AuthResponse.openid,
                token = SessionData.AuthResponse.token
            };

            return await DoRequest("/v2/profile/userinfo?channelid=131&gameid=29080&os=5",
                request, SourceGenerationContext.Default.SimpleRequest,
                SourceGenerationContext.Default.GetProfileResponse, true);
        }
    }
}
