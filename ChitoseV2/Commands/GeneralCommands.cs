using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Linq;
using System.Net;

namespace ChitoseV2
{
    internal class GeneralCommands : CommandSet
    {
        public void AddCommands(DiscordClient client, CommandService commands)
        {
            commands.CreateCommand("myrole").Do(async (e) =>
            {
                var role = string.Join(" , ", e.User.Roles);

                await e.Channel.SendMessage(string.Format("```{0} your roles are: {1}```", e.User.Mention, role));
            });

            commands.CreateCommand("myav").Do(async (e) =>
            {
                await e.Channel.SendMessage(string.Format("{0}'s avatar is:  {1}", e.User.Mention, e.User.AvatarUrl));
            });

            commands.CreateCommand("triggered").Parameter("mention").Do(async (e) =>
            {
                await e.Message.Delete();
                await e.Channel.SendMessage(string.Format("元気ね{0}くん。いい事あったかい？", e.GetArg("mention")));
            });

            commands.CreateCommand("osu").Parameter("user").Do(async (e) =>
            {
                using (WebClient osuclient = new WebClient())
                {
                    osuclient.DownloadFile(new Uri(string.Format("https://lemmmy.pw/osusig/sig.php?colour=pink&uname={0}&pp=1&countryrank", e.GetArg("user"))), Chitose.TempDirectory + e.GetArg("user") + "Signature.png");
                }

                await e.Channel.SendFile(Chitose.TempDirectory + e.GetArg("user") + "Signature.png");

                File.Delete(Chitose.TempDirectory + e.GetArg("user") + "Signature.png");
            });

            commands.CreateGroup("purge", cgb =>
            {
                commands.CreateCommand("purge").Do(async (e) =>
                {
                    Message[] chanMessages = e.Channel.Messages.Take(50).ToArray();

                    await e.Channel.DeleteMessages(chanMessages);

                    await e.Channel.SendMessage("Messages purged! :skull:");
                });

                commands.CreateCommand("purge").Parameter("user").Do(async (e) =>
                {
                    string user = e.GetArg("user");

                    if (user[1] == '@')
                    {
                        Message[] userMessages = e.Channel.Messages.Take(50).Where(messages => e.Message.User.Id == ulong.Parse(user.Substring(2, user.Length - 3))).ToArray();

                        await e.Channel.DeleteMessages(userMessages);

                        await e.Channel.SendMessage(string.Format("Messages purged! :skull:", user));
                    }
                    else
                    {
                        Message[] userMessages = e.Channel.Messages.Where(message => message.User.Name.ToLowerInvariant() == user.ToLowerInvariant()).ToArray();

                        await e.Channel.DeleteMessages(userMessages);

                        await e.Channel.SendMessage(string.Format("Messages purged! :skull:", user));
                    }
                });
            });

            commands.CreateCommand("rate").Parameter("waifu", ParameterType.Multiple).Do(async (e) =>
            {
                string waifu = string.Join(" ", e.Args);
                uint waifurating;

                waifurating = ((uint)waifu.GetHashCode()) % 101;

                if (waifu.ToLowerInvariant() == "shigetora" || waifu.ToLowerInvariant() == "cookiezi")
                {
                    waifurating = 727;
                }

                await e.Channel.SendMessage(string.Format("{0} is a {1}!", waifu, waifurating));
            });

            commands.CreateCommand("jesus").Parameter("question", ParameterType.Multiple).Do(async (e) =>
            {
                await e.Channel.SendMessage("Jesus: Lolis are the answer");
            });
        }
    }
}