using Discord;
using Discord.Commands;
using System;
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
                Mal.AnimeResult anime = Mal.FindMyAnime(title, Chitose.MALUsername, Chitose.MALPassword);
                string description = TagMatcher.Replace(anime.synopsis, string.Empty);
                await e.Channel.SendFile(new Uri(anime.image));
                await e.Channel.SendMessage($"**{anime.title}** \n ```{description}```");
            });

            commands.CreateCommand("mal").Parameter("user").Do(async (e) =>
            {
                await e.Channel.SendMessage("i want to die");
            });
        }
    }
}