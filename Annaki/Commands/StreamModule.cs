﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Annaki.Commands
{
    public class StreamModule : BaseModule
    {
        [Command("bind-stream")]
        public async Task BindStream(CommandContext ctx)
        {
            Program.StreamNotificationChannel = ctx.Channel.Id;

            DiscordMessage message = await ctx.RespondAsync("Bound channel as stream notification channel!");

            await Task.Delay(10000);

            await message.DeleteAsync();
            await ctx.Message.DeleteAsync();
        }

        protected override void Setup(DiscordClient client)
        {
        }
    }
}
