using Discord;
using Discord.Commands;
using Google.Cloud.Vision.V1;
using OsuApi;
using OsuApi.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Sd = System.Drawing;

namespace ChitoseV2
{
    internal class Osu_ : ICommandSet
    {
        private static readonly ImageAnnotatorClient ImageAnnotatorClient = ImageAnnotatorClient.Create();
        private static readonly JavaScriptSerializer json = new JavaScriptSerializer();
        private static readonly Regex NewBeatmapUrlMatcher = new Regex(@"(?<full_link>(https:\/\/)?osu.ppy.sh\/beatmapsets\/(?<beatmap_set_id>\d+)(#osu\/(?<beatmap_id>\d+))?\S*)");
        private static readonly Regex OldBeatmapUrlMatcher = new Regex(@"(?<full_link>(https:\/\/)?osu.ppy.sh\/(?<b_s>[bs])\/(?<beatmap_id>\d+)\S*)");
        private static readonly Api OsuApi = new Api(Chitose.APIKey);
        private string lastAttachment;
        private string beatmap { get; set; }

        public void AddCommands(DiscordClient client, CommandService commands)
        {
            commands.CreateCommand("user").Parameter("user").Do(async (e) =>
            {
                string username = string.Join(" ", e.Args);
                OsuApi.Model.User user = await OsuApi.GetUser.WithUser(username).Result();
                Score[] bestScores = await OsuApi.GetBestPlay.WithId(username).Result(10);
                Score bestScore = bestScores.First();
                var beatmaps = await OsuApi.GetSpecificBeatmap.WithId(bestScore.BeatmapId).Results();
                Beatmap beatmap = beatmaps.First();
                string userInformation = FormatUserInformation(user, bestScore, beatmap);
                await e.Channel.SendMessage(userInformation);
                if (user == null)
                {
                    await e.Channel.SendMessage($"User \"{username}\" not found");
                }
            });

            commands.CreateCommand("scores").Parameter("beatmapid").Do(async (e) =>
            {
                Score[] scores = await OsuApi.GetScores.OnBeatmapWithId(e.GetArg("beatmapid")).WithMods(Mods.Nightcore).Results();
                Console.WriteLine(scores.Length);
            });

            commands.CreateCommand("beatmap").Parameter("creator", ParameterType.Unparsed).Do(async (e) =>
            {
                try
                {
                    await e.Message.Delete();

                    Image image = null;
                    try
                    {
                        using (var temporaryStream = new MemoryStream())
                        using (Sd.Bitmap croppedBeatmapImage = AcquireAndCropBeatmapImage(lastAttachment))
                        {
                            croppedBeatmapImage.Save(temporaryStream, Sd.Imaging.ImageFormat.Png);
                            temporaryStream.Position = 0;
                            image = await Image.FromStreamAsync(temporaryStream);
                        }
                    }
                    catch (Exception exception)
                    {
                        throw new BeatmapAnalysisException("Failed to save image", exception);
                    }

                    var textList = await ImageAnnotatorClient.DetectTextAsync(image);
                    string[] beatmapInformation = textList.First().Description.Split('\n');
                    string beatmapNameAndDifficulty = beatmapInformation[0];
                    int locationOfBy = beatmapInformation[1].IndexOf("by");
                    string beatmapper = (e.GetArg("creator") != string.Empty) ? e.GetArg("creator") : beatmapInformation[1].Substring(locationOfBy + 3);
                    IEnumerable<BeatmapSetResult> sortedBeatmaps = GetBeatmapsByMapper(beatmapper)
                        .OrderByDescending(result => Extensions.CalculateSimilarity(result.Name, beatmapNameAndDifficulty));
                    BeatmapSetResult beatmapResult = sortedBeatmaps.FirstOrDefault();
                    if (beatmapResult == null)
                        throw new BeatmapAnalysisException("Failed to detect creator. Try the command again by specifying the creator.");

                    var splitIndex = -1;
                    var bestSimilarity = 0.0;
                    for (var candidateSplitIndex = 0; candidateSplitIndex <= beatmapNameAndDifficulty.Length; candidateSplitIndex++)
                    {
                        var candidateSimilarity = Extensions.CalculateSimilarity(beatmapResult.Name, beatmapNameAndDifficulty.Substring(0, candidateSplitIndex));
                        if (candidateSimilarity > bestSimilarity)
                        {
                            splitIndex = candidateSplitIndex;
                            bestSimilarity = candidateSimilarity;
                        }
                    }
                    var beatmapName = beatmapNameAndDifficulty.Substring(0, splitIndex);
                    var difficultyName = beatmapNameAndDifficulty.Substring(splitIndex);

                    IEnumerable<Beatmap> potentialBeatmaps = Enumerable.Empty<Beatmap>();
                    foreach (BeatmapSetResult potentialBeatmapResult in sortedBeatmaps.TakeWhile(result => Extensions.CalculateSimilarity(result.Name, beatmapName) / bestSimilarity > 0.99))
                    {
                        potentialBeatmaps = potentialBeatmaps.Concat(await OsuApi.GetBeatmapSet.WithSetId(potentialBeatmapResult.SetId).Results(20));
                    }
                    var selectedBeatmap = potentialBeatmaps.OrderByDescending(beatmap => Extensions.CalculateSimilarity(beatmap.Difficulty, difficultyName)).FirstOrDefault();
                    if (selectedBeatmap == null)
                        throw new BeatmapAnalysisException("Failed to retrieve beatmap");
                    await e.Channel.SendMessage(FormatBeatmapInformation(selectedBeatmap));
                }
                catch (BeatmapAnalysisException exception)
                {
                    await e.Channel.SendMessage("Analysis failed: " + exception.Message);
                }
            });

            commands.CreateCommand("leaderboard").Parameter("beatmap").Do(async (e) =>
            {
                try
                {
                    BeatmapResult result = ExtractBeatmapFromText(e.GetArg("beatmap"));
                    if (result == null)
                        throw new Exception("Invalid beatmap URL");
                    var beatmaps = await (result.IsSet ? OsuApi.GetBeatmapSet.WithSetId(result.Id).Results(20) : OsuApi.GetSpecificBeatmap.WithId(result.Id).Results(20));
                    Beatmap beatmap = beatmaps.FirstOrDefault();
                    if (beatmap == null)
                        throw new Exception("Unable to access beatmap URL");

                    Score[] scores = await OsuApi.GetScores.OnBeatmapWithId(beatmap.BeatmapId).Results(25);
                    await e.Channel.SendMessage(FormatLeaderboardInformation(scores, beatmap));
                }
                catch (Exception exception)
                {
                    await e.Channel.SendMessage(exception.Message);
                }
            });

            client.MessageReceived += async (s, e) =>
            {
                if (e.Message.User.IsBot == false && e.Message.Text[0] != '!')
                {
                    foreach (BeatmapResult result in ExtractBeatmapsFromText(e.Message.Text))
                    {
                        if (result.FullLink == e.Message.Text)
                        {
                            await e.Message.Delete();
                        }
                        Beatmap[] beatmaps = await (result.IsSet ? OsuApi.GetBeatmapSet.WithSetId(result.Id).Results() : OsuApi.GetSpecificBeatmap.WithId(result.Id).Results(1));
                        if (beatmaps.Length > 0)
                        {
                            await e.Channel.SendFile(new Uri($"https://assets.ppy.sh/beatmaps/{beatmaps[0].BeatmapSetId}/covers/cover.jpg"));
                            await e.Channel.SendMessage(result.IsSet ? FormatBeatmapSetInformation(new BeatmapSet(beatmaps)) : FormatBeatmapInformation(beatmaps.First()));
                        }
                        await Task.Delay(1000);
                    }
                }

                if (e.Message.Attachments.Length == 1)
                {
                    lastAttachment = e.Message.Attachments[0].Url;
                }
                if (e.Message.Embeds.Length == 1)
                {
                    lastAttachment = e.Message.Embeds[0].Url;
                }
            }; //Beatmap Info
        }

