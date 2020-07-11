﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;
using SplatNet2.Net.Api.Data.Battles;
using SplatNet2.Net.Api.Data.Battles.Gears;
using SplatNet2.Net.Api.Network.Data;
using SplatNet2.Net.Monitor.Workers;
using TwitchLib.Api;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Api.V5.Models.Users;
using LogLevel = NLog.LogLevel;

namespace Annaki
{
    public static class Program
    {
        public static DiscordClient Client;
        public static BattleMonitor BattleMonitor;

        public static ulong StreamNotificationChannel { get; set; } = 0;

        private static TwitchAPI twitchApi;

        private static CommandsNextModule commands;

        private static readonly Logger ClassLogger = LogManager.GetCurrentClassLogger();
        private static readonly Logger DiscordLogger = LogManager.GetLogger("Discord API");

        public static async Task Main(string[] args)
        {
            // Make sure Log folder exists
            Directory.CreateDirectory(Path.Combine(Globals.AppPath, "Logs"));

            // Checks for existing latest log
            if (File.Exists(Path.Combine(Globals.AppPath, "Logs", "latest.log")))
            {
                // This is no longer the latest log; move to backlogs
                string oldLogFileName = File.ReadAllLines(Path.Combine(Globals.AppPath, "Logs", "latest.log"))[0];
                File.Move(Path.Combine(Globals.AppPath, "Logs", "latest.log"), Path.Combine(Globals.AppPath, "Logs", oldLogFileName));
            }

            // Builds a file name to prepare for future backlogging
            string logFileName = $"{DateTime.Now:dd-MM-yy}-1.log";

            // Loops until the log file doesn't exist
            int index = 2;
            while (File.Exists(Path.Combine(Globals.AppPath, "Logs", logFileName)))
            {
                logFileName = $"{DateTime.Now:dd-MM-yy}-{index}.log";
                index++;
            }

            // Logs the future backlog file name
            File.WriteAllText(Path.Combine(Globals.AppPath, "Logs", "latest.log"), $"{logFileName}\n");

            // Set up logging through NLog
            LoggingConfiguration config = new LoggingConfiguration();

            FileTarget logfile = new FileTarget("logfile")
            {
                FileName = Path.Combine(Globals.AppPath, "Logs", "latest.log"),
                Layout = "[${time}] [${level:uppercase=true}] [${logger}] ${message}"
            };
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);


            ColoredConsoleTarget coloredConsoleTarget = new ColoredConsoleTarget
            {
                UseDefaultRowHighlightingRules = true
            };
            config.AddRule(LogLevel.Info, LogLevel.Fatal, coloredConsoleTarget);
            LogManager.Configuration = config;

            string settingsLocation = Path.Combine(Globals.AppPath, "Data", "settings.json");
            string jsonFile = File.ReadAllText(settingsLocation);

            // Load the settings from file, then store it in the globals
            Globals.BotSettings = JsonConvert.DeserializeObject<Settings>(jsonFile);

            Client = new DiscordClient(new DiscordConfiguration
            {
                Token = Globals.BotSettings.BotToken,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = DSharpPlus.LogLevel.Debug
            });

