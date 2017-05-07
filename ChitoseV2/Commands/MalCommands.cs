using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace ChitoseV2
{
    internal class MalCommands : ICommandSet
    {
        private static readonly Regex TagMatcher = new Regex("<.*>|\\[/?i\\]");

        public void AddCommands(DiscordClient client, CommandService commands)
        {
            commands.CreateCommand("anime").Parameter("animename", ParameterType.Unparsed).Do(async (e) =>
            {
                string title = e.GetArg("animename");
                string temporaryFile = Chitose.TempDirectory + title + " Pic.png";
                Mal.AnimeResult anime = Mal.FindMyAnime(title, Chitose.MALUsername, Chitose.MALPassword);
                using (WebClient downloadclient = new WebClient())
                {
                    downloadclient.DownloadFile(new Uri(anime.image), temporaryFile);
                }
                string description = TagMatcher.Replace(anime.synopsis, string.Empty);
                await e.Channel.SendFile(temporaryFile);
                File.Delete(temporaryFile);
                await e.Channel.SendMessage($"**{anime.title}** \n ```{description}```");
            });

            commands.CreateCommand("mal").Parameter("user").Do(async (e) =>
            {
                await e.Channel.SendMessage("i want to die");
            });
        }
    }
}