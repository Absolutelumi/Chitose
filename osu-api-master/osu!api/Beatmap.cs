using Osu.Utils;
using System;
using System.IO;

namespace Osu
{
    /// <summary>
    /// Containing beatmap information.
    /// </summary>
    public class Beatmap
    {
        #region ~CONSTRUCTOR~

        internal Beatmap(JsonTextReader jsonReader)
        {
            int Depth = jsonReader.Depth;
            while (jsonReader.Read())
            {
                switch (jsonReader.TokenType)
                {
                    case JsonToken.PropertyName:
                        switch (jsonReader.Value.ToString())
                        {
                            case "approved":
                                this.Approved = (RankStatus)(int)jsonReader.ReadAsInt32();
                                break;
                            case "approved_date":
                                this.ApprovedDate = jsonReader.ReadAsDateTime();
                                break;
                            case "last_update":
                                this.LastUpdate = jsonReader.ReadAsDateTime();
                                break;
                            case "artist":
                                this.Artist = jsonReader.ReadAsString();
                                break;
                            case "beatmap_id":
                                this.BeatmapId = long.Parse(jsonReader.ReadAsString());
                                break;
                            case "beatmapset_id":
                                this.BeatmapSetId = long.Parse(jsonReader.ReadAsString());
                                break;
                            case "bpm":
                                this.Bpm = jsonReader.ReadAsDouble();
                                break;
                            case "creator":
                                this.Creator = jsonReader.ReadAsString();
                                break;
                            case "difficultyrating":
                                this.DifficultyRating = jsonReader.ReadAsDouble();
                                break;
                            case "diff_size":
                                this.DiffSize = jsonReader.ReadAsDouble();
                                break;
                            case "diff_overall":
                                this.DiffOverall = jsonReader.ReadAsDouble();
                                break;
                            case "diff_approach":
                                this.DiffApproach = jsonReader.ReadAsDouble();
                                break;
                            case "diff_drain":
                                this.DiffDrain = jsonReader.ReadAsDouble();
                                break;
                            case "hit_length":
                                this.HitLength = jsonReader.ReadAsInt32();
                                break;
                            case "source":
                                this.Source = jsonReader.ReadAsString();
                                break;
                            case "genre_id":
                                this.Genre = (Genre)jsonReader.ReadAsInt32();
                                break;
                            case "language_id":
                                this.Language = (Language)jsonReader.ReadAsInt32();
                                break;
                            case "title":
                                this.Title = jsonReader.ReadAsString();
                                break;
                            case "total_length":
                                this.TotalLength = jsonReader.ReadAsInt32();
                                break;
                            case "version":
                                this.Version = jsonReader.ReadAsString();
                                break;
                            case "file_md5":
                                this.FileMd5 = jsonReader.ReadAsString();
                                break;
                            case "mode":
                                this.Mode = (Mode)jsonReader.ReadAsInt32();
                                break;
                            case "tags":
                                this.Tags = jsonReader.ReadAsString().Split(' ');
                                break;
                            case "favourite_count":
                                this.FavouriteCount = jsonReader.ReadAsInt32();
                                break;
                            case "playcount":
                                this.PlayCount = jsonReader.ReadAsInt32();
                                break;
                            case "passcount":
                                this.PassCount = jsonReader.ReadAsInt32();
                                break;
                            case "max_combo":
                                this.MaxCombo = jsonReader.ReadAsInt32();
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
        /// Converts the string representation of json response to its <see cref="Beatmap"/> equivalent. 
        /// </summary>
        /// <param name="s">A string containing a json response to convert.</param>
        /// <returns>A <see cref="Beatmap"/> equivalent to the json response contained in s.</returns>
        public static Beatmap Parse(string s)
        {
            using (var stringReader = new StringReader(s))
            using (var jsonReader = new JsonTextReader(stringReader))
            {
                return new Beatmap(jsonReader);
            }
        }

        /// <summary>
        /// Converts the string representation of json response to its <see cref="Beatmap"/> equivalent. A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="s">A string containing a json response to convert.</param>
        /// <param name="result">When this method returns, contains <see cref="Beatmap"/> equivalent of the number contained in s, if the conversion succeeded, or null if the conversion failed. The conversion fails if the s parameter is null or String.Empty, is not of the correct format, or doesn't represent a json response. This parameter is passed uninitialized.</param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string s, out Beatmap result)
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

        /// <summary>
        /// Rank status
        /// </summary>
        public RankStatus? Approved { get; internal set; }

        /// <summary>
        /// Last update date, timezone same as above. May be after approved_date if map was unranked and reranked.
        /// </summary>
        public DateTime? LastUpdate { get; internal set; }

        /// <summary>
        /// Date ranked.
        /// </summary>
        public DateTime? ApprovedDate { get; internal set; }

        public string Artist { get; internal set; }

        /// <summary>
        /// Beatmap_id is per difficulty.
        /// </summary>
        public long? BeatmapId { get; internal set; }

        /// <summary>
        /// Beatmapset_id groups difficulties into a set.
        /// </summary>
        public long? BeatmapSetId { get; internal set; }

        public double? Bpm { get; internal set; }

        public string Creator { get; internal set; }

        /// <summary>
        /// The amount of stars the map would have ingame and on the website.
        /// </summary>
        public double? DifficultyRating { get; internal set; }

        /// <summary>
        /// Circle size value (CS).
        /// </summary>
        public double? DiffSize { get; internal set; }

        /// <summary>
        /// Overall difficulty (OD).
        /// </summary>
        public double? DiffOverall { get; internal set; }

        /// <summary>
        /// Approach Rate (AR).
        /// </summary>
        public double? DiffApproach { get; internal set; }

        /// <summary>
        /// Healthdrain (HP).
        /// </summary>
        public double? DiffDrain { get; internal set; }

        /// <summary>
        /// Seconds from first note to last note not including breaks.
        /// </summary>
        public int? HitLength { get; internal set; }

        public string Source { get; internal set; }

        public Genre? Genre { get; internal set; }

        public Language? Language { get; internal set; }

        /// <summary>
        /// Song name.
        /// </summary>
        public string Title { get; internal set; }

        /// <summary>
        /// Seconds from first note to last note including breaks.
        /// </summary>
        public int? TotalLength { get; internal set; }

        /// <summary>
        /// Difficulty name.
        /// </summary>
        public string Version { get; internal set; }

        /// <summary>
        /// Md5 hash of the beatmap.
        /// </summary>
        public string FileMd5 { get; internal set; }

        /// <summary>
        /// Game mode.
        /// </summary>
        public Mode? Mode { get; internal set; }

        /// <summary>
        /// Beatmap tags.
        /// </summary>
        public string[] Tags { get; internal set; }

        /// <summary>
        ///  Number of times the beatmap was favourited.
        /// </summary>
        public int? FavouriteCount { get; internal set; }

        /// <summary>
        /// Number of times the beatmap was played.
        /// </summary>
        public int? PlayCount { get; internal set; }

        /// <summary>
        /// Number of times the beatmap was passed, completed (the user didn't fail or retry).
        /// </summary>
        public int? PassCount { get; internal set; }

        /// <summary>
        /// The maximum combo an user can reach playing this beatmap.
        /// </summary>
        public int? MaxCombo { get; internal set; }

        #endregion
    }
}
