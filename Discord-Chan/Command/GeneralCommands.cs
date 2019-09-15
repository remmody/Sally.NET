﻿using Discord.Commands;
using Newtonsoft.Json;
using Sally_NET.Service;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sally_NET.Command
{
    public class GeneralCommands : ModuleBase
    {
        public static readonly string PongMessage = "pong";

        [Command("ping")]
        public async Task Ping()
        {
            await Context.Message.Channel.SendMessageAsync($"Pong! `{Math.Abs(Math.Round((DateTimeOffset.UtcNow - Context.Message.CreatedAt).TotalMilliseconds))} ms`");
        }

        [Command("help")]
        public async Task GetHelpPage()
        {
            await Context.Message.Channel.SendMessageAsync("If you are looking for help open the following webpage: <https://its-sally.net>");
        }

        [Command("commands")]
        public async Task GetCommandPage()
        {
            await Context.Message.Channel.SendMessageAsync("Here you can find the list of all available commands: <https://its-sally.net/commands>");
        }

        [Command("meme")]
        public async Task SendRandomMeme()
        {
            dynamic jsonResponse = JsonConvert.DeserializeObject<dynamic>(ApiRequestService.StartRequest("memeapi"));
            await Context.Message.Channel.SendMessageAsync((string)jsonResponse.image);
        }
    }
}
