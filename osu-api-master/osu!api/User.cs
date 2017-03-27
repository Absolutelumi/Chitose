using Osu.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Osu
{
    /// <summary>
    /// Containing user information.
    /// </summary>
    public class User
    {
        #region ~CONSTRUCTOR~

        internal User(JsonTextReader jsonReader)
        {
            int Depth = jsonReader.Depth;
            while (jsonReader.Read())
            {
                switch (jsonReader.TokenType)
                {
                    case JsonToken.PropertyName:
                        switch (jsonReader.Value.ToString())
                        {
                            case "user_id":
                                this.UserId = jsonReader.ReadAsInt32();
                                break;
                            case "username":
                                this.Username = jsonReader.ReadAsString();
                                break;
                            case "count300":
                                this.Count300 = jsonReader.ReadAsInt32();
                                break;
                            case "count100":
                                this.Count100 = jsonReader.ReadAsInt32();
                                break;
                            case "count50":
                                this.Count50 = jsonReader.ReadAsInt32();
                                break;
                            case "playcount":
                                this.PlayCount = jsonReader.ReadAsInt32();
                                break;
                            case "ranked_score":
                                this.RankedScore = long.Parse(jsonReader.ReadAsString());
                                break;
                            case "total_score":
                                this.TotalScore = long.Parse(jsonReader.ReadAsString());
                                break;
                            case "pp_rank":
                                this.PPRank = jsonReader.ReadAsInt32();
                                break;
                            case "level":
                                this.Level = jsonReader.ReadAsDouble();
                                break;
                            case "pp_raw":
                                this.PPRaw = jsonReader.ReadAsDouble();
                                break;
                            case "accuracy":
                                this.Accuracy = jsonReader.ReadAsDouble();
                                break;
                            case "count_rank_ss":
                                this.CountRankSS = jsonReader.ReadAsInt32();
                                break;
                            case "count_rank_s":
                                this.CountRankS = jsonReader.ReadAsInt32();
                                break;
                            case "count_rank_a":
                                this.CountRankA = jsonReader.ReadAsInt32();
                                break;
                            case "country":
                                this.Country = jsonReader.ReadAsString();
                                break;
                            case "pp_country_rank":
                                this.PPCountryRank = jsonReader.ReadAsInt32();
                                break;
                            case "events":
                                IList<Event> events = new List<Event>();
                                while (jsonReader.Read())
                                    if (jsonReader.TokenType == JsonToken.StartObject)
                                        events.Add(new Event(jsonReader));
                                this.Events = new ReadOnlyCollection<Event>(events);
                                break;
                            default:

                                break;
                        }
                        break;
                    case JsonToken.EndObject:
                        if (jsonReader.Depth == Depth)
                            return;
                        break;
                }
            }
        }

        #endregion

        #region ~STATIC METHODS~

        /// <summary>
        /// Converts the string representation of json response to its <see cref="User"/> equivalent. 
        /// </summary>
        /// <param name="s">A string containing a json response to convert.</param>
        /// <returns>A <see cref="User"/> equivalent to the json response contained in s.</returns>
        public static User Parse(string s)
        {
            using (var stringReader = new StringReader(s))
            using (var jsonReader = new JsonTextReader(stringReader))
            {
                return new User(jsonReader);
            }
        }

        /// <summary>
        /// Converts the string representation of json response to its <see cref="User"/> equivalent. A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="s">A string containing a json response to convert.</param>
        /// <param name="result">When this method returns, contains <see cref="User"/> equivalent of the number contained in s, if the conversion succeeded, or null if the conversion failed. The conversion fails if the s parameter is null or String.Empty, is not of the correct format, or doesn't represent a json response. This parameter is passed uninitialized.</param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string s, out User result)
        {
            try
            {
                result = Parse(s);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        #endregion

        #region ~PROPERTIES~

        public int? UserId { get; internal set; }

        public string Username { get; internal set; }

        /// <summary>
        /// Total amount for all ranked and approved beatmaps played.
        /// </summary>
        public int? Count300 { get; internal set; }

        /// <summary>
        /// Total amount for all ranked and approved beatmaps played.
        /// </summary>
        public int? Count100 { get; internal set; }

        /// <summary>
        /// Total amount for all ranked and approved beatmaps played.
        /// </summary>
        public int? Count50 { get; internal set; }

        /// <summary>
        /// Only counts ranked and approved beatmaps.
        /// </summary>
        public int? PlayCount { get; internal set; }

        /// <summary>
        /// Counts the best individual score on each ranked and approved beatmaps.
        /// </summary>
        public long? RankedScore { get; internal set; }

        /// <summary>
        /// Counts every score on ranked and approved beatmaps.
        /// </summary>
        public long? TotalScore { get; internal set; }

        public int? PPRank { get; internal set; }

        public double? Level { get; internal set; }

        public double? PPRaw { get; internal set; }

        public double? Accuracy { get; internal set; }

        public int? CountRankSS { get; internal set; }

        public int? CountRankS { get; internal set; }

        public int? CountRankA { get; internal set; }

        /// <summary>
        /// Uses the ISO3166-1 alpha-2 country code naming. See this for more information: https://en.wikipedia.org/wiki/ISO_3166-1
        /// </summary>
        public string Country { get; internal set; }

        /// <summary>
        /// The user's rank in the country.
        /// </summary>
        public int? PPCountryRank { get; internal set; }

        /// <summary>
        /// Contains events for this user.
        /// </summary>
        public ReadOnlyCollection<Event> Events { get; internal set; }

        #endregion
    }
}
