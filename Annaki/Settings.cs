﻿using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SplatNet2.Net.Api.Data.Battles.Gears;
using SplatNet2.Net.Api.Network.Data;

namespace Annaki
{
    public class Settings
    {

        [JsonProperty("bot_token")]
        public string BotToken { get; set; }

        [JsonProperty("twitch_client_id")]
        public string TwitchClientId { get; set; }

        [JsonProperty("notification_id")] 
        public ulong StreamNotificationChannelId { get; set; }

        [JsonProperty("splatnet_channel_id")]
        public ulong SplatNetGearChannelId { get; set; }

        [JsonProperty("twitch_token")]
        public string TwitchSecret { get; set; }

        [JsonProperty("prefix")]
        public string Prefix { get; set; }

        [JsonProperty("users")]
        public Dictionary<ulong, UserSettings> Users { get; set; }

        public void SaveSettings()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);

            File.WriteAllText(Path.Combine(Globals.AppPath, "Data", "settings.json"), json);
        }
    }

    public class UserSettings
    {
        [JsonProperty("iksm_cookie")]
        public SplatnetCookie Cookie { get; set; }

        [JsonProperty("read_battle_numbers")]
        public int[] ReadBattleNumbers { get; set; }

        [JsonProperty("watched_headgear")]
        public List<Headgear> WatchedHeadgear { get; set; }

        [JsonProperty("watched_clothing")]
        public List<Clothing> WatchedClothing { get; set; }

        [JsonProperty("watched_shoes")]
        public List<Shoes> WatchedShoes { get; set; }

        [JsonProperty("notified")]
        public bool Notified { get; set; }
    }
}
