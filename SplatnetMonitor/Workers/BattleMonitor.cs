using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SplatNet2.Net.Api;
using SplatNet2.Net.Api.Data;
using SplatNet2.Net.Api.Data.Battles;
using SplatNet2.Net.Api.Data.Battles.Gears;
using SplatNet2.Net.Api.Network;
using SplatNet2.Net.Api.Network.Data;

namespace SplatNet2.Net.Monitor.Workers
{
    public class BattleMonitor
    {
        public event EventHandler<SplatoonPlayer[]> HeadgearFound;
        public event EventHandler<SplatoonPlayer[]> ClothingFound;
        public event EventHandler<SplatoonPlayer[]> ShoesFound;
        public event EventHandler<Dictionary<int, string>> BattlesRetrieved;
        public event EventHandler<SplatnetCookie> CookieRefreshed;
        public event EventHandler<Exception> CookieExpired;
        public event EventHandler<(bool stopped, Exception exception)> ExceptionOccured;

        private readonly List<Headgear> lookingForHeadGears = new List<Headgear>();
        private readonly List<Clothing> lookingForClothing = new List<Clothing>();
        private readonly List<Shoes> lookingForShoes = new List<Shoes>();

        private const string A_VERSION = "1.5.5";

        private Cookie iksmCookie;

        private readonly List<int> readBattleNumbers = new List<int>();

        private int genericErrorCount;

        public BattleMonitor(params int[] readBattleNumbers)
        {
            if (readBattleNumbers != null)
                this.readBattleNumbers.AddRange(readBattleNumbers);
        }

        public async Task InitializeAsync(SplatnetCookie splatnetCookie)
        {
            this.iksmCookie = splatnetCookie?.Cookie ?? (await this.AuthenticateCookie()).Cookie;

            SplatNetApiClient.ApplyIksmCookie(this.iksmCookie);
        }

        private async Task<SplatnetCookie> AuthenticateCookie()
        {
            using SplatnetAuthClient splatnetClient = new SplatnetAuthClient();

            string sessionToken = await splatnetClient.LogIn(A_VERSION);
            SplatnetCookie splatnetCookie = await splatnetClient.GetCookie(sessionToken, A_VERSION);

            this.CookieRefreshed?.Invoke(this, splatnetCookie);

            return splatnetCookie;
        }

        public void AddWatchedHeadgear(int headgearId, GearEnums.Ability primaryAbility)
        {
            this.lookingForHeadGears.Add(new Headgear {HeadgearId = headgearId, MainAbility = primaryAbility});
        }

        public void RemoveWatchedHeadgear(int headgearId, GearEnums.Ability primaryAbility)
        {
            this.lookingForHeadGears.RemoveAll(x => x.MainAbility == primaryAbility && x.HeadgearId == headgearId);
        }

        public List<Headgear> GetWatchedHeadgear()
        {
            return new List<Headgear>(this.lookingForHeadGears);
        }

        public void AddWatchedClothing(int clothesId, GearEnums.Ability primaryAbility)
        {
            this.lookingForClothing.Add(new Clothing{ClothingId = clothesId, MainAbility = primaryAbility});
        }

        public void RemoveWatchedClothing(int clothesId, GearEnums.Ability primaryAbility)
        {
            this.lookingForClothing.RemoveAll(x => x.ClothingId == clothesId && x.MainAbility == primaryAbility);
        }

        public List<Clothing> GetWatchedClothing()
        {
            return new List<Clothing>(this.lookingForClothing);
        }

        public void AddWatchedShoes(int shoesId, GearEnums.Ability primaryAbility)
        {
            this.lookingForShoes.Add(new Shoes{ShoeId = shoesId, MainAbility = primaryAbility});
        }

        public void RemoveWatchedShoes(int shoesId, GearEnums.Ability primaryAbility)
        {
            this.lookingForShoes.RemoveAll(x => x.ShoeId == shoesId && x.MainAbility == primaryAbility);
        }

        public List<Shoes> GetWatchedShoes()
        {
            return new List<Shoes>(this.lookingForShoes);
        }

        public void ResetErrorCount()
        {
            this.genericErrorCount = 0;
        }

        public async Task BeginMonitor()
        {
            while (true)
            {
                if (this.genericErrorCount >= 3)
                {
                    continue;
                }

                string[] battles;

                try
                {
                    battles = await SplatNetApiClient.RetrieveBattles(this.readBattleNumbers.ToArray());
                }
                catch (AuthenticationException ex)
                {
                    this.CookieExpired?.Invoke(this, ex);

                    Console.WriteLine("Cookie is invalid. Please follow the steps to create a new cookie.");

                    SplatNetApiClient.ApplyIksmCookie((await AuthenticateCookie()).Cookie);

                    battles = await SplatNetApiClient.RetrieveBattles(this.readBattleNumbers.ToArray());
                }
                catch (Exception ex)
                {
                    this.ExceptionOccured?.Invoke(this, (this.genericErrorCount >= 3, ex));

                    this.genericErrorCount++;

                    continue;
                }

                if (!battles.Any())
                    continue;

                string latestBattleJson = null;
                int latestBattleNumber = -1;

                Dictionary<int, string> battleDictionary = new Dictionary<int, string>();

                foreach (string splatoonBattle in battles)
                {
                    JObject splatnetJson = JObject.Parse(splatoonBattle);

                    int battleNumber = splatnetJson["battle_number"].Value<int>();

                    battleDictionary[battleNumber] = splatoonBattle;

                    this.readBattleNumbers.Add(battleNumber);

                    if (latestBattleNumber < battleNumber)
                    {
                        latestBattleNumber = battleNumber;
                        latestBattleJson = splatoonBattle;
                    }
                }

                this.BattlesRetrieved?.Invoke(this, battleDictionary);

                SplatoonBattle latestBattle = await SplatNetApiClient.ParseBattle(latestBattleJson);

                List<SplatoonPlayer> otherPlayers = latestBattle.Players.Where(x => !x.IsMe).ToList();

                foreach (Headgear headgear in this.lookingForHeadGears)
                {
                    SplatoonPlayer[] foundPlayer = otherPlayers
                        .Where(x =>
                            x.Gear.Headgear.MainAbility == headgear.MainAbility &&
                            (headgear.HeadgearId == -1 || x.Gear.Headgear.HeadgearId == headgear.HeadgearId))
                        .ToArray();

                    if (foundPlayer.Any())
                        this.HeadgearFound?.Invoke(this, foundPlayer);
                }

                foreach (Clothing clothing in this.lookingForClothing)
                {
                    SplatoonPlayer[] foundPlayer = otherPlayers
                        .Where(x => 
                            x.Gear.Clothing.MainAbility == clothing.MainAbility &&
                            (clothing.ClothingId == -1 || x.Gear.Clothing.ClothingId == clothing.ClothingId))
                        .ToArray();

                    if (foundPlayer.Any())
                        this.ClothingFound?.Invoke(this, foundPlayer);
                }

                foreach (Shoes shoes in this.lookingForShoes)
                {
                    SplatoonPlayer[] foundPlayer = otherPlayers
                        .Where(x =>
                            x.Gear.Shoes.MainAbility == shoes.MainAbility &&
                            (shoes.ShoeId == -1 || x.Gear.Shoes.ShoeId == shoes.ShoeId))
                        .ToArray();

                    if (foundPlayer.Any())
                        this.ShoesFound?.Invoke(this, foundPlayer);
                }

                // Every 180 seconds. Unsure if this is too much?
                await Task.Delay(180000);
            }
        }
    }
}
