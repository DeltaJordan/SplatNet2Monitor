using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SplatNet2.Net.Api.Data.Battles;
using SplatNet2.Net.Api.Data.Battles.Gears;

namespace SplatNet2.Net.Api.Data
{
    public static class BattleJsonReader
    {
        public static SplatoonBattle ParseBattle(string scoreboardJson)
        {
            JObject scoreboardJObject;

            try
            {
                scoreboardJObject = JObject.Parse(scoreboardJson);
            }
            catch (Exception e)
            {
                // TODO Proper Library Exception Handling
                Console.WriteLine(e);
                throw;
            }

            return ParseBattle(scoreboardJObject);
        }

        public static SplatoonBattle ParseBattle(JObject scoreboardJson)
        {
            // Repeated variables.
            int battleNumber = scoreboardJson["battle_number"].Value<int>();

            // Used primarily to check whether this is league, ranked, turf war, or splatfest
            LobbyType mode = scoreboardJson["type"].Value<string>() switch
            {
                "regular" => LobbyType.Regular,
                "gachi" => LobbyType.Gachi,
                "league" => LobbyType.League,
                "fes" => LobbyType.Splatfest,
                _ => throw new ArgumentOutOfRangeException()
            };

            SplatoonBattle splatoonBattle = new SplatoonBattle();

            switch (scoreboardJson["game_mode"]["key"].Value<string>())
            {
                case "regular":
                    splatoonBattle.Lobby = Lobby.TurfWar;
                    splatoonBattle.LobbyGroup = LobbyGroup.Regular;
                    break;
                case "gachi":
                    splatoonBattle.Lobby = Lobby.Ranked;
                    splatoonBattle.LobbyGroup = LobbyGroup.Gachi;
                    break;
                case "league_pair":
                    splatoonBattle.Lobby = Lobby.LeaguePair;
                    splatoonBattle.LobbyGroup = LobbyGroup.Gachi;
                    break;
                case "league_team":
                    splatoonBattle.Lobby = Lobby.LeagueTeam;
                    splatoonBattle.LobbyGroup = LobbyGroup.Gachi;
                    break;
                case "private":
                    splatoonBattle.Lobby = Lobby.Private;
                    splatoonBattle.LobbyGroup = LobbyGroup.Private;
                    break;
                case "fest_solo":
                    splatoonBattle.Lobby = Lobby.SplatfestNormal;
                    splatoonBattle.LobbyGroup = LobbyGroup.Splatfest;
                    break;
                case "fest_team":
                    splatoonBattle.Lobby = Lobby.SplatfestPro;
                    splatoonBattle.LobbyGroup = LobbyGroup.Splatfest;
                    break;
                default:
                    splatoonBattle.Lobby = (Lobby) (-1);
                    splatoonBattle.LobbyGroup = (LobbyGroup) (-1);
                    break;
            }

            splatoonBattle.GameMode = scoreboardJson["rule"]["key"].Value<string>() switch
            {
                "turf_war" => GameMode.TurfWar,
                "splat_zones" => GameMode.SplatZones,
                "tower_control" => GameMode.TowerControl,
                "rainmaker" => GameMode.Rainmaker,
                "clam_blitz" => GameMode.ClamBlitz,
                _ => throw new ArgumentOutOfRangeException()
            };

            splatoonBattle.SplatNetId = scoreboardJson["player_result"]["player"]["principal_id"].Value<string>();
            splatoonBattle.MyName = scoreboardJson["player_result"]["player"]["nickname"].Value<string>();
            splatoonBattle.Stage = (Stage)scoreboardJson["stage"]["id"].Value<int>();
            splatoonBattle.MyWeapon = (Weapon)scoreboardJson["player_result"]["player"]["weapon"]["id"].Value<int>();
            splatoonBattle.Result = scoreboardJson["my_team_result"]["key"].Value<string>() == "victory"
                ? BattleResult.Victory
                : BattleResult.Defeat;

            splatoonBattle.LobbyType = mode;

            try
            {
                splatoonBattle.MyTeamPercentage = scoreboardJson["my_team_percentage"].Value<decimal?>();
                splatoonBattle.OtherTeamPercentage = scoreboardJson["other_team_percentage"].Value<decimal?>();
            }
            catch
            {
                // Pass
            }

            try
            {
                splatoonBattle.MyTeamCount = scoreboardJson["my_team_count"].Value<byte>();
                splatoonBattle.OtherTeamCount = scoreboardJson["other_team_count"].Value<byte>();

                splatoonBattle.Knockout = splatoonBattle.MyTeamCount == 100 || splatoonBattle.OtherTeamCount == 100;
            }
            catch
            {
                // Pass
            }

            splatoonBattle.MyTurfInked = scoreboardJson["player_result"]["game_paint_point"].Value<int>();

            if (splatoonBattle.GameMode == GameMode.TurfWar && splatoonBattle.Result == BattleResult.Victory)
                splatoonBattle.MyTurfInked += 1000;

            splatoonBattle.Kills = scoreboardJson["player_result"]["kill_count"].Value<int>();
            splatoonBattle.Assists = scoreboardJson["player_result"]["assist_count"].Value<int>();
            splatoonBattle.KA = splatoonBattle.Kills + splatoonBattle.Assists;
            splatoonBattle.Specials = scoreboardJson["player_result"]["special_count"].Value<int>();
            splatoonBattle.Deaths = scoreboardJson["player_result"]["death_count"].Value<int>();

            splatoonBattle.LevelBefore = scoreboardJson["player_result"]["player"]["player_rank"].Value<int>();
            splatoonBattle.LevelAfter = scoreboardJson["player_rank"].Value<int>();
            splatoonBattle.StarRank = scoreboardJson["star_rank"].Value<int>();

            try
            {
                splatoonBattle.RankBefore = scoreboardJson["udemae"]["name"].Value<string>();
                splatoonBattle.RankAfter = scoreboardJson["player_result"]["player"]["udemae"]["name"].Value<string>();
                splatoonBattle.SPlusNumberBefore = scoreboardJson["udemae"]["s_plus_number"].Value<int?>();
                splatoonBattle.SPlusNumberAfter = scoreboardJson["player_result"]["player"]["udemae"]["s_plus_number"].Value<int?>();
            }
            catch
            {
                // Pass
            }

            try
            {
                splatoonBattle.IsXRank = scoreboardJson["udemae"]["is_x"].Value<bool>();

                if (splatoonBattle.IsXRank)
                {
                    splatoonBattle.XPower = scoreboardJson["x_power"].Value<decimal?>();

                    if (mode == LobbyType.Gachi)
                    {
                        splatoonBattle.LobbyPower = scoreboardJson["estimate_x_power"].Value<decimal?>();
                    }
                }
            }
            catch
            {
                // Pass
            }

            // Amount of time the battle lasted in seconds
            int elapsedBattleTime;

            try
            {
                elapsedBattleTime = scoreboardJson["elapsed_time"].Value<int>();
            }
            catch
            {
                elapsedBattleTime = 180;
            }

            splatoonBattle.StartTime = DateTime.UnixEpoch.AddSeconds(scoreboardJson["start_time"].Value<double>());
            splatoonBattle.EndTime = splatoonBattle.StartTime.AddSeconds(elapsedBattleTime);

            splatoonBattle.PrivateNote = $"Battle #{battleNumber}";
            splatoonBattle.BattleNumber = battleNumber;

            if (mode == LobbyType.League)
            {
                splatoonBattle.MyTeamId = scoreboardJson["tag_id"].Value<string>();
                splatoonBattle.LeaguePoints = scoreboardJson["league_point"].Value<decimal?>();
                splatoonBattle.MyTeamEstimatedPoints = scoreboardJson["my_estimate_league_point"].Value<decimal?>();
                splatoonBattle.OtherTeamEstimatedPoints = scoreboardJson["other_estimate_league_point"].Value<decimal?>();
            }

            if (mode == LobbyType.Gachi)
            {
                splatoonBattle.EstimateGachiPower = scoreboardJson["estimate_gachi_power"].Value<decimal?>();
            }

            if (mode == LobbyType.Regular)
            {
                splatoonBattle.Freshness = scoreboardJson["win_meter"].Value<decimal?>();
            }

            splatoonBattle.Gender =
                scoreboardJson["player_result"]["player"]["player_type"]["style"].Value<string>() == "girl"
                    ? Gender.Female
                    : Gender.Male;

            splatoonBattle.Species =
                scoreboardJson["player_result"]["player"]["player_type"]["species"].Value<string>() == "inklings"
                    ? Species.Inkling
                    : Species.Octoling;

            List<SplatoonPlayer> myTeamPlayers = new List<SplatoonPlayer>
            {
                new SplatoonPlayer
                {
                    Deaths = splatoonBattle.Deaths,
                    Gender = splatoonBattle.Gender,
                    IsMe = true,
                    KA = splatoonBattle.KA,
                    Kills = splatoonBattle.Kills,
                    Level = splatoonBattle.LevelBefore,
                    MyTeam = true,
                    Name = splatoonBattle.MyName,
                    Points = splatoonBattle.MyTurfInked,
                    Specials = splatoonBattle.Specials,
                    Weapon = splatoonBattle.MyWeapon,
                    StarRank = splatoonBattle.StarRank,
                    Species = splatoonBattle.Species,
                    SplatNetId = splatoonBattle.SplatNetId,
                    SortScore = scoreboardJson["player_result"]["sort_score"].Value<decimal>(),
                    Gear = new Gear
                    {
                        Headgear = new Headgear
                        {
                            BrandName = scoreboardJson["player_result"]["player"]["head"]["brand"]["name"].Value<string>(),
                            HeadgearId = scoreboardJson["player_result"]["player"]["head"]["id"].Value<int>(),
                            Name = scoreboardJson["player_result"]["player"]["head"]["name"].Value<string>(),
                            MainAbility = (GearEnums.Ability) scoreboardJson["player_result"]["player"]["head_skills"]["main"]["id"].Value<int>(),
                            SecondaryAbilities = ParseSubAbilities(scoreboardJson["player_result"]["player"]["head_skills"]["subs"].Children())
                        },
                        Clothing = new Clothing
                        {
                            BrandName = scoreboardJson["player_result"]["player"]["clothes"]["brand"]["name"].Value<string>(),
                            ClothingId = scoreboardJson["player_result"]["player"]["clothes"]["id"].Value<int>(),
                            Name = scoreboardJson["player_result"]["player"]["clothes"]["name"].Value<string>(),
                            MainAbility = (GearEnums.Ability) scoreboardJson["player_result"]["player"]["clothes_skills"]["main"]["id"].Value<int>(),
                            SecondaryAbilities = ParseSubAbilities(scoreboardJson["player_result"]["player"]["clothes_skills"]["subs"].Children())
                        },
                        Shoes = new Shoes
                        {
                            BrandName = scoreboardJson["player_result"]["player"]["shoes"]["brand"]["name"].Value<string>(),
                            ShoeId = scoreboardJson["player_result"]["player"]["shoes"]["id"].Value<int>(),
                            Name = scoreboardJson["player_result"]["player"]["shoes"]["name"].Value<string>(),
                            MainAbility = (GearEnums.Ability) scoreboardJson["player_result"]["player"]["shoes_skills"]["main"]["id"].Value<int>(),
                            SecondaryAbilities = ParseSubAbilities(scoreboardJson["player_result"]["player"]["shoes_skills"]["subs"].Children())
                        }
                    }
                }
            };

            foreach (JToken token in scoreboardJson["my_team_members"].Children())
            {
                myTeamPlayers.Add(ParsePlayer(token, true, splatoonBattle, mode, scoreboardJson));
            }

            IOrderedEnumerable<SplatoonPlayer> initialOrdering = splatoonBattle.GameMode != GameMode.TurfWar
                ? myTeamPlayers.OrderByDescending(x => x.SortScore)
                : myTeamPlayers.OrderByDescending(x => x.Points);

            List<SplatoonPlayer> myTeamOrdered = initialOrdering
                .ThenByDescending(x => x.KA)
                .ThenByDescending(x => x.Specials)
                .ThenByDescending(x => x.Deaths)
                .ThenByDescending(x => x.Kills)
                .ThenByDescending(x => x.Name).ToList();

            for (int n = 0; n < myTeamOrdered.Count; n++)
            {
                if (!myTeamOrdered[n].IsMe) continue;

                splatoonBattle.TeamRank = n + 1;
                break;
            }

            List<SplatoonPlayer> otherTeamPlayers = new List<SplatoonPlayer>();

            foreach (JToken token in scoreboardJson["other_team_members"].Children())
            {
                otherTeamPlayers.Add(ParsePlayer(token, false, splatoonBattle, mode, scoreboardJson));
            }

            initialOrdering = splatoonBattle.GameMode != GameMode.TurfWar
                ? otherTeamPlayers.OrderByDescending(x => x.SortScore)
                : otherTeamPlayers.OrderByDescending(x => x.Points);

            List<SplatoonPlayer> otherTeamOrdered = initialOrdering
                .ThenByDescending(x => x.KA)
                .ThenByDescending(x => x.Specials)
                .ThenByDescending(x => x.Deaths)
                .ThenByDescending(x => x.Kills)
                .ThenByDescending(x => x.Name).ToList();

            List<SplatoonPlayer> allPlayers = myTeamOrdered.Concat(otherTeamOrdered).ToList();

            for (int n = 0; n < allPlayers.Count; n++)
            {
                allPlayers[n].TeamRank = n < 4 ? n + 1 : n - 3;
            }

            splatoonBattle.Players = allPlayers.ToArray();

            return splatoonBattle;
        }

