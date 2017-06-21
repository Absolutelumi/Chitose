using Discord;
using Discord.Commands;
using System;
using System.Linq;

namespace ChitoseV2
{
    internal class ServerUpdates : ICommandSet
    {
        private MusicModule music;

        private Random random; 

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

                var role = e.Server.FindRoles("normies");

                await user.AddRoles(role.ToArray());

                await channel.SendMessage(string.Format("@everyone {0} has joined the server!", user.Name));
            };

            commands.CreateCommand("dothatshitnigga").Do(async (e) =>
            {
                User[] users = e.Server.Users.ToArray();

                foreach (User user in users)
                {
                    int randomR = random.Next(0, 256);

                    int randomG = random.Next(0, 256);

                    int randomB = random.Next(0, 256);

                    Role userRole = await e.Server.CreateRole(name: $"{e.User.Name}", color: new Color(randomR, randomG, randomB));

                    Role[] roles = { userRole };

                    await user.AddRoles(roles);
                }

                await e.Channel.SendMessage("the shit is done my nigga");
            });

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