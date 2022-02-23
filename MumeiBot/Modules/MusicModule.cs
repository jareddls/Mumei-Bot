using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MumeiBot.Handlers;
using MumeiBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;

namespace MumeiBot.Modules
{
    public class MusicModule : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;

        public MusicModule(LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
        }

        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinAsync()
        {
            if (_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm already connected to a voice channel!");
                return;
            }

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                await ReplyAsync($"Joined **{voiceState.VoiceChannel.Name}**!");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("leave", RunMode = RunMode.Async)]
        public async Task LeaveAsync()
        {
            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel!");
                return;
            }

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            try
            {
                //get the player
                var player = _lavaNode.GetPlayer(Context.Guild);

                //if the player is playing, stop it
                if (player.PlayerState is PlayerState.Playing)
                {
                    await player.StopAsync();
                }

                //leave the voice channel
                await _lavaNode.LeaveAsync(voiceState.VoiceChannel);
                await ReplyAsync($"Done playing music. Left **{voiceState.VoiceChannel.Name}**!");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task PlayAsync([Remainder] string query)
        {

            //checks if user is connected to a vc
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            /*if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }*/

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);

                //get the player for that guild
                var player = _lavaNode.GetPlayer(Context.Guild);

                LavaTrack track;
                var search = Uri.IsWellFormedUriString(query, UriKind.Absolute) ?
                    await _lavaNode.SearchAsync(query)
                    : await _lavaNode.SearchYouTubeAsync(query);

                if (search.LoadStatus == LoadStatus.NoMatches)
                {
                    await ReplyAsync($"I wasn't able to find anything for {query}.");
                    return;
                }
                //find track
                /*var searchResponse = await _lavaNode.SearchYouTubeAsync(query);
                if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
                    searchResponse.LoadStatus == LoadStatus.NoMatches)
                {
                    await ReplyAsync($"I wasn't able to find anything for `{query}`.");
                    return;
                }*/

                track = search.Tracks.FirstOrDefault();
                //var track = searchResponse.Tracks.FirstOrDefault();

                //player.Track != null
                if ( player.Track != null && (player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused || player.PlayerState is PlayerState.Stopped))
                {

                    player.Queue.Enqueue(track);
                    await ReplyAsync($"Queued: **{track.Title}**");
                }
                else
                {
                    //var thumbnail = await track.FetchArtworkAsync();
                    await player.PlayAsync(track);
                    await ReplyAsync($"Now Playing: **{track.Title}**");
                    await player.UpdateVolumeAsync((ushort)30);
                }

            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }

