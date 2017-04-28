using Discord;
using Discord.Commands;
using System.Linq;
using System.Text;

namespace ChitoseV2
{
    internal class MusicCommands : CommandSet
    {
        private MusicModule music;

        public MusicCommands(MusicModule music)
        {
            this.music = music;
        }

        public void AddCommands(DiscordClient client, CommandService commands)
        {
            commands.CreateGroup("music", cgb =>
            {
                cgb.CreateCommand("add").Parameter("song", ParameterType.Multiple).Description("Adds the most relevant video to the queue").Do(async (e) =>
                {
                    string title = await music.AddToQueue(e.Args);
                    if (title != null)
                    {
                        await e.Channel.SendMessage("Added " + title + " to queue");
                    }
                    else
                    {
                        await e.Channel.SendMessage("Couldn't find videos");
                    }
                });

                cgb.CreateCommand("clear").Do(async (e) =>
                {
                    music.ClearQueue();
                    await e.Channel.SendMessage("Queue cleared");
                });

                cgb.CreateCommand("skip").Do(async (e) =>
                {
                    bool success = music.Skip();
                    await e.Channel.SendMessage(success ? "Song skipped" : "No song playing");
                });

                cgb.CreateCommand("queue").Do(async (e) =>
                {
                    string[] queue = music.GetQueue();
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine("Queue:");
                    builder.AppendLine("```");
                    for (int i = 0; i < queue.Length; i++)
                    {
                        builder.AppendLine((i + 1) + ": " + queue[i]);
                    }
                    builder.AppendLine("```");
                    await e.Channel.SendMessage(builder.ToString());
                });

                cgb.CreateCommand("next").Parameter("index").Do(async (e) =>
                {
                    int index = -1;
                    bool success = int.TryParse(e.GetArg("index"), out index);
                    string title = music.MoveToTopOfQueue(index);
                    if (!success || title == null)
                    {
                        await e.Channel.SendMessage("Please enter a valid number in the queue");
                    }
                    else
                    {
                        await e.Channel.SendMessage("Moved " + title + " to the top of the queue");
                    }
                });

                cgb.CreateCommand("remove").Parameter("index").Do(async (e) =>
                {
                    int index = -1;
                    bool success = int.TryParse(e.GetArg("index"), out index);
                    string title = music.RemoveFromQueue(index);
                    if (!success || title == null)
                    {
                        await e.Channel.SendMessage("Please enter a valid number in the queue");
                    }
                    else
                    {
                        await e.Channel.SendMessage("Removed " + title);
                    }
                });

                cgb.CreateCommand("play").Do(async (e) =>
                {
                    bool success = music.StartPlaying();
                    if (success)
                    {
                        await e.Channel.SendMessage("Started playing");
                    }
                    else
                    {
                        await e.Channel.SendMessage("Already playing or not in room");
                    }
                });

                cgb.CreateCommand("stop").Do(async (e) =>
                {
                    bool success = music.StopPlaying();
                    if (success)
                    {
                        await e.Channel.SendMessage("Stopped playing");
                    }
                    else
                    {
                        await e.Channel.SendMessage("Already stopped");
                    }
                });

                cgb.CreateCommand("pause").Do(async (e) =>
                {
                    if (!music.SetPause(true))
                    {
                        await e.Channel.SendMessage("Already paused");
                    }
                });

                cgb.CreateCommand("resume").Do(async (e) =>
                {
                    if (!music.SetPause(false))
                    {
                        await e.Channel.SendMessage("Not paused");
                    }
                });

                cgb.CreateCommand("join").Parameter("channel", ParameterType.Optional).Do(async (e) =>
                {
                    if (e.GetArg("channel").Length >= 1)
                    {
                        var voiceChannel = client.FindServers("Too Too Roo").FirstOrDefault().VoiceChannels.FirstOrDefault(x => x.Name.ToLowerInvariant() == e.GetArg("channel").ToLowerInvariant());
                        if (voiceChannel == null)
                        {
                            await e.Channel.SendMessage(e.GetArg("channel") + " does not exist!");
                            return;
                        }
                        if (voiceChannel.Users.Count() != 0)
                        {
                            bool success = await music.ConnectTo(voiceChannel);
                            if (success)
                            {
                                await e.Channel.SendMessage("Joined " + voiceChannel.Name);
                            }
                            else
                            {
                                await e.Channel.SendMessage("Already in " + voiceChannel.Name);
                            }
                        }
                        else
                        {
                            await e.Channel.SendMessage("I am not going to an empty room!");
                        }
                    }
                    else
                    {
                        var voiceChannel = e.User.VoiceChannel;

                        if (voiceChannel.Users.Count() != 0)
                        {
                            bool success = await music.ConnectTo(voiceChannel);
                            if (success)
                            {
                                await e.Channel.SendMessage("Joined " + voiceChannel.Name);
                            }
                            else
                            {
                                await e.Channel.SendMessage("Already in " + voiceChannel.Name);
                            }
                        }
                        else
                        {
                            await e.Channel.SendMessage("I am not going to an empty room!");
                        }
                    }
                });

                cgb.CreateCommand("leave").Do(async (e) =>
                {
                    bool success = music.Leave();
                    if (success)
                    {
                        await e.Channel.SendMessage("Left");
                    }
                    else
                    {
                        await e.Channel.SendMessage("Not in a channel");
                    }
                });

                cgb.CreateCommand("volume").Parameter("volume").Do(async (e) =>
                {
                    float value;
                    bool success = float.TryParse(e.GetArg("volume"), out value);
                    if (!success || value < 0.0f || value > 100.0f)
                    {
                        await e.Channel.SendMessage("Please enter a number between 0 and 100");
                    }
                    else
                    {
                        music.Volume = value / 100.0f;
                    }
                });
            });
        }
    }
}