        private static GearEnums.Ability[] ParseSubAbilities(IEnumerable<JToken> tokens)
        {
            List<GearEnums.Ability> abilities = new List<GearEnums.Ability>();

            foreach (JToken token in tokens)
            {
                try
                {
                    abilities.Add((GearEnums.Ability)token["id"].Value<int>());
                }
                catch
                {
                    abilities.Add(GearEnums.Ability.None);
                }
            }

            return abilities.ToArray();
        }

        private static SplatoonPlayer ParsePlayer(JToken token, bool myTeam, SplatoonBattle splatoonBattle, LobbyType mode, JObject scoreboardJObject)
        {
            SplatoonPlayer player = new SplatoonPlayer
            {
                SortScore = token["sort_score"].Value<decimal>(),
                Points = token["game_paint_point"].Value<int>(),
                Kills = token["kill_count"].Value<int>(),
                Specials = token["special_count"].Value<int>(),
                Deaths = token["death_count"].Value<int>(),
                Weapon = (Weapon)token["player"]["weapon"]["id"].Value<int>(),
                Level = token["player"]["player_rank"].Value<int>(),
                MyTeam = myTeam,
                Name = token["player"]["nickname"].Value<string>(),
                SplatNetId = token["player"]["principal_id"].Value<string>(),
                StarRank = token["player"]["star_rank"].Value<int>(),
                Species = token["player"]["player_type"]["species"].Value<string>() == "inklings"
                    ? Species.Inkling
                    : Species.Octoling,
                Gender = token["player"]["player_type"]["style"].Value<string>() == "girl"
                    ? Gender.Female
                    : Gender.Male,
                Gear = new Gear
                {
                    Headgear = new Headgear
                    {
                        BrandName = token["player"]["head"]["brand"]["name"].Value<string>(),
                        HeadgearId = token["player"]["head"]["id"].Value<int>(),
                        Name = token["player"]["head"]["name"].Value<string>(),
                        ImageUrl = $"https://app.splatoon2.nintendo.net/{token["player"]["head"]["image"].Value<string>()}",
                        MainAbility = (GearEnums.Ability)token["player"]["head_skills"]["main"]["id"].Value<int>(),
                        SecondaryAbilities = ParseSubAbilities(token["player"]["head_skills"]["subs"].Children())
                    },
                    Clothing = new Clothing
                    {
                        BrandName = token["player"]["clothes"]["brand"]["name"].Value<string>(),
                        ClothingId = token["player"]["clothes"]["id"].Value<int>(),
                        Name = token["player"]["clothes"]["name"].Value<string>(),
                        ImageUrl = $"https://app.splatoon2.nintendo.net/{token["player"]["clothes"]["image"].Value<string>()}",
                        MainAbility = (GearEnums.Ability)token["player"]["clothes_skills"]["main"]["id"].Value<int>(),
                        SecondaryAbilities = ParseSubAbilities(token["player"]["clothes_skills"]["subs"].Children())
                    },
                    Shoes = new Shoes
                    {
                        BrandName = token["player"]["shoes"]["brand"]["name"].Value<string>(),
                        ShoeId = token["player"]["shoes"]["id"].Value<int>(),
                        Name = token["player"]["shoes"]["name"].Value<string>(),
                        ImageUrl = $"https://app.splatoon2.nintendo.net/{token["player"]["shoes"]["image"].Value<string>()}",
                        MainAbility = (GearEnums.Ability)token["player"]["shoes_skills"]["main"]["id"].Value<int>(),
                        SecondaryAbilities = ParseSubAbilities(token["player"]["shoes_skills"]["subs"].Children())
                    }
                }
            };

            player.KA = player.Kills + token["assist_count"].Value<int>();

            switch (mode)
            {
                case LobbyType.Gachi:
                case LobbyType.League:
                    try
                    {
                        player.Rank = token["player"]["udemae"]["name"].Value<string>();
                    }
                    catch
                    {
                        // Pass
                    }

                    break;
                case LobbyType.Regular:
                case LobbyType.Splatfest:
                    if (splatoonBattle.Result == BattleResult.Victory)
                    {
                        player.Points += 1000;
                    }
                    break;
            }

            try
            {
                if (scoreboardJObject["crown_players"].Type != JTokenType.Null &&
                    scoreboardJObject["crown_players"].Values<string>().Contains(player.SplatNetId))
                {
                    player.Top500 = true;
                }
            }
            catch
            {
                player.Top500 = false;
            }

            return player;
        }
    }
}
