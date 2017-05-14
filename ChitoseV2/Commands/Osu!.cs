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
using Sd = System.Drawing;

namespace ChitoseV2
{
    internal class Osu_ : ICommandSet
    {
        private static readonly Regex BeatmapUrlMatcher = new Regex(@"(?<full_link>(?<beatmap_link>(https:\/\/)?osu.ppy.sh\/(?<b_s>[bs])\/(?<beatmap_id>\d+))\S*)");
        private static readonly ImageAnnotatorClient ImageAnnotatorClient = ImageAnnotatorClient.Create();
        private static readonly JavaScriptSerializer json = new JavaScriptSerializer();
        private static readonly Api OsuApi = new Api(Chitose.APIKey);
        private string lastAttachment;
        private string beatmap { get; set; }

        public void AddCommands(DiscordClient client, CommandService commands)
        {
            commands.CreateCommand("user").Parameter("user").Do(async (e) =>
            {
                string username = string.Join(" ", e.Args);
                Osu.User user = await OsuApi.GetUserAsync(username);
                if (user.UserId.HasValue)
                {
                    ReadOnlyCollection<Scores> bestScores = await OsuApi.GetUserBestAsync(user.UserId);
                    var bestScore = bestScores.FirstOrDefault();
                    Beatmap bestPlayBeatmap = (await OsuApi.GetBeatmapsAsync(b: bestScore.BeatmapId, limit: 1)).FirstOrDefault();
                    var userInformation = new StringBuilder()
                        .AppendLine($"**{user.Username}**")
                        .AppendLine($"User Best: **{bestPlayBeatmap.Title} [{bestPlayBeatmap.Version}]** giving __**{bestScore.PP}pp**__")
                        .AppendLine("```")
                        .AppendLine($"Rank: {user.PPRank}")
                        .AppendLine($"Performance Points: {user.PPRaw}")
                        .AppendLine($"Country: {user.Country}")
                        .AppendLine($"Country Rank: {user.PPCountryRank}")
                        .AppendLine($"Level: {user.Level}")
                        .AppendLine("```")
                        .ToString();
                    await e.Channel.SendMessage(userInformation);
                }
                else
                {
                    await e.Channel.SendMessage($"User \"{username}\" not found");
                }
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
                    IEnumerable<BeatmapResult> sortedBeatmaps = GetBeatmapsByMapper(beatmapper)
                        .OrderByDescending(result => Extensions.CalculateSimilarity(result.Name, beatmapNameAndDifficulty));
                    BeatmapResult beatmapResult = sortedBeatmaps.FirstOrDefault();
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
                    foreach (BeatmapResult potentialBeatmapResult in sortedBeatmaps.TakeWhile(result => Extensions.CalculateSimilarity(result.Name, beatmapName) / bestSimilarity > 0.99))
                    {
                        potentialBeatmaps = potentialBeatmaps.Concat(await OsuApi.GetBeatmapsAsync(s: potentialBeatmapResult.SetId, limit: 20));
                    }
                    var selectedBeatmap = potentialBeatmaps.OrderByDescending(beatmap => Extensions.CalculateSimilarity(beatmap.Version, difficultyName)).FirstOrDefault();
                    if (selectedBeatmap == null)
                        throw new BeatmapAnalysisException("Failed to retrieve beatmap");
                    await e.Channel.SendMessage(FormatBeatmapInformation(selectedBeatmap));
                }
                catch (BeatmapAnalysisException exception)
                {
                    await e.Channel.SendMessage("Analysis failed: " + exception.Message);
                }
            });

            commands.CreateCommand("leaderboards").Parameter("beatmap").Do(async (e) =>
            {
                if (BeatmapUrlMatcher.IsMatch(e.Message.Text) && e.Message.User.IsBot == false)
                {
                    var match = BeatmapUrlMatcher.Match(e.Message.Text);
                    string link = match.Groups["beatmap_link"].Value;
                    bool isSet = match.Groups["b_s"].Value == "s";
                    bool first = false; 
                    long? beatmapId = long.Parse(match.Groups["beatmap_id"].Value);
                    if (match.Groups["full_link"].Value == e.Message.Text)
                    {
                        await e.Message.Delete();
                    }
                    ReadOnlyCollection<Beatmap> beatmaps = await (isSet ? OsuApi.GetBeatmapsAsync(s: beatmapId, limit: 20) : OsuApi.GetBeatmapsAsync(b: beatmapId, limit: 1));
                    ReadOnlyCollection<Scores> scores = await OsuApi.GetScoresAsync(b: (long)beatmaps.First().BeatmapId, m: Mods.None, limit: 50);

                    await e.Channel.SendMessage(FormatLeaderboardInformation(scores)); 
                }
            });

