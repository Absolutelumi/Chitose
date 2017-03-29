using Discord;
using Discord.Commands;

namespace ChitoseV2
{
    internal interface CommandSet
    {
        void AddCommands(DiscordClient client, CommandService commands);
    }
}