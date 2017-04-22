using Discord;
using Discord.Commands;
using Osu;
using System; 
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ChitoseV2
{
    internal class Osu_ : CommandSet
    {
        private string beatmap { get; set; }
        Api api = new Api(Chitose.APIKey);

        public void AddCommands(DiscordClient client, CommandService commands)
        {
            commands.CreateCommand("user").Parameter("user").Do(async (e) =>
            {
                string user = string.Join(" ", e.Args);
                var userStats = await api.GetUserAsync(user);
                int? userId = userStats.UserId;
                var userBestList = await api.GetUserBestAsync(userId);
                var userBest = userBestList.FirstOrDefault();
                var Beatmaps = await Api.GetBeatmapsAsync(Chitose.APIKey, null, null, userBest.BeatmapId, null, null, false, null, 10);
                Beatmap BestPlayMap = Beatmaps.FirstOrDefault(); 

                await e.Channel.SendMessage(string.Format("**{0}**", userStats.Username));
                await e.Channel.SendMessage(string.Format("User Best  :  **{0}** giving __**{1}pp**__", BestPlayMap.Title, userBest.PP));
                await e.Channel.SendMessage(string.Format("```Rank : {0} \n Performance Points : {4} \nCountry : {1} \n Country Rank : {2} \nLevel : {3}```", userStats.PPRank, userStats.Country, userStats.PPCountryRank, userStats.Level, userStats.PPRaw));
            });

            client.MessageReceived += async (s, e) =>
            {
                if (e.Message.ToString().Contains("https://osu.ppy.sh/b/") && e.Message.User.IsBot == false)
                {
                    string url = e.Message.ToString();

                    string link = url.Split(':')[1] + ':' + url.Split(':')[2];

                    long? BMID = GetBMCode(url);

                    ReadOnlyCollection<Beatmap> Beatmaps = await Api.GetBeatmapsAsync(Chitose.APIKey, null, null, BMID, null, null, false, null, 10);

                    Beatmap BM = Beatmaps.FirstOrDefault();

                    await e.Message.Delete();

                    await e.Channel.SendMessage(string.Format("__***{0}***__ by ***{1}*** \n **Created by *{9}***  |  **Status : *{8}*** \n ***Download Link*** : **{10}** \n **Beatmap Info**\n```Ar {2} | Od {3} | Cs {4} | Hp {5} | Stars {6} | BPM {7} | Length {11}``` \n ",
                            BM.Title, BM.Artist, BM.DiffApproach, BM.DiffOverall, BM.DiffSize, BM.DiffDrain, Math.Round(Convert.ToDouble(BM.DifficultyRating), 2), Convert.ToInt32(BM.Bpm), BM.Approved.ToString(), BM.Creator, link, ToMinutes(BM.TotalLength)));
                }
            };
        }

        private long? GetBMCode(string url)
        {
            string id = url.Split('/')[4];

            int numID = Convert.ToInt32(id);

            return numID; 
        }

        private string ToMinutes(int? Seconds)
        {
            int seconds = Convert.ToInt32(Seconds); 

            int minutes = seconds / 60;
            int something = minutes * 60;
            int newseconds = seconds - something;

            return string.Format("{0}:{1}", minutes, newseconds); 
        }
    }
}
