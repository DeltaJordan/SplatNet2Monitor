using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using NLog;
using SplatNet2.Net.Api.Data.Battles;
using SplatNet2.Net.Api.Exceptions;
using SplatNet2.Net.Api.Network.Data;

namespace Annaki.Events.Workers
{
    public class BattleMonitorEventWorker
    {
        public ulong UserId { get; }
        private static readonly Logger ClassLogger = LogManager.GetCurrentClassLogger();

        public BattleMonitorEventWorker(ulong userId)
        {
            this.UserId = userId;
        }

        public void BattleMonitor_BattlesRetrieved(object sender, Dictionary<int, string> e)
        {
            foreach ((int battleNumber, string battleJson) in e)
            {
                string battleDirectory = Directory.CreateDirectory(Path.Combine(Globals.AppPath, "Battles", this.UserId.ToString())).FullName;

                File.WriteAllText(Path.Combine(battleDirectory, $"Battle #{battleNumber}"), battleJson);
            }

            if (Globals.BotSettings.Users[this.UserId].ReadBattleNumbers == null)
            {
                Globals.BotSettings.Users[this.UserId].ReadBattleNumbers = e.Keys.ToArray();
            }
            else
            {
                Globals.BotSettings.Users[this.UserId].ReadBattleNumbers = 
                    Globals.BotSettings.Users[this.UserId].ReadBattleNumbers.Concat(e.Keys).ToArray();
            }

            Globals.BotSettings.SaveSettings();

            ClassLogger.Info($"Saved {e.Count} battles (#{e.Keys.OrderBy(x => x).First()}-#{e.Keys.OrderBy(x => x).Last()}).");
        }

        public async void BattleMonitor_CookieRefreshed(object sender, SplatnetCookie e)
        {
            DiscordMember owner = await Annaki.Client.Guilds.First().Value
                .GetMemberAsync(Annaki.Client.CurrentApplication.Owners.First().Id);

            DiscordUser discordUser = await Annaki.Client.GetUserAsync(this.UserId);

            DiscordDmChannel dmChannel = await owner.CreateDmChannelAsync();

            if (Globals.BotSettings.Users == null)
            {
                Globals.BotSettings.Users = new Dictionary<ulong, UserSettings>();
            }

            if (!Globals.BotSettings.Users.ContainsKey(this.UserId))
            {
                Globals.BotSettings.Users[this.UserId] = new UserSettings();
            }

            Globals.BotSettings.Users[this.UserId].Cookie = e;
            Globals.BotSettings.SaveSettings();

            ClassLogger.Info("Cookie has been refreshed and updated successfully.");

            await dmChannel.SendMessageAsync(
                $"Cookie for {discordUser.Username}#{discordUser.Discriminator} has been refreshed and updated successfully.");
        }

        public async void BattleMonitor_HeadgearFound(object sender, SplatoonPlayer[] e)
        {
            DiscordMember owner = await Annaki.Client.Guilds.First().Value
                .GetMemberAsync(Annaki.Client.CurrentApplication.Owners.First().Id);

            DiscordDmChannel dmChannel = await owner.CreateDmChannelAsync();

            List<DiscordEmbedBuilder> gearBuilders = new List<DiscordEmbedBuilder>();

            foreach (SplatoonPlayer player in e)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "Headgear Found.",
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = player.Gear.Headgear.ImageUrl }
                };

                embedBuilder.AddField(player.Name,
                    $"{player.Gear.Headgear.Name} - {player.Gear.Headgear.MainAbility}");

                gearBuilders.Add(embedBuilder);
            }

            foreach (DiscordEmbedBuilder embedBuilder in gearBuilders)
            {
                await dmChannel.SendMessageAsync(embed: embedBuilder.Build());
            }
        }

        public async void BattleMonitor_ClothingFound(object sender, SplatoonPlayer[] e)
        {
            DiscordMember owner = await Annaki.Client.Guilds.First().Value
                .GetMemberAsync(Annaki.Client.CurrentApplication.Owners.First().Id);

            DiscordDmChannel dmChannel = await owner.CreateDmChannelAsync();

            List<DiscordEmbedBuilder> gearBuilders = new List<DiscordEmbedBuilder>();

            foreach (SplatoonPlayer player in e)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "Clothing Found.",
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = player.Gear.Clothing.ImageUrl }
                };

                embedBuilder.AddField(player.Name,
                    $"{player.Gear.Clothing.Name} - {player.Gear.Clothing.MainAbility}");

                gearBuilders.Add(embedBuilder);
            }

            foreach (DiscordEmbedBuilder embedBuilder in gearBuilders)
            {
                await dmChannel.SendMessageAsync(embed: embedBuilder.Build());
            }
        }

        public async void BattleMonitor_ShoesFound(object sender, SplatoonPlayer[] e)
        {
            DiscordMember owner = await Annaki.Client.Guilds.First().Value
                .GetMemberAsync(Annaki.Client.CurrentApplication.Owners.First().Id);

            DiscordDmChannel dmChannel = await owner.CreateDmChannelAsync();

            List<DiscordEmbedBuilder> gearBuilders = new List<DiscordEmbedBuilder>();

            foreach (SplatoonPlayer player in e)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "Feet Found.",
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = player.Gear.Shoes.ImageUrl }
                };

                embedBuilder.AddField(player.Name,
                    $"{player.Gear.Shoes.Name} - {player.Gear.Shoes.MainAbility}");

                gearBuilders.Add(embedBuilder);
            }

            foreach (DiscordEmbedBuilder embedBuilder in gearBuilders)
            {
                await dmChannel.SendMessageAsync(embed: embedBuilder.Build());
            }
        }

        public async void BattleMonitor_CookieExpired(object sender, ExpiredCookieException e)
        {
            if (Globals.BotSettings.Users[this.UserId].Notified)
            {
                return;
            }

            const string domain = "kelpdo.me";

            DiscordMember owner = await Annaki.Client.Guilds.First().Value
                .GetMemberAsync(Annaki.Client.CurrentApplication.Owners.First().Id);

            // TODO Use Main server for production.
            DiscordMember userMember = await Annaki.Client.Guilds.First(x => x.Key == 738823408686727248).Value
                .GetMemberAsync(this.UserId);

            DiscordDmChannel userDmChannel = await userMember.CreateDmChannelAsync();

            await userDmChannel.SendMessageAsync($"Your Nintendo cookie has expired. " +
                                                 $"Please proceed to https://{domain}/Refresh to refresh your cookie.");

            DiscordDmChannel dmChannel = await owner.CreateDmChannelAsync();

            await dmChannel.SendMessageAsync(
                $"The cookie has expired for {userMember.Username}#{userMember.Discriminator}. " +
                $"No further battles can be saved until this issue is resolved.");

            await dmChannel.SendMessageAsync(e.ToString());

            Globals.BotSettings.Users[this.UserId].Notified = true;
            Globals.BotSettings.SaveSettings();
        }
    }
}
