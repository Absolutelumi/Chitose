using Discord;
using Discord.Commands;
using Google.Cloud.Vision.V1;
using Osu;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
                    if (match.Groups["full_link"].Value == e.Message.Text)
                    {
                        await e.Message.Delete();
                    }
                    var beatmaps = await Api.GetBeatmapsAsync(Chitose.APIKey, null, set ? BMID : null, set ? null : BMID, null, null, false, null, set ? 20 : 1);
                    await e.Channel.SendMessage(set ? FormatBeatmapSetInformation(new BeatmapSet(beatmaps)) : FormatBeatmapInformation(beatmaps.First()));
                }

                if (e.Message.Attachments.Length == 1)
                {
                    lastAttachment = e.Message.Attachments[0].Url;
                }
                if (e.Message.Embeds.Length == 1)
                {
                    lastAttachment = e.Message.Embeds[0].Url;
                }
                if (e.Message.Text.ToLowerInvariant() == "!beatmap" && (e.Message.Attachments.Length != 0 || lastAttachment != null))
                {
                    try
                    {
                        if (e.Message.Attachments.Length == 0)
                        {
                            await e.Message.Delete();
                        }
                        else
                        {
                            lastAttachment = e.Message.Attachments[0].Url;
                        }

                        Image image = null;
                        string temporaryFilePath = Chitose.TempDirectory + "BeatmapImage.png";
                        try
                        {
                            using (var temporaryFile = File.Create(temporaryFilePath))
                            using (var croppedBeatmapImage = AcquireAndCropBeatmapImage(lastAttachment))
                            {
                                croppedBeatmapImage.Save(temporaryFile, System.Drawing.Imaging.ImageFormat.Png);
                            }
                            image = await Image.FromFileAsync(temporaryFilePath);
                            File.Delete(temporaryFilePath);
                        }
                        catch (Exception ex)
                        {
                            throw new BeatmapAnalysisException("Failed to save image", ex);
                        }

                        var textList = await ImageAnnotatorClient.DetectTextAsync(image);
                        string[] beatmapInformation = textList.First().Description.Split('\n');
                        string beatmapNameAndDifficulty = beatmapInformation[0];
                        int locationOfBy = beatmapInformation[1].IndexOf("by");
                        string beatmapper = beatmapInformation[1].Substring(locationOfBy + 3);
                        BeatmapResult beatmapResult = GetBeatmapsByMapper(beatmapper).OrderByDescending(result => Extensions.CalculateSimilarity(result.Name, beatmapNameAndDifficulty)).FirstOrDefault();
                        if (beatmapResult == null)
                            throw new BeatmapAnalysisException("Failed to detect creator");

                        int splitIndex = -1;
                        double bestSimilarity = 0.0;
                        for (int candidateSplitIndex = 0; candidateSplitIndex <= beatmapNameAndDifficulty.Length; candidateSplitIndex++)
                        {
                            double candidateSimilarity = Extensions.CalculateSimilarity(beatmapResult.Name, beatmapNameAndDifficulty.Substring(0, candidateSplitIndex));
                            if (candidateSimilarity > bestSimilarity)
                            {
                                splitIndex = candidateSplitIndex;
                                bestSimilarity = candidateSimilarity;
                            }
                        }
                        string beatmapName = beatmapNameAndDifficulty.Substring(0, splitIndex);
                        string difficultyName = beatmapNameAndDifficulty.Substring(splitIndex);

                        var beatmapUrl = @"https://osu.ppy.sh/s/" + beatmapResult.SetId;
                        var beatmapPageSource = WebRequest.CreateHttp(beatmapUrl).GetResponse().GetResponseStream().ReadString();
                        var difficulties = BeatmapDifficultyMatcher.Matches(beatmapPageSource).Cast<System.Text.RegularExpressions.Match>().Select(match =>
                        {
                            var id = int.Parse(match.Groups["difficulty_id"].Value);
                            var name = match.Groups["difficulty_name"].Value.HtmlDecode();
                            return new Difficulty(name, id);
                        });
                        Difficulty beatmapDifficulty = difficulties.OrderByDescending(difficulty => Extensions.CalculateSimilarity(difficulty.Name, difficultyName)).FirstOrDefault();
                        if (beatmapDifficulty == null)
                            throw new BeatmapAnalysisException("Failed to access beatmap");
                        ReadOnlyCollection<Beatmap> beatmaps = await Api.GetBeatmapsAsync(Chitose.APIKey, null, null, beatmapDifficulty.Id, null, null, false, null, 10);
                        Beatmap selectedBeatmap = beatmaps.FirstOrDefault();
                        await e.Channel.SendMessage(FormatBeatmapInformation(selectedBeatmap));
                    }
                    catch (BeatmapAnalysisException exception)
                    {
                        await e.Channel.SendMessage("Analysis failed: " + exception.Message);
                    }
                }
            };
        }

        private static List<BeatmapResult> GetBeatmapsByMapper(string beatmapper)
        {
            string beatmapQueryUrl = @"http://osusearch.com/query/?mapper=" + beatmapper.UrlEncode();
            var beatmapQueryRequest = WebRequest.CreateHttp(beatmapQueryUrl);
            string queryResponse = beatmapQueryRequest.GetResponse().GetResponseStream().ReadString();
            BeatmapSearchResult searchResult = json.Deserialize<BeatmapSearchResult>(queryResponse);
            int resultCount = searchResult.result_count;
            List<BeatmapResult> beatmapResults = searchResult.beatmaps.Select(result => new BeatmapResult(result)).ToList();
            int queryAttempts = 1;
            while (beatmapResults.Count < resultCount)
            {
                var additionalQueryRequest = WebRequest.CreateHttp(beatmapQueryUrl + "&offset=" + queryAttempts++);
                queryResponse = additionalQueryRequest.GetResponse().GetResponseStream().ReadString();
                searchResult = json.Deserialize<BeatmapSearchResult>(queryResponse);
                beatmapResults = beatmapResults.Concat(searchResult.beatmaps.Select(result => new BeatmapResult(result))).ToList();
            }
            return beatmapResults;
        }

        private System.Drawing.Bitmap AcquireAndCropBeatmapImage(string url)
        {
            var request = WebRequest.CreateHttp(url);
            using (var beatmapImage = new System.Drawing.Bitmap(request.GetResponse().GetResponseStream()))
            {
                int headerHeight = (int)(beatmapImage.Height * 0.123f);
                System.Drawing.Bitmap header = beatmapImage.Clone(new System.Drawing.Rectangle(0, 0, beatmapImage.Width, headerHeight), System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                return header;
            }
        }

        private string CleanDiscordString(string text) => Regex.Replace(text, @"\*", @" ");

        private string FormatBeatmapInformation(Beatmap beatmap)
        {
            StringBuilder informationBuilder = new StringBuilder();
            informationBuilder.AppendLine($"__***{CleanDiscordString(beatmap.Title)} [{CleanDiscordString(beatmap.Version)}]***__  by ***{CleanDiscordString(beatmap.Artist)}***");
            informationBuilder.AppendLine($"**Created by *{beatmap.Creator}***  |  **Status : *{beatmap.Approved}***");
            informationBuilder.AppendLine($"***Download Link*** : **https://osu.ppy.sh/b/{beatmap.BeatmapId}**");
            informationBuilder.AppendLine("**Beatmap Info**");
            informationBuilder.AppendLine("```");
            informationBuilder.AppendLine($"AR {beatmap.DiffApproach} | OD {beatmap.DiffOverall} | CS {beatmap.DiffSize} | HP {beatmap.DiffDrain} | Stars {beatmap.DifficultyRating:#.##} | BPM {beatmap.Bpm:#.##} | Length {ToMinutes(beatmap.TotalLength)}");
            informationBuilder.AppendLine("```");
            return informationBuilder.ToString();
        }

        private string FormatBeatmapSetInformation(BeatmapSet beatmapSet)
        {
            StringBuilder informationBuilder = new StringBuilder();
            informationBuilder.AppendLine($"__***{CleanDiscordString(beatmapSet.Title)}***__  by ***{CleanDiscordString(beatmapSet.Artist)}***");
            informationBuilder.AppendLine($"**Created by *{beatmapSet.Creator}***  |  **Status : *{beatmapSet.Status}***");
            informationBuilder.AppendLine($"***Download Link*** : **https://osu.ppy.sh/s/{beatmapSet.Id}**");
            informationBuilder.AppendLine("**Beatmap Info**");
            informationBuilder.AppendLine("```");
            informationBuilder.AppendLine($"AR {beatmapSet.ApproachRate} | OD {beatmapSet.OverallDifficulty} | CS {beatmapSet.CircleSize} | HP {beatmapSet.HealthDrain} | Stars {beatmapSet.Stars.Format("#.##")} | BPM {beatmapSet.Bpm:#.##} | Length {ToMinutes(beatmapSet.Length)}");
            informationBuilder.AppendLine("```");
            return informationBuilder.ToString();
        }

        private string ToMinutes(int? seconds) => TimeSpan.FromSeconds(seconds.Value).ToString(@"m\:ss");

        private class BeatmapAnalysisException : Exception
        {
            public BeatmapAnalysisException() : base()
            {
            }

            public BeatmapAnalysisException(string message) : base(message)
            {
            }

            public BeatmapAnalysisException(string message, Exception inner) : base(message, inner)
            {
            }
        }

        private class BeatmapResult
        {
            public string Name { get; private set; }
            public int SetId { get; private set; }

            public BeatmapResult(BeatmapSearchResult.Beatmap beatmap)
            {
                Name = beatmap.artist + " - " + beatmap.title;
                SetId = beatmap.beatmapset_id;
            }
        }

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

#pragma warning restore 0649

        private class BeatmapSet
        {
            public Interval ApproachRate { get; private set; }
            public string Artist { get; private set; }
            public double Bpm { get; private set; }
            public Interval CircleSize { get; private set; }
            public string Creator { get; private set; }
            public ReadOnlyCollection<string> Difficulties { get; private set; }
            public Interval HealthDrain { get; private set; }
            public int Id { get; private set; }
            public int Length { get; private set; }
            public Interval OverallDifficulty { get; private set; }
            public Interval Stars { get; private set; }
            public string Status { get; private set; }
            public string Title { get; private set; }

            public BeatmapSet(IEnumerable<Beatmap> beatmaps)
            {
                ApproachRate = new Interval(beatmaps.Select(beatmap => beatmap.DiffApproach.Value));
                Artist = beatmaps.First().Artist;
                Bpm = beatmaps.First().Bpm.Value;
                CircleSize = new Interval(beatmaps.Select(beatmap => beatmap.DiffSize.Value));
                Creator = beatmaps.First().Creator;
                Difficulties = beatmaps.Select(beatmap => beatmap.Version).ToList().AsReadOnly();
                HealthDrain = new Interval(beatmaps.Select(beatmap => beatmap.DiffDrain.Value));
                Id = (int)beatmaps.First().BeatmapSetId.Value;
                Length = beatmaps.First().TotalLength.Value;
                OverallDifficulty = new Interval(beatmaps.Select(beatmap => beatmap.DiffOverall.Value));
                Stars = new Interval(beatmaps.Select(beatmap => beatmap.DifficultyRating.Value));
                Status = beatmaps.First().Approved.ToString();
                Title = beatmaps.First().Title;
            }

            public class Interval
            {
                private double maximum;
                private double minimum;

                public Interval(IEnumerable<double> values)
                {
                    minimum = values.Min();
                    maximum = values.Max();
                }

                public string Format(string format)
                {
                    return maximum == minimum ? string.Format($"{{0:{format}}}", maximum) : string.Format($"{{0:{format}}}-{{1:{format}}}", minimum, maximum);
                }

                public override string ToString()
                {
                    return maximum == minimum ? $"{maximum}" : $"{minimum}-{maximum}";
                }
            }
        }

        private class Difficulty
        {
            public int Id { get; private set; }
            public string Name { get; private set; }

            public Difficulty(string name, int id)
            {
                Name = name;
                Id = id;
            }
        }
    }
}