        private static List<BeatmapSetResult> GetBeatmapsByMapper(string beatmapper)
        {
            var beatmapQueryUrl = $"http://osusearch.com/query/?mapper={beatmapper.UrlEncode()}";
            var beatmapQueryRequest = WebRequest.CreateHttp(beatmapQueryUrl);
            string queryResponse = beatmapQueryRequest.GetResponse().GetResponseStream().ReadString();
            var searchResult = json.Deserialize<BeatmapSearchResult>(queryResponse);
            int resultCount = searchResult.result_count;
            List<BeatmapSetResult> beatmapResults = searchResult.beatmaps.Select(result => new BeatmapSetResult(result)).ToList();
            int queryAttempts = 1;
            while (beatmapResults.Count < resultCount)
            {
                var additionalQueryRequest = WebRequest.CreateHttp($"{beatmapQueryUrl}&offset={queryAttempts++}");
                queryResponse = additionalQueryRequest.GetResponse().GetResponseStream().ReadString();
                searchResult = json.Deserialize<BeatmapSearchResult>(queryResponse);
                beatmapResults.AddRange(searchResult.beatmaps.Select(result => new BeatmapSetResult(result)));
            }
            return beatmapResults;
        }

        private Sd.Bitmap AcquireAndCropBeatmapImage(string url)
        {
            var request = WebRequest.CreateHttp(url);
            using (var beatmapImage = new Sd.Bitmap(request.GetResponse().GetResponseStream()))
            {
                int headerHeight = (int)(beatmapImage.Height * 0.123f);
                Sd.Bitmap header = beatmapImage.Clone(new Sd.Rectangle(0, 0, beatmapImage.Width, headerHeight), Sd.Imaging.PixelFormat.Format32bppRgb);
                return header;
            }
        }

