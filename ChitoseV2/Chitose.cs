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
        private AudioService audio;
        private DiscordClient client;
        private CommandService commands;
        private MusicModule music;
        private char prefix = '!';
        public static Random random = new Random();

        public Chitose()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

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
            //End

            client.ExecuteAndWait(async () =>
            {
                await client.Connect(new StreamReader(File.OpenRead(ConfigDirectory + "token.txt")).ReadToEnd(), TokenType.Bot);

                client.SetGame("with lolis～");
            });
        }
    }
}