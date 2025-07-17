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
    public abstract class BaseClient
    {
        public abstract string BaseUrl { get; }

        private readonly HttpClient client;
        private string? domainIP;

        public BaseClient()
        {
            // ignore SSL errors
            var handler = new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    }
            };

            client = new(handler);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "/*");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "identity");
        }


        protected async Task<string> SendRequest(string url, string content, bool includeSdkAndSource)
        {
            long ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (includeSdkAndSource)
                url += "&sdk_version=1.24.00.873";

            url += $"&seq=29080-3fb18098-0d92-4fad-8c98-8063a121acb2-{ts}-10";

            if (includeSdkAndSource)
                url += "&source=&ts=" + ts;
            url += "&sig=" + GenerateSig(url, content, includeSdkAndSource);

            var hc = new StringContent(content);
            hc.Headers.ContentType = new("application/json", "UTF-8");

            var response = await client.PostAsync($"https://" + domainIP + url, hc);

            return await response.Content.ReadAsStringAsync();
        }
        public static string GenerateSig(string url, string content, bool useNormalKey)
        {
            // ACCOUNT_SDK_KEY value in INTLConfig.ini
            string key = "fbdb256e459ddcb8cd2acfe1b62d63ed";

            if (useNormalKey) key = "3d0ef5272bf6bc22fd484c18d96be5c0"; // SDK_KEY value

            //Javascript library key: 0d88135dd851f81f9601e477b261a137

            return CreateMD5(url + content + key);
        }
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = System.Security.Cryptography.MD5.HashData(inputBytes);

            return Convert.ToHexString(hashBytes).ToLower(); // .NET 5 +
        }
        internal async Task ConfigureAsync(bool official, string ip)
        {
            if (official)
            {
                domainIP = await DNS.GetIpAsync(BaseUrl);
            }
            else
            {
                domainIP = ip;
            }

            client.DefaultRequestHeaders.Host = BaseUrl;
        }
        protected static DeviceInfo GenerateDeviceInfo()
        {
            return new()
            {
                lang_type = "en",
                app_version = "0.0.6.566(0.0.6.566)",
                // TODO
            };
        }
        protected async Task<ClientResult<T>> DoRequest<T, G>(string url, G body, JsonTypeInfo<G> bodyType, JsonTypeInfo<T> responseType, bool isLevelInf) where T : IntlApiResponse
        {
            try
            {
                var requestText = JsonSerializer.Serialize(body, bodyType);

                var result = await SendRequest(url, requestText, isLevelInf);

                T? responseObject = JsonSerializer.Deserialize(result, responseType);

                if (responseObject == null)
                {
                    return new(false, "Response is null", null, null);
                }
                else
                {
                    if (responseObject.ret != 0)
                    {
                        return new(false, "Error from server: " + responseObject.msg, null, null);
                    }
                    else
                    {
                        return new(true, "", responseObject, null);
                    }
                }
            }
            catch (Exception ex)
            {
                return new(false, "Exception occured", null, ex);
            }
        }
    }
}
