using Discord;
using Discord.Commands;
using Google.Cloud.Vision.V1;
using Osu;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace ChitoseV2
{
    internal class Osu_ : ICommandSet
    {
        private static readonly Regex BeatmapDifficultyMatcher = new Regex(@"<a class='beatmapTab (active)?' href='\/b\/(?<difficulty_id>\d+).*<span>(?<difficulty_name>.*)<\/span>");
        private static readonly Regex BeatmapUrlMatcher = new Regex(@"(?<full_link>(?<beatmap_link>(https:\/\/)?osu.ppy.sh\/(?<b_s>[bs])\/(?<beatmap_id>\d+))\S*)");
        private static readonly ImageAnnotatorClient ImageAnnotatorClient = ImageAnnotatorClient.Create();
        private static readonly JavaScriptSerializer json = new JavaScriptSerializer();
        private Api api = new Api(Chitose.APIKey);
        private string lastAttachment;
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
                            CleanDiscordString(set ? BM.Title : (BM.Title + " [" + BM.Version + "]")), CleanDiscordString(BM.Artist), BM.DiffApproach, BM.DiffOverall, BM.DiffSize, BM.DiffDrain, Math.Round(Convert.ToDouble(BM.DifficultyRating), 2), Convert.ToInt32(BM.Bpm), BM.Approved.ToString(), BM.Creator, link, ToMinutes(BM.TotalLength)));
                }
                if (e.Message.Attachments.Length == 1)
                {
                    lastAttachment = e.Message.Attachments[0].Url;
                }
                if (e.Message.Embeds.Length == 1)
                {
                    lastAttachment = e.Message.Embeds[0].Url;
                }
                if (e.Message.Text.ToLowerInvariant() == "!beatmap" && lastAttachment != null)
                {
                    try
                    {
                        await e.Message.Delete();
                        var croppedBeatmapImage = AcquireAndCropBeatmapImage(lastAttachment);
                        string temporaryFile = Chitose.TempDirectory + "BeatmapImage.png";
                        FileStream imageFile = File.Create(temporaryFile);
                        croppedBeatmapImage.Save(imageFile, System.Drawing.Imaging.ImageFormat.Png);
                        croppedBeatmapImage.Dispose();
                        imageFile.Close();
                        var image = await Google.Cloud.Vision.V1.Image.FromFileAsync(temporaryFile);
                        File.Delete(temporaryFile);
                        var textList = await ImageAnnotatorClient.DetectTextAsync(image);
                        string[] beatmapInformation = textList.First().Description.Split('\n');
                        string beatmapNameDifficulty = beatmapInformation[0];
                        int locationOfBy = beatmapInformation[1].IndexOf("by");
                        string beatmapCreator = beatmapInformation[1].Substring(locationOfBy + 3);
                        string searchUrl = @"http://osusearch.com/query/?mapper=" + beatmapCreator.UrlEncode();
                        var beatmapSearch = WebRequest.CreateHttp(searchUrl);
                        string resultPage = beatmapSearch.GetResponse().GetResponseStream().ReadString();
                        BeatmapSearchResult searchResult = json.Deserialize<BeatmapSearchResult>(resultPage);
                        int count = searchResult.result_count;
                        Dictionary<int, string> results = searchResult.beatmaps.ToDictionary(beatmap => beatmap.beatmapset_id, beatmap => beatmap.artist + " " + beatmap.title);
                        int attempts = 1;
                        while (results.Count < count)
                        {
                            beatmapSearch = WebRequest.CreateHttp(searchUrl + "&offset=" + attempts++);
                            resultPage = beatmapSearch.GetResponse().GetResponseStream().ReadString();
                            searchResult = json.Deserialize<BeatmapSearchResult>(resultPage);
                            results = results.Concat(searchResult.beatmaps.ToDictionary(beatmap => beatmap.beatmapset_id, beatmap => beatmap.artist + " " + beatmap.title)).ToDictionary(pair => pair.Key, pair => pair.Value);
                        }
                        KeyValuePair<int, string> bestResult = results.OrderByDescending(result => Extensions.CalculateSimilarity(result.Value, beatmapNameDifficulty)).First();
                        int bestIndex = -1;
                        double bestSimiliarity = 0.0;
                        for (int i = beatmapNameDifficulty.Length; i > 0; i--)
                        {
                            double similarity = Extensions.CalculateSimilarity(bestResult.Value, beatmapNameDifficulty.Substring(0, i));
                            if (similarity > bestSimiliarity)
                            {
                                bestIndex = i;
                                bestSimiliarity = similarity;
                            }
                        }
                        string name = beatmapNameDifficulty.Substring(0, bestIndex);
                        string difficulty = beatmapNameDifficulty.Substring(bestIndex);
                        var beatmapDifficultySearch = WebRequest.CreateHttp(@"https://osu.ppy.sh/s/" + bestResult.Key);
                        resultPage = beatmapDifficultySearch.GetResponse().GetResponseStream().ReadString();
                        var match = BeatmapDifficultyMatcher.Match(resultPage);
                        results.Clear();
                        while (match.Success)
                        {
                            int id = int.Parse(match.Groups["difficulty_id"].Value);
                            string difficultyName = match.Groups["difficulty_name"].Value.HtmlDecode();
                            results[id] = difficultyName;
                            match = match.NextMatch();
                        }
                        File.WriteAllText(Chitose.TempDirectory + "Html.txt", resultPage);
                        bestResult = results.OrderByDescending(result => Extensions.CalculateSimilarity(result.Value, difficulty)).First();
                        ReadOnlyCollection<Beatmap> beatmaps = await Api.GetBeatmapsAsync(Chitose.APIKey, null, null, bestResult.Key, null, null, false, null, 10);
                        Beatmap selectedBeatmap = beatmaps.FirstOrDefault();
                        await e.Channel.SendMessage(string.Format("__***{0}***__ by ***{1}*** \n **Created by *{9}***  |  **Status : *{8}*** \n ***Download Link*** : **{10}** \n **Beatmap Info**\n```Ar {2} | Od {3} | Cs {4} | Hp {5} | Stars {6} | BPM {7} | Length {11}``` \n ",
                                CleanDiscordString(selectedBeatmap.Title + " [" + selectedBeatmap.Version + "]"), CleanDiscordString(selectedBeatmap.Artist), selectedBeatmap.DiffApproach, selectedBeatmap.DiffOverall, selectedBeatmap.DiffSize, selectedBeatmap.DiffDrain, Math.Round(Convert.ToDouble(selectedBeatmap.DifficultyRating), 2), Convert.ToInt32(selectedBeatmap.Bpm), selectedBeatmap.Approved.ToString(), selectedBeatmap.Creator, @"https://osu.ppy.sh/b/" + bestResult.Key, ToMinutes(selectedBeatmap.TotalLength)));
                    }
                    catch (Exception exception)
                    {
                    }
                }
            };
        }

        private Bitmap AcquireAndCropBeatmapImage(string url)
        {
            var request = WebRequest.CreateHttp(url);
            Bitmap beatmapImage = new Bitmap(request.GetResponse().GetResponseStream());
            int height = (int)(beatmapImage.Height * 0.123f);
            Bitmap songInfo = beatmapImage.Clone(new Rectangle(0, 0, beatmapImage.Width, height), System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            beatmapImage.Dispose();
            return songInfo;
        }

        private string CleanDiscordString(string text) => Regex.Replace(text, @"\*", @" ");

        private string ToMinutes(int? seconds) => TimeSpan.FromSeconds(seconds.Value).ToString(@"m\:ss");

#pragma warning disable 0649

        private class BeatmapSearchResult
        {
            public Beatmap[] beatmaps;
            public int result_count;

            public class Beatmap
            {
                public string artist;
                public int beatmapset_id;
                public string title;
            }
        }
    }

#pragma warning restore 0649
}