using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Infrastructure;
using Microsoft.Extensions.Logging;
using MumeiBot.Common;
using MumeiBot.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MumeiBot.Modules
{
    //don't forget to comment out the logging after you're done
    public class General : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<General> _logger;
        private readonly Images _images;
        private readonly ServerHelper _serverHelper;
        public General(ILogger<General> logger, Images images, ServerHelper ranksHelper) {
            _logger = logger;
            _images = images;
            _serverHelper = ranksHelper;
        }

        [Command("ya")]
        public async Task Ping()
        {
            await ReplyAsync("yeet", options: new RequestOptions()
            {
                AuditLogReason = "this is a reason"
            });
            _logger.LogInformation($"{Context.User.Username} executed the ya command!");
        }

        [Command("back me up")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task BMU()
        {
            var replies = new List<string>();

            //string[] responses = { "first res, "second res", "third res" };
            //await ReplyAsync(responses[new Random().Next(0.responses.Count())]);
            replies.Add($"DON'T TALK TO MY {Context.User.Username} LIKE THAT!!");
            replies.Add("of course i will! they def tweakin");
            replies.Add("sorry? i don't remember what's just happened");
            replies.Add("HOO do i have to SQUARE UP");
            replies.Add($"{Context.User.Username} is 100% a know-it-owl!! {Context.User.Username} owlways speaks the truth");
            replies.Add("owl always be backing you up; now hoo am i ratio'ing");

            var answer = replies[new Random().Next(replies.Count)]; //- 1 at the end of count blocks last reply?

            await ReplyAsync(answer);

            _logger.LogInformation($"{Context.User.Username} executed the BMU command!");
        }

        [Command("ratio this mf")]
        //either bot owner, or admin perms
        [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
        [RequireOwner(Group = "Permission")]

        public async Task Ratio(SocketGuildUser user = null)
        {
            await Context.Channel.TriggerTypingAsync();
            if (user == null)
            {
                await ReplyAsync("don't care");
                await Task.Delay(750);
                await ReplyAsync("didn't ask");
                await Task.Delay(750);
                await ReplyAsync("ratio");
                await Task.Delay(750);
                await ReplyAsync("+");
                await Task.Delay(750);
                await ReplyAsync("the hood hates you");
                await Task.Delay(750);
                await ReplyAsync("+");
                await Task.Delay(750);
                await ReplyAsync("youngboy is better");
            }
            else
            {
                await ReplyAsync(user.Mention + " don't care");
                await Task.Delay(750);
                await ReplyAsync("didn't ask");
                await Task.Delay(750);
                await ReplyAsync("ratio");
                await Task.Delay(750);
                await ReplyAsync("+");
                await Task.Delay(750);
                await ReplyAsync("the hood hates you");
                await Task.Delay(750);
                await ReplyAsync("+");
                await Task.Delay(750);
                await ReplyAsync("youngboy is better");
            }
            _logger.LogInformation($"{Context.User.Username} executed the ratio command!");
        }

        [Command("info")]
        public async Task Info(SocketGuildUser user = null)
        {
            if (user == null)
            {
                //using embed builder
                var builder = new EmbedBuilder()
                    //if left side is null (no avatar), then use the right
                    .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                    .WithDescription("In this message you can see some info about yourself!")
                    .WithColor(new Color(136, 120, 195))
                    .AddField("User ID", Context.User.Id, true)
                    .AddField("Discriminator", Context.User.Discriminator, true)
                    .AddField("Created at", Context.User.CreatedAt.ToString("MM/dd/yyyy"), true)
                    .AddField("Joined at", (Context.User as SocketGuildUser).JoinedAt.Value.ToString("MM/dd/yyyy"), true)
                    .AddField("Roles", string.Join(" ", (Context.User as SocketGuildUser).Roles.Select(x => x.Mention)))
                    //adds the nice color of the role
                    .WithCurrentTimestamp();
                //will build above in embed
                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(null, false, embed);
                _logger.LogInformation($"{Context.User.Username} executed the info command!");
            }
            else
            {
                var builder = new EmbedBuilder()
                   .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                   .WithDescription($"In this meessage you can see some info about {user.Username}!")
                   .WithColor(new Color(33, 176, 252))
                   .AddField("User ID", user.Id, true)
                   .AddField("Discriminator", user.Discriminator, true)
                   .AddField("Created at", user.CreatedAt.ToString("MM/dd/yyyy"), true)
                   .AddField("Joined at", user.JoinedAt.Value.ToString("MM/dd/yyyy"), true)
                   .AddField("Roles", string.Join(" ", (Context.User as SocketGuildUser).Roles.Select(x => x.Mention)))
                   .WithCurrentTimestamp();

                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(null, false, embed);
                _logger.LogInformation($"{Context.User.Username} executed the info command!");
            }
        }

        [Command("server")]
        public async Task Server()
        {
            var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithDescription("In this message you can find some nice information about the current server.")
                .WithTitle($"{Context.Guild.Name} Information")
                .WithColor(new Color(33, 176, 252))
                .AddField("Created at", Context.Guild.CreatedAt.ToString("MM/dd/yyyy"), true)
                .AddField("Membercount", (Context.Guild as SocketGuild).MemberCount + " members", true)
                .AddField("Online users", (Context.Guild as SocketGuild).Users.Where(x => x.Status != UserStatus.Offline).Count() + " members", true);

            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
            _logger.LogInformation($"{Context.User.Username} executed the server command!");
        }

        [Command("purge")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        //[RequireBotPermission] for bot specific permissions require
        public async Task Purge(int amount)
        {
            //get all messages from selected channel
            //should be no more than the amount we passed
            //+1 also removes the purge command
            var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);

            var message = await Context.Channel.SendMessageAsync($"{messages.Count() - 1} messages deleted successfully!");
            await Task.Delay(2500);
            await message.DeleteAsync();
        }

        [Command("echo")]
        [RequireOwner]
        public async Task EchoAsync([Remainder] string text)
        {
            var messages = await Context.Channel.GetMessagesAsync(1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
            await ReplyAsync(text);
            _logger.LogInformation($"{Context.User.Username} executed the echo command!");
        }

        [Command("math")]
        public async Task MathAsync([Remainder] string math)
        {
            var dt = new DataTable();
            var result = dt.Compute(math, null);

            /*await ReplyAsync($"Result: {result}");*/
            await Context.Channel.SendSuccessAsync("Success", $"The result was {result}.");
            _logger.LogInformation($"{Context.User.Username} executed the math command!");
        }

        /*//when this is executed, is executed out of every under command
        //single process, own process, so it doesn't block any other tasks
        //from executing
        [Command("image", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task Image(SocketGuildUser user)
        {
            var messages = await Context.Channel.GetMessagesAsync(1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
            var path = await _images.CreateImageAsync(user);
            await Context.Channel.SendFileAsync(path);
            File.Delete(path);
        }*/

        [Command("rank", RunMode = RunMode.Async)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Rank([Remainder]string identifier)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _serverHelper.GetRanksAsync(Context.Guild);

            IRole role;

            //if identifier was parsable to a ulong, then we can check with the ulong
            //if the role exists
            if(ulong.TryParse(identifier, out ulong roleId))
            {
                var roleById = Context.Guild.Roles.FirstOrDefault(x => x.Id == roleId);
                if(roleById == null)
                {
                    await ReplyAsync("That role does not exist!");
                    return;
                }

                role = roleById;
            }
            else
            {
                var roleByName= Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, identifier, StringComparison.CurrentCultureIgnoreCase));
                if(roleByName == null)
                {
                    await ReplyAsync("That role does not exist!");
                    return;
                }

                role = roleByName;
            }

            if(ranks.All(x => x.Id != role.Id))
            {
                await ReplyAsync("That rank does not exist!");
                return;
            }

            if((Context.User as SocketGuildUser).Roles.Any(x => x.Id == role.Id))
            {
                await (Context.User as SocketGuildUser).RemoveRoleAsync(role);
                await ReplyAsync($"Successfully removed the rank {role.Mention} from you.");
                return;
            }

            await (Context.User as SocketGuildUser).AddRoleAsync(role);
            await ReplyAsync($"Successfully added the rank {role.Mention} to you.");
        }
    }
}