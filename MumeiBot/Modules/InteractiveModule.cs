using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


//FIGURE OUT HOW TO DO NEW INTEGRATED PAGINATOR BUTTONS FROM DISCORD
//https://support.discord.com/hc/en-us/articles/1500012250861-Bots-Buttons
//https://discord.com/developers/docs/interactions/message-components
namespace MumeiBot.Modules
{
    public class InteractiveModule : InteractiveBase
    {
        // DeleteAfterAsync will send a message and asynchronously delete it after the timeout has popped
        // This method will not block.
        [Command("delete")]
        public async Task<RuntimeResult> Test_DeleteAfterAsync()
        {
            //or timeout: TimeSpan.FromSeconds(10)
            await ReplyAndDeleteAsync("I will be deleted in 10 seconds!",timeout: new TimeSpan(0, 0, 10));
            return Ok();
        }

        // NextMessageAsync will wait for the next message to come in over the gateway, given certain criteria
        // By default, this will be limited to messages from the source user in the source channel
        // This method will block the gateway, so it should be ran in async mode.
        [Command("next", RunMode = RunMode.Async)]
        public async Task Test_NextMessageAsync()
        {
            await ReplyAsync("What is 2+2?");
            //nextmessageasync() can take arguments
            //one of which the fromSourceUser matters
            //if its true, then only take in from the same user
            //if not, then take in from any user
            var response = await NextMessageAsync(true, true, new TimeSpan(0,0,10));
            if (response != null)
            {
                if (response.Content == "4")
                    await ReplyAsync($"Correct! The answer was 4.");
                else
                    await ReplyAsync("Wrong! The answer was 4.");
            }
            else
                await ReplyAsync("You did not reply before the timeout.");

            //you can keep this going on and on to the next question!
            /*response = await NextMessageAsync();*/
        }

        // PagedReplyAsync will send a paginated message to the channel
        // You can customize the paginator by creating a PaginatedMessage object
        // You can customize the criteria for the paginator as well, which defaults to restricting to the source user
        // This method will not block.
        [Command("help")]
        public async Task Test_Paginator()
        {
            //possibly up to 12 pages
            var pages = new[] { "**Help**\n\n`help` - Show the help command",
                                "**Help**\n\n`prefix` - View or change the prefix",
                                "**Help**\n\n`ya` - Returns yeet",
                               };

            PaginatedMessage paginatedMessage = new PaginatedMessage()
            {
                Pages = pages,
                Options = new PaginatedAppearanceOptions()
                {
                    InformationText = "This is a test!",
                    Info = new Emoji("⚽")
                },
                Color = new Color(33, 176, 252),
                Title = "My nice interactive paginator",
            };
            
            await PagedReplyAsync(paginatedMessage);
        }
    }
}
