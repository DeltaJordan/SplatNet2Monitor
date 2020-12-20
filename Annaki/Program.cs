using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Annaki.Events.Workers;
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
using TwitchLib.Api.Helix.Models.Games;
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

        private static Timer merchTimer;

        private static TwitchAPI twitchApi;

        private static CommandsNextExtension commands;

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
                ReconnectIndefinitely = true,
                AutoReconnect = true,
                Token = Globals.BotSettings.BotToken,
                TokenType = TokenType.Bot,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug 
            });

            commands = Client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new []{ Globals.BotSettings.Prefix },
                CaseSensitive = false
            });

            commands.RegisterCommands(Assembly.GetExecutingAssembly());

            await Client.ConnectAsync();

            twitchApi = new TwitchAPI();
            twitchApi.Settings.ClientId = Globals.BotSettings.TwitchClientId;
            twitchApi.Settings.Secret = Globals.BotSettings.TwitchSecret;
            twitchApi.Settings.AccessToken = twitchApi.Helix.Extensions.GetAccessToken();

            LiveStreamMonitorService liveStreamMonitorService = new LiveStreamMonitorService(twitchApi);
            liveStreamMonitorService.SetChannelsByName(new List<string> { "DeltaJordan" });
            liveStreamMonitorService.OnStreamOnline += LiveStreamMonitorService_OnStreamOnline;
            liveStreamMonitorService.Start();

            await StartMonitor();
        }

        private static async void LiveStreamMonitorService_OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            if (Globals.BotSettings.StreamNotificationChannelId == 0)
                return;

            User user = await twitchApi.V5.Users.GetUserByIDAsync(e.Stream.UserId);

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = e.Stream.Title,
                Url = $"https://www.twitch.tv/{user.Name}",
                ImageUrl =
                    $"https://static-cdn.jtvnw.net/previews-ttv/live_user_{user.Name}-320x180.jpg?rnd={new Random().Next()}",
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Width = 270,
                    Height = 270,
                    Url = user.Logo
                },
                Color = new DiscordColor(100, 65, 165)
            };

            embedBuilder.WithAuthor(user.DisplayName);

            try
            {
                GetGamesResponse gamesResponse =
                    await twitchApi.Helix.Games.GetGamesAsync(new List<string> {e.Stream.GameId});
                Game streamingGame = gamesResponse.Games.First();

                embedBuilder.WithFooter($"Playing: {streamingGame.Name}", streamingGame.BoxArtUrl);
            }
            catch
            {
                embedBuilder.WithTimestamp(DateTimeOffset.Now);
            }

            DiscordChannel notificationChannel =
                await Client.GetChannelAsync(Globals.BotSettings.StreamNotificationChannelId);
            DiscordRole notificationRole = notificationChannel.Guild.Roles.FirstOrDefault(x =>
                string.Equals(x.Value.Name, "Notifications", StringComparison.InvariantCultureIgnoreCase)).Value;

            if (notificationRole == null)
                return;

            await notificationChannel.SendMessageAsync($"{notificationRole.Mention} DeltaJordan is live!",
                embed: embedBuilder.Build());
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

            BattleMonitor.ShoesFound += BattleMonitorEventWorker.BattleMonitor_ShoesFound;
            BattleMonitor.ClothingFound += BattleMonitorEventWorker.BattleMonitor_ClothingFound;
            BattleMonitor.HeadgearFound += BattleMonitorEventWorker.BattleMonitor_HeadgearFound;
            BattleMonitor.CookieRefreshed += BattleMonitorEventWorker.BattleMonitor_CookieRefreshed;
            BattleMonitor.BattlesRetrieved += BattleMonitorEventWorker.BattleMonitor_BattlesRetrieved;
            BattleMonitor.CookieExpired += ExceptionEventWorker.BattleMonitor_CookieExpired;
            BattleMonitor.ExceptionOccured += ExceptionEventWorker.BattleMonitor_ExceptionOccured;

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
    }
}
