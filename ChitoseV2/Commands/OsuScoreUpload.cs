using Discord;
using Discord.Commands;
using Google.Cloud.Vision.V1;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using Sd = System.Drawing;
using System.Timers; 
using OsuApi;
using OsuApi.Model;
using System.Drawing;

namespace ChitoseV2.Commands
{
    internal class OsuScoreUpload : ICommandSet
    {
        private static readonly Api OsuApi = new Api(Chitose.APIKey);
        private static readonly string OsuScorePath = Chitose.ConfigDirectory + "Osu!Score.txt";
        private static readonly string BaseImagePath;
        private static readonly string TempImagePath; 
        string[] Users = GetUsers();

        public void AddCommands(DiscordClient client, CommandService commands)
        {
            Channel OsuChan = client.FindServers("Too Too Roo").First().FindChannels("osu").First();
            Timer Timer = new Timer(60000);
            Timer.Elapsed += (_, __) => SendUserRecentScore(OsuChan); 
            Timer.Start();

            commands.CreateCommand("Follow").Parameter("User", ParameterType.Unparsed).Do(async (e) =>
            {
                OsuApi.Model.User User = await OsuApi.GetUser.WithUser(string.Join(" ", e.Args)).Result();
                if (User != null)
                {
                    File.AppendAllText(OsuScorePath, User.Username + Environment.NewLine);
                    Users = GetUsers();
                    await e.Channel.SendMessage($"{User.Username} has been added! Any ranked score {User.Username} makes will show up in {OsuChan.Mention}!");
                }
                else
                    await e.Channel.SendMessage("User not found!"); 
            }); 
        }

        public async void SendUserRecentScore(Channel OsuChan)
        {
            foreach (string User in Users)
            {
                Score[] UserRecentScores = await OsuApi.GetUserRecent.WithUser(User).Results();
                Score MostRecentScore = UserRecentScores.First();
                if (IsNewScore(MostRecentScore) == true)
                    await OsuChan.SendFile(FormatScoreImage(MostRecentScore));
            }
        }

        static public string[] GetUsers()
        {
            string[] Users = new string[File.ReadLines(OsuScorePath).Count()];
            Users = File.ReadAllLines(OsuScorePath).ToArray();
            return Users; 
        }

        public string FormatScoreImage(Score score)
        {
            Bitmap ScoreImage = new Bitmap(BaseImagePath);

            ScoreImage.Save(TempImagePath); 

            return TempImagePath; 
        }

        public bool IsNewScore(Score score)
        {
            return true; 
        }
    }
}
