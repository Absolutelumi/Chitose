using Discord;
using Discord.Commands;
using System;
using System.IO;
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
        }
    }
}