        private string CleanDiscordString(string text) => Regex.Replace(text, @"\*", @" ");

        private BeatmapResult ExtractBeatmapFromText(string text)
        {
            return ExtractBeatmapsFromText(text).FirstOrDefault();
        }

        private IEnumerable<BeatmapResult> ExtractBeatmapsFromText(string text)
        {
            var oldBeatmaps = ExtractOldBeatmapsFromText(text);
            var newBeatmaps = ExtractNewBeatmapsFromText(text);
            var sortedBeatmaps = oldBeatmaps.Concat(newBeatmaps).OrderBy(beatmap => beatmap.Key).Select(beatmap => beatmap.Value);
            var sentBeatmaps = new HashSet<BeatmapResult>();
            foreach (BeatmapResult result in sortedBeatmaps)
            {
                if (!sentBeatmaps.Contains(result))
                {
                    sentBeatmaps.Add(result);
                    yield return result;
                }
            }
        }

        private IEnumerable<KeyValuePair<int, BeatmapResult>> ExtractNewBeatmapsFromText(string text)
        {
            var match = NewBeatmapUrlMatcher.Match(text);
            while (match.Success)
            {
                bool isSet = !match.Groups["beatmap_id"].Success;
                string beatmapId = match.Groups[isSet ? "beatmap_set_id" : "beatmap_id"].Value;
                yield return new KeyValuePair<int, BeatmapResult>
                (
                    match.Index,
                    new BeatmapResult
                    {
                        FullLink = match.Value,
                        IsSet = isSet,
                        Id = beatmapId
                    }
                );
                match = match.NextMatch();
            }
        }

        private IEnumerable<KeyValuePair<int, BeatmapResult>> ExtractOldBeatmapsFromText(string text)
        {
            var match = OldBeatmapUrlMatcher.Match(text);
            while (match.Success)
            {
                bool isSet = match.Groups["b_s"].Value == "s";
                string beatmapId = match.Groups["beatmap_id"].Value;
                yield return new KeyValuePair<int, BeatmapResult>
                (
                    match.Index,
                    new BeatmapResult
                    {
                        FullLink = match.Value,
                        IsSet = isSet,
                        Id = beatmapId
                    }
                );
                match = match.NextMatch();
            }
        }

        private string FormatBeatmapInformation(Beatmap beatmap)
        {
            return new StringBuilder()
                .AppendLine($"__***{CleanDiscordString(beatmap.Title)} [{CleanDiscordString(beatmap.Difficulty)}]***__  by ***{CleanDiscordString(beatmap.Artist)}***")
                .AppendLine($"**Created by *{beatmap.Beatmapper}***  |  **Status : *{beatmap.Status}***")
                .AppendLine($"***Download Link*** : **https://osu.ppy.sh/beatmapsets/{beatmap.BeatmapSetId}#osu/{beatmap.BeatmapId}**")
                .AppendLine("**Beatmap Info**")
                .AppendLine("```")
                .Append($"AR {beatmap.ApproachRate} | OD {beatmap.OverallDifficulty} | CS {beatmap.CircleSize} | HP {beatmap.HealthDrain} | ")
                .AppendLine($"Stars {beatmap.Stars:#.##} | BPM {beatmap.Bpm:#.##} | Length {ToMinutes(beatmap.TotalLength)}")
                .AppendLine("```")
                .ToString();
        }

