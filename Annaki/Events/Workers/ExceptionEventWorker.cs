using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DSharpPlus.Entities;
using SplatNet2.Net.Api.Exceptions;

namespace Annaki.Events.Workers
{
    public static class ExceptionEventWorker
    {
        public static async void BattleMonitor_ExceptionOccured(object sender, (bool stopped, Exception exception) e)
        {
            DiscordMember owner = await Program.Client.Guilds.First().Value
                .GetMemberAsync(Program.Client.CurrentApplication.Owners.First().Id);

            DiscordDmChannel dmChannel = await owner.CreateDmChannelAsync();

            List<string> errorChunks = new List<string>();

            using StringReader stringReader = new StringReader(e.exception.ToString());

            string readLine;
            string currentChunk = string.Empty;

            while ((readLine = await stringReader.ReadLineAsync()) != null)
            {
                readLine += "\n";

                if (currentChunk.Length + readLine.Length > 2000)
                {
                    errorChunks.Add(currentChunk);

                    currentChunk = readLine;

                    continue;
                }

                currentChunk += readLine;
            }

            errorChunks.Add(currentChunk);

            if (e.stopped)
            {
                await dmChannel.SendMessageAsync(
                    "Too many errors have occured. To reset the bot's error count, run `a.reset`, preferably after fixing any issues.");

                foreach (string errorChunk in errorChunks)
                {
                    await dmChannel.SendMessageAsync(errorChunk);
                }
            }
            else
            {
                await dmChannel.SendMessageAsync(
                    "An error has occured. The exception count has increased by one.");

                foreach (string errorChunk in errorChunks)
                {
                    await dmChannel.SendMessageAsync(errorChunk);
                }
            }
        }

        public static async void BattleMonitor_CookieExpired(object sender, ExpiredCookieException e)
        {
            DiscordMember owner = await Program.Client.Guilds.First().Value
                .GetMemberAsync(Program.Client.CurrentApplication.Owners.First().Id);

            DiscordDmChannel dmChannel = await owner.CreateDmChannelAsync();

            await dmChannel.SendMessageAsync(
                "The cookie has expired. No further battles can be saved until this issue is resolved.\n" +
                $"Reauthentication url: {e.ReAuthUrl}");

            await dmChannel.SendMessageAsync(e.ToString());
        }
    }
}