            client.MessageReceived += async (s, e) =>
            {
                //Beatmap Info
                if (BeatmapUrlMatcher.IsMatch(e.Message.Text) && e.Message.User.IsBot == false)
                {
                    var match = BeatmapUrlMatcher.Match(e.Message.Text);
                    string link = match.Groups["beatmap_link"].Value;
                    bool isSet = match.Groups["b_s"].Value == "s";
                    long? beatmapId = long.Parse(match.Groups["beatmap_id"].Value);
                    if (match.Groups["full_link"].Value == e.Message.Text)
                    {
                        await e.Message.Delete();
                    }
                    ReadOnlyCollection<Beatmap> beatmaps = await (isSet ? OsuApi.GetBeatmapsAsync(s: beatmapId, limit: 20) : OsuApi.GetBeatmapsAsync(b: beatmapId, limit: 1));
                    await e.Channel.SendMessage(isSet ? FormatBeatmapSetInformation(new BeatmapSet(beatmaps)) : FormatBeatmapInformation(beatmaps.First()));
                }

                if (e.Message.Attachments.Length == 1)
                {
                    lastAttachment = e.Message.Attachments[0].Url;
                }
                if (e.Message.Embeds.Length == 1)
                {
                    lastAttachment = e.Message.Embeds[0].Url;
                }
            };
        }

        private static List<BeatmapResult> GetBeatmapsByMapper(string beatmapper)
        {
            var beatmapQueryUrl = $"http://osusearch.com/query/?mapper={beatmapper.UrlEncode()}";
            var beatmapQueryRequest = WebRequest.CreateHttp(beatmapQueryUrl);
            string queryResponse = beatmapQueryRequest.GetResponse().GetResponseStream().ReadString();
            var searchResult = json.Deserialize<BeatmapSearchResult>(queryResponse);
            int resultCount = searchResult.result_count;
            List<BeatmapResult> beatmapResults = searchResult.beatmaps.Select(result => new BeatmapResult(result)).ToList();
            int queryAttempts = 1;
            while (beatmapResults.Count < resultCount)
            {
                var additionalQueryRequest = WebRequest.CreateHttp($"{beatmapQueryUrl}&offset={queryAttempts++}");
                queryResponse = additionalQueryRequest.GetResponse().GetResponseStream().ReadString();
                searchResult = json.Deserialize<BeatmapSearchResult>(queryResponse);
                beatmapResults.AddRange(searchResult.beatmaps.Select(result => new BeatmapResult(result)));
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

        private string FormatBeatmapInformation(Beatmap beatmap)
        {
            return new StringBuilder()
                .AppendLine($"__***{CleanDiscordString(beatmap.Title)} [{CleanDiscordString(beatmap.Version)}]***__  by ***{CleanDiscordString(beatmap.Artist)}***")
                .AppendLine($"**Created by *{beatmap.Creator}***  |  **Status : *{beatmap.Approved}***")
                .AppendLine($"***Download Link*** : **https://osu.ppy.sh/b/{beatmap.BeatmapId}**")
                .AppendLine("**Beatmap Info**")
                .AppendLine("```")
                .Append($"AR {beatmap.DiffApproach} | OD {beatmap.DiffOverall} | CS {beatmap.DiffSize} | HP {beatmap.DiffDrain} | ")
                .AppendLine($"Stars {beatmap.DifficultyRating:#.##} | BPM {beatmap.Bpm:#.##} | Length {ToMinutes(beatmap.TotalLength)}")
                .AppendLine("```")
                .ToString();
        }

        private string FormatBeatmapSetInformation(BeatmapSet beatmapSet)
        {
            return new StringBuilder()
                .AppendLine($"__***{CleanDiscordString(beatmapSet.Title)}***__  by ***{CleanDiscordString(beatmapSet.Artist)}***")
                .AppendLine($"**Created by *{beatmapSet.Creator}***  |  **Status : *{beatmapSet.Status}***")
                .AppendLine($"***Download Link*** : **https://osu.ppy.sh/s/{beatmapSet.Id}**")
                .AppendLine("**Beatmap Info**")
                .AppendLine("```")
                .Append($"AR {beatmapSet.ApproachRate} | OD {beatmapSet.OverallDifficulty} | CS {beatmapSet.CircleSize} | HP {beatmapSet.HealthDrain} | ")
                .AppendLine($"Stars {beatmapSet.Stars.Format("#.##")} | BPM {beatmapSet.Bpm:#.##} | Length {ToMinutes(beatmapSet.Length)}")
                .AppendLine("```")
                .ToString();
        }

        private string FormatLeaderboardInformation(ReadOnlyCollection<Scores> scores)
        {

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
                    if (maximum == minimum)
                        return string.Format($"{{0:{format}}}", maximum);
                    return string.Format($"{{0:{format}}}-{{1:{format}}}", minimum, maximum);
                }

                public override string ToString() => maximum == minimum ? $"{maximum}" : $"{minimum}-{maximum}";
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