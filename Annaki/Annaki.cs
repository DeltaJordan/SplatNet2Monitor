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
    public static class Annaki
    {
        public static DiscordClient Client;
        public static readonly Dictionary<ulong, BattleMonitor> BattleMonitors = new Dictionary<ulong, BattleMonitor>();
        public static readonly Dictionary<ulong, BattleMonitorEventWorker> BattleMonitorEventWorkers = new Dictionary<ulong, BattleMonitorEventWorker>();

        private static Timer merchTimer;

        private static TwitchAPI twitchApi;

        private static CommandsNextExtension commands;

        public static async Task Start()
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

            try
            {
                twitchApi = new TwitchAPI();
                twitchApi.Settings.ClientId = Globals.BotSettings.TwitchClientId;
                twitchApi.Settings.Secret = Globals.BotSettings.TwitchSecret;
                twitchApi.Settings.AccessToken = twitchApi.Helix.Extensions.GetAccessToken();

                LiveStreamMonitorService liveStreamMonitorService = new LiveStreamMonitorService(twitchApi);
                liveStreamMonitorService.SetChannelsByName(new List<string> { "DeltaJordan" });
                liveStreamMonitorService.OnStreamOnline += LiveStreamMonitorService_OnStreamOnline;
                liveStreamMonitorService.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            await StartMonitor();
        }

        private static async void LiveStreamMonitorService_OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            try
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
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private static async Task StartMonitor()
        {

            if (Globals.BotSettings.Users == null)
            {
                return;
            }

            foreach (ulong id in Globals.BotSettings.Users.Keys)
            {
                await InitializeBattleMonitor(id);
            }
        }

        public static async Task InitializeBattleMonitor(ulong userId)
        {
            UserSettings userSettings = null;

            if (Globals.BotSettings.Users != null && Globals.BotSettings.Users.ContainsKey(userId))
            {
                userSettings = Globals.BotSettings.Users[userId];
            }
                
            SplatnetCookie splatnetCookie = userSettings?.Cookie;

            BattleMonitor battleMonitor = await BattleMonitor.CreateInstance(splatnetCookie, userSettings?.ReadBattleNumbers);

            BattleMonitorEventWorker battleMonitorEventWorker = new BattleMonitorEventWorker(userId);

            battleMonitor.ShoesFound += battleMonitorEventWorker.BattleMonitor_ShoesFound;
            battleMonitor.ClothingFound += battleMonitorEventWorker.BattleMonitor_ClothingFound;
            battleMonitor.HeadgearFound += battleMonitorEventWorker.BattleMonitor_HeadgearFound;
            battleMonitor.CookieRefreshed += battleMonitorEventWorker.BattleMonitor_CookieRefreshed;
            battleMonitor.BattlesRetrieved += battleMonitorEventWorker.BattleMonitor_BattlesRetrieved;
            battleMonitor.CookieExpired += battleMonitorEventWorker.BattleMonitor_CookieExpired;
            battleMonitor.ExceptionOccured += ExceptionEventWorker.BattleMonitor_ExceptionOccured;

            BattleMonitorEventWorkers.Add(userId, battleMonitorEventWorker);

            if (userSettings?.WatchedHeadgear != null)
            {
                foreach (Headgear headgear in userSettings.WatchedHeadgear)
                {
                    battleMonitor.AddWatchedHeadgear(headgear.HeadgearId, headgear.MainAbility);
                }
            }

            if (userSettings?.WatchedClothing != null)
            {
                foreach (Clothing clothing in userSettings.WatchedClothing)
                {
                    battleMonitor.AddWatchedClothing(clothing.ClothingId, clothing.MainAbility);
                }
            }

            if (userSettings?.WatchedShoes != null)
            {
                foreach (Shoes shoes in userSettings.WatchedShoes)
                {
                    battleMonitor.AddWatchedShoes(shoes.ShoeId, shoes.MainAbility);
                }
            }

            await battleMonitor.BeginMonitor();

            BattleMonitors.Add(userId, battleMonitor);
        }
    }
}
