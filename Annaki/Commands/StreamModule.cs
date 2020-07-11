using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace Annaki.Commands
{
    public class StreamModule : BaseModule
    {
        [Command("bind-stream"), RequireOwner]
        public async Task BindStream(CommandContext ctx)
        {
            Globals.BotSettings.StreamNotificationChannelId = ctx.Channel.Id;
            Globals.BotSettings.SaveSettings();

            DiscordMessage message = await ctx.RespondAsync("Bound channel as stream notification channel!");

            await Task.Delay(10000);

            await ctx.Message.DeleteAsync();
            await message.DeleteAsync();
        }

        [Command("notif")]
        public async Task ToggleNotifications(CommandContext ctx)
        {
            DiscordRole notificationRole = ctx.Guild.Roles.FirstOrDefault(x =>
                string.Equals(x.Name, "Notifications", StringComparison.InvariantCultureIgnoreCase));

            if (notificationRole == null)
                return;

            if (ctx.Member.Roles.Any(x => x.Id == notificationRole.Id))
            {
                await ctx.Member.RevokeRoleAsync(notificationRole);
                await ctx.RespondAsync("Removed role successfully.");
            }
            else
            {
                await ctx.Member.GrantRoleAsync(notificationRole);
                await ctx.RespondAsync("Added role successfully!");
            }
        }

        protected override void Setup(DiscordClient client)
        {
        }
    }
}
