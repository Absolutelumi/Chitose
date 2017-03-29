using Discord;
using Discord.Commands;
using Mayushii.Services;
using RedditSharp;
using System;
using System.IO;
using System.Linq;
using System.Net;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using ImageProcessor;

namespace ChitoseV2
{
    internal class Pictures : CommandSet
    {
        #region Extra Methods
        string RandomAlbum()
        {
            string[] ImgurAlbums = File.ReadAllLines(Chitose.NSFWPath); 
            return ImgurAlbums.Random(); 
        }
        #endregion

        public void AddCommands(DiscordClient client, CommandService commands)
        {
            #region API Variables
            var reddit = new Reddit();
            var ImgClient = new ImgurClient(Chitose.ImgurKey, Chitose.ImgurSecret);
            var endpoint = new AccountEndpoint(ImgClient);
            #endregion

            #region General
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
            #endregion

            #region NSFW Commands
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
            #endregion

            #region Picture Edits
            using (ImageFactory Imagefactory = new ImageFactory(preserveExifData: true))
            {
                commands.CreateCommand("blur").Parameter("blurlvl/picture", ParameterType.Multiple).Do(async (e) =>
                {
                    int blurlvl = Convert.ToInt32(e.Args[0]); 
                    string ImageLink = e.Args[1]; 
                    string ImagePath = Extensions.DownloadFile(ImageLink);

                    Imagefactory.Load(ImagePath).GaussianBlur(blurlvl).Save(ImagePath); 

                    await e.Channel.SendFile(ImagePath);

                    File.Delete(ImagePath); 
                });

                commands.CreateCommand("tint").Parameter("color/picture", ParameterType.Multiple).Do(async (e) =>
                {
                    string color = e.Args[0];
                    string picture = e.Args[1];

                    string ImagePath = Extensions.DownloadFile(picture);

                    Imagefactory.Tint(System.Drawing.Color.FromName(color)).Save(ImagePath);
                    await e.Channel.SendFile(ImagePath);
                    File.Delete(ImagePath);
                });

                commands.CreateCommand("replacecolor").Parameter("targetCol/replaceCol/Image", ParameterType.Multiple).Do(async (e) =>
                {
                    string targetCol = e.Args[0];
                    string replaceCol = e.Args[1];
                    string Imagelink = e.Args[2];

                    string Imagepath = Extensions.DownloadFile(Imagelink);

                    Imagefactory.Load(Imagepath).ReplaceColor(System.Drawing.Color.FromName(targetCol), System.Drawing.Color.FromName(replaceCol)).Save(Imagepath);

                    await e.Channel.SendFile(Imagepath); 
                });
            }
            #endregion
        }
    }
}