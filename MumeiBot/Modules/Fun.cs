using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MumeiBot.Modules
{
    public class Fun : ModuleBase
    {
        public class General : ModuleBase<SocketCommandContext>
        {
            private readonly ILogger<General> _logger;
            private readonly Servers _servers;

            public General(ILogger<General> logger, Servers servers)
            {
                _logger = logger;
                _servers = servers;
            }

            [Command("wholesome")]
            //alias u can use for ur command, does the same thing as typing wholesome
            [Alias("reddit")]
            public async Task wholesome(string subreddit = null)
            {
                var client = new HttpClient();
                //calls to reddit's api using getstringasync
                var result = await client.GetStringAsync($"https://reddit.com/r/{subreddit ?? "WholesomeMemes"}/random.json?limit=1");
                //square bracket means that the input is an array
                //if the subreddit is not null, then there will be a square bracket at the start
                //of the result, else it will not show
                if (!result.StartsWith("["))
                {
                    await Context.Channel.SendMessageAsync("This subreddit doesn't exist.");
                    return;
                }
                //json array
                JArray arr = JArray.Parse(result);
                JObject post = JObject.Parse(arr[0]["data"]["children"][0]["data"].ToString());
                /*post["num_comments"];*/

                //nsfw checker
                if (post["over_18"].ToString() == "True" && !(Context.Channel as ITextChannel).IsNsfw)
                {
                    await ReplyAsync("The subreddit contains NSFW content, while this is a SFW channel.");
                    return;
                }

                var builder = new EmbedBuilder()
                    .WithImageUrl(post["url"].ToString())
                    .WithColor(new Color(255, 69, 0))
                    .WithTitle(post["title"].ToString())
                    .WithUrl("https://reddit.com" + post["permalink"].ToString())
                    //providing this variable in a string so no need for tostring()
                    .WithFooter($"🗨 {post["num_comments"]} ⬆️ {post["ups"]}");
                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(null, false, embed);
                _logger.LogInformation($"{Context.User.Username} executed the wholesome command!");
            }

            [Command("kiss me")]
            [RequireUserPermission(GuildPermission.Administrator)]
            public async Task smooch()
            {
                await ReplyAsync("_**smooches**_");
                _logger.LogInformation($"{Context.User.Username} executed the smooch command!");
            }

            [Command("i love you")]
            [RequireUserPermission(GuildPermission.Administrator)]
            public async Task ily()
            {
                await ReplyAsync("i love you too ❤️");
                _logger.LogInformation($"{Context.User.Username} executed the ily command!");
            }


            [Command("dumb")]
            public async Task dumb()
            {
                var builder = new EmbedBuilder()
                .WithImageUrl("https://c.tenor.com/smmsu2CX0yYAAAAd/loading-mumei.gif")
                .WithTitle("Huh?")
                .WithColor(new Color(171, 131, 109));

                var messages = await Context.Channel.GetMessagesAsync(1).FlattenAsync();
                await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);

                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(null, false, embed);
                _logger.LogInformation($"{Context.User.Username} executed the dumb command!");
                await Task.Delay(5000);
            }

        }
    }
}
