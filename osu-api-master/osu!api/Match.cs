using Osu.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Osu
{
    /// <summary>
    /// Containing match information, and player's result.
    /// </summary>
    public class Match
    {
        #region ~CONSTRUCTOR~

        internal Match(JsonTextReader jsonReader)
        {
            int Depth = jsonReader.Depth;
            while (jsonReader.Read())
            {
                switch (jsonReader.TokenType)
                {
                    case JsonToken.PropertyName:
                        switch (jsonReader.Value.ToString())
                        {
                            case "match_id":
                                this.MatchId = long.Parse(jsonReader.ReadAsString());
                                break;
                            case "name":
                                this.Name = jsonReader.ReadAsString();
                                break;
                            case "start_time":
                                this.StartTime = jsonReader.ReadAsDateTime();
                                break;
                            case "end_time":
                                this.EndTime = jsonReader.ReadAsDateTime();
                                break;
                            case "games":
                                IList<Game> games = new List<Game>();
                                while (jsonReader.Read())
                                    if (jsonReader.TokenType == JsonToken.StartObject)
                                        games.Add(new Game(jsonReader));
                                this.Games = new ReadOnlyCollection<Game>(games);
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
        /// Converts the string representation of json response to its <see cref="Match"/> equivalent. 
        /// </summary>
        /// <param name="s">A string containing a json response to convert.</param>
        /// <returns>A <see cref="Match"/> equivalent to the json response contained in s.</returns>
        public static Match Parse(string s)
        {
            using (var stringReader = new StringReader(s))
            using (var jsonReader = new JsonTextReader(stringReader))
            {
                return new Match(jsonReader);
            }
        }

        /// <summary>
        /// Converts the string representation of json response to its <see cref="Match"/> equivalent. A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="s">A string containing a json response to convert.</param>
        /// <param name="result">When this method returns, contains <see cref="Match"/> equivalent of the number contained in s, if the conversion succeeded, or null if the conversion failed. The conversion fails if the s parameter is null or String.Empty, is not of the correct format, or doesn't represent a json response. This parameter is passed uninitialized.</param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string s, out Match result)
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

        public long? MatchId { get; internal set; }

        public string Name { get; internal set; }

        public DateTime? StartTime { get; internal set; }

        public DateTime? EndTime { get; internal set; }

        public ReadOnlyCollection<Game> Games { get; internal set; }

        #endregion
    }
}
