using Discord;
using Discord.Commands;
using Mayushii.Services;
using RedditSharp;
using System;
using System.IO;
using System.Linq;
using System.Net;
using Imgur;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;

namespace ChitoseV2
{
    internal class Pictures : CommandSet
    {
        string RandomAlbum()
        {
            string[] ImgurAlbums = File.ReadAllLines(Chitose.NSFWPath); 
            return ImgurAlbums.Random(); 
        }

        public void AddCommands(DiscordClient client, CommandService commands)
        {
            var reddit = new Reddit();

            var ImgClient = new ImgurClient("c78392a95466b85", "499350b2c5dff500da481771f9a35497c395e41e");
            var endpoint = new AccountEndpoint(ImgClient); 

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

            /* NSFW Commands */
            commands.CreateCommand("i").Do(async (e) =>
            {
                string Album = RandomAlbum();

                var resultAlbum = await endpoint.GetAlbumAsync(Album, "Absolutelumi");

                var image = Extensions.Random(resultAlbum.Images.ToArray());

                var NSFWChannel = e.Server.FindChannels("nsfw").FirstOrDefault();

                await NSFWChannel.SendMessage(image.Link);
            });

            commands.CreateCommand("addalbum").Parameter("albumID").Do((e) =>
            {
                string albumID = e.GetArg("albumID");

                File.AppendAllText(Chitose.NSFWPath, "\r\n" + albumID); 
            }); 
        }
    }
}