        private string FormatBeatmapSetInformation(BeatmapSet beatmapSet)
        {
            return new StringBuilder()
                .AppendLine($"__***{CleanDiscordString(beatmapSet.Title)}***__  by ***{CleanDiscordString(beatmapSet.Artist)}***")
                .AppendLine($"**Created by *{beatmapSet.Beatmapper}***  |  **Status : *{beatmapSet.Status}***")
                .AppendLine($"***Download Link*** : **https://osu.ppy.sh/beatmapsets/{beatmapSet.Id}**")
                .AppendLine("**Beatmap Info**")
                .AppendLine("```")
                .Append($"AR {beatmapSet.ApproachRate} | OD {beatmapSet.OverallDifficulty} | CS {beatmapSet.CircleSize} | HP {beatmapSet.HealthDrain} | ")
                .AppendLine($"Stars {beatmapSet.Stars.Format("#.##")} | BPM {beatmapSet.Bpm:#.##} | Length {ToMinutes(beatmapSet.Length)}")
                .AppendLine("```")
                .ToString();
        }

        private string FormatLeaderboardInformation(Score[] scores, Beatmap beatmap)
        {
            StringBuilder leaderboard = new StringBuilder();
            int rank = 1;
            leaderboard.AppendLine($"__***{CleanDiscordString(beatmap.Title)} [{CleanDiscordString(beatmap.Difficulty)}]***__  by ***{CleanDiscordString(beatmap.Artist)}***");
            leaderboard.AppendLine($"```");
            foreach (Score score in scores)
            {
                leaderboard.AppendLine($"#{rank}  {score.Username}:  Score {score.TotalScore} - {score.PP}pp - Max Combo {score.Combo} | Mods: {score.Mods}");
                rank++;
            }
            leaderboard.AppendLine($"```");
            return leaderboard.ToString();
        }

        private string FormatUserInformation(OsuApi.Model.User user, Score bestPlay, Beatmap beatmap)
        {
            var userInformation = new StringBuilder()
                    .AppendLine($"**{user.Username}**")
                    .AppendLine($"User Best: **{beatmap.Title} [{beatmap.Difficulty}]** giving __**{bestPlay.PP}pp**__")
                    .AppendLine("```")
                    .AppendLine($"Rank: {user.Rank}")
                    .AppendLine($"Performance Points: {user.PP}")
                    .AppendLine($"Country: {user.Country}")
                    .AppendLine($"Country Rank: {user.CountryRank}")
                    .AppendLine($"Level: {user.Level}")
                    .AppendLine("```");
            return userInformation.ToString();
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
            public string FullLink;
            public string Id;
            public bool IsSet;

            public override bool Equals(object obj)
            {
                return GetHashCode() == obj.GetHashCode();
            }

            public override int GetHashCode()
            {
                return $"{Id}:{IsSet}".GetHashCode();
            }
        }

        private class BeatmapSearchResult
        {
            public Beatmap[] beatmaps;
            public int result_count;

            public class Beatmap
            {
                public string artist;
                public string beatmapset_id;
                public string title;
            }
        }

        private class BeatmapSet
        {
            public Interval ApproachRate { get; private set; }
            public string Artist { get; private set; }
            public string Beatmapper { get; private set; }
            public double Bpm { get; private set; }
            public Interval CircleSize { get; private set; }
            public ReadOnlyCollection<string> Difficulties { get; private set; }
            public Interval HealthDrain { get; private set; }
            public string Id { get; private set; }
            public int Length { get; private set; }
            public Interval OverallDifficulty { get; private set; }
            public Interval Stars { get; private set; }
            public string Status { get; private set; }
            public string Title { get; private set; }

            public BeatmapSet(IEnumerable<Beatmap> beatmaps)
            {
                ApproachRate = new Interval(beatmaps.Select(beatmap => beatmap.ApproachRate));
                Artist = beatmaps.First().Artist;
                Bpm = beatmaps.First().Bpm;
                CircleSize = new Interval(beatmaps.Select(beatmap => beatmap.CircleSize));
                Beatmapper = beatmaps.First().Beatmapper;
                Difficulties = beatmaps.Select(beatmap => beatmap.Difficulty).ToList().AsReadOnly();
                HealthDrain = new Interval(beatmaps.Select(beatmap => beatmap.HealthDrain));
                Id = beatmaps.First().BeatmapSetId;
                Length = beatmaps.First().TotalLength;
                OverallDifficulty = new Interval(beatmaps.Select(beatmap => beatmap.OverallDifficulty));
                Stars = new Interval(beatmaps.Select(beatmap => beatmap.Stars));
                Status = beatmaps.First().Status.ToString();
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
                    if (maximum == minimum)
                        return string.Format($"{{0:{format}}}", maximum);
                    return string.Format($"{{0:{format}}}-{{1:{format}}}", minimum, maximum);
                }

                public override string ToString() => maximum == minimum ? $"{maximum}" : $"{minimum}-{maximum}";
            }
        }

        private class BeatmapSetResult
        {
            public string Name { get; private set; }
            public string SetId { get; private set; }

            public BeatmapSetResult(BeatmapSearchResult.Beatmap beatmap)
            {
                Name = beatmap.artist + " - " + beatmap.title;
                SetId = beatmap.beatmapset_id;
            }
        }

#pragma warning disable 0649
#pragma warning restore 0649

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