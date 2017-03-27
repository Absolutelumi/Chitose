using Osu.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Osu
{
    /// <summary>
    /// Containing game information.
    /// </summary>
    public class Game
    {
        #region ~CONSTRUCTOR~

        internal Game(JsonTextReader jsonReader)
        {
            int Depth = jsonReader.Depth;
            while (jsonReader.Read())
            {
                switch (jsonReader.TokenType)
                {
                    case JsonToken.PropertyName:
                        switch (jsonReader.Value.ToString())
                        {
                            case "game_id":
                                this.GameId = long.Parse(jsonReader.ReadAsString());
                                break;
                            case "start_time":
                                this.StartTime = jsonReader.ReadAsDateTime();
                                break;
                            case "end_time":
                                this.EndTime = jsonReader.ReadAsDateTime();
                                break;
                            case "beatmap_id":
                                this.Beatmap_Id = long.Parse(jsonReader.ReadAsString());
                                break;
                            case "play_mode":
                                this.PlayMode = (Mode)jsonReader.ReadAsInt32();
                                break;
                            case "match_type":
                                this.MatchType = jsonReader.ReadAsInt32();
                                break;
                            case "scoring_type":
                                this.ScoringType = (WinningCondition)jsonReader.ReadAsInt32();
                                break;
                            case "team_type":
                                this.TeamType = (TeamType)jsonReader.ReadAsInt32();
                                break;
                            case "mods":
                                this.Mods = (Mods)jsonReader.ReadAsInt32();
                                break;
                            case "scores":
                                IList<Scores> scores = new List<Scores>();
                                while (jsonReader.Read())
                                    if (jsonReader.TokenType == JsonToken.StartObject)
                                        scores.Add(new Scores(jsonReader));
                                this.Scores = new ReadOnlyCollection<Scores>(scores);
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
        /// Converts the string representation of json response to its <see cref="Game"/> equivalent. 
        /// </summary>
        /// <param name="s">A string containing a json response to convert.</param>
        /// <returns>A <see cref="Game"/> equivalent to the json response contained in s.</returns>
        public static Game Parse(string s)
        {
            using (var stringReader = new StringReader(s))
            using (var jsonReader = new JsonTextReader(stringReader))
            {
                return new Game(jsonReader);
            }
        }

        /// <summary>
        /// Converts the string representation of json response to its <see cref="Game"/> equivalent. A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="s">A string containing a json response to convert.</param>
        /// <param name="result">When this method returns, contains <see cref="Game"/> equivalent of the number contained in s, if the conversion succeeded, or null if the conversion failed. The conversion fails if the s parameter is null or String.Empty, is not of the correct format, or doesn't represent a json response. This parameter is passed uninitialized.</param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string s, out Game result)
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

        public long? GameId { get; internal set; }

        public DateTime? StartTime { get; internal set; }

        public DateTime? EndTime { get; internal set; }

        public long? Beatmap_Id { get; internal set; }

        public Mode? PlayMode { get; internal set; }

        public int? MatchType { get; internal set; }

        public WinningCondition? ScoringType { get; internal set; }

        public TeamType? TeamType { get; internal set; }

        public Mods? Mods { get; internal set; }

        public ReadOnlyCollection<Scores> Scores { get; internal set; }

        #endregion
    }
}
