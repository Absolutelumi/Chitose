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

            commands.CreateCommand("angry").Do(async (e) =>
            {
                await e.Channel.SendMessage("http://i.imgur.com/b6uQ2h3.jpg");
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
