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

            // Chitose Pics
            string[] angry = {"http://i.imgur.com/b6uQ2h3.jpg", "http://i.imgur.com/lj79YCb.jpg", "http://i.imgur.com/vvjuVpo.jpg", "http://i.imgur.com/Qxi3hAt.jpg" };

            string[] happy = {"http://i.imgur.com/oxalgZk.jpg", "http://i.imgur.com/U3CL8fs.jpg", "http://i.imgur.com/bpeZN88.jpg", "http://i.imgur.com/Q64gLJt.jpg", "http://i.imgur.com/tAIjkqv.jpg", "http://i.imgur.com/PVw15ZG.jpg", "http://i.imgur.com/ImuziTe.jpg" };

            string[] shocked = {"http://i.imgur.com/hm3YpNr.jpg", "http://i.imgur.com/yQWmz3E.jpg", "http://i.imgur.com/16R7Mhm.jpg", "http://i.imgur.com/ptr3ooV.jpg", "http://i.imgur.com/gFXvDPQ.jpg" };

            string[] annoyed = { "http://i.imgur.com/MBCIIOP.jpg", "http://i.imgur.com/DRL6p1W.jpg", "http://i.imgur.com/X55CFKN.jpg", "http://i.imgur.com/SZu64dW.jpg", "http://i.imgur.com/NDYirtB.jpg" };

            string[] disappointed = { "http://i.imgur.com/KWvTyQZ.jpg", "http://i.imgur.com/OOE7b0m.jpg", "http://i.imgur.com/AiJc5H6.jpg", "http://i.imgur.com/MFbtUGB.jpg", "http://i.imgur.com/8zkp1g5.jpg", "http://i.imgur.com/1SIW8dZ.jpg", "http://i.imgur.com/qRr5pQd.jpg" };

            string[] hopeful = {"http://i.imgur.com/HsRWc7J.jpg", "http://i.imgur.com/ABVbxGD.jpg", "http://i.imgur.com/pOVJ0Ml.jpg", "http://i.imgur.com/xsdY9Ks.jpg" };

            string[] thinking = { "http://i.imgur.com/gFVFqbq.jpg", "http://i.imgur.com/bHOBgL1.jpg", "http://i.imgur.com/DV03EVp.jpg" }; 

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

            // Chitose Commands
            commands.CreateCommand("angry").Do(async (e) =>
            {
                int rand = random.Next(0, angry.Length); 
                await e.Channel.SendMessage(angry[rand]);
            });

            commands.CreateCommand("happy").Do(async (e) =>
            {
                int rand = random.Next(0, happy.Length);
                await e.Channel.SendMessage(happy[rand]);
            });

            commands.CreateCommand("shocked").Do(async (e) =>
            {
                int rand = random.Next(0, shocked.Length);
                await e.Channel.SendMessage(shocked[rand]);
            });

            commands.CreateCommand("annoyed").Do(async (e) =>
            {
                int rand = random.Next(0, annoyed.Length);
                await e.Channel.SendMessage(annoyed[rand]);
            });

            commands.CreateCommand("disappointed").Do(async (e) =>
            {
                int rand = random.Next(0, disappointed.Length);
                await e.Channel.SendMessage(disappointed[rand]);
            });

            commands.CreateCommand("hopeful").Do(async (e) =>
            {
                int rand = random.Next(0, hopeful.Length);
                await e.Channel.SendMessage(hopeful[rand]);
            });

            commands.CreateCommand("thinking").Do(async (e) =>
            {
                int rand = random.Next(0, thinking.Length);
                await e.Channel.SendMessage(thinking[rand]);
            });

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
