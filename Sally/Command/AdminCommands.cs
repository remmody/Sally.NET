﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Sally.NET.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sally.Command
{
    public class AdminCommands : ModuleBase
    {
        //execution of commands need admin permission
        [Group("sudo")]
        public class SudoCommands : ModuleBase
        {
            [Command("whois")]
            public async Task WhoIs(ulong userId)
            {
                if (Context.Message.Channel is SocketGuildChannel guildChannel)
                {
                    //check if the user, which has written the message, has admin rights or is server owner
                    if (!isAuthorized())
                    {
                        await Context.Message.Channel.SendMessageAsync($"{Context.Message.Author.Username}, you dont have the permissions to do this!");
                        return;
                    }
                    if (Program.MyGuild.Users.ToList().Find(u => u.Id == userId) == null)
                    {
                        await Context.Message.Channel.SendMessageAsync("User couldn't be found.");
                        return;
                    }
                    //user has admin rights
                    await Context.Message.Channel.SendMessageAsync($"{userId} => {Program.MyGuild.Users.ToList().Find(u => u.Id == userId)}");
                }
                else
                {
                    
                }
            }
            [Command("reverse")]
            public async Task ReverseUsernames()
            {
                if (Context.Guild == null)
                {
                    await Context.Message.Channel.SendMessageAsync("This command can't be used here.");
                    return;
                }
                //check if the user, which has written the message, has admin rights
                if (!isAuthorized())
                {
                    await Context.Message.Channel.SendMessageAsync($"{Context.Message.Author.Username}, you dont have the permissions to do this!");
                    return;
                }
                foreach (SocketGuildUser guildUser in Program.MyGuild.Users)
                {
                    //"remove" owner
                    if (guildUser.Id == Program.MyGuild.OwnerId)
                        continue;
                    await guildUser.ModifyAsync(u => u.Nickname = new String((guildUser.Nickname != null ? guildUser.Nickname : guildUser.Username).Reverse().ToArray()));
                }
            }
            private bool isAuthorized()
            {
                if ((Context.Message.Author as SocketGuildUser)?.Roles.ToList().FindAll(r => r.Permissions.Administrator) == null)
                {
                    return false;
                }
                return true;
            }
            [Command("prefix")]
            public async Task ChangePrefix(char prefix)
            {
                //try casting to guild channel
                if (Context.Message.Channel is SocketGuildChannel guildChannel)
                {
                    //check if this user is an admin of the specific guild
                    SocketGuildUser user = guildChannel.Guild.Users.ToList().Find(u => u.Id == Context.Message.Author.Id);
                    if (isAuthorized())
                    {
                        CommandHandlerService.IdPrefixCollection[guildChannel.Guild.Id] = prefix;
                        File.WriteAllText("meta/prefix.json", JsonConvert.SerializeObject(CommandHandlerService.IdPrefixCollection));
                        await Context.Message.Channel.SendMessageAsync($"Now the new prefix is \"{prefix}\"");
                    }
                    else
                    {
                        await Context.Message.Channel.SendMessageAsync("You have no permission.");
                    }
                }
                else
                {
                    //channel isn't a guild channel
                    await Context.Message.Channel.SendMessageAsync("This command has no effect here. Try using it on a guild.");
                }
            }

            [Command("addrole")]
            public async Task AddRankRole(int index, ulong roleId)
            {
                if (Context.Message.Channel is SocketGuildChannel guildChannel)
                {
                    if (!isAuthorized())
                    {
                        await Context.Message.Channel.SendMessageAsync("You have no permission for this command.");
                        return;
                    }
                    ulong guildId = guildChannel.Guild.Id;
                    SocketGuild guild = guildChannel.Guild;
                    Dictionary<int, ulong> guildRankRoleCollection = new Dictionary<int, ulong>();
                    guildRankRoleCollection = RoleManagerService.RankRoleCollection[guildId];
                    if (index > 500)
                    {
                        await Context.Message.Channel.SendMessageAsync("Desired level is too high.");
                        return;
                    }
                    if (guild.Roles.ToList().Find(r => r.Id == roleId) == null)
                    {
                        await Context.Message.Channel.SendMessageAsync("There is no role with this id on this guild.");
                        return;
                    }
                    if (guildRankRoleCollection.Count >= 20)
                    {
                        await Context.Message.Channel.SendMessageAsync("You can't add anymore roles.");
                        return;
                    }
                    if (guildRankRoleCollection.Values.ToList().Contains(roleId))
                    {
                        await Context.Message.Channel.SendMessageAsync("You have already added this role.");
                        return;
                    }
                    if (!guildRankRoleCollection.ContainsKey(index))
                    {
                        guildRankRoleCollection.Add(index, roleId);
                    }
                    guildRankRoleCollection[index] = roleId;
                    RoleManagerService.RankRoleCollection[guildId] = guildRankRoleCollection;
                    RoleManagerService.SaveRankRoleCollection();
                    await Context.Message.Channel.SendMessageAsync($"{guild.Roles.ToList().Find(r => r.Id == roleId).Name} was added with Level {index}.");
                }
                else
                {
                    await Context.Message.Channel.SendMessageAsync("You can only perfom this command on a server.");
                }
            }

            [Command("rmrole")]
            public async Task RemoveRankRole(int index)
            {
                if (Context.Message.Channel is SocketGuildChannel guildChannel)
                {
                    //check if user has admin priviliges
                    if (!isAuthorized())
                    {
                        await Context.Message.Channel.SendMessageAsync("You cant do this.");
                        return;
                    }
                    ulong guildId = guildChannel.Guild.Id;
                    SocketGuild guild = guildChannel.Guild;
                    Dictionary<int, ulong> guildRankRoleCollection = new Dictionary<int, ulong>();
                    guildRankRoleCollection = RoleManagerService.RankRoleCollection[guildId];
                    if (guildRankRoleCollection == null)
                    {
                        await Context.Message.Channel.SendMessageAsync("There are no roles to remove.");
                        return;
                    }
                    //if (guild.Roles.ToList().Find(r => r.Id == roleId) == null)
                    //{
                    //    await Context.Message.Channel.SendMessageAsync("This role dont exist on this server.");
                    //    return;
                    //}
                    //if (guildRankRoleCollection.Values.ToList().Contains(roleId))
                    //{
                    //    await Context.Message.Channel.SendMessageAsync("You cant remove this role, because it was never added.");
                    //    return;
                    //}
                    if (!guildRankRoleCollection.ContainsKey(index))
                    {
                        await Context.Message.Channel.SendMessageAsync("The current index is not in your role collection.");
                        return;
                    }
                    guildRankRoleCollection.Remove(index);
                    RoleManagerService.SaveRankRoleCollection();
                    await Context.Message.Channel.SendMessageAsync("Role successfully removed.");
                }
                else
                {
                    await Context.Message.Channel.SendMessageAsync("This is a server command only.");
                }
            }
        }

        [Command("showroles")]
        public async Task ShowRankRoles()
        {
            if (Context.Message.Channel is SocketGuildChannel guildChannel)
            {
                ulong guildId = guildChannel.Guild.Id;
                SocketGuild guild = guildChannel.Guild;
                Dictionary<int, ulong> guildRankRoleCollection = new Dictionary<int, ulong>();
                guildRankRoleCollection = RoleManagerService.RankRoleCollection[guildId];
                if (guildRankRoleCollection.Count == 0)
                {
                    await Context.Message.Channel.SendMessageAsync("You didn't added rank roles yet.");
                    return;
                }
                EmbedBuilder embed = new EmbedBuilder()
                    .WithTitle("Current Rank Roles")
                    .WithFooter(NET.DataAccess.File.FileAccess.GENERIC_FOOTER, NET.DataAccess.File.FileAccess.GENERIC_THUMBNAIL_URL)
                    .WithTimestamp(DateTime.Now);

                foreach (KeyValuePair<int, ulong> entry in guildRankRoleCollection)
                {
                    SocketRole rankRole = guild.Roles.ToList().Find(r => r.Id == entry.Value);
                    embed.AddField(entry.Key.ToString(), rankRole.Name);
                }

                await Context.Message.Channel.SendMessageAsync(embed: embed.Build());
            }
            else
            {
                await Context.Message.Channel.SendMessageAsync("You can only perfom this command on a server.");
            }
        }


        //execute commands only when the author is the bot owner
        [Group("owner")]
        public class OwnerCommands : ModuleBase
        {
            [Command("apiRequests")]
            public async Task ShowCurrentApiRequests()
            {
                if (Context.Message.Author.Id != Program.BotConfiguration.meId)
                {
                    await Context.Message.Channel.SendMessageAsync("permission denied");
                    return;
                }
                await Program.Me.SendMessageAsync($"There are currently {Program.RequestCounter} Requests.");
            }

            [Command("shutdown")]
            public async Task ShutdownBot()
            {
                if (Context.Message.Author.Id != Program.BotConfiguration.meId)
                {
                    await Context.Message.Channel.SendMessageAsync("permission denied");
                    return;
                }
                await Program.Me.SendMessageAsync("I am shutting down now");
                Environment.Exit(0);
            }

            [Command("restart")]
            public async Task RestartBot()
            {
                if (Context.Message.Author.Id != Program.BotConfiguration.meId)
                {
                    await Context.Message.Channel.SendMessageAsync("permission denied");
                    return;
                }
                await Program.Me.SendMessageAsync("I am restarting now");
                Environment.Exit(1);
            }

            [Command("update")]
            public async Task PerformUpdate()
            {
                if (Context.Message.Author.Id != Program.BotConfiguration.meId)
                {
                    await Context.Message.Channel.SendMessageAsync("permission denied");
                    return;
                }

                //perform update
                await Program.Me.SendMessageAsync("I am updating now");
                Environment.Exit(2);
            }
        }
    }
}
