using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Linq;
using System.Net;
using Google.Cloud.Translation.V2;

namespace ChitoseV2
{
    internal class GeneralCommands : ICommandSet
    {
        public void AddCommands(DiscordClient client, CommandService commands)
        {
            commands.CreateCommand("myrole").Do(async (e) =>
            {
                var role = string.Join(" , ", e.User.Roles);

                await e.Channel.SendMessage(string.Format("```{0} your roles are: {1}```", e.User.Mention, role));
            });

            commands.CreateCommand("myav").Do(async (e) => { await e.Channel.SendMessage(string.Format("{0}'s avatar is:  {1}", e.User.Mention, e.User.AvatarUrl)); });

            commands.CreateCommand("av").Parameter("user").Do(async (e) => 
            {
                User user = e.Server.FindUsers(string.Join(" ", e.Args)).FirstOrDefault();
                await e.Channel.SendMessage(string.Format("{0}'s avatar is {1}", user.Mention, user.AvatarUrl));
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

                        await e.Channel.SendMessage(string.Format("{0}'s messages purged! :skull:", user));
                    }
                    else
                    {
                        Message[] userMessages = e.Channel.Messages.Where(message => message.User.Name.ToLowerInvariant() == user.ToLowerInvariant()).ToArray();

                        await e.Channel.DeleteMessages(userMessages);

                        await e.Channel.SendMessage(string.Format("{0}'s messages purged! :skull:", user));
                    }
                });
            });

            commands.CreateCommand("rate").Parameter("waifu", ParameterType.Unparsed).Do(async (e) =>
            {
                string waifu = e.GetArg("waifu");
                var waifuRating = ((uint)waifu.GetHashCode()) % 101;

                if (waifu.ToLowerInvariant() == "shigetora" || waifu.ToLowerInvariant() == "cookiezi")
                {
                    waifuRating = 727;
                }

                await e.Channel.SendMessage(string.Format("{0} is a{2} {1}!", waifu, waifuRating, ((int)waifuRating).StartsWithVowelSound() ? "n" : string.Empty));
            });

            commands.CreateCommand("jesus").Parameter("question", ParameterType.Unparsed).Do(async (e) =>
            {
                await e.Channel.SendMessage("Jesus: Lolis are the answer");
            });

            commands.CreateCommand("translate").Parameter("message", ParameterType.Multiple).Do(async (e) =>
            {
                TranslationClient translator = TranslationClient.Create();
                string text = string.Join(" ", e.Args);
                Detection sourceLanguage = translator.DetectLanguage(text);
                Language english = new Language("english", LanguageCodes.English);
                TranslationResult response = translator.TranslateText(text, LanguageCodes.English, sourceLanguage.Language);

                await e.Channel.SendMessage(response.TranslatedText);
            });

            //client.MessageReceived += async (s, e) =>
            //{
            //    if (e.Message.Text == string.Empty)
            //        return;
            //    TranslationClient translator = TranslationClient.Create();
            //    Detection detection = translator.DetectLanguage(e.Message.Text);
            //    if (detection.IsReliable == false)
            //        return;
            //    if (e.Message.Text.ToLowerInvariant() == "translate")
            //    {
            //        await e.Message.Delete();
            //        Message foreignMessage = e.Channel.Messages.Last();
            //        string text = foreignMessage.Text;
            //        User user = foreignMessage.User; 
            //        Detection sourceLanguage = translator.DetectLanguage(text);
            //        TranslationResult response = translator.TranslateText(text, LanguageCodes.English, sourceLanguage.Language);

            //        await e.Channel.SendMessage(string.Format("{0}'s message means \"{1}\"", user.Name, response.TranslatedText));
            //    }
            //    else if (detection.Language != LanguageCodes.Japanese && detection.Language != LanguageCodes.English)
            //    {
            //        string text = e.Message.Text;
            //        TranslationResult response = translator.TranslateText(text, LanguageCodes.English, detection.Language, TranslationModel.NeuralMachineTranslation);

            //        await e.Channel.SendMessage(string.Format("{0}'s message means \"{1}\"", e.User.Name, response.TranslatedText));
            //    }
            //};
        }
    }
}