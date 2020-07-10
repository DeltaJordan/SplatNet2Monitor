using System;
using Newtonsoft.Json;

namespace SplatNet2.Net.Api.Data.Battles
{
    /// <summary>
    /// Base class to hold data from the splatnet 2 api. Derived classes can be serialized into a JSON compatible with stat.ink.
    /// </summary>
    public class SplatoonBattle
    {
        // TODO Add JsonConverters for stat.ink compatibility. Possibly with a method that serializes specifically for stat.ink?
        
        /// <summary>
        /// References the mode this lobby was started from.
        /// </summary>
        [JsonProperty("lobby")]
        public Lobby Lobby { get; set; }

        /// <summary>
        /// References a more generalized type of this lobby. Implemented for stat.ink compatibility.
        /// </summary>
        [JsonProperty("mode")]
        public LobbyGroup LobbyGroup { get; set; }

        /// <summary>
        /// References another grouping of this lobby. Separates "League" from "Gachi". 
        /// </summary>
        public LobbyType LobbyType { get; set; }

        /// <summary>
        /// References the principal id of the user. 
        /// </summary>
        public string SplatNetId { get; set; }

        /// <summary>
        /// References the game mode of the lobby.
        /// </summary>
        [JsonProperty("rule")]
        public GameMode GameMode { get; set; }

        /// <summary>
        /// References the stage this battle was played on.
        /// </summary>
        [JsonProperty("stage")]
        public Stage Stage { get; set; }

        /// <summary>
        /// References the weapon the player used.
        /// </summary>
        [JsonProperty("weapon")]
        public Weapon MyWeapon { get; set; }

        /// <summary>
        /// References the player's name. 
        /// </summary>
        public string MyName { get; set; }

        /// <summary>
        /// References the result of the battle (win or loss).
        /// </summary>
        [JsonProperty("result")]
        public BattleResult Result { get; set; }

        /// <summary>
        /// References the percentage of ink turfed by the player's team in a Turf War match. Null if not a turf war match.
        /// </summary>
        [JsonProperty("my_team_percent")]
        public decimal? MyTeamPercentage { get; set; }

        /// <summary>
        /// References the percentage of ink turfed by the enemy's team in a Turf War match. Null if not a turf war match.
        /// </summary>
        [JsonProperty("his_team_percent")]
        public decimal? OtherTeamPercentage { get; set; }

        /// <summary>
        /// References whether the game ended in a knockout. True if so, false if not.
        /// </summary>
        [JsonProperty("knock_out")]
        public bool? Knockout { get; set; }

        /// <summary>
        /// References the amount of turf the player inked in the match, plus a win bonus of 1000 if the match was a turf war.
        /// </summary>
        [JsonProperty("my_point")]
        public int MyTurfInked { get; set; }

        /// <summary>
        /// References the amount of kills the player got in a match.
        /// </summary>
        [JsonProperty("kill")]
        public int Kills { get; set; }

        /// <summary>
        /// References the amount of assists the player got in a match. This property will be not be serialized by default.
        /// </summary>
        public int Assists { get; set; }

        /// <summary>
        /// References the combined amount of kills and assists the player got in a match.
        /// </summary>
        [JsonProperty("kill_or_assist")]
        public int KA { get; set; }

        /// <summary>
        /// References the amount of specials used by the player in a match.
        /// </summary>
        [JsonProperty("special")]
        public int Specials { get; set; }

        /// <summary>
        /// References the amount of times the player died in a match
        /// </summary>
        [JsonProperty("death")]
        public int Deaths { get; set; }

        /// <summary>
        /// References the level of the player before the match
        /// </summary>
        [JsonProperty("level")]
        public int LevelBefore { get; set; }

        /// <summary>
        /// References the level of the player after the match
        /// </summary>
        [JsonProperty("level_after")]
        public int LevelAfter { get; set; }

        /// <summary>
        /// References the star rank, commonly called prestige, of the player.
        /// </summary>
        [JsonProperty("star_rank")]
        public int StarRank { get; set; }

