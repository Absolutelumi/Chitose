using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;

namespace RedditSharp
{
    public interface IWebAgent
    {
        CookieContainer Cookies { get; set; }
        string AuthCookie { get; set; }
        string AccessToken { get; set; }

        HttpWebRequest CreateRequest(string url, string method);

        HttpWebRequest CreateGet(string url);

        HttpWebRequest CreatePost(string url);

        string GetResponseString(Stream stream);

        void WritePostBody(Stream stream, object data, params string[] additionalFields);

        JToken CreateAndExecuteRequest(string url);

        JToken ExecuteRequest(HttpWebRequest request);
    }
}