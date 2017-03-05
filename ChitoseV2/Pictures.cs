using Discord;
using Discord.Commands;
using Mayushii.Services;
using PixivLib;
using RedditSharp;
using System;
using System.IO;
using System.Linq;
using System.Net;

namespace ChitoseV2
{
    internal class Pictures : CommandSet
    {
        public void AddCommands(DiscordClient client, CommandService commands)
        {
            var reddit = new Reddit();

            commands.CreateCommand("reddit").Parameter("subreddit").Do(async (e) =>
            {
                var subreddit = reddit.GetSubreddit(e.GetArg("subreddit"));
                int indexer = Extensions.rng.Next(100);

                var postlist = subreddit.Hot.Take(100);
                var post = postlist.ToList()[indexer];
                string posturl = post.Url.ToString();

                await e.Channel.SendMessage(posturl);
            });

            commands.CreateCommand("show").Parameter("keyword", ParameterType.Multiple).Do(async (e) =>
            {
                string[] arg = e.Args; 
                string url = DanbooruService.GetRandomImage(arg);
                string temppath = Chitose.TempDirectory + arg.ToString() + "booru.png";

                using (WebClient downloadclient = new WebClient())
                {
                    downloadclient.DownloadFile(new Uri(url), temppath);
                }

                await e.Channel.SendFile(temppath);

                File.Delete(temppath);
            });

            commands.CreateCommand("pixiv").Parameter("keyword", ParameterType.Multiple).Do(async (e) =>
            {
                throw new NotImplementedException(); 
            }); 
        }
    }
}