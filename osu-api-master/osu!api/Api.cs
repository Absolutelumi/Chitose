using Osu.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Osu
{
    public class Api
    {
        #region ~PROPERTIES~

        public string ApiKey { get; internal set; }

        #endregion

        #region ~CONSTRUCTOR~

        public Api(string apiKey)
        {
            ApiKey = apiKey;
        }

        #endregion

        #region ~PUBLIC STATIC METHODS~

        /// <summary>
        /// Retrieve general beatmap information.
        /// </summary>
        /// <param name="k">Api key.</param>
        /// <param name="since">Return all beatmaps ranked since this date.</param>
        /// <param name="s">Specify a beatmapset_id to return metadata from.</param>
        /// <param name="b">Specify a beatmap_id to return metadata from.</param>
        /// <param name="u">Specify a user_id or a username to return metadata from.</param>
        /// <param name="m">Mode(0 = osu!, 1 = Taiko, 2 = CtB, 3 = osu!mania). Optional, maps of all modes are returned by default.</param>
        /// <param name="a">Specify whether converted beatmaps are included(false = not included, true = included). Only has an effect if m is chosen and not false. Converted maps show their converted difficulty rating.Optional, default is false.</param>
        /// <param name="h">The beatmap hash.It can be used, for instance, if you're trying to get what beatmap has a replay played in, as .osr replays only provide beatmap hashes (example of hash: a5b99395a42bd55bc5eb1d2411cbdf8b). Optional, by default all beatmaps are returned independently from the hash.</param>
        /// <param name="limit">Amount of results.Optional, default and maximum are 500.</param>
        /// <returns>A list containing all beatmaps (one per difficulty) matching criteria.</returns>
        public static async Task<ReadOnlyCollection<Beatmap>> GetBeatmapsAsync(string k, DateTime? since = null, long? s = null, long? b = null, long? u = null, Mode? m = null, bool a = false, string h = null, int limit = 500)
        {
            var request = CreateRequestGetBeatmaps(k, since, s, b, u, m, a, h, limit);
            var response = await request.GetResponseAsync();
            using (var stream = new StreamReader(response.GetResponseStream()))
            {
                return ParseGetBeatmaps(await new StreamReader(response.GetResponseStream()).ReadToEndAsync());
            }
        }

        /// <summary>
        /// Retrieve general user information.
        /// </summary>
        /// <param name="k">Api key.</param>
        /// <param name="u">Specify a user_id or a username to return metadata from.</param>
        /// <param name="m">Mode (0 = osu!, 1 = Taiko, 2 = CtB, 3 = osu!mania).</param>
        /// <param name="event_days">Max number of days between now and last event date. Range of 1-31. Default value is 1.</param>
        /// <returns>A list containing user information.</returns>
        public static async Task<User> GetUserAsync(string k, object u, Mode m = 0, int? event_days = null)
        {
            var request = CreateRequestGetUser(k, u, m, event_days);
            var response = await request.GetResponseAsync();
            using (var stream = new StreamReader(response.GetResponseStream()))
            {
                return ParseGetUser(await new StreamReader(response.GetResponseStream()).ReadToEndAsync());
            }
        }

        /// <summary>
        /// Retrieve information about the top 100 scores of a specified beatmap.
        /// </summary>
        /// <param name="k">Api key.</param>
        /// <param name="b">Specify a beatmap_id to return score information from(required).</param>
        /// <param name="u">Specify a user_id or a username to return score information for.</param>
        /// <param name="m">Specify a mod or mod combination.</param>
        /// <param name="limit">Amount of results from the top(range between 1 and 100 - defaults to 50).</param>
        /// <returns>A list containing the top 100 scores of the specified beatmap.</returns>
        public static async Task<ReadOnlyCollection<Scores>> GetScoresAsync(string k, long b, object u = null, Mods m = 0, int limit = 50)
        {
            var request = CreateRequestGetScores(k, b, u, m, limit);
            var response = await request.GetResponseAsync();
            using (var stream = new StreamReader(response.GetResponseStream()))
            {
                return ParseGetScores(await new StreamReader(response.GetResponseStream()).ReadToEndAsync());
            }
        }

        /// <summary>
        /// Get the top scores for the specified user.
        /// </summary>
        /// <param name="k">Api key.</param>
        /// <param name="u">Specify a user_id or a username to return score information for.</param>
        /// <param name="m">Mode (0 = osu!, 1 = Taiko, 2 = CtB, 3 = osu!mania). Optional, default value is 0.</param>
        /// <param name="limit">Amount of results from the top(range between 1 and 100 - defaults to 50).</param>
        /// <returns>A list containing the top 10 scores for the specified user.</returns>
        public static async Task<ReadOnlyCollection<Scores>> GetUserBestAsync(string k, object u, Mode m = 0, int limit = 10)
        {
            var request = CreateRequestGetUserBest(k, u, m, limit);
            var response = await request.GetResponseAsync();
            using (var stream = new StreamReader(response.GetResponseStream()))
            {
                return ParseGetScores(await new StreamReader(response.GetResponseStream()).ReadToEndAsync());
            }
        }

        /// <summary>
        /// Gets the user's ten most recent plays.
        /// </summary>
        /// <param name="k">Api key.</param>
        /// <param name="u">Specify a user_id or a username to return score information for.</param>
        /// <param name="m">Mode (0 = osu!, 1 = Taiko, 2 = CtB, 3 = osu!mania). Optional, default value is 0.</param>
        /// <param name="limit">Amount of results (range between 1 and 50 - defaults to 10).</param>
        /// <returns>A list containing the top 10 scores for the specified user.</returns>
        public static async Task<ReadOnlyCollection<Scores>> GetUserRecentAsync(string k, object u, Mode m = 0, int limit = 10)
        {
            var request = CreateRequestGetUserRecent(k, u, m, limit);
            var response = await request.GetResponseAsync();
            using (var stream = new StreamReader(response.GetResponseStream()))
            {
                return ParseGetScores(await new StreamReader(response.GetResponseStream()).ReadToEndAsync());
            }
        }

        /// <summary>
        /// Retrieve information about multiplayer match.
        /// </summary>
        /// <param name="k">Api key.</param>
        /// <param name="mp">Match id to get information from.</param>
        /// <returns>A list containing match information, and player's result.</returns>
        public static async Task<Match> GetMatchAsync(string k, long mp)
        {
            var request = CreateRequestGetMatch(k, mp);
            var response = await request.GetResponseAsync();
            using (var stream = new StreamReader(response.GetResponseStream()))
            {
                return ParseGetMatch(await new StreamReader(response.GetResponseStream()).ReadToEndAsync());
            }
        }

        #endregion

        #region ~PRIVATE METHODS~

        private static HttpWebRequest CreateRequestGetBeatmaps(string k, DateTime? since = null, long? s = null, long? b = null, object u = null, Mode? m = null, bool a = false, string h = null, int limit = 500)
        {
            string requestUri = string.Format("https://osu.ppy.sh/api/get_beatmaps?k={0}", k);
            if (since != null)
                requestUri += string.Format("&since={0}-{1}-{2}", since.Value.Year, since.Value.Month, since.Value.Day);
            if (s != null)
                requestUri += string.Format("&s={0}", s.Value);
            if (b != null)
                requestUri += string.Format("&b={0}", b.Value);
            if (u != null)
                if (u is int)
                    requestUri += string.Format("&u={0}&type={1}", u, "id");
                else if (u is string)
                    requestUri += string.Format("&u={0}&type={1}", u, "string");
            if (m != null)
                requestUri += string.Format("&m={0}", (int)m.Value);
            if (a != false)
                requestUri += string.Format("&a={0}", 1);
            if (h != null)
                requestUri += string.Format("&h={0}", h);
            if (limit != 500)
                requestUri += string.Format("&limit={0}", limit);

            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = "GET";

            return request;
        }

        private static ReadOnlyCollection<Beatmap> ParseGetBeatmaps(string jsonString)
        {
            IList<Beatmap> result = new List<Beatmap>();
            using (var stringReader = new StringReader(jsonString))
            using (var jsonReader = new JsonTextReader(stringReader))
            {
                while (jsonReader.Read())
                {
                    if (jsonReader.TokenType == JsonToken.StartObject)
                        result.Add(new Beatmap(jsonReader));
                }
            }
            return new ReadOnlyCollection<Beatmap>(result);
        }

        private static HttpWebRequest CreateRequestGetUser(string k, object u, Mode m = 0, int? event_days = null)
        {
            string requestUri = string.Format("https://osu.ppy.sh/api/get_user?k={0}", k);

            if (u is int)
                requestUri += string.Format("&u={0}&type={1}", u, "id");
            else if (u is string)
                requestUri += string.Format("&u={0}&type={1}", u, "string");
            if (m != 0)
                requestUri += string.Format("&m={0}", (int)m);
            if (event_days != null)
                requestUri += string.Format("&event_days={0}", event_days.Value);

            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = "GET";

            return request;
        }

        private static User ParseGetUser(string jsonString)
        {
            return User.Parse(jsonString);
        }

        private static HttpWebRequest CreateRequestGetScores(string k, long b, object u = null, Mods m = 0, int limit = 50)
        {
            string requestUri = string.Format("https://osu.ppy.sh/api/get_scores?k={0}&b={1}", k, b);

            if (u != null)
                if (u is int)
                    requestUri += string.Format("&u={0}&type={1}", u, "id");
                else if (u is string)
                    requestUri += string.Format("&u={0}&type={1}", u, "string");
            if (m != 0)
                requestUri += string.Format("&m={0}", m);
            if (limit != 50)
                requestUri += string.Format("&limit={0}", limit);

            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = "GET";

            return request;
        }

        private static ReadOnlyCollection<Scores> ParseGetScores(string jsonString)
        {
            IList<Scores> result = new List<Scores>();
            using (var stringReader = new StringReader(jsonString))
            using (var jsonReader = new JsonTextReader(stringReader))
            {
                while (jsonReader.Read())
                {
                    if (jsonReader.TokenType == JsonToken.StartObject)
                        result.Add(new Scores(jsonReader));
                }
            }
            return new ReadOnlyCollection<Scores>(result);
        }

        private static HttpWebRequest CreateRequestGetUserBest(string k, object u, Mode m = 0, int limit = 10)
        {
            string requestUri = string.Format("https://osu.ppy.sh/api/get_user_best?k={0}", k);

            if (u != null)
                if (u is int)
                    requestUri += string.Format("&u={0}&type={1}", u, "id");
                else if (u is string)
                    requestUri += string.Format("&u={0}&type={1}", u, "string");
            if (m != 0)
                requestUri += string.Format("&m={0}", m);
            if (limit != 10)
                requestUri += string.Format("&limit={0}", limit);

            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = "GET";

            return request;
        }

        private static HttpWebRequest CreateRequestGetUserRecent(string k, object u, Mode m = 0, int limit = 10)
        {
            string requestUri = string.Format("https://osu.ppy.sh/api/get_user_recent?k={0}", k);

            if (u != null)
                if (u is int)
                    requestUri += string.Format("&u={0}&type={1}", u, "id");
                else if (u is string)
                    requestUri += string.Format("&u={0}&type={1}", u, "string");
            if (m != 0)
                requestUri += string.Format("&m={0}", m);
            if (limit != 10)
                requestUri += string.Format("&limit={0}", limit);

            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = "GET";

            return request;
        }

        private static HttpWebRequest CreateRequestGetMatch(string k, long mp)
        {
            string requestUri = string.Format("https://osu.ppy.sh/api/get_match?k={0}&mp={1}", mp);

            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = "GET";

            return request;
        }

        private static Match ParseGetMatch(string jsonString)
        {
            return Match.Parse(jsonString);
        }
        
        #endregion

        #region ~PUBLIC METHODS~

        /// <summary>
        /// Retrieve general beatmap information.
        /// </summary>
        /// <param name="since">Return all beatmaps ranked since this date.</param>
        /// <param name="s">Specify a beatmapset_id to return metadata from.</param>
        /// <param name="b">Specify a beatmap_id to return metadata from.</param>
        /// <param name="u">Specify a user_id to return metadata from.</param>
        /// <param name="m">Mode(0 = osu!, 1 = Taiko, 2 = CtB, 3 = osu!mania). Optional, maps of all modes are returned by default.</param>
        /// <param name="a">Specify whether converted beatmaps are included(false = not included, true = included). Only has an effect if m is chosen and not false. Converted maps show their converted difficulty rating.Optional, default is false.</param>
        /// <param name="h">The beatmap hash.It can be used, for instance, if you're trying to get what beatmap has a replay played in, as .osr replays only provide beatmap hashes (example of hash: a5b99395a42bd55bc5eb1d2411cbdf8b). Optional, by default all beatmaps are returned independently from the hash.</param>
        /// <param name="limit">Amount of results.Optional, default and maximum are 500.</param>
        /// <returns>A list containing all beatmaps (one per difficulty) matching criteria.</returns>
        public async Task<ReadOnlyCollection<Beatmap>> GetBeatmapsAsync(DateTime? since = null, long? s = null, long? b = null, object u = null, Mode? m = null, bool a = false, string h = null, int limit = 500)
        {
            var request = CreateRequestGetBeatmaps(ApiKey, since, s, b, u, m, a, h, limit);
            var response = await request.GetResponseAsync();
            using (var stream = new StreamReader(response.GetResponseStream()))
            {
                return ParseGetBeatmaps(await new StreamReader(response.GetResponseStream()).ReadToEndAsync());
            }
        }

        /// <summary>
        /// Retrieve general user information.
        /// </summary>
        /// <param name="u">Specify a user_id or a username to return metadata from.</param>
        /// <param name="m">Mode (0 = osu!, 1 = Taiko, 2 = CtB, 3 = osu!mania).</param>
        /// <param name="event_days">Max number of days between now and last event date. Range of 1-31. Default value is 1.</param>
        /// <returns>A list containing user information.</returns>
        public async Task<User> GetUserAsync(object u, Mode m = 0, int? event_days = null)
        {
            var request = CreateRequestGetUser(ApiKey, u, m, event_days);
            var response = await request.GetResponseAsync();
            using (var stream = new StreamReader(response.GetResponseStream()))
            {
                return ParseGetUser(await new StreamReader(response.GetResponseStream()).ReadToEndAsync());
            }
        }

        /// <summary>
        /// Retrieve information about the top 100 scores of a specified beatmap.
        /// </summary>
        /// <param name="b">Specify a beatmap_id to return score information from(required).</param>
        /// <param name="u">Specify a user_id or a username to return score information for.</param>
        /// <param name="m">Specify a mod or mod combination.</param>
        /// <param name="limit">Amount of results from the top(range between 1 and 100 - defaults to 50).</param>
        /// <returns>A list containing the top 100 scores of the specified beatmap.</returns>
        public async Task<ReadOnlyCollection<Scores>> GetScoresAsync(long b, object u = null, Mods m = 0, int limit = 50)
        {
            var request = CreateRequestGetScores(ApiKey, b, u, m, limit);
            var response = await request.GetResponseAsync();
            using (var stream = new StreamReader(response.GetResponseStream()))
            {
                return ParseGetScores(await new StreamReader(response.GetResponseStream()).ReadToEndAsync());
            }
        }

        /// <summary>
        /// Get the top scores for the specified user.
        /// </summary>
        /// <param name="u">Specify a user_id or a username to return score information for.</param>
        /// <param name="m">Mode (0 = osu!, 1 = Taiko, 2 = CtB, 3 = osu!mania). Optional, default value is 0.</param>
        /// <param name="limit">Amount of results from the top(range between 1 and 100 - defaults to 50).</param>
        /// <returns>A list containing the top 10 scores for the specified user.</returns>
        public async Task<ReadOnlyCollection<Scores>> GetUserBestAsync(object u, Mode m = 0, int limit = 10)
        {
            var request = CreateRequestGetUserBest(ApiKey, u, m, limit);
            var response = await request.GetResponseAsync();
            using (var stream = new StreamReader(response.GetResponseStream()))
            {
                return ParseGetScores(await new StreamReader(response.GetResponseStream()).ReadToEndAsync());
            }
        }

        /// <summary>
        /// Gets the user's ten most recent plays.
        /// </summary>
        /// <param name="k">Api key.</param>
        /// <param name="u">Specify a user_id or a username to return score information for.</param>
        /// <param name="m">Mode (0 = osu!, 1 = Taiko, 2 = CtB, 3 = osu!mania). Optional, default value is 0.</param>
        /// <param name="limit">Amount of results (range between 1 and 50 - defaults to 10).</param>
        /// <returns>A list containing the top 10 scores for the specified user.</returns>
        public async Task<ReadOnlyCollection<Scores>> GetUserRecentAsync(object u, Mode m = 0, int limit = 10)
        {
            var request = CreateRequestGetUserRecent(ApiKey, u, m, limit);
            var response = await request.GetResponseAsync();
            using (var stream = new StreamReader(response.GetResponseStream()))
            {
                return ParseGetScores(await new StreamReader(response.GetResponseStream()).ReadToEndAsync());
            }
        }

        /// <summary>
        /// Retrieve information about multiplayer match.
        /// </summary>
        /// <param name="k">Api key.</param>
        /// <param name="mp">Match id to get information from.</param>
        /// <returns>A list containing match information, and player's result.</returns>
        public async Task<Match> GetMatchAsync(long mp)
        {
            var request = CreateRequestGetUserRecent(ApiKey, mp);
            var response = await request.GetResponseAsync();
            using (var stream = new StreamReader(response.GetResponseStream()))
            {
                return ParseGetMatch(await new StreamReader(response.GetResponseStream()).ReadToEndAsync());
            }
        }



        #endregion

    }
}
