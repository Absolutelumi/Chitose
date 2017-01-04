using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ChitoseV2
{
    class Chitose
    {
        DiscordClient client;
        CommandService commands; 

        public Chitose()
        {
            Random random = new Random();

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

            commands = client.GetService<CommandService>();

            while (line != null)
            {
                string[] command = line.Split(';');
                Console.WriteLine(line); 
                string[] urls = command[1].Split(',');
                

                commands.CreateCommand(command[0]).Do(async (e) =>
                {
                    await e.Channel.SendMessage(urls[random.Next(urls.Length)]);
                });
                line = filereader.ReadLine();
            }

            client.UserJoined += async (s, e) =>
            {
                var channel = e.Server.FindChannels("announcements").FirstOrDefault(); 
                
                var user = e.User;

                await channel.SendMessage(string.Format("@everyone {0} has joined the server!", user.Name));
            };

            commands.CreateCommand("help").Do(async (e) =>
            {
                await e.Channel.SendMessage("Prefix   :   ' > '");

                await e.Channel.SendMessage("Chitose reaction picture commands :");

                await e.Channel.SendMessage(":angry: angry  -  :smile: happy  -  :thinking: thinking");
                await e.Channel.SendMessage(":disappointed: disappointed  -  :frowning: annoyed  -  :smiley: hopeful");
                await e.Channel.SendMessage(":exclamation: shocked"); 
            });

            client.ExecuteAndWait(async () =>
            {
                await client.Connect("MjY1MzU3OTQwNDU2Njg1NTc5.C0yg1w.f3gHb1eCeuMpAMqGGuc - 16Cm1wQ", TokenType.Bot);
            });
        }

        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message); 
        }

       
    }
}
