using Discord;
using Discord.Commands;
using RestSharp.Extensions.MonoHttp;
using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace ChitoseV2
{
    internal class MAL : CommandSet
    {
        myAnimeList myAnimeList = new myAnimeList();

        private string title { get; set; }
        private string image { get; set; }
        private string description { get; set; }

        public void AddCommands(DiscordClient client, CommandService commands)
        {
            commands.CreateCommand("anime").Parameter("animename", ParameterType.Multiple).Do(async (e) =>
            {
                string tempdir = Chitose.TempDirectory + title + " Pic.png";
                Regex tags = new Regex("<.*>");
                
                myAnimeList.AnimeResult anime = myAnimeList.FindMyAnime(title, Chitose.MALUsername, Chitose.MALPassword);

                title = anime.title; 
                image = anime.image;

                using (WebClient downloadclient = new WebClient())
                {
                    downloadclient.DownloadFile(new Uri(image), tempdir);
                }

                description = HttpUtility.HtmlDecode(tags.Replace(anime.synopsis, string.Empty));

                await e.Channel.SendFile(tempdir);
                await e.Channel.SendMessage(string.Format("**{0}** \n ```{1}```",title, description));
            });

            commands.CreateCommand("mal").Parameter("user").Do(async (e) =>
            {
                await e.Channel.SendMessage("i want to die"); 
            });
        } 
    }
}