            /*var searchResponse = await _lavaNode.SearchYouTubeAsync(query);
            if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
                searchResponse.LoadStatus == LoadStatus.NoMatches)
            {
                await ReplyAsync($"I wasn't able to find anything for `{query}`.");
                return;
            }*/




        }

        [Command("skip", RunMode = RunMode.Async)]
        public async Task Skip()
        {
            //checks if user is in vc
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            //same as var player = _lavaNode.Get
            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync("You need to be in the same voice channel as me!");
                return;
            }

            //if queue is empty
            if (player.Queue.Count == 0 && player.PlayerState == PlayerState.Playing)
            {
                try
                {
                    await _lavaNode.LeaveAsync(voiceState.VoiceChannel);
                    await ReplyAsync($"Skipped the last song of the queue. Now leaving {voiceState.VoiceChannel.Name}!");
                }
                catch (Exception exception)
                {
                    await ReplyAsync(exception.Message);
                }
            }

            else
            {
                /*await player.SkipAsync();
                await ReplyAsync("Skipped the last song of the queue.");*/
                await player.SkipAsync();
                await ReplyAsync($"Skipped! Now playing **{player.Track.Title}**!");
            }

        }

        [Command("pause", RunMode = RunMode.Async)]
        public async Task Pause()
        {
            //checks if user is in vc
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync("You need to be in the same voice channel as me!");
                return;
            }

            if (player.PlayerState == PlayerState.Paused || player.PlayerState == PlayerState.Stopped)
            {
                await ReplyAsync("The music is already paused!");
                return;
            }

            await player.PauseAsync();
            await ReplyAsync("Paused the music!");
        }

        [Command("resume", RunMode = RunMode.Async)]
        public async Task Resume()
        {
            //checks if user is in vc
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync("You need to be in the same voice channel as me!");
                return;
            }

            if (player.PlayerState == PlayerState.Playing)
            {
                await ReplyAsync("The music is already playing!");
                return;
            }

            await player.ResumeAsync();
            await ReplyAsync("Resumed the music!");
        }

        [Command("queue", RunMode = RunMode.Async)]
        public async Task ListAsync()
        {
            try
            {
                //for making a format for our list/queue
                var descriptionBuilder = new StringBuilder();

                //get player and make sure it isn't null
                var player = _lavaNode.GetPlayer(Context.Guild);
                if (player == null)
                {
                    await ReplyAsync("Could not acquire player.");
                }

                if (player.PlayerState is PlayerState.Playing)
                {
                    //if queue count is less than 1 and current track != null then we won't reply with list
                    //just replying with an embed that displays the current track instead
                    if (player.Queue.Count < 1 && player.Track != null)
                    {
                        await ReplyAsync($"Now Playing: **{player.Track.Title}**!");
                    }
                    else
                    {
                        //now we know if we have something int he q worth replying with, so we iterate through all the tracks in the queue.
                        //next add track title and url, use discord markdown to display everything neatly
                        //this tracknum variable is used to display the number in which the song is in place (start at 2
                        //because we're including the current song).

                        var trackNum = 2;
                        foreach (LavaTrack track in player.Queue)
                        {
                            descriptionBuilder.Append($"{trackNum}: [{track.Title}]({track.Url}) - {track.Id}\n");
                            trackNum++;
                        }

                        await ReplyAsync($"Now Playing: [{player.Track.Title}]({player.Track.Url}) \n{descriptionBuilder}");
                        return;
                    }
                }
                else
                {
                    await ReplyAsync("Player doesn't seem to be playing anything right now.");
                    return;
                }
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
                return;
            }
        }

        [Command("stop", RunMode = RunMode.Async)]
        public async Task StopAsync()
        {
            try
            {
                var player = _lavaNode.GetPlayer(Context.Guild);

                if (player == null)
                {
                    await ReplyAsync("Could not acquire player.");
                    return;
                }

                //check if player exists, then if so, check if it is playing
                //if its playing we can stop
                if (player.PlayerState is PlayerState.Playing)
                {
                    await player.StopAsync();
                }

                await ReplyAsync("Playback and playlist has been cleared.");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("volume")]
        public async Task SetVolumeAsync(int volume)
        {

            /*if (volume == "default")
            {
                var player = _lavaNode.GetPlayer(Context.Guild);
                await ReplyAsync($"Volume is currently set to {player.Volume}%.");
                return;
            }*/


            if (volume > 150 || volume <= 0)
            {
                await ReplyAsync("Volume must be between 1 and 150.");
                return;
            }


            try
            {
                var player = _lavaNode.GetPlayer(Context.Guild);
                await player.UpdateVolumeAsync((ushort)volume);
                await ReplyAsync($"Volume has been set to {volume}%.");
                return;
            }
            catch (InvalidOperationException exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("state")]
        public async Task State()
        {
            var player = _lavaNode.GetPlayer(Context.Guild);
            await ReplyAsync($"{player.PlayerState}");
            return;
        }

        public async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            if (!args.Reason.ShouldPlayNext())
            {
                return;
            }

            if (!args.Player.Queue.TryDequeue(out var queuable))
            {
                return;
            }

            if (!(queuable is LavaTrack track))
            {
                await args.Player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
                return;
            }

            await args.Player.PlayAsync(track);
            await args.Player.TextChannel.SendMessageAsync($"Now Playing: [{track.Title}]({track.Url})");
        }

    }
}
