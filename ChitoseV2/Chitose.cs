using Discord;
using Discord.Audio;
using Discord.Commands;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using YoutubeExtractor;

namespace ChitoseV2
{
    internal class Chitose
    {
        private static readonly string ConfigDirectory = Properties.Settings.Default.ConfigDirectory;
        private static readonly string FfmpegPath = Properties.Settings.Default.FfmpegPath;
        private static readonly string TempDirectory = Properties.Settings.Default.TempDirectory;
        private IAudioClient _vClient;
        private AudioService audio;
        private AudioStatus audioStatus = AudioStatus.Stopped;
        private object AudioStatusLock = new object();
        private DiscordClient client;
        private CommandService commands;
        private YouTubeService youtubeService;

        public Chitose()
        {
            youtubeService = new YouTubeService(new BaseClientService.Initializer() { ApiKey = "AIzaSyCiwm6X53K2uXqGfGBVY1RSfp25U7h-wp8", ApplicationName = GetType().Name });
            Random random = new Random();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

            client = new DiscordClient(input =>
            {
                input.LogLevel = LogSeverity.Info;
                input.LogHandler = Log;
            });

            client.UsingCommands(input =>
            {
                input.PrefixChar = '>';
                input.AllowMentionPrefix = true;
            });

            client.UsingAudio(x =>
            {
                x.Mode = AudioMode.Outgoing;
            });

            commands = client.GetService<CommandService>();

            audio = client.GetService<AudioService>();

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
                var voiceChannel = client.FindServers("Too Too Roo").FirstOrDefault().VoiceChannels.FirstOrDefault(x => x.Name == "Music");
                if (voiceChannel.Users.Count() == 1)
                {
                    await audio.Leave(voiceChannel);
                    _vClient = null;
                }
            };

            client.UserBanned += async (s, e) =>
            {
                var channel = e.Server.FindChannels("announcements").FirstOrDefault();

                await channel.SendMessage(string.Format("@everyone {0} has been banned from the server.", e.User.Name));
            };

            commands.CreateCommand("myrole").Do(async (e) =>
            {
                var role = string.Join(" , ", e.User.Roles);

                await e.Channel.SendMessage(string.Format("```Your roles are: {0} ```", role));
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

            commands.CreateCommand("join").Parameter("channel").Do(async (e) =>
            {
                var voiceChannel = client.FindServers("Too Too Roo").FirstOrDefault().VoiceChannels.FirstOrDefault(x => x.Name == e.GetArg("channel"));
                if (voiceChannel.Users.Count() != 0)
                {
                    _vClient = await audio.Join(voiceChannel);
                }
                else
                {
                    await e.Channel.SendMessage("I am not going to an empty room!");
                }
            });

            commands.CreateCommand("play").Parameter("song", ParameterType.Multiple).Do(async (e) =>
            {
                if (_vClient != null)
                {
                    if (audioStatus == AudioStatus.Playing)
                    {
                        lock (AudioStatusLock)
                        {
                            audioStatus = AudioStatus.Stopping;
                            Console.WriteLine("Audio Stop Requested");
                        }
                    }
                    var searchRequest = youtubeService.Search.List("snippet");
                    searchRequest.Order = SearchResource.ListRequest.OrderEnum.ViewCount;
                    searchRequest.Q = string.Join("+", e.Args);
                    searchRequest.MaxResults = 25;
                    var response = await searchRequest.ExecuteAsync();
                    var result = response.Items.FirstOrDefault(x => x.Id.Kind == "youtube#video");
                    if (result != null)
                    {
                        string link = $"https://www.youtube.com/watch?v={result.Id.VideoId}";
                        IEnumerable<VideoInfo> infos = DownloadUrlResolver.GetDownloadUrls(link);
                        VideoInfo video = infos.OrderByDescending(info => info.AudioBitrate).FirstOrDefault();
                        if (video != null)
                        {
                            if (video.RequiresDecryption)
                            {
                                DownloadUrlResolver.DecryptDownloadUrl(video);
                            }
                            string videoFile = TempDirectory + CleanFileName(video.Title + video.VideoExtension);
                            string audioFile = TempDirectory + CleanFileName(video.Title + ".mp3");
                            var videoDownloader = new VideoDownloader(video, videoFile);
                            videoDownloader.Execute();
                            File.Delete(audioFile);
                            Process process = new Process();
                            process.StartInfo.FileName = FfmpegPath + "ffmpeg.exe";
                            process.StartInfo.RedirectStandardOutput = true;
                            process.StartInfo.RedirectStandardError = true;
                            process.StartInfo.Arguments = $"-i \"{videoFile}\" \"{audioFile}\"";
                            process.StartInfo.UseShellExecute = false;
                            process.StartInfo.CreateNoWindow = true;
                            process.Start();
                            process.WaitForExit();
                            process.Close();
                            SendAudio(audioFile);
                        }
                    }
                }
            });

            client.ExecuteAndWait(async () =>
            {
                await client.Connect("MjY1MzU3OTQwNDU2Njg1NTc5.C08iSQ.0JuccBwAn2mYftmvgNdygJyIK-w", TokenType.Bot);
            });
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
                    _vClient.Send(buffer, 0, blockSize); // Send the buffer to Discord
                }
            }
        }

        private static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private enum AudioStatus { Stopped, Stopping, Playing };
    }
}