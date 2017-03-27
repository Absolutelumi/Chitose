using Discord;
using Discord.Commands;
using Osu;
using RestSharp.Extensions.MonoHttp;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace ChitoseV2
{
    internal class Osu_ : CommandSet
    {
        private string beatmap { get; set; }
        Api api = new Api(Chitose.APIKey);

        public void AddCommands(DiscordClient client, CommandService commands)
        {
            commands.CreateCommand("bm").Parameter("beatmap", ParameterType.Multiple).Do(async (e) =>
            {
                beatmap = string.Join(" ", e.Args).ToLowerInvariant();

                Message botMessage = await e.Channel.SendMessage("Loading..."); 
                var Beatmaps = await Api.GetBeatmapsAsync(beatmap);
                Beatmap BM = Beatmaps[1];

                await botMessage.Edit(string.Format("**{0}** : {1}", BM.Title, BM.Approved.ToString())); 
            });

            commands.CreateCommand("user").Parameter("user").Do(async (e) =>
            {
                string user = string.Join(" ", e.Args);
                var userStats = await api.GetUserAsync(user);
                int? userId = userStats.UserId;
                var userBest = await api.GetUserBestAsync(userId); 

                await e.Channel.SendMessage(string.Format("**{0}**", userStats.Username));
                await e.Channel.SendMessage(string.Format("```Rank : {0} \n Performance Points : {4} \nCountry : {1} \n Country Rank : {2} \nLevel : {3}```", userStats.PPRank, userStats.Country, userStats.PPCountryRank, userStats.Level, userStats.PPRaw));
                await e.Channel.SendMessage(userBest.ToString());
            });
        }
    }
}
