using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SplatNet2.Net.Api.Maths;
using SplatNet2.Net.Api.Network.Data;
using SplatNet2.Net.Api.Network.Extensions;

namespace SplatNet2.Net.Api.Network
{
    public class SplatnetAuthClient
    {
        private readonly RandomGenerator random = new RandomGenerator();

        private static readonly CookieContainer cookies;
        private static readonly LoggingHandler loggingHandler;
        private static readonly HttpClient httpClient;

        private string version = "unknown";

        static SplatnetAuthClient()
        {
            cookies = new CookieContainer();

            loggingHandler = new LoggingHandler(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                CookieContainer = cookies
            });

            httpClient = new HttpClient(loggingHandler);
        }

        public async Task<string> LogIn(string version)
        {
            string authState = Base64UrlEncoder.Encode(this.random.GenerateRandomBytes(36));

            string authCodeVerifier = Base64UrlEncoder.Encode(this.random.GenerateRandomBytes(32)).Replace("=", "");

            using SHA256 hash = SHA256.Create();
            Encoding enc = Encoding.UTF8;
            byte[] hashArray = hash.ComputeHash(enc.GetBytes(authCodeVerifier));

            string authCodeChallenge = Base64UrlEncoder.Encode(hashArray).Replace("=", "");

            httpClient.DefaultRequestHeaders.Clear();

            httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8n");
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate,br");
            httpClient.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
            httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            httpClient.DefaultRequestHeaders.Add("DNT", "1");
            httpClient.DefaultRequestHeaders.Add("Host", "accounts.nintendo.com");
            httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 7.1.2; Pixel Build/NJH47D; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/59.0.3071.125 Mobile Safari/537.36");


            string body = $@"{{
                            ""state"":                              ""{authState}"",
		                    ""redirect_uri"":                        ""npf71b963c1b7b6d119://auth"",
		                    ""client_id"":                           ""71b963c1b7b6d119"",
		                    ""scope"":                               ""openid user user.birthday user.mii user.screenName"",
		                    ""response_type"":                       ""session_token_code"",
		                    ""session_token_code_challenge"":        ""{authCodeChallenge}"",
		                    ""session_token_code_challenge_method"": ""S256"",
		                    ""theme"":                               ""login_form""
                            }}";

            Console.WriteLine(body);

            const string url = "https://accounts.nintendo.com/connect/1.0.0/authorize";

            Uri requestUri = new Uri(url);

            Dictionary<string, string> paramDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);

            foreach (KeyValuePair<string, string> pair in paramDict)
            {
                requestUri = requestUri.AddParameter(pair.Key, pair.Value);
            }

            HttpRequestMessage requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = requestUri
            };

            await httpClient.SendAsync(requestMessage);

            string postLogin = requestUri.AbsoluteUri;

            Console.WriteLine("\nMake sure you have fully read the \"Cookie generation\" section of the readme before proceeding. To manually input a cookie instead, enter \"skip\" at the prompt below.");
            Console.WriteLine("\nNavigate to this URL in your browser:");
            Console.WriteLine(postLogin);
            Console.WriteLine("Log in, right click the \"Select this account\" button, copy the link address, and paste it below:");

            while (true)
            {
                try
                {
                    string useAccountUrl = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(useAccountUrl) || useAccountUrl.ToLower() == "skip")
                        return "skip";
                    string sessionTokenCode = Regex.Match(useAccountUrl, "de=(.*)&").Groups[1].Value;
                    return await this.GetSessionToken(sessionTokenCode, authCodeVerifier);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        public async Task<string> GetSessionToken(string sessionTokenCode, string authCodeVerifier)
        {
            httpClient.DefaultRequestHeaders.Clear();

            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US");
            httpClient.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
            httpClient.DefaultRequestHeaders.Add("Host", "accounts.nintendo.com");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "OnlineLounge/1.9.0 NASDKAPI Android");

            Dictionary<string, string> body = new Dictionary<string, string>
            {
                ["client_id"] = "71b963c1b7b6d119",
                ["session_token_code"] = sessionTokenCode,
                ["session_token_code_verifier"] = authCodeVerifier
            };

            const string url = "https://accounts.nintendo.com/connect/1.0.0/api/session_token";

            HttpRequestMessage requestMessage = new HttpRequestMessage
            {
                Content = new FormUrlEncodedContent(body),
                RequestUri = new Uri(url),
                Method = HttpMethod.Post
            };

            // requestMessage.Content.Headers.ContentLength = 540;

            using HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);

            return JObject.Parse(await responseMessage.Content.ReadAsStringAsync())["session_token"].Value<string>();
        }

        public async Task<SplatnetCookie> GetCookie(string sessionToken, string ver)
        {
            this.version = ver;

            int timestamp = (int) (DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;
            string guid = Guid.NewGuid().ToString();

            httpClient.DefaultRequestHeaders.Clear();

            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US");
            httpClient.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
            httpClient.DefaultRequestHeaders.Add("Host", "accounts.nintendo.com");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "OnlineLounge/1.9.0 NASDKAPI Android");

            dynamic tokenBody = new JObject();
            tokenBody.client_id = "71b963c1b7b6d119";
            tokenBody.session_token = sessionToken;
            tokenBody.grant_type = "urn:ietf:params:oauth:grant-type:jwt-bearer-session-token";

            const string tokenUrl = "https://accounts.nintendo.com/connect/1.0.0/api/token";

            HttpRequestMessage tokenRequestMessage = new HttpRequestMessage
            {
                Content = new StringContent(tokenBody.ToString(), Encoding.UTF8, "application/json"),
                RequestUri = new Uri(tokenUrl),
                Method = HttpMethod.Post
            };

            HttpResponseMessage tokenResponseMessage = await httpClient.SendAsync(tokenRequestMessage);

            JObject tokenJsonResponse = JObject.Parse(await tokenResponseMessage.Content.ReadAsStringAsync());

            try
            {
                httpClient.DefaultRequestHeaders.Clear();

                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
                httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US");
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenJsonResponse["access_token"].Value<string>()}");
                httpClient.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
                httpClient.DefaultRequestHeaders.Add("Host", "api.accounts.nintendo.com");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "OnlineLounge/1.9.0 NASDKAPI Android");
            }
            catch
            {
                Console.WriteLine("Not a valid authorization request. Please delete config.txt and try again.");
                Console.WriteLine("Error from Nintendo (in api/token step):");
                Console.WriteLine(tokenJsonResponse.ToString(Formatting.Indented));
                Console.WriteLine("Press any key to exit...");
                Environment.Exit(1);
            }

            const string userInfoUrl = "https://api.accounts.nintendo.com/2.0.0/users/me";

            HttpResponseMessage userInfoResponseMessage = await httpClient.GetAsync(userInfoUrl);

            JObject userInfoJson = JObject.Parse(await userInfoResponseMessage.Content.ReadAsStringAsync());

            string nickname = userInfoJson["nickname"].Value<string>();

            dynamic parameter;

            try
            {
                string idToken = tokenJsonResponse["access_token"].Value<string>();

                JObject flapGJObject = await this.CallFlapGApi(idToken, guid, timestamp, "nso");

                parameter = new JObject();
                parameter.f = flapGJObject["f"];
                parameter.naIdToken = flapGJObject["p1"];
                parameter.timestamp = flapGJObject["p2"];
                parameter.requestId = flapGJObject["p3"];
                parameter.naCountry = userInfoJson["country"];
                parameter.naBirthday = userInfoJson["birthday"];
                parameter.language = userInfoJson["language"];
            }
            catch
            {
                Console.WriteLine("Error(s) from Nintendo:");
                Console.WriteLine(tokenJsonResponse.ToString(Formatting.Indented));
                Console.WriteLine(userInfoJson.ToString(Formatting.Indented));
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);

                throw;
            }

            httpClient.DefaultRequestHeaders.Clear();

            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer");
            httpClient.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
            httpClient.DefaultRequestHeaders.Add("Host", "api-lp1.znc.srv.nintendo.net");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "com.nintendo.znca/1.9.0 (Android/7.1.2)");
            httpClient.DefaultRequestHeaders.Add("X-Platform", "Android");
            httpClient.DefaultRequestHeaders.Add("X-ProductVersion", "1.9.0");

            dynamic splatoonTokenBody = new JObject();
            splatoonTokenBody.parameter = parameter;

            const string splatoonTokenUrl = "https://api-lp1.znc.srv.nintendo.net/v1/Account/Login";

            HttpResponseMessage splatoonTokenResponseMessage = await httpClient.PostAsync(splatoonTokenUrl,
                new StringContent(splatoonTokenBody.ToString(), Encoding.UTF8, "application/json"));

            JObject splatoonTokenJObject = JObject.Parse(await splatoonTokenResponseMessage.Content.ReadAsStringAsync());

            JObject flapGApp;

            try
            {
                string idToken = splatoonTokenJObject["result"]["webApiServerCredential"]["accessToken"]
                    .Value<string>();

                flapGApp = await this.CallFlapGApi(idToken, guid, timestamp, "app");
            }
            catch
            {
                Console.WriteLine("Error from Nintendo (in Account/Login step):");
                Console.WriteLine(splatoonTokenJObject.ToString(Formatting.Indented));
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);

                throw;
            }

            httpClient.DefaultRequestHeaders.Clear();

            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {splatoonTokenJObject["result"]["webApiServerCredential"]["accessToken"].Value<string>()}");
            httpClient.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
            httpClient.DefaultRequestHeaders.Add("Host", "api-lp1.znc.srv.nintendo.net");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "com.nintendo.znca/1.9.0 (Android/7.1.2)");
            httpClient.DefaultRequestHeaders.Add("X-Platform", "Android");
            httpClient.DefaultRequestHeaders.Add("X-ProductVersion", "1.9.0");

            dynamic splatoonAccessTokenBody = new JObject();
            parameter = new JObject();

            parameter.id = 5741031244955648;
            parameter.f = flapGApp["f"];
            parameter.registrationToken = flapGApp["p1"];
            parameter.timestamp = flapGApp["p2"];
            parameter.requestId = flapGApp["p3"];

            splatoonAccessTokenBody.parameter = parameter;

            const string splatoonAccessTokenUrl = "https://api-lp1.znc.srv.nintendo.net/v2/Game/GetWebServiceToken";

            HttpResponseMessage splatoonAccessTokenResponseMessage = await httpClient.PostAsync(splatoonAccessTokenUrl,
                new StringContent(splatoonAccessTokenBody.ToString(), Encoding.UTF8, "application/json"));

            JObject splatoonAccessTokenJObject = JObject.Parse(await splatoonAccessTokenResponseMessage.Content.ReadAsStringAsync());

            try
            {
                httpClient.DefaultRequestHeaders.Clear();

                httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate");
                httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US");
                httpClient.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
                httpClient.DefaultRequestHeaders.Add("DNT", "0");
                httpClient.DefaultRequestHeaders.Add("Host", "app.splatoon2.nintendo.net");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 7.1.2; Pixel Build/NJH47D; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/59.0.3071.125 Mobile Safari/537.36");
                httpClient.DefaultRequestHeaders.Add("X-GameWebToken", splatoonAccessTokenJObject["result"]["accessToken"].Value<string>());
                httpClient.DefaultRequestHeaders.Add("X-IsAppAnalyticsOptedIn", "false");
                httpClient.DefaultRequestHeaders.Add("X-IsAnalyticsOptedIn", "false");
                httpClient.DefaultRequestHeaders.Add("X-Requested-With", "com.nintendo.znca");
            }
            catch
            {
                Console.WriteLine("Error from Nintendo (in Game/GetWebServiceToken step):");
                Console.WriteLine(splatoonAccessTokenJObject.ToString(Formatting.Indented)); 
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);

                throw;
            }

            const string cookieUrl = "https://app.splatoon2.nintendo.net/?lang=en-US";

            await httpClient.GetAsync(cookieUrl);

            Cookie cookie = cookies.GetCookies(new Uri(cookieUrl))["iksm_session"];

            return new SplatnetCookie
            {
                Nickname = nickname,
                Cookie = cookie
            };
        }

        public async Task<string> GetHashFromS2SApi(string idToken, int timestamp)
        {
            const int errors = 0;

            if (errors >= 5)
            {
                Console.WriteLine("Too many errors received from the splatnet2statink API. Further requests have been blocked until the \"api_errors\" line is manually removed from config.json.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);
                Environment.Exit(1);
            }

            try
            {
                httpClient.DefaultRequestHeaders.Clear();

                httpClient.DefaultRequestHeaders.Add("User-Agent", $"splatnet2statink/{this.version}");

                Dictionary<string, string> apiBodyDictionary = new Dictionary<string, string>
                {
                    {"naIdToken", idToken },
                    {"timestamp", timestamp.ToString()}
                };

                const string url = "https://elifessler.com/s2s/api/gen2";

                HttpRequestMessage requestMessage = new HttpRequestMessage
                {
                    Content = new FormUrlEncodedContent(apiBodyDictionary),
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(url)
                };

                HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);

                return JObject.Parse(await responseMessage.Content.ReadAsStringAsync())["hash"].Value<string>();
            }
            catch
            {
                Console.WriteLine("Error from the splatnet2statink API.");

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);

                throw;
            }
        }

        public async Task<JObject> CallFlapGApi(string idToken, string guid, int timestamp, string type)
        {
            try
            {
                string hash = await this.GetHashFromS2SApi(idToken, timestamp);

                httpClient.DefaultRequestHeaders.Clear();

                httpClient.DefaultRequestHeaders.Add("x-guid", guid);
                httpClient.DefaultRequestHeaders.Add("x-hash", hash);
                httpClient.DefaultRequestHeaders.Add("x-iid", type);
                httpClient.DefaultRequestHeaders.Add("x-time", timestamp.ToString());
                httpClient.DefaultRequestHeaders.Add("x-token", idToken);
                httpClient.DefaultRequestHeaders.Add("x-ver", "3");

                HttpResponseMessage responseMessage =
                    await httpClient.GetAsync("https://flapg.com/ika2/api/login?public");

                return JObject.Parse(await responseMessage.Content.ReadAsStringAsync())["result"].Value<JObject>();
            }
            catch
            {
                Console.WriteLine("Error from the flapg API.");

                throw;
            }
        }
    }
}
