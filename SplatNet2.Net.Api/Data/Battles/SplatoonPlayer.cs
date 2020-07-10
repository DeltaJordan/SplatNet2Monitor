using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SplatNet2.Net.Api.Data.Battles
{
    public class SplatoonPlayer
    {
        /// <summary>
        /// References whether this player was on the user's team.
        /// </summary>
        [JsonProperty("team")]
        public bool MyTeam { get; set; }

        /// <summary>
        /// References whether this player was the user.
        /// </summary>
        [JsonProperty("is_me")]
        public bool IsMe { get; set; }

        /// <summary>
        /// References this player's weapon.
        /// </summary>
        [JsonProperty("weapon")]
        public Weapon Weapon { get; set; }

        /// <summary>
        /// References this player's level.
        /// </summary>
        [JsonProperty("level")]
        public int Level { get; set; }

        /// <summary>
        /// Used to order the team. Sort order is sort_score or turf inked (depending on mode), k+a, specials, deaths (more = better), kills, nickname
        /// </summary>
        [JsonProperty("rank_in_team")]
        public int TeamRank { get; set; }

        /// <summary>
        /// References the combined amount of kills and assists the player got in a match.
        /// </summary>
        [JsonProperty("kill_or_assist")]
        public int KA { get; set; }

        /// <summary>
        /// References the amount of kills the player got in a match.
        /// </summary>
        [JsonProperty("kill")]
        public int Kills { get; set; }

        /// <summary>
        /// References the amount of times the player died in a match.
        /// </summary>
        [JsonProperty("death")]
        public int Deaths { get; set; }

        /// <summary>
        /// References the amount of specials the player used in a match.
        /// </summary>
        [JsonProperty("special")]
        public int Specials { get; set; }

        /// <summary>
        /// References the amount of turf inked in a match.
        /// </summary>
        [JsonProperty("point")]
        public int Points { get; set; }

        /// <summary>
        /// References the name of the player.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// References the SplatNet ID of the player.
        /// </summary>
        [JsonProperty("splatnet_id")]
        public string SplatNetId { get; set; }

        /// <summary>
        /// References the star rank, commonly called prestige, of the player.
        /// </summary>
        [JsonProperty("star_rank")]
        public int StarRank { get; set; }

        /// <summary>
        /// References the gender of this player.
        /// </summary>
        [JsonProperty("gender")]
        public Gender Gender { get; set; }

        /// <summary>
        /// References the species of this player.
        /// </summary>
        [JsonProperty("species")]
        public Species Species { get; set; }

        /// <summary>
        /// References the rank of the player.
        /// </summary>
        [JsonProperty("rank")]
        public string Rank { get; set; }

        [JsonProperty("top_500")]
        public bool Top500 { get; set; }

        /// <summary>
        /// Possibly references the Power of the player, or some other score to sort the player based on power.
        /// </summary>
        public decimal SortScore { get; set; }

        /// <summary>
        /// References the gear used by the player.
        /// </summary>
        public Gear Gear { get; set; }
    }
}
