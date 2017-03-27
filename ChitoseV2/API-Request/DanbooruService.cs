using ChitoseV2;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace Mayushii.Services
{
    internal static class DanbooruService
    {
        private static JavaScriptSerializer json;
        private static Random random;

        static DanbooruService()
        {
            json = new JavaScriptSerializer();
            random = new Random();
        }

        public static string GetRandomImage(string[] arg)
        {
            Post[] posts = GetPosts(arg).Where(post => post.ImageUrl != null).ToArray();
            if (posts.Length > 0)
            {
                return posts.Random().ImageUrl;
            }
            else
            {
                return null;
            }
        }

        private static Post[] GetPosts(string[] arg, int? page = null)
        {
            StringBuilder urlBuilder = new StringBuilder();
            urlBuilder.AppendFormat("https://danbooru.donmai.us/posts.json?limit=100&tags={0}", string.Join("+", arg));
            if (page != null)
            {
                urlBuilder.AppendFormat("&page={0}", page);
            }

            HttpWebRequest postRequest = WebRequest.CreateHttp(urlBuilder.ToString());

            try
            {
                WebResponse response = postRequest.GetResponse();
                string postResponse = response.GetResponseStream().ReadString();
                return json.Deserialize<Post[]>(postResponse);
            }
            catch (WebException)
            {
                return new Post[0];
            }
        }

#pragma warning disable 0649

        private class Post
        {
            public string large_file_url;

            public string ImageUrl
            {
                get
                {
                    return string.IsNullOrEmpty(large_file_url) ? null : "https://danbooru.donmai.us" + large_file_url;
                }
            }
        }

#pragma warning restore 0649
    }
}