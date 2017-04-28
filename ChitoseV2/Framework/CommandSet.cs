using Discord;
using Discord.Commands;

namespace ChitoseV2
{
    internal interface ICommandSet
    {
        void AddCommands(DiscordClient client, CommandService commands);
    }
}