        /// <summary>
        /// References the rank after the match.
        /// </summary>
        [JsonProperty("rank_after")]
        public string RankAfter { get; set; }

        /// <summary>
        /// References the rank before the match
        /// </summary>
        [JsonProperty("rank")]
        public string RankBefore { get; set; }

        /// <summary>
        /// References the S+ Rank before the match. Null if not S+.
        /// </summary>
        [JsonProperty("rank_exp_after")]
        public int? SPlusNumberBefore { get; set; }

        /// <summary>
        /// References the S+ Rank after the match. Null if not S+.
        /// </summary>
        [JsonProperty("rank_exp")]
        public int? SPlusNumberAfter { get; set; }

        /// <summary>
        /// References whether this player is X Rank. 
        /// </summary>
        public bool IsXRank { get; set; }

        /// <summary>
        /// References the X Power after the match.
        /// Note that the API does not provide information for before a match,
        /// and as such this must be calculated manually.
        /// </summary>
        [JsonProperty("x_power_after")]
        public decimal? XPower { get; set; }

        /// <summary>
        /// References the Nintendo provided estimate of the X Power of the team.
        /// </summary>
        [JsonProperty("estimate_x_power")]
        public decimal? LobbyPower { get; set; }

        /// <summary>
        /// References the rank of the player worldwide. Null if the rank is below a certain number (which has not definitively been found).
        /// </summary>
        [JsonProperty("worldwide_rank")]
        public int? WorldwideRank { get; set; }

        /// <summary>
        /// References the start time of the match.
        /// </summary>
        [JsonProperty("start_at")]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// References when this match ended.
        /// </summary>
        [JsonProperty("end_at")]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// References the battle number formatted as a string. Mainly used for stat.ink.
        /// </summary>
        [JsonProperty("private_note")]
        public string PrivateNote { get; set; }

        /// <summary>
        /// References the battle number provided by SplatNet 2.
        /// </summary>
        [JsonProperty("splatnet_number")]
        public int BattleNumber { get; set; }
        /// <summary>
        /// References the objective count of the player's team in a Ranked battle.
        /// </summary>
        [JsonProperty("my_team_count")]
        public byte MyTeamCount { get; set; }

        /// <summary>
        /// References the objective count of the enemy's team in a Ranked battle.
        /// </summary>
        [JsonProperty("his_team_count")]
        public byte OtherTeamCount { get; set; }

        /// <summary>
        /// References the player's team id.
        /// </summary>
        [JsonProperty("my_team_id")]
        public string MyTeamId { get; set; }

        /// <summary>
        /// Unsure of what this property references. Possibly the average between both team's powers?
        /// </summary>
        [JsonProperty("league_point")]
        public decimal? LeaguePoints { get; set; }

        /// <summary>
        /// References the estimated power of the current player's league team.
        /// </summary>
        [JsonProperty("my_team_estimate_league_point")]
        public decimal? MyTeamEstimatedPoints { get; set; }

        /// <summary>
        /// References the estimated power of the other team.
        /// </summary>
        [JsonProperty("his_team_estimate_league_point")]
        public decimal? OtherTeamEstimatedPoints { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("estimate_gachi_power")]
        public decimal? EstimateGachiPower { get; set; }

        /// <summary>
        /// References the freshness value of the weapon used in the battle. Unused in non-turf battles.
        /// </summary>
        [JsonProperty("freshness")]
        public decimal? Freshness { get; set; }

        /// <summary>
        /// References the player character's gender.
        /// </summary>
        [JsonProperty("gender")]
        public Gender Gender { get; set; }

        /// <summary>
        /// References the player character's species.
        /// </summary>
        [JsonProperty("species")]
        public Species Species { get; set; }

        /// <summary>
        /// Used to order the team. Sort order is sort_score or turf inked (depending on mode), k+a, specials, deaths (more = better), kills, nickname
        /// </summary>
        [JsonProperty("rank_in_team")]
        public int TeamRank { get; set; }

        // TODO Splatfest

        /// <summary>
        /// References the players in the lobby.
        /// </summary>
        [JsonProperty("players")]
        public SplatoonPlayer[] Players { get; set; }
    }
}
