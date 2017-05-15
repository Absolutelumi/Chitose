using OsuApi.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace OsuApi.Queries
{
    public interface IBeatmapQuery
    {
        IBeatmapQuery AllowConvertedBeatmaps(bool allow);

        IBeatmapQuery CreatedBy(string user);

        Task<Beatmap[]> GetBeatmaps(int? limit = 500);

        IBeatmapQuery Since(DateTime date);

        IBeatmapQuery WithHash(string hash);

        IBeatmapQuery WithId(string id);

        IBeatmapQuery WithMode(Mode mode);
    }

    public interface IBeatmapSetQuery
    {
        IBeatmapQuery WithSetId(string setId);
    }

    public interface IBeatmapSpecificQuery
    {
        IBeatmapQuery WithId(string id);
    }

    internal class BeatmapQuery : IBeatmapQuery, IBeatmapSetQuery, IBeatmapSpecificQuery
    {
        private Dictionary<string, string> Parameters;

        internal BeatmapQuery(string apiKey)
        {
            Parameters = new Dictionary<string, string>
            {
                { "k", apiKey }
            };
        }

        public IBeatmapQuery AllowConvertedBeatmaps(bool allow)
        {
            Parameters["a"] = allow ? "1" : "0";
            return this;
        }

        public IBeatmapQuery CreatedBy(string user)
        {
            Parameters["u"] = user;
            return this;
        }

        public async Task<Beatmap[]> GetBeatmaps(int? limit)
        {
            if (limit.HasValue)
            {
                Parameters["limit"] = $"{limit}";
            }
            var queryString = string.Join("&", Parameters.Select(pair => $"{pair.Key}={pair.Value}"));
            var beatmapRequest = WebRequest.CreateHttp($"https://osu.ppy.sh/api/get_beatmaps?{queryString}");
            var response = await beatmapRequest.GetResponseAsync();
            string responseJson = new StreamReader(response.GetResponseStream()).ReadToEnd();
            return Api.Json.Deserialize<Beatmap[]>(responseJson);
        }

        public IBeatmapQuery Since(DateTime date)
        {
            Parameters["since"] = date.ToString("yyyy-MM-dd H:mm:ss");
            return this;
        }

        public IBeatmapQuery WithHash(string hash)
        {
            Parameters["h"] = hash;
            return this;
        }

        public IBeatmapQuery WithId(string id)
        {
            Parameters["b"] = id;
            return this;
        }

        public IBeatmapQuery WithMode(Mode mode)
        {
            Parameters["m"] = $"{(int)mode}";
            return this;
        }

        public IBeatmapQuery WithSetId(string setId)
        {
            Parameters["s"] = setId;
            return this;
        }
    }
}