using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ScottPlot;
using SplatNet2.Net.Api.Data.Battles;

namespace Annaki.Data
{
    public class RankedPlotter
    {
        private List<SplatoonBattle> battles;
        private Plot plot;

        public RankedPlotter(string mode, params SplatoonBattle[] battles)
        {
            this.plot = new Plot();
            this.plot.Title($"Ranked Overview for {mode}");
            this.plot.XLabel("Match Number");
            this.plot.YLabel("X Power");

            this.battles = new List<SplatoonBattle>(battles);
            this.PlotAll();
        }

        public void AddBattle(SplatoonBattle battle)
        {
            if (this.battles.All(x => x.BattleNumber != battle.BattleNumber))
                this.battles.Add(battle);
        }

        public void RemoveBattle(SplatoonBattle battle)
        {
            if (this.battles.Any(x => x.BattleNumber == battle.BattleNumber))
                this.battles.RemoveAt(this.battles.FindIndex(x => x.BattleNumber == battle.BattleNumber));
        }

        public void PlotAll()
        {
            this.plot.Clear();
            this.PlotBattles();
            this.PlotLobbyAverages();
            this.plot.Axis(y1: 1900, y2: 3200);
            this.plot.Grid(xSpacing: 1, ySpacing: 50);
            this.plot.Legend();
        }

        public void PlotBattles()
        {
            var enumerable = this.battles.OrderBy(x => x.BattleNumber)
                .Select((x, y) => new {Index = y, Power = x.XPower}).ToArray();

            double[] xPoints = enumerable.Select(x => (double) x.Index).ToArray();
            double[] yPoints = enumerable.Select(x => (double) x.Power.GetValueOrDefault(-1)).ToArray();

            this.plot.PlotScatter(xPoints, yPoints, Color.OrangeRed, label: "X Power After Match");
        }

        public void PlotLobbyAverages()
        {
            var enumerable = this.battles.OrderBy(x => x.BattleNumber)
                .Select((x, y) => new { Index = y, Power = x.LobbyPower }).ToArray();

            double[] xPoints = enumerable.Select(x => (double)x.Index).ToArray();
            double[] yPoints = enumerable.Select(x => (double)x.Power.GetValueOrDefault(-1)).ToArray();

            this.plot.PlotScatter(xPoints, yPoints, Color.Blue, label: "Lobby Power");
        }

        public void SaveToFile(string path)
        {
            this.plot.SaveFig(path);
        }
    }
}
