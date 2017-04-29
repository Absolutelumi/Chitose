using Discord;
using Discord.Commands;
using ImageProcessor;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Mayushii.Services;
using RedditSharp;
using System;
using System.IO;
using System.Linq;
using System.Net;

namespace ChitoseV2
{
    internal class Pictures : ICommandSet
    {
        private AccountEndpoint endpoint;
        private ImgurClient imgurClient;
        private Reddit redditClient;
        private Channel NSFWChannel;

        public Pictures()
        {
            redditClient = new Reddit();
            imgurClient = new ImgurClient(Chitose.ImgurKey, Chitose.ImgurSecret);
            endpoint = new AccountEndpoint(imgurClient);
        }

        public void AddCommands(DiscordClient client, CommandService commands)
        {
            AddGeneralCommands(commands);
            AddNsfwCommands(commands);
            AddPictureEditingCommands(commands);
        }

        private static void AddPictureEditingCommands(CommandService commands)
        {
            using (ImageFactory imagefactory = new ImageFactory(preserveExifData: true))
            {
                commands.CreateCommand("blur").Parameter("blurlvl/picture", ParameterType.Multiple).Do(async (e) =>
                {
                    int blurlvl = Convert.ToInt32(e.Args[0]);
                    string ImageLink = e.Args[1];
                    string ImagePath = Extensions.DownloadFile(ImageLink);

                    imagefactory.Load(ImagePath).GaussianBlur(blurlvl).Save(ImagePath);

                    await e.Channel.SendFile(ImagePath);

                    File.Delete(ImagePath);
                });

                commands.CreateCommand("tint").Parameter("color/picture", ParameterType.Multiple).Do(async (e) =>
                {
                    string color = e.Args[0];
                    string picture = e.Args[1];

                    string ImagePath = Extensions.DownloadFile(picture);

                    imagefactory.Tint(System.Drawing.Color.FromName(color)).Save(ImagePath);
                    await e.Channel.SendFile(ImagePath);
                    File.Delete(ImagePath);
                });

                commands.CreateCommand("replacecolor").Parameter("targetCol/replaceCol/Image", ParameterType.Multiple).Do(async (e) =>
                {
                    string targetCol = e.Args[0];
                    string replaceCol = e.Args[1];
                    string Imagelink = e.Args[2];

                    string Imagepath = Extensions.DownloadFile(Imagelink);

                    imagefactory.Load(Imagepath).ReplaceColor(System.Drawing.Color.FromName(targetCol), System.Drawing.Color.FromName(replaceCol)).Save(Imagepath);

                    await e.Channel.SendFile(Imagepath);
                });
            }
        }

        private void AddGeneralCommands(CommandService commands)
        {
            commands.CreateCommand("reddit").Parameter("subreddit").Do(async (e) =>
            {
                var subreddit = redditClient.GetSubreddit(e.GetArg("subreddit"));
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
        }

        private void AddNsfwCommands(CommandService commands)
        {
            commands.CreateCommand("i").Do(async (e) =>
            {
                if(IsNSFW(e.Server) == true)
                {
                    string Album = RandomAlbum();

                    var resultAlbum = await endpoint.GetAlbumAsync(Album, "Absolutelumi");

                    var image = Extensions.Random(resultAlbum.Images.ToArray());

                    var NSFWChannel = FindNSFWChannel(e.Server); 

                    if (NSFWChannel != null)
                    {
                        await NSFWChannel.SendMessage(image.Link);
                    }
                    else
                    {
                        await e.Channel.SendMessage(image.Link);
                    }
                }
            });

            commands.CreateCommand("addalbum").Parameter("albumID").Do((e) =>
            {
                string albumID = e.GetArg("albumID");

                File.AppendAllText(Chitose.NSFWPath, "\r\n" + albumID);
            });

            commands.CreateCommand("NSFW").Parameter("Channel").Do((e) =>
            {
                string Channel = string.Join(" ", e.Args);
                NSFWChannel = e.Server.FindChannels(Channel).FirstOrDefault();
                if (NSFWChannel != null)
                {
                    ChangeNSFWChannel(e.Server, NSFWChannel); 
                    e.Channel.SendMessage(string.Format("{0} is now the channel all NSFW commands will send to!", NSFWChannel.Name));
                }
                else
                {
                    e.Channel.SendMessage("Channel not found!");
                }
            });

            commands.CreateCommand("NSFWAllow").Parameter("Enable/Disable").Do((e) =>
            {
                if(e.Args[0].ToLowerInvariant() == "enable")
                {
                    ChangeNSFWAllow(e.Server, true);
                    e.Channel.SendMessage("NSFW is now enabled! Change NSFW channel by using !NSFW <Channel Name>. \n If you do not do this, the NSFW commands will default to the channel the command was sent in."); 
                }
                else if(e.Args[0].ToLowerInvariant() == "disable")
                {
                    ChangeNSFWAllow (e.Server, false);
                    e.Channel.SendMessage("NSFW is now disabled!"); 
                }
            });
        }

        private string RandomAlbum()
        {
            string[] imgurAlbums = File.ReadAllLines(Chitose.NSFWPath);
            return imgurAlbums.Random();
        }

        private bool IsNSFW(Server server)
        {
            StreamReader filereader = new StreamReader(Chitose.ConfigDirectory + "NSFW.txt");
            string line = filereader.ReadLine();
            while(line != null)
            {
                if(ulong.Parse(line.Split('|')[0]) == server.Id)
                {
                    if(line.Split('|')[1] == "allow")
                    {
                        return true;
                    }
                    else
                    {
                        return false; 
                    }
                }
            }
            return false; 
        }

        public void NewServer(Server server)
        {
            StreamWriter streamwriter = new StreamWriter(Chitose.ConfigDirectory + "NSFW.txt");
            streamwriter.WriteLine(string.Format("{0}|disabled|null", server.Id.ToString()));
        }

        public void ChangeNSFWAllow(Server server, bool NSFW)
        {
            string[] lines = File.ReadAllLines(Chitose.ConfigDirectory + "NSFW.txt");
            for(int i = 0; i < lines.Length; i++)
            {
                if(ulong.Parse(lines[i].Split('|')[0]) == server.Id)
                {
                    lines[i] = string.Format("{0}|{1}|{3}", server.Id.ToString(), NSFW ? "enabled" : "disabled", FindNSFWChannel(server).Name);
                    break;
                }
            }
            File.WriteAllLines(Chitose.ConfigDirectory + "NSFW.txt", lines); 
        }

        public void ChangeNSFWChannel(Server server, Channel channel)
        {
            string[] lines = File.ReadAllLines(Chitose.ConfigDirectory + "NSFW.txt");
            for (int i = 0; i < lines.Length; i++)
            {
                if (ulong.Parse(lines[i].Split('|')[0]) == server.Id)
                {
                    lines[i] = string.Format("{0}|{1}|{3}", server.Id.ToString(), lines[i].Split('|')[1], channel.Name);
                    break;
                }
            }
            File.WriteAllLines(Chitose.ConfigDirectory + "NSFW.txt", lines);
        }

        public Channel FindNSFWChannel(Server server)
        {
            StreamReader filereader = new StreamReader(Chitose.ConfigDirectory + "NSFW.txt");
            string line = filereader.ReadLine();
            while (line != null)
            {
                if(line.Split('|')[0] == server.Id.ToString())
                {
                    if(line.Split('|')[2] == "null")
                    {
                        return null; 
                    }
                    else
                    {
                        return server.FindChannels(line.Split('|')[2]).FirstOrDefault();
                    }
                }
            }
            return null; 
        }
    }
}