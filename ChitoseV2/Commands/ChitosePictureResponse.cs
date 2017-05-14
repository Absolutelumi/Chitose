using Discord;
using Discord.Commands;
using System;
using System.IO;

namespace ChitoseV2
{
    internal class ChitosePictureResponse : ICommandSet
    {
        public void AddCommands(DiscordClient client, CommandService commands)
        {
            commands.CreateGroup("chitose", cgb =>
            {
                StreamReader filereader = new System.IO.StreamReader(Chitose.ConfigDirectory + "Chitose.txt");
                string line = filereader.ReadLine();
                while (line != null)
                {
                    string[] command = line.Split(';');
                    string[] urls = command[1].Split(',');

                    cgb.CreateCommand(command[0]).Description(command[0].ToTitleCase() + " Chitose").Do(async (e) =>
                    {
                        string url = urls.Random();
                        await e.Channel.SendFile(new Uri(url));
                        await e.Message.Delete();
                    });
                    line = filereader.ReadLine();
                }
            });
        }
    }
}