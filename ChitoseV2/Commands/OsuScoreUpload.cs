using Discord;
using Discord.Commands;
using OsuApi;
using OsuApi.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ChitoseV2.Commands
{
    internal class OsuScoreUpload : ICommandSet
    {
        private static readonly string BaseImagePath;
        private static readonly Api OsuApi = new Api(Chitose.APIKey);
        private static readonly string OsuScorePath = Chitose.ConfigDirectory + "Osu!Score.txt";
        private static readonly string TempImagePath;
        private DiscordClient Client;
        private Dictionary<string, DateTime> LatestUpdate;

        public void AddCommands(DiscordClient client, CommandService commands)
        {
            Client = client;
            Timer timer = new Timer(60000);
            timer.AutoReset = true;
            timer.Elapsed += (_, __) => SendUserRecentScore();
            timer.Start();

            LatestUpdate = new Dictionary<string, DateTime>();
            GetUsers();

            commands.CreateCommand("Follow").Parameter("User", ParameterType.Unparsed).Do(async (e) =>
            {
                OsuApi.Model.User user = await OsuApi.GetUser.WithUser(string.Join(" ", e.Args)).Result();
                if (user != null)
                {
                    var osuChannel = client.FindServers("Too Too Roo").First().FindChannels("osu-scores").First();
                    if (!LatestUpdate.ContainsKey(user.Username))
                    {
                        SaveUserData()
                    }
                    await e.Channel.SendMessage($"{user.Username} has been added! Any ranked score {user.Username} makes will show up in {osuChannel.Mention}!");
                }
                else
                    await e.Channel.SendMessage("User not found!");
            });

            commands.CreateCommand("Unfollow").Parameter("User", ParameterType.Unparsed).Do(async (e) =>
            {
                OsuApi.Model.User user = await OsuApi.GetUser.WithUser(string.Join(" ", e.Args)).Result();
                if (LatestUpdate.ContainsKey(user.Username))
                {
                    

                    await e.Channel.SendMessage($"{user.Username} has been removed!"); 
                }
                else
                    await e.Channel.SendMessage("User not on record.");
            }); 
        }

        public void SaveUserData()
        {
            void AddUsers(OsuApi.Model.User user)
            {
                UpdateLatestUpdate(user.Username, new DateTime(0));
            }

            void RemoveUser(OsuApi.Model.User user)
            {
                var tempFile = Path.GetTempFileName();
                var keptUsers = File.ReadLines(OsuScorePath).Where(x => x != $"{user.Username}");
                File.WriteAllLines(OsuScorePath, keptUsers);
                File.Delete(OsuScorePath);
                File.Move(tempFile, OsuScorePath); 
            }
        }

        private string FormatScoreImage(Score score)
        {
            Bitmap ScoreImage = new Bitmap(BaseImagePath);

            ScoreImage.Save(TempImagePath);

            return TempImagePath;
        }

        private string FormatUserScore(string user, Score score, Beatmap beatmap)
        {
            return new StringBuilder()
                .AppendLine($"{user} just got a {score.Accuracy:P2} on {beatmap.Title} [{beatmap.Difficulty}]")
                .AppendLine($"with {score.Combo} combo and {score.Mods}")
                .AppendLine($"*300s:* {score.NumberOf300s}  *100s:* {score.NumberOf100s}  *50s:* {score.NumberOf50s}  *Misses:* {score.NumberOfMisses}")
                .ToString();
        }

        private void GetUsers()
        {
            foreach (var data in File.ReadAllLines(OsuScorePath))
            {
                var splitData = data.Split(',');
                LatestUpdate[splitData[0]] = DateTime.Parse(splitData[1]);
            }
        }

        private bool IsNewScore(Score score) => score.Date.CompareTo(LatestUpdate[score.Username]) > 0;

        private async void SendUserRecentScore()
        {
            Channel osuChannel = Client.FindServers("Too Too Roo").First().FindChannels("osu-scores").First();
            var users = LatestUpdate.Keys.ToArray();
            foreach (string user in users)
            {
                Score[] UserRecentScores = await OsuApi.GetUserRecent.WithUser(user).Results();
                foreach (var recentScore in UserRecentScores.OrderBy(score => score.Date))
                {
                    if (IsNewScore(recentScore))
                    {
                        UpdateLatestUpdate(recentScore.Username, recentScore.Date);
                        var beatmap = (await OsuApi.GetSpecificBeatmap.WithId(recentScore.BeatmapId).Results(1)).FirstOrDefault();
                        await osuChannel.SendFile(new Uri($"https://assets.ppy.sh/beatmaps/{beatmap.BeatmapSetId}/covers/cover.jpg"));
                        await osuChannel.SendMessage(FormatUserScore(user, recentScore, beatmap));
                        await Task.Delay(1000);
                    }
                }
            }
        }

        private void UpdateLatestUpdate(string user, DateTime time)
        {
            LatestUpdate[user] = time;
            File.WriteAllLines(OsuScorePath, LatestUpdate.Select(update => $"{update.Key},{update.Value}"));
        }
    }
}