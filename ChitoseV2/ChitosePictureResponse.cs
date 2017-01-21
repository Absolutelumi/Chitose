using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Net;

namespace ChitoseV2
{
    internal class ChitosePictureResponse : CommandSet
    {
        public void AddCommands(DiscordClient client, CommandService commands)
        {
            commands.CreateGroup("chitose", cgb =>
            {
                System.IO.StreamReader filereader = new System.IO.StreamReader(Chitose.ConfigDirectory + "Chitose.txt");
                string line = filereader.ReadLine();
                while (line != null)
                {
                    string[] command = line.Split(';');
                    string[] urls = command[1].Split(',');

                    cgb.CreateCommand(command[0]).Description(command[0].ToTitleCase() + " Chitose").Do(async (e) =>
                    {
                        string url = urls.Random();
                        string temppath = Chitose.TempDirectory + command[0].ToTitleCase() + " Chitose.png";
                        using (WebClient downloadclient = new WebClient())
                        {
                            downloadclient.DownloadFile(new Uri(url), temppath);
                        }
                        await e.Channel.SendFile(temppath);
                        File.Delete(temppath);
                        await e.Message.Delete();
                    });
                    line = filereader.ReadLine();
                }
            });
        }
    }
}