            commands = Client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = Globals.BotSettings.Prefix,
                CaseSensitive = false
            });

            commands.RegisterCommands(Assembly.GetExecutingAssembly());

            await Client.ConnectAsync();

            twitchApi = new TwitchAPI();
            LiveStreamMonitorService liveStreamMonitorService = new LiveStreamMonitorService(twitchApi);
            liveStreamMonitorService.SetChannelsByName(new List<string> { "DeltaJordan" });
            liveStreamMonitorService.OnStreamOnline += LiveStreamMonitorService_OnStreamOnline;
            liveStreamMonitorService.Start();

            await StartMonitor();
        }

        private static async void LiveStreamMonitorService_OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            if (StreamNotificationChannel == 0)
                return;

            User user = await twitchApi.V5.Users.GetUserByIDAsync(e.Stream.UserId);

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = e.Stream.Title,
                Url = $"https://www.twitch.tv/{user.Name}",
                ImageUrl = e.Stream.ThumbnailUrl,
                ThumbnailUrl = user.Logo,
                Timestamp = e.Stream.StartedAt
            };

            embedBuilder.WithAuthor(user.Name);

            DiscordChannel notificationChannel = await Client.GetChannelAsync(StreamNotificationChannel);
            DiscordRole notificationRole = notificationChannel.Guild.Roles.FirstOrDefault(x =>
                string.Equals(x.Name, "Notifications", StringComparison.InvariantCultureIgnoreCase));

            if (notificationRole == null)
                return;

            await notificationChannel.SendMessageAsync($"{notificationRole.Mention}", embed: embedBuilder.Build());
        }

        private static async Task StartMonitor()
        {
            SplatnetCookie splatnetCookie;

            try
            {
                splatnetCookie = Globals.BotSettings.Cookie;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                splatnetCookie = null;
            }

            BattleMonitor = new BattleMonitor(Globals.BotSettings.ReadBattleNumbers);

            BattleMonitor.ShoesFound += BattleMonitor_ShoesFound;
            BattleMonitor.ClothingFound += BattleMonitor_ClothingFound;
            BattleMonitor.HeadgearFound += BattleMonitor_HeadgearFound;
            BattleMonitor.CookieRefreshed += BattleMonitor_CookieRefreshed;
            BattleMonitor.BattlesRetrieved += BattleMonitor_BattlesRetrieved;
            BattleMonitor.CookieExpired += BattleMonitor_CookieExpired;
            BattleMonitor.ExceptionOccured += BattleMonitor_ExceptionOccured;

            if (Globals.BotSettings.WatchedHeadgear != null)
            {
                foreach (Headgear headgear in Globals.BotSettings.WatchedHeadgear)
                {
                    BattleMonitor.AddWatchedHeadgear(headgear.HeadgearId, headgear.MainAbility);
                }
            }

            if (Globals.BotSettings.WatchedClothing != null)
            {
                foreach (Clothing clothing in Globals.BotSettings.WatchedClothing)
                {
                    BattleMonitor.AddWatchedClothing(clothing.ClothingId, clothing.MainAbility);
                }
            }

            if (Globals.BotSettings.WatchedShoes != null)
            {
                foreach (Shoes shoes in Globals.BotSettings.WatchedShoes)
                {
                    BattleMonitor.AddWatchedShoes(shoes.ShoeId, shoes.MainAbility);
                }
            }

            await BattleMonitor.InitializeAsync(splatnetCookie);

            await BattleMonitor.BeginMonitor();
        }

        private static async void BattleMonitor_ExceptionOccured(object sender, (bool stopped, Exception exception) e)
        {
            DiscordDmChannel dmChannel = await Client.CreateDmAsync(Client.CurrentApplication.Owner);

            if (e.stopped)
            {
                await dmChannel.SendMessageAsync(
                    "Too many errors have occured. To reset the bot's error count, run `a.reset`, preferably after fixing any issues.");

                await dmChannel.SendMessageAsync(e.exception.ToString());
            }
            else
            {
                await dmChannel.SendMessageAsync(
                    "An error has occured. The exception count has increased by one.");

                await dmChannel.SendMessageAsync(e.exception.ToString());
            }
        }

        private static async void BattleMonitor_CookieExpired(object sender, Exception e)
        {
            DiscordDmChannel dmChannel = await Client.CreateDmAsync(Client.CurrentApplication.Owner);

            await dmChannel.SendMessageAsync(
                "The cookie has expired. No further battles can be saved until this issue is resolved.");

            await dmChannel.SendMessageAsync(e.ToString());
        }

        private static void BattleMonitor_BattlesRetrieved(object sender, Dictionary<int, string> e)
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

        private static void BattleMonitor_CookieRefreshed(object sender, SplatnetCookie e)
        {
            Globals.BotSettings.Cookie = e;
            Globals.BotSettings.SaveSettings();

            ClassLogger.Info("Cookie has been refreshed and updated successfully.");
        }

        private static async void BattleMonitor_HeadgearFound(object sender, SplatoonPlayer[] e)
        {
            DiscordDmChannel dmChannel = await Client.CreateDmAsync(Client.CurrentApplication.Owner);

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = "Headgear found."
            };

            foreach (SplatoonPlayer player in e)
            {
                embedBuilder.AddField(player.Name, 
                    $"{player.Gear.Headgear.Name} - {player.Gear.Headgear.MainAbility}");
            }

            await dmChannel.SendMessageAsync(embed:embedBuilder.Build());
        }

        private static async void BattleMonitor_ClothingFound(object sender, SplatoonPlayer[] e)
        {
            DiscordDmChannel dmChannel = await Client.CreateDmAsync(Client.CurrentApplication.Owner);

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = "Clothing found."
            };

            foreach (SplatoonPlayer player in e)
            {
                embedBuilder.AddField(player.Name, 
                    $"{player.Gear.Clothing.Name} - {player.Gear.Clothing.MainAbility}");
            }

            await dmChannel.SendMessageAsync(embed: embedBuilder.Build());
        }

        private static async void BattleMonitor_ShoesFound(object sender, SplatoonPlayer[] e)
        {
            DiscordDmChannel dmChannel = await Client.CreateDmAsync(Client.CurrentApplication.Owner);

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = "Shoes found."
            };

            foreach (SplatoonPlayer player in e)
            {
                embedBuilder.AddField(player.Name, 
                    $"{player.Gear.Shoes.Name} - {player.Gear.Shoes.MainAbility}");
            }

            await dmChannel.SendMessageAsync(embed: embedBuilder.Build());
        }
    }
}
