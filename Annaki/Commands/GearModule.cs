using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Annaki.Algorithms;
using Annaki.Gear;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using NLog;
using SplatNet2.Net.Api.Data;

namespace Annaki.Commands
{
    public class GearModule : BaseModule
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Command("remove"), RequireOwner]
        public async Task Remove(CommandContext ctx, string gearName, string mainAbility)
        {
            gearName = gearName.ToLower().Replace(" ", "");
            mainAbility = mainAbility.ToLower().Replace(" ", "");

            GearEnums.Ability ability = Enum.GetValues(typeof(GearEnums.Ability))
                .Cast<GearEnums.Ability>()
                .OrderBy(x => LevenshteinDistance.Compute(x.ToString().ToLower(), mainAbility))
                .First();

            if (ability == GearEnums.Ability.None)
            {
                await ctx.RespondAsync(
                    "Cannot find ability in the database of abilities. Please try to be more specific with your request.");
                return;
            }

            if (gearName.StartsWith("any"))
            {
                switch (gearName.Split('-')[1])
                {
                    case "head":
                        Program.BattleMonitor.RemoveWatchedHeadgear(-1, ability);
                        break;
                    case "cloth":
                    case "shirt":
                        Program.BattleMonitor.RemoveWatchedClothing(-1, ability);
                        break;
                    case "shoe":
                    case "shoes":
                        Program.BattleMonitor.RemoveWatchedShoes(-1, ability);
                        break;
                }

                await ctx.RespondAsync(
                    $"Now monitoring for all {gearName.Split('-')[1]} with the ability {ability} (Sorry for the weird formatting). " +
                    $"{ctx.Client.CurrentApplication.Owner.Mention} will notify you if the gear is found.");
                return;
            }

            string[] headStrings = Enum.GetNames(typeof(KnownGear.Headgear)).ToArray();
            string[] shirtStrings = Enum.GetNames(typeof(KnownGear.Clothes)).ToArray();
            string[] feetStrings = Enum.GetNames(typeof(KnownGear.Shoes)).ToArray();

            IGrouping<int, string> headGroups =
                headStrings.GroupBy(x => LevenshteinDistance.Compute(x.ToLower(), gearName))
                    .OrderBy(x => x.Key)
                    .First();
            IGrouping<int, string> shirtGroups =
                shirtStrings.GroupBy(x => LevenshteinDistance.Compute(x.ToLower(), gearName))
                    .OrderBy(x => x.Key)
                    .First();
            IGrouping<int, string> feetGroups =
                feetStrings.GroupBy(x => LevenshteinDistance.Compute(x.ToLower(), gearName))
                    .OrderBy(x => x.Key)
                    .First();

            string[] foundGear;

            if (headGroups.Key < shirtGroups.Key && headGroups.Key < feetGroups.Key)
            {
                foundGear = headGroups.ToArray();

                if (foundGear.Count() > 1)
                {
                    await ctx.RespondAsync(
                        "The gear name you entered is not close enough to any specific gear. Try to be more specific.");
                    return;
                }

                int headgear = (int)Enum.Parse(typeof(KnownGear.Headgear), foundGear.First());

                Program.BattleMonitor.RemoveWatchedHeadgear(headgear, ability);

                Globals.BotSettings.WatchedHeadgear = Program.BattleMonitor.GetWatchedHeadgear();
                Globals.BotSettings.SaveSettings();
            }
            else if (shirtGroups.Key < headGroups.Key && shirtGroups.Key < feetGroups.Key)
            {
                foundGear = shirtGroups.ToArray();

                if (foundGear.Count() > 1)
                {
                    await ctx.RespondAsync(
                        "The gear name you entered is not close enough to any specific gear. Try to be more specific.");
                    return;
                }

                int shirt = (int)Enum.Parse(typeof(KnownGear.Clothes), foundGear.First());

                Program.BattleMonitor.RemoveWatchedClothing(shirt, ability);

                Globals.BotSettings.WatchedClothing = Program.BattleMonitor.GetWatchedClothing();
                Globals.BotSettings.SaveSettings();
            }
            else if (feetGroups.Key < headGroups.Key && feetGroups.Key < shirtGroups.Key)
            {
                foundGear = feetGroups.ToArray();

                if (foundGear.Count() > 1)
                {
                    await ctx.RespondAsync(
                        "The gear name you entered is not close enough to any specific gear. Try to be more specific.");
                    return;
                }

                int shoes = (int)Enum.Parse(typeof(KnownGear.Shoes), foundGear.First());

                Program.BattleMonitor.RemoveWatchedShoes(shoes, ability);

                Globals.BotSettings.WatchedShoes = Program.BattleMonitor.GetWatchedShoes();
                Globals.BotSettings.SaveSettings();
            }
            else
            {
                await ctx.RespondAsync(
                    "The gear name you entered is not close enough to any specific gear. Try to be more specific.");
                return;
            }

            await ctx.RespondAsync(
                $"No longer monitoring for {foundGear.First()} with the ability {ability} (Sorry for the weird formatting).");
        }

        [Command("request"), Description("Request a gear piece with the specified main ability.")]
        public async Task Request(CommandContext ctx, string gearName, string mainAbility)
        {
            try
            {
                gearName = gearName.ToLower().Replace(" ", "");
                mainAbility = mainAbility.ToLower().Replace(" ", "");

                GearEnums.Ability ability = Enum.GetValues(typeof(GearEnums.Ability))
                    .Cast<GearEnums.Ability>()
                    .OrderBy(x => LevenshteinDistance.Compute(x.ToString().ToLower(), mainAbility))
                    .First();

                if (ability == GearEnums.Ability.None)
                {
                    await ctx.RespondAsync(
                        "Cannot find ability in the database of abilities. Please try to be more specific with your request.");
                    return;
                }

                if (ctx.User.Id != ctx.Client.CurrentApplication.Owner.Id)
                {
                    if (gearName.StartsWith("any"))
                    {
                        return;
                    }
                }
                else if (gearName.StartsWith("any"))
                {
                    switch (gearName.Split('-')[1])
                    {
                        case "head":
                            Program.BattleMonitor.AddWatchedHeadgear(-1, ability);
                            break;
                        case "cloth":
                        case "shirt":
                            Program.BattleMonitor.AddWatchedClothing(-1, ability);
                            break;
                        case "shoe":
                        case "shoes":
                            Program.BattleMonitor.AddWatchedShoes(-1, ability);
                            break;
                    }

                    await ctx.RespondAsync(
                        $"Now monitoring for all {gearName.Split('-')[1]} with the ability {ability} (Sorry for the weird formatting). " +
                        $"{ctx.Client.CurrentApplication.Owner.Mention} will notify you if the gear is found.");
                    return;
                }

                string[] headStrings = Enum.GetNames(typeof(KnownGear.Headgear)).ToArray();
                string[] shirtStrings = Enum.GetNames(typeof(KnownGear.Clothes)).ToArray();
                string[] feetStrings = Enum.GetNames(typeof(KnownGear.Shoes)).ToArray();

                IGrouping<int, string> headGroups =
                    headStrings.GroupBy(x => LevenshteinDistance.Compute(x.ToLower(), gearName))
                        .OrderBy(x => x.Key)
                        .First();
                IGrouping<int, string> shirtGroups =
                    shirtStrings.GroupBy(x => LevenshteinDistance.Compute(x.ToLower(), gearName))
                        .OrderBy(x => x.Key)
                        .First();
                IGrouping<int, string> feetGroups =
                    feetStrings.GroupBy(x => LevenshteinDistance.Compute(x.ToLower(), gearName))
                        .OrderBy(x => x.Key)
                        .First();

                string[] foundGear;

                if (headGroups.Key < shirtGroups.Key && headGroups.Key < feetGroups.Key)
                {
                    foundGear = headGroups.ToArray();

                    if (foundGear.Count() > 1)
                    {
                        await ctx.RespondAsync(
                            "The gear name you entered is not close enough to any specific gear. Try to be more specific.");
                        return;
                    }

                    int headgear = (int) Enum.Parse(typeof(KnownGear.Headgear), foundGear.First());

                    Program.BattleMonitor.AddWatchedHeadgear(headgear, ability);

                    Globals.BotSettings.WatchedHeadgear = Program.BattleMonitor.GetWatchedHeadgear();
                    Globals.BotSettings.SaveSettings();
                }
                else if (shirtGroups.Key < headGroups.Key && shirtGroups.Key < feetGroups.Key)
                {
                    foundGear = shirtGroups.ToArray();

                    if (foundGear.Count() > 1)
                    {
                        await ctx.RespondAsync(
                            "The gear name you entered is not close enough to any specific gear. Try to be more specific.");
                        return;
                    }

                    int shirt = (int) Enum.Parse(typeof(KnownGear.Clothes), foundGear.First());

                    Program.BattleMonitor.AddWatchedClothing(shirt, ability);

                    Globals.BotSettings.WatchedClothing = Program.BattleMonitor.GetWatchedClothing();
                    Globals.BotSettings.SaveSettings();
                }
                else if (feetGroups.Key < headGroups.Key && feetGroups.Key < shirtGroups.Key)
                {
                    foundGear = feetGroups.ToArray();

                    if (foundGear.Count() > 1)
                    {
                        await ctx.RespondAsync(
                            "The gear name you entered is not close enough to any specific gear. Try to be more specific.");
                        return;
                    }

                    int shoes = (int) Enum.Parse(typeof(KnownGear.Shoes), foundGear.First());

                    Program.BattleMonitor.AddWatchedShoes(shoes, ability);

                    Globals.BotSettings.WatchedShoes = Program.BattleMonitor.GetWatchedShoes();
                    Globals.BotSettings.SaveSettings();
                }
                else
                {
                    await ctx.RespondAsync(
                        "The gear name you entered is not close enough to any specific gear. Try to be more specific.");
                    return;
                }

                await ctx.RespondAsync(
                    $"Now monitoring for {foundGear.First()} with the ability {ability} (Sorry for the weird formatting). " +
                    $"{ctx.Client.CurrentApplication.Owner.Mention} will notify you if the gear is found.");
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        protected override void Setup(DiscordClient client)
        {

        }
    }
}
