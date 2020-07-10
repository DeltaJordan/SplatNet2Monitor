using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Annaki.Commands
{
    public class AdminModule : BaseModule
    {
        [Command("reset")]
        public async Task Reset(CommandContext ctx)
        {
            Program.BattleMonitor.ResetErrorCount();

            await ctx.RespondAsync("Successfully reset error count.");
        }

        protected override void Setup(DiscordClient client)
        {
            
        }
    }
}
