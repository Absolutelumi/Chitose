using Discord;
using Discord.Commands;
using Osu;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace ChitoseV2
{
    internal class Osu_ : ICommandSet
    {
        private static readonly Regex BeatmapUrlMatcher = new Regex(@"(?<full_link>(?<beatmap_link>(https:\/\/)?osu.ppy.sh\/(?<b_s>[bs])\/(?<beatmap_id>\d+))\S*)");
        private Api api = new Api(Chitose.APIKey);
        private string beatmap { get; set; }

        public void AddCommands(DiscordClient client, CommandService commands)
        {
            commands.CreateCommand("user").Parameter("user").Do(async (e) =>
            {
                string user = string.Join(" ", e.Args);
                var userStats = await api.GetUserAsync(user);
                int? userId = userStats.UserId;
                if (userId.HasValue)
                {
                    var userBestList = await api.GetUserBestAsync(userId);
                    var userBest = userBestList.FirstOrDefault();
                    var Beatmaps = await Api.GetBeatmapsAsync(Chitose.APIKey, null, null, userBest.BeatmapId, null, null, false, null, 10);
                    Beatmap BestPlayMap = Beatmaps.FirstOrDefault();
                    await e.Channel.SendMessage(string.Format("**{0}**", userStats.Username));
                    await e.Channel.SendMessage(string.Format("User Best: **{0} [{1}]** giving __**{2}pp**__", BestPlayMap.Title, BestPlayMap.Version, userBest.PP));
                    await e.Channel.SendMessage(string.Format("```Rank: {0} \nPerformance Points: {4} \nCountry: {1} \nCountry Rank: {2} \nLevel: {3}```", userStats.PPRank, userStats.Country, userStats.PPCountryRank, userStats.Level, userStats.PPRaw));
                }
                else
                {
                    await e.Channel.SendMessage(string.Format("User \"{0}\" not found", user));
                }
            });

            client.MessageReceived += async (s, e) =>
            {
                //Beatmap Info
                if (BeatmapUrlMatcher.IsMatch(e.Message.Text) && e.Message.User.IsBot == false && e.Message.Text.Split(' ').Length == 1)
                {
                    var match = BeatmapUrlMatcher.Match(e.Message.Text);
                    string link = match.Groups["beatmap_link"].Value;
                    bool set = match.Groups["b_s"].Value == "s";
                    long? BMID = long.Parse(match.Groups["beatmap_id"].Value);

                    ReadOnlyCollection<Beatmap> Beatmaps = await Api.GetBeatmapsAsync(Chitose.APIKey, null, set ? BMID : null, set ? null : BMID, null, null, false, null, 10);
                    Beatmap BM = Beatmaps.FirstOrDefault();

                    if (match.Groups["full_link"].Value == e.Message.Text)
                    {
                        await e.Message.Delete();
                    }
                    await e.Channel.SendMessage(string.Format("__***{0}***__ by ***{1}*** \n **Created by *{9}***  |  **Status : *{8}*** \n ***Download Link*** : **{10}** \n **Beatmap Info**\n```Ar {2} | Od {3} | Cs {4} | Hp {5} | Stars {6} | BPM {7} | Length {11}``` \n ",
                            CleanDiscordString(BM.Title), CleanDiscordString(BM.Artist), BM.DiffApproach, BM.DiffOverall, BM.DiffSize, BM.DiffDrain, Math.Round(Convert.ToDouble(BM.DifficultyRating), 2), Convert.ToInt32(BM.Bpm), BM.Approved.ToString(), BM.Creator, link, ToMinutes(BM.TotalLength)));
                }
            };
        }

        private string CleanDiscordString(string text) => Regex.Replace(text, @"\*", @" ");

        private string ToMinutes(int? seconds) => TimeSpan.FromSeconds(seconds.Value).ToString(@"m\:ss");
    }
}