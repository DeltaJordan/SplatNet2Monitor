using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Annaki.Data;
using Annaki.Extensions;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using SplatNet2.Net.Api.Data;
using SplatNet2.Net.Api.Data.Battles;
using SplatNet2.Net.Api.Network;
using SplatNet2.Net.Monitor.Workers;

namespace Annaki.Commands
{
    [Group("plot")]
    public class SplatoonStatsModule : BaseModule
    {
        [Command("day")]
        public async Task PlotDayBattles(CommandContext ctx, string mode)
        {
            GameMode gameMode = Enum.Parse<GameMode>(mode);

            if (gameMode == GameMode.TurfWar)
                return;

            string battlePath = Path.Combine(Globals.AppPath, "Battles");

            List<SplatoonBattle> splatoonBattles = new List<SplatoonBattle>();

            foreach (string file in Directory.EnumerateFiles(battlePath))
            {
                SplatoonBattle battle = await SplatNetApiClient.ParseBattle(await File.ReadAllTextAsync(file));

                if (battle.LobbyType != LobbyType.Gachi && battle.Lobby != Lobby.Ranked)
                    continue;

                if (battle.GameMode == gameMode)
                {
                    if (battle.EndTime.Date == DateTime.Now.Date)
                        splatoonBattles.Add(battle);
                }
            }

            await this.PlotBattles(ctx, gameMode.ToModeName(), splatoonBattles.ToArray());
        }

        [Command("month")]
        public async Task PlotMonthBattles(CommandContext ctx, string mode)
        {
            GameMode gameMode = Enum.Parse<GameMode>(mode);

            if (gameMode == GameMode.TurfWar)
                return;

            string battlePath = Path.Combine(Globals.AppPath, "Battles");

            List<SplatoonBattle> splatoonBattles = new List<SplatoonBattle>();

            foreach (string file in Directory.EnumerateFiles(battlePath))
            {
                SplatoonBattle battle = await SplatNetApiClient.ParseBattle(await File.ReadAllTextAsync(file));

                if (battle.LobbyType != LobbyType.Gachi && battle.Lobby != Lobby.Ranked)
                    continue;

                if (battle.GameMode == gameMode)
                {
                    if (battle.EndTime.Month == DateTime.Now.Month && battle.EndTime.Year == DateTime.Now.Year)
                        splatoonBattles.Add(battle);
                }
            }

            await this.PlotBattles(ctx, gameMode.ToModeName(), splatoonBattles.ToArray());
        }

        private async Task PlotBattles(CommandContext ctx, string gameMode, SplatoonBattle[] splatoonBattles)
        {
            RankedPlotter plotter = new RankedPlotter(gameMode, splatoonBattles);

            string outputPath = Path.Combine(Globals.AppPath, "graph.png");

            plotter.SaveToFile(outputPath);

            await ctx.RespondWithFileAsync(outputPath);
        }

        protected override void Setup(DiscordClient client)
        {
        }
    }
}
