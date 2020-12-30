using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SplatNet2.Net.Api.Data;
using SplatNet2.Net.Api.Data.Battles;

namespace Annaki.Data.Processors
{
    public static class BattleCacheProcessor
    {
        public static string[] GetModeArray(GameMode gameMode, ulong userId)
        {
            string userBattleDir = Path.Combine(Globals.AppPath, "Battles", userId.ToString());

            if (!Directory.Exists(userBattleDir))
            {
                return Array.Empty<string>();
            }

            string cacheFolder = Directory.CreateDirectory(Path.Combine(Globals.AppPath, "Cache", "Battle")).FullName;
            string cacheFile = Path.Combine(cacheFolder, $"{userId}.json");

            BattleCache battleCache = File.Exists(cacheFile)
                ? JsonConvert.DeserializeObject<BattleCache>(File.ReadAllText(cacheFile))
                : new BattleCache
                {
                    ReadBattleNumbers = Array.Empty<int>(),
                    ClamsFormattedArray = Array.Empty<string>(),
                    RainFormattedArray = Array.Empty<string>(),
                    TowerFormattedArray = Array.Empty<string>(),
                    ZonesFormattedArray = Array.Empty<string>()
                };

            List<int> readBattleNumbers = new List<int>();
            readBattleNumbers.AddRange(battleCache.ReadBattleNumbers);

            IEnumerable<SplatoonBattle> newBattles = Directory.EnumerateFiles(userBattleDir)
                .Where(unsortedBattle => !readBattleNumbers.Contains(int.Parse(Path.GetFileNameWithoutExtension(unsortedBattle)[8..])))
                .Select(file => BattleJsonReader.ParseBattle(File.ReadAllText(file))).ToList();

            readBattleNumbers.AddRange(newBattles.Select(x => x.BattleNumber));
            battleCache.ReadBattleNumbers = readBattleNumbers.ToArray();

            var dataLines = newBattles
                .Where(x => x.LobbyType == LobbyType.Gachi || x.Lobby == Lobby.Ranked)
                .Where(x => x.XPower.HasValue && x.XPower > 1650)
                .GroupBy(x => x.GameMode,
                    x => x, 
                    (key, g) => new {GameMode = key, Battles = g.ToList()});

            foreach (var groupedBattles in dataLines)
            {
                switch (groupedBattles.GameMode)
                {
                    case GameMode.TurfWar:
                        break;
                    case GameMode.SplatZones:
                        List<string> zonesList = new List<string>();
                        zonesList.AddRange(battleCache.ZonesFormattedArray);
                        zonesList.AddRange(groupedBattles.Battles.Select(battle =>
                            $"[new Date({battle.EndTime.ToUniversalTime().Subtract(DateTime.UnixEpoch).TotalMilliseconds}), {battle.XPower}, {battle.LobbyPower}]"));
                        battleCache.ZonesFormattedArray = zonesList.ToArray();
                        break;
                    case GameMode.TowerControl:
                        List<string> towerList = new List<string>();
                        towerList.AddRange(battleCache.TowerFormattedArray);
                        towerList.AddRange(groupedBattles.Battles.Select(battle =>
                            $"[new Date({battle.EndTime.ToUniversalTime().Subtract(DateTime.UnixEpoch).TotalMilliseconds}), {battle.XPower}, {battle.LobbyPower}]"));
                        battleCache.TowerFormattedArray = towerList.ToArray();
                        break;
                    case GameMode.Rainmaker:
                        List<string> rainList = new List<string>();
                        rainList.AddRange(battleCache.RainFormattedArray);
                        rainList.AddRange(groupedBattles.Battles.Select(battle =>
                            $"[new Date({battle.EndTime.ToUniversalTime().Subtract(DateTime.UnixEpoch).TotalMilliseconds}), {battle.XPower}, {battle.LobbyPower}]"));
                        battleCache.RainFormattedArray = rainList.ToArray();
                        break;
                    case GameMode.ClamBlitz:
                        List<string> clamList = new List<string>();
                        clamList.AddRange(battleCache.ClamsFormattedArray);
                        clamList.AddRange(groupedBattles.Battles.Select(battle =>
                            $"[new Date({battle.EndTime.ToUniversalTime().Subtract(DateTime.UnixEpoch).TotalMilliseconds}), {battle.XPower}, {battle.LobbyPower}]"));
                        battleCache.ClamsFormattedArray = clamList.ToArray();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            File.WriteAllText(cacheFile, JsonConvert.SerializeObject(battleCache));

            return gameMode switch
            {
                GameMode.TurfWar => Array.Empty<string>(),
                GameMode.SplatZones => battleCache.ZonesFormattedArray,
                GameMode.TowerControl => battleCache.TowerFormattedArray,
                GameMode.Rainmaker => battleCache.RainFormattedArray,
                GameMode.ClamBlitz => battleCache.ClamsFormattedArray,
                _ => throw new ArgumentOutOfRangeException(nameof(gameMode), gameMode, null)
            };
        }
    }
}
