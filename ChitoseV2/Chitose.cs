using Discord;
using Discord.Audio;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace ChitoseV2
{
    class Chitose
    {
        DiscordClient client;
        CommandService commands;
        AudioService audio;
        

        public Chitose()
        {
            Random random = new Random();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
        | SecurityProtocolType.Tls11
        | SecurityProtocolType.Tls12
        | SecurityProtocolType.Ssl3;

            System.IO.StreamReader filereader = new System.IO.StreamReader("C:\\Users\\Scott\\Desktop\\BOT\\Chitose.txt");

            string line = filereader.ReadLine();

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
                    client.DownloadFile(new Uri(string.Format("https://lemmmy.pw/osusig/sig.php?colour=pink&uname={0}&pp=1&countryrank", e.GetArg("user"))), @"C:\\Users\\Scott\\Desktop\\BOT\\OsuSigTEMP.png");
                }

                await e.Channel.SendFile("C:\\Users\\Scott\\Desktop\\BOT\\OsuSigTEMP.png");

                File.Delete("C:\\Users\\Scott\\Desktop\\BOT\\OsuSigTEMP.png");
            });

            commands.CreateCommand("help").Do(async (e) =>
            {
                await e.Channel.SendMessage(string.Format("```{0}Prefix : '>' \n Chitose reaction picture commands : \n angry  - happy - thinking \n disappointed - annoyed - hopeful \n shocked```", e.User.Mention));

                await e.Channel.SendMessage("```Commands : 'myrole' , 'myav' , 'osu (user)' ```");
            });

            // Music

            commands.CreateCommand("join").Do(async (e) =>
            {
                var voiceChannel = client.FindServers("Too Too Roo").FirstOrDefault().VoiceChannels.FirstOrDefault(x => x.Name == "Music");

                await audio.Join(voiceChannel); 

                
            });

            client.ChannelUpdated += (s, e) =>
            {
                var voiceChannel = client.FindServers("Too Too Roo").FirstOrDefault().VoiceChannels.FirstOrDefault(x => x.Name == "Music");

                if (e.After.Users.Count() == 1)
                {
                    audio.Leave(voiceChannel); 
                }
            };

            commands.CreateCommand("play").Parameter("song").Do(async (e) =>
            {
                
            }); 

            client.ExecuteAndWait(async () =>
            {
                await client.Connect("MjY1MzU3OTQwNDU2Njg1NTc5.C08iSQ.0JuccBwAn2mYftmvgNdygJyIK-w", TokenType.Bot);
            });

        }

        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message); 
        }

       
    }
}
