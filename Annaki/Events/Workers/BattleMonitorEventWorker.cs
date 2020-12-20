using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using NLog;
using SplatNet2.Net.Api.Data.Battles;
using SplatNet2.Net.Api.Network.Data;

namespace Annaki.Events.Workers
{
    public static class BattleMonitorEventWorker
    {
        private static readonly Logger ClassLogger = LogManager.GetCurrentClassLogger();

        public static void BattleMonitor_BattlesRetrieved(object sender, Dictionary<int, string> e)
        {
            foreach ((int battleNumber, string battleJson) in e)
            {
                string battleDirectory = Directory.CreateDirectory(Path.Combine(Globals.AppPath, "Battles")).FullName;

                File.WriteAllText(Path.Combine(battleDirectory, $"Battle #{battleNumber}"), battleJson);
            }

            if (Globals.BotSettings.ReadBattleNumbers == null)
            {
                Globals.BotSettings.ReadBattleNumbers = e.Keys.ToArray();
            }
            else
            {
                Globals.BotSettings.ReadBattleNumbers = Globals.BotSettings.ReadBattleNumbers.Concat(e.Keys).ToArray();
            }

            Globals.BotSettings.SaveSettings();

            ClassLogger.Info($"Saved {e.Count} battles (#{e.Keys.OrderBy(x => x).First()}-#{e.Keys.OrderBy(x => x).Last()}).");
        }

        public static async void BattleMonitor_CookieRefreshed(object sender, SplatnetCookie e)
        {
            DiscordMember owner = await Program.Client.Guilds.First().Value
                .GetMemberAsync(Program.Client.CurrentApplication.Owners.First().Id);

            DiscordDmChannel dmChannel = await owner.CreateDmChannelAsync();

            Globals.BotSettings.Cookie = e;
            Globals.BotSettings.SaveSettings();

            ClassLogger.Info("Cookie has been refreshed and updated successfully.");

            await dmChannel.SendMessageAsync("Cookie has been refreshed and updated successfully.");
        }

        public static async void BattleMonitor_HeadgearFound(object sender, SplatoonPlayer[] e)
        {
            DiscordMember owner = await Program.Client.Guilds.First().Value
                .GetMemberAsync(Program.Client.CurrentApplication.Owners.First().Id);

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

        public static async void BattleMonitor_ClothingFound(object sender, SplatoonPlayer[] e)
        {
            DiscordMember owner = await Program.Client.Guilds.First().Value
                .GetMemberAsync(Program.Client.CurrentApplication.Owners.First().Id);

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

        public static async void BattleMonitor_ShoesFound(object sender, SplatoonPlayer[] e)
        {
            DiscordMember owner = await Program.Client.Guilds.First().Value
                .GetMemberAsync(Program.Client.CurrentApplication.Owners.First().Id);

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
    }
}
