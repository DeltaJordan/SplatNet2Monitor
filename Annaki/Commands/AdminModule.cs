using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Annaki.Commands
{
    public class AdminModule : BaseExtension
    {
        [Command("reset")]
        public async Task Reset(CommandContext ctx)
        {
            Program.BattleMonitor.ResetErrorCount();

            await ctx.RespondAsync("Successfully reset error count.");
        }

        public async Task Auth(CommandContext ctx, [RemainingText] string authInfo)
        {
            if (await Program.BattleMonitor.RefreshCookie(authInfo))
            {
                await ctx.RespondAsync(
                    "Cookie refresh seemed to complete successfully. Wait for confirmation message if it has not already been received.");
            }
            else
            {
                await ctx.RespondAsync("Error occured. Try again if possible.");
            }
        }

        protected override void Setup(DiscordClient client)
        {
            
        }
    }
}
