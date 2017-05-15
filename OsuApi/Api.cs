using OsuApi.Queries;
using System.Web.Script.Serialization;

namespace OsuApi
{
    public class Api
    {
        public static readonly JavaScriptSerializer Json = new JavaScriptSerializer();
        private string ApiKey;

        public Api(string apiKey)
        {
            ApiKey = apiKey;
        }

        public IBeatmapSetQuery BeatmapSet()
        {
            return new BeatmapQuery(ApiKey);
        }

        public IBeatmapSpecificQuery SpecificBeatmap()
        {
            return new BeatmapQuery(ApiKey);
        }

        public IUserSpecificQuery User()
        {
            return new UserQuery(ApiKey); 
        }
    }
}