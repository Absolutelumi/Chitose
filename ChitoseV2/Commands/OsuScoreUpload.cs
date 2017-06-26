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
using ChitoseV2.Framework;
using System.Drawing.Imaging;

namespace ChitoseV2.Commands
{
    internal class OsuScoreUpload : ICommandSet
    {
        private static readonly Api OsuApi = new Api(Chitose.APIKey);
        private static readonly string OsuScorePath = Chitose.ConfigDirectory + "Osu!Score.txt";
        private DiscordClient Client;
        private Dictionary<string, DateTime> LatestUpdate;

        public void AddCommands(DiscordClient client, CommandService commands)
        {
            Client = client;
            Timer timer = new Timer(10000);
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
                        UpdateUser(user.Username, new DateTime(0));
                    }
                    await e.Channel.SendMessage($"{user.Username} has been added! Any ranked score {user.Username} makes will show up in {osuChannel.Mention}!");
                }
                else
                {
                    await e.Channel.SendMessage("User not found!");
                }
            });

            commands.CreateCommand("Unfollow").Parameter("User", ParameterType.Unparsed).Do(async (e) =>
            {
                OsuApi.Model.User user = await OsuApi.GetUser.WithUser(string.Join(" ", e.Args)).Result();
                if (LatestUpdate.ContainsKey(user.Username))
                {
                    RemoveUser(user.Username);
                    await e.Channel.SendMessage($"{user.Username} has been removed!");
                }
                else
                {
                    await e.Channel.SendMessage("User not on record.");
                }
            });

            commands.CreateCommand("followlist").Do(async (e) =>
            {
                string[] users = LatestUpdate.Keys.ToArray();
                await e.Channel.SendMessage($"Currently followed users: \n ```{string.Join(", ", users)}```"); 
            });

            commands.CreateCommand("latestupdate").Parameter("user", ParameterType.Unparsed).Do(async (e) =>
            {
                string user = string.Join(" ", e.Args);
                DateTime updateDate;
                if (LatestUpdate.ContainsKey(user))
                {
                    LatestUpdate.TryGetValue(user, out updateDate);
                    string latestUpdate = updateDate.ToString(); 
                    await e.Channel.SendMessage($"User {user} was last updated on {latestUpdate}"); 
                }
                else
                {
                    await e.Channel.SendMessage("User not found! Note that the capitals on the username must match your username"); 
                }
            }); 

            commands.CreateCommand("Sample").Do(async (e) =>
            {
                string username = e.User.Name; 
                OsuApi.Model.User user = await OsuApi.GetUser.WithUser(username).Result();
                var scores = await OsuApi.GetUserRecent.WithUser(username).Results();
                Score score = scores.First();
                var beatmaps = await OsuApi.GetSpecificBeatmap.WithId(score.BeatmapId).Results();
                Beatmap beatmap = beatmaps.First(); 
                using (var temporaryStream = new MemoryStream())
                {
                    OsuScoreImage.CreateScorePanel(user, score, beatmap).Save(temporaryStream, ImageFormat.Png);
                    temporaryStream.Position = 0;
                    await e.Channel.SendFile("scoreImage.png", temporaryStream);
                }
            }); 
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

        private void RemoveUser(string user)
        {
            LatestUpdate.Remove(user);
            SaveLatestUpdates();
        }

        private void SaveLatestUpdates()
        {
            File.WriteAllLines(OsuScorePath, LatestUpdate.Select(update => $"{update.Key},{update.Value}"));
        }

        private async void SendUserRecentScore()
        {
            Channel osuChannel = Client.FindServers("Too Too Roo").First().FindChannels("osu-scores").First();
            var users = LatestUpdate.Keys.ToArray();
            foreach (string username in users)
            {
                try
                {
                    Score[] UserRecentScores = await OsuApi.GetUserRecent.WithUser(username).Results();
                    foreach (var recentScore in UserRecentScores.OrderBy(score => score.Date))
                    {
                        if (IsNewScore(recentScore) && recentScore.Rank != Rank.F)
                        {
                            UpdateUser(recentScore.Username, recentScore.Date);
                            var beatmap = (await OsuApi.GetSpecificBeatmap.WithId(recentScore.BeatmapId).Results(1)).FirstOrDefault();
                            OsuApi.Model.User user = await OsuApi.GetUser.WithUser(username).Result();
                            using (var temporaryStream = new MemoryStream())
                            {
                                OsuScoreImage.CreateScorePanel(user, recentScore, beatmap).Save(temporaryStream, ImageFormat.Png);
                                temporaryStream.Position = 0;
                                await osuChannel.SendFile("scoreImage.png", temporaryStream);
                            }
                            await Task.Delay(5000);
                            return;
                        }
                    }
                }
                catch { }
            }
        }

        private void UpdateUser(string user, DateTime time)
        {
            LatestUpdate[user] = time;
            SaveLatestUpdates();
        }
    }
}