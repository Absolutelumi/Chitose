using System.IO;
using System.Net;
using System.Xml;

namespace ChitoseV2
{
    internal static class Mal
    {
        public static AnimeResult FindMyAnime(string search, string username, string password)
        {
            HttpWebRequest request = WebRequest.CreateHttp($"https://myanimelist.net/api/anime/search.xml?q={search.UrlEncode()}");
            request.Headers.Add("Authorization", $"Basic {System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes($"{username}:{password}"))}");
            request.KeepAlive = false;
            try
            {
                using (Stream stream = request.GetResponse().GetResponseStream())
                {
                    XmlDocument document = new XmlDocument();
                    document.Load(stream);
                    XmlNode anime = document["anime"];
                    if (anime.HasChildNodes)
                    {
                        XmlNode answer = anime["entry"];
                        return new AnimeResult
                        {
                            valid = true,
                            title = answer["title"].InnerText,
                            synopsis = answer["synopsis"].InnerText.HtmlDecode(),
                            image = answer["image"].InnerText
                        };
                    }
                }
            }
            catch { }
            return new AnimeResult() { valid = false };
        }

        public struct AnimeResult
        {
            public string title, synopsis, image;
            public bool valid;
        }
    }
}