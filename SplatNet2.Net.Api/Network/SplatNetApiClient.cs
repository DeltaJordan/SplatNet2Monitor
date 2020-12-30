using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SplatNet2.Net.Api.Data;
using SplatNet2.Net.Api.Data.Battles;
using SplatNet2.Net.Api.Data.Battles.Gears;
using SplatNet2.Net.Api.Exceptions;

namespace SplatNet2.Net.Api.Network
{
    public class SplatNetApiClient
    {
        private HttpClient httpClient;
        private CookieContainer cookies;

        private Cookie iksmCookie;

        private void InitializeHttpClient()
        {
            this.httpClient?.Dispose();

            this.cookies = new CookieContainer();

            this.httpClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                CookieContainer = this.cookies
            });

            this.httpClient.DefaultRequestHeaders.Clear();

            this.httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            this.httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate");
            this.httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US");
            this.httpClient.DefaultRequestHeaders.Add("Host", "app.splatoon2.nintendo.net");
            this.httpClient.DefaultRequestHeaders.Add("Referer", "https://app.splatoon2.nintendo.net/home");
            this.httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 7.1.2; Pixel Build/NJH47D; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/59.0.3071.125 Mobile Safari/537.36");
            this.httpClient.DefaultRequestHeaders.Add("x-requested-with", "XMLHttpRequest");
            this.httpClient.DefaultRequestHeaders.Add("x-unique-id", "15132846168746164214");
            this.httpClient.DefaultRequestHeaders.Add("x-timezone-offset", ((int)TimeZoneInfo.Local.BaseUtcOffset.TotalSeconds).ToString());
        }

        /// <summary>
        /// Initializes the HttpClient and applies the authentication cookie to the cookie collection.
        /// </summary>
        /// <param name="cookie"></param>
        public void ApplyIksmCookie(Cookie cookie)
        {
            this.InitializeHttpClient();

            this.iksmCookie = cookie;

            this.cookies.Add(cookie);
        }

        public async Task<string[]> RetrieveBattles(params int[] ignoredBattleNumbers)
        {
            if (this.iksmCookie == null)
            {
                throw new NullReferenceException("IKSM Cookie has not been set.");
            }

            JObject battleJson = await this.RetrieveBattleJson();

            JEnumerable<JToken> tokens;
            try
            {
                tokens = battleJson["results"].Children();
            }
            catch
            {
                throw new ExpiredCookieException("Unable to authenticate with provided cookie. Please refresh the cookie before continuing.", null);
            }

            List<string> splatoonBattles = new List<string>();

            foreach (JToken token in tokens)
            {
                int battleNumber = token["battle_number"].Value<int>();

                if (ignoredBattleNumbers.Contains(battleNumber))
                    continue;

                splatoonBattles.Add((await this.RetrieveScoreboardJson(battleNumber)).ToString(Formatting.Indented));
            }

            return splatoonBattles.ToArray();
        }

        private async Task<JObject> RetrieveScoreboardJson(int battleNumber)
        {
            string url = $"https://app.splatoon2.nintendo.net/api/results/{battleNumber}";

            HttpResponseMessage responseMessage = await this.httpClient.GetAsync(url);

            return JObject.Parse(await responseMessage.Content.ReadAsStringAsync());
        }

        private async Task<JObject> RetrieveBattleJson()
        {
            const string url = "https://app.splatoon2.nintendo.net/api/results";

            HttpResponseMessage responseMessage = await this.httpClient.GetAsync(url);

            return JObject.Parse(await responseMessage.Content.ReadAsStringAsync());
        }

        private async Task<JObject> RetrieveMerchJson()
        {
            const string url = "https://app.splatoon2.nintendo.net/api/onlineshop/merchandises";

            HttpResponseMessage responseMessage = await this.httpClient.GetAsync(url);

            return JObject.Parse(await responseMessage.Content.ReadAsStringAsync());
        }
    }
}
