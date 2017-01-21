using Discord;
using Discord.Audio;
using Discord.Commands;
using Mayushii.Services;
using NAudio.Wave;
using RedditSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace ChitoseV2
{
    internal class Chitose
    {
        public static readonly string ConfigDirectory = Properties.Settings.Default.ConfigDirectory;
        public static readonly string FfmpegPath = Properties.Settings.Default.FfmpegPath;
        public static readonly string TempDirectory = Properties.Settings.Default.TempDirectory;
        private static readonly JavaScriptSerializer json = new JavaScriptSerializer();
        private IAudioClient _vClient;
        private AudioService audio;
        private AudioStatus audioStatus = AudioStatus.Stopped;
        private object AudioStatusLock = new object();
        private DiscordClient client;
        private CommandService commands;
        private MusicModule music;
        private float volume = 0.5f;
        private object VolumeLock = new object();
        private char prefix = '!'; 

        public Chitose()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
            Random random = new Random();

            //Reddit Variables
            var reddit = new Reddit(); 

            //Client Setup
            client = new DiscordClient(input =>
            {
                input.LogLevel = LogSeverity.Info;
                input.LogHandler = Log;
            });

            client.UsingCommands(input =>
            {
                input.PrefixChar = prefix;
                input.AllowMentionPrefix = true;
            });

            client.UsingAudio(x =>
            {
                x.Mode = AudioMode.Outgoing;
            });

            //Services
            commands = client.GetService<CommandService>();

            audio = client.GetService<AudioService>();

            music = new MusicModule(audio);
            music.OnSongChanged += async (title) =>
            {
                await client.FindServers("Too Too Roo").FirstOrDefault().TextChannels.Where(channel => channel.Name == "music").FirstOrDefault().SendMessage("Now playing " + (title ?? "nothing"));
            };

            //Chitose Picture Response
            System.IO.StreamReader filereader = new System.IO.StreamReader(ConfigDirectory + "Chitose.txt");
            string line = filereader.ReadLine();
            while (line != null)
            {
                string[] command = line.Split(';');
                Console.WriteLine(line);
                string[] urls = command[1].Split(',');

                commands.CreateCommand(command[0]).Do(async (e) =>
                {
                    await e.Channel.SendMessage(urls[random.Next(urls.Length)]);
                    await e.Message.Delete();
                });
                line = filereader.ReadLine();
            }

            Console.Clear();

            //Server Updates
            client.UserJoined += async (s, e) =>
            {
                var channel = e.Server.FindChannels("announcements").FirstOrDefault();

                var user = e.User;

                await channel.SendMessage(string.Format("@everyone {0} has joined the server!", user.Name));
            };

            client.UserLeft += async (s, e) =>
            {
                var channel = e.Server.FindChannels("announcements").FirstOrDefault();

                var user = e.User;

                await channel.SendMessage(string.Format("@everyone {0} has left the server.", user.Name));
            };

            client.UserUpdated += async (s, e) =>
            {
                var voiceChannel = client.FindServers("Too Too Roo").FirstOrDefault().FindUsers("Chitose").FirstOrDefault().VoiceChannel;

                if (voiceChannel != null)
                {
                    if (voiceChannel.Users.Count() == 1)
                    {
                        await audio.Leave(voiceChannel);
                        _vClient = null;
                    }
                }
            };

            client.UserBanned += async (s, e) =>
            {
                var channel = e.Server.FindChannels("announcements").FirstOrDefault();

                await channel.SendMessage(string.Format("@everyone {0} has been banned from the server.", e.User.Name));
            };

            //General Commands
            commands.CreateCommand("myrole").Do(async (e) =>
            {
                var role = string.Join(" , ", e.User.Roles);

                await e.Channel.SendMessage(string.Format("```{0} your roles are: {1}```", e.User.Mention, role));
            });

            commands.CreateCommand("myav").Do(async (e) =>
            {
                await e.Channel.SendMessage(string.Format("{0}'s avatar is:  {1}", e.User.Mention, e.User.AvatarUrl));
            });

            commands.CreateCommand("triggered").Parameter("mention").Do(async (e) =>
            {
                await e.Message.Delete();
                await e.Channel.SendMessage(string.Format("元気ね{0}くん。いい事あったかい？", e.GetArg("mention")));
            });

            commands.CreateCommand("osu").Parameter("user").Do(async (e) =>
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(new Uri(string.Format("https://lemmmy.pw/osusig/sig.php?colour=pink&uname={0}&pp=1&countryrank", e.GetArg("user"))), TempDirectory + e.GetArg("user") + "Signature.png");
                }

                await e.Channel.SendFile(TempDirectory + e.GetArg("user") + "Signature.png");

                File.Delete(TempDirectory + e.GetArg("user") + "Signature.png");
            });

            commands.CreateCommand("help").Do(async (e) =>
            {
                await e.Channel.SendMessage(string.Format("```{0}Prefix : '>' \n Chitose reaction picture commands : \n angry  - happy - thinking \n disappointed - annoyed - hopeful \n shocked```", e.User.Mention));

                await e.Channel.SendMessage("```Commands : 'myrole' , 'myav' , 'osu (user)' ```");
            });

            // Music

            commands.CreateGroup("music", cgb =>
            {
                cgb.CreateCommand("add").Parameter("song", ParameterType.Multiple).Do(async (e) =>
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

                cgb.CreateCommand("join").Parameter("channel").Do(async (e) =>
                {
                    var voiceChannel = client.FindServers("Too Too Roo").FirstOrDefault().VoiceChannels.FirstOrDefault(x => x.Name.ToLowerInvariant() == e.GetArg("channel").ToLowerInvariant());
                    if (voiceChannel == null)
                    {
                        await e.Channel.SendMessage(e.GetArg("channel") + " does not exist!");
                        return;
                    }
                    if(e.GetArg("channel") == null)
                    {
                        var UserVoiceChannel = e.User.VoiceChannel;

                        await music.ConnectTo(UserVoiceChannel);
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

                cgb.CreateCommand("help").Do(async (e) =>
                {
                    await e.User.SendMessage(string.Format("Current prefix: {0} \n music add [search terms, multiple words allowed] => Adds most relevant video to the end of the quene \n music clear => Clears the quene \n music skip => Skips the currently playing song \n music quene => Shows the quene \n music next [index of song] => Moves the specified song to the top of the quene \n music remove [index of song] => Removes the specified song from the quene \n music play => Starts playing (If there are no songs on the quene, it will automatically play the next song added) \n music stop => Stops playing \n music pause => Pauses the current song \n music resume => Resumes the current song \n music joni [name of voice channel] => Joins the specified channel \n music leave => Leaves the current channel \n music volume [0 - 100] => Sets the volume" , prefix));
                }); 
            });

            commands.CreateCommand("playfile").Do(async (e) =>
            {
                string audioFile = TempDirectory + CleanFileName("Gillum" + ".mp3");
                await e.Channel.SendMessage("Playing " + "gillum");
                Process process = new Process();
                process.StartInfo.FileName = FfmpegPath + "ffmpeg.exe";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.Arguments = $"-i \"{audioFile}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit();
                process.Close();
                SendAudio(audioFile);
            });

            //Japanese

            commands.CreateCommand("jisho").Parameter("search", ParameterType.Multiple).Do(async (e) =>
            {
                HttpWebRequest getJapanese = WebRequest.CreateHttp(@"http://jisho.org/api/v1/search/words?keyword=" + string.Join("+", e.Args));
                using (Stream response = (await getJapanese.GetResponseAsync()).GetResponseStream())
                {
                    string jsonResult = new StreamReader(response).ReadToEnd();
                    JishoResponse result = json.Deserialize<JishoResponse>(jsonResult);
                    StringBuilder message = new StringBuilder();
                    for (int i = 0; i < Math.Min(5, result.data.Length); i++)
                    {
                        JishoResponse.Result word = result.data[i];
                        message.AppendLine("```");
                        message.AppendLine(word.is_common ? "Common" : "Uncommon");
                        message.AppendLine("Japanese Translations:");
                        message.AppendLine("\t" + string.Join(", ", word.japanese.Select(o => o.word == null ? o.reading : o.word + " (" + o.reading + ")")));
                        message.AppendLine("English Translations:");
                        message.AppendLine("\t" + string.Join("\n\t", word.senses.Select(o => string.Join(", ", o.english_definitions) + " (" + string.Join(", ", o.parts_of_speech) + ")")));
                        message.AppendLine("```");
                    }
                    await e.Channel.SendMessage(message.ToString());
                }
            });

            //Pictures

            commands.CreateCommand("reddit").Parameter("subreddit").Do(async (e) =>
            {
                var subreddit = reddit.GetSubreddit(e.GetArg("subreddit"));
                int indexer = random.Next(100);

                var postlist = subreddit.Hot.Take(100);
                var post = postlist.ToList()[indexer];
                string posturl = post.Url.ToString();

                await e.Channel.SendMessage(posturl);
            });

            commands.CreateCommand("show").Parameter("keyword", ParameterType.Multiple).Do(async (e) =>
            {
                string[] arg = e.Args;
                string url = DanbooruService.GetRandomImage(arg);
                string temppath = TempDirectory + arg.ToString() + "booru.png";

                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(new Uri(url), temppath);
                }

                await e.Channel.SendFile(temppath);

                File.Delete(temppath);
            });

            client.ExecuteAndWait(async () =>
            {
                await client.Connect(new StreamReader(File.OpenRead(ConfigDirectory + "token.txt")).ReadToEnd(), TokenType.Bot);

                client.SetGame("with lolis～");
            });
        }

        public static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        public void SendAudio(string filePath)
        {
            lock (AudioStatusLock)
            {
                audioStatus = AudioStatus.Playing;
                Console.WriteLine("Audio Starting");
            }
            var channelCount = client.GetService<AudioService>().Config.Channels; // Get the number of AudioChannels our AudioService has been configured to use.
            var OutFormat = new WaveFormat(48000, 16, channelCount); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.
            using (var MP3Reader = new Mp3FileReader(filePath)) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
            using (var resampler = new MediaFoundationResampler(MP3Reader, OutFormat)) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
            {
                resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
                int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
                byte[] buffer = new byte[blockSize];
                int byteCount;

                while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0) // Read audio into our buffer, and keep a loop open while data is present
                {
                    lock (AudioStatusLock)
                    {
                        if (audioStatus == AudioStatus.Stopping)
                        {
                            audioStatus = AudioStatus.Stopped;
                            Console.WriteLine("Audio Stopped");
                            return;
                        }
                    }
                    if (byteCount < blockSize)
                    {
                        // Incomplete Frame
                        for (int i = byteCount; i < blockSize; i++)
                            buffer[i] = 0;
                    }
                    for (int i = 0; i < buffer.Length; i += 2)
                    {
                        short sample = (short)(buffer[i] | (buffer[i + 1] << 8));
                        lock (VolumeLock)
                        {
                            short result = (short)(sample * volume);
                            buffer[i] = (byte)(result & 0xFF);
                            buffer[i + 1] = (byte)(result >> 8);
                        }
                    }
                    _vClient.Send(buffer, 0, blockSize); // Send the buffer to Discord
                }
            }
        }

        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private enum AudioStatus { Stopped, Stopping, Playing };

        public class JishoResponse
        {
            public Result[] data;

            public class Result
            {
                public bool is_common;

                public Japanese[] japanese;

                public Details[] senses;

                public string[] tags;

                public class Details
                {
                    public string[] english_definitions;
                    public string[] parts_of_speech;
                }

                public class Japanese
                {
                    public string reading;
                    public string word;
                }
            }
        }
    }
}