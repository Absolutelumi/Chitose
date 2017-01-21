using Discord;
using Discord.Audio;
using Discord.Commands;
using RedditSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Script.Serialization;

namespace ChitoseV2
{
    internal class Chitose
    {
        public static readonly string ConfigDirectory = Properties.Settings.Default.ConfigDirectory;
        public static readonly string FfmpegPath = Properties.Settings.Default.FfmpegPath;
        public static readonly string TempDirectory = Properties.Settings.Default.TempDirectory;
        private static readonly JavaScriptSerializer json = new JavaScriptSerializer();
        private AudioService audio;
        private DiscordClient client;
        private CommandService commands;
        private MusicModule music;
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
                input.LogHandler = (_, e) => Console.WriteLine(e.Message);
            });

            client.UsingCommands(input =>
            {
                input.PrefixChar = prefix;
                input.AllowMentionPrefix = true;
                input.HelpMode = HelpMode.Public;
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

            List<CommandSet> commandSets = new List<CommandSet>() { new MusicCommands(music) };

            commandSets.ForEach(set => set.AddCommands(client, commands));

            // Move below commands into command sets
            /*
            //Chitose Picture Response
            System.IO.StreamReader filereader = new System.IO.StreamReader(Chitose.ConfigDirectory + "Chitose.txt");
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
                    client.DownloadFile(new Uri(string.Format("https://lemmmy.pw/osusig/sig.php?colour=pink&uname={0}&pp=1&countryrank", e.GetArg("user"))), Chitose.TempDirectory + e.GetArg("user") + "Signature.png");
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
                    string jsonResult = response.ReadString();
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
                string temppath = Chitose.TempDirectory + arg.ToString() + "booru.png";

                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(new Uri(url), temppath);
                }

                await e.Channel.SendFile(temppath);

                File.Delete(temppath);
            });
            */

            client.ExecuteAndWait(async () =>
            {
                await client.Connect(File.OpenRead(ConfigDirectory + "token.txt").ReadString(), TokenType.Bot);

                client.SetGame("with lolis～");
            });
        }

#pragma warning disable 0649

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

#pragma warning restore 0649
    }
}