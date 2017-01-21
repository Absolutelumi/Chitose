using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChitoseV2
{
    internal class ChitosePictureResponse : CommandSet
    {
        public void AddCommands(DiscordClient client, CommandService commands)
        {
            System.IO.StreamReader filereader = new System.IO.StreamReader(Chitose.ConfigDirectory + "Chitose.txt");
            string line = filereader.ReadLine();
            while (line != null)
            {
                string[] command = line.Split(';');
                Console.WriteLine(line);
                string[] urls = command[1].Split(',');

                commands.CreateCommand(command[0]).Do(async (e) =>
                {
                    await e.Channel.SendMessage(urls.Random());
                    await e.Message.Delete();
                });
                line = filereader.ReadLine();
            }

            Console.Clear();
        }
    }
}
