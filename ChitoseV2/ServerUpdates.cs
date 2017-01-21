using Discord;
using Discord.Commands;
using System.Linq;

namespace ChitoseV2
{
    internal class ServerUpdates : CommandSet
    {
        private MusicModule music;

        public ServerUpdates(MusicModule music)
        {
            this.music = music;
        }

        public void AddCommands(DiscordClient client, CommandService commands)
        {
            client.UserJoined += async (s, e) =>
            {
                var channel = e.Server.FindChannels("announcements").FirstOrDefault();

                var user = e.User;

                await channel.SendMessage(string.Format("@everyone {0} has joined the server!", user.Name));
            };

            client.UserLeft += async (s, e) =>
            {
                var channel = e.Server.FindChannels("announcements").FirstOrDefault();

                var user = e.User;

                await channel.SendMessage(string.Format("@everyone {0} has left the server.", user.Name));
            };

            client.UserUpdated += (s, e) =>
            {
                var voiceChannel = client.FindServers("Too Too Roo").FirstOrDefault().FindUsers("Chitose").FirstOrDefault().VoiceChannel;

                if (voiceChannel != null)
                {
                    if (voiceChannel.Users.Count() == 1)
                    {
                        music.Leave();
                    }
                }
            };

            client.UserBanned += async (s, e) =>
            {
                var channel = e.Server.FindChannels("announcements").FirstOrDefault();

                await channel.SendMessage(string.Format("@everyone {0} has been banned from the server.", e.User.Name));
            };
        }
    }
}