using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ScottPlot;
using SplatNet2.Net.Api.Data;
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
            decimal max = 3200;

            decimal? power = this.battles.Select(x => x.XPower > x.LobbyPower ? x.XPower : x.LobbyPower).OrderByDescending(x => x).First();

            max = power ?? max;

            max = Math.Round(max / 100, MidpointRounding.AwayFromZero) * 100;

            this.plot.Clear();
            this.PlotBattles();
            this.PlotLobbyAverages();
            this.plot.Axis(y1: 1900, y2: (double) max);
            this.plot.Grid(xSpacing: 1, ySpacing: 50);
            this.plot.Legend();
            this.plot.Style(Style.Black);
        }

        public void PlotBattles()
        {
            List<SplatoonBattle> sortedBattles = this.battles.OrderBy(x => x.BattleNumber).ToList();
            var enumerable = sortedBattles.Select((x, y) => new {Index = y, Power = x.XPower}).ToArray();

            double[] xPoints = enumerable.Select(x => (double) x.Index).ToArray();
            double[] yPoints = enumerable.Select(x => (double) x.Power.GetValueOrDefault(-1)).ToArray();

            this.plot.PlotScatter(xPoints, yPoints, Color.White, markerShape: MarkerShape.none, label: "X Power After Match");

            for (int i = 0; i < xPoints.Length; i++)
            {
                double xPoint = xPoints[i];
                double yPoint = yPoints[i];
                Color resultColor = sortedBattles[i].Result == BattleResult.Victory ? Color.Green : Color.Red;
                MarkerShape resultShape = sortedBattles[i].Result == BattleResult.Victory
                    ? MarkerShape.triUp
                    : MarkerShape.triDown;

                int textOffset = sortedBattles[i].Result == BattleResult.Victory ? 10 : -10;

                this.plot.PlotPoint(xPoint, yPoint, markerShape: resultShape, color: resultColor);

                if (i > 0)
                {
                    this.plot.PlotText(
                        $"{Math.Abs((sortedBattles[i - 1].XPower - sortedBattles[i].XPower).GetValueOrDefault(0))}",
                        xPoint, yPoint + textOffset, resultColor, alignment: TextAlignment.middleCenter);
                }
            }

            decimal sessionDelta = (sortedBattles.Last().XPower - sortedBattles.First().XPower).GetValueOrDefault(0);
            Color deltaColor = sessionDelta > 0 ? Color.Green : Color.Red;

            this.plot.PlotAnnotation($"Session {sessionDelta}", -10, fontColor: deltaColor, shadow: true,
                fillColor: Color.Black, lineColor: Color.White, fillAlpha: 1);
        }

        public void PlotLobbyAverages()
        {
            var enumerable = this.battles.OrderBy(x => x.BattleNumber)
                .Select((x, y) => new { Index = y, Power = x.LobbyPower }).ToArray();

            double[] xPoints = enumerable.Select(x => (double)x.Index).ToArray();
            double[] yPoints = enumerable.Select(x => (double)x.Power.GetValueOrDefault(-1)).ToArray();

            this.plot.PlotScatter(xPoints, yPoints, Color.Blue, label: "Lobby Power", lineWidth: 0, markerShape: MarkerShape.openDiamond);
        }

        public void SaveToFile(string path)
        {
            this.plot.SaveFig(path);
        }
    }
}
