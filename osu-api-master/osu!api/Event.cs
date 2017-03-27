using Osu.Utils;
using System;
using System.IO;

namespace Osu
{
    /// <summary>
    /// Containing event information.
    /// </summary>
    public class Event
    {
        #region ~CONSTRUCTOR~

        internal Event(JsonTextReader jsonReader)
        {
            int Depth = jsonReader.Depth;
            while (jsonReader.Read())
            {
                switch (jsonReader.TokenType)
                {
                    case JsonToken.PropertyName:
                        switch (jsonReader.Value.ToString())
                        {
                            case "display_html":
                                this.DisplayHtml = jsonReader.ReadAsString();
                                break;
                            case "beatmap_id":
                                this.BeatmapId = jsonReader.ReadAsInt32();
                                break;
                            case "beatmapset_id":
                                this.BeatmapSetId = jsonReader.ReadAsInt32();
                                break;
                            case "date":
                                this.Date = jsonReader.ReadAsDateTime();
                                break;
                            case "epicfactor":
                                this.Epificator = jsonReader.ReadAsInt32();
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
        /// Converts the string representation of json response to its <see cref="Event"/> equivalent. 
        /// </summary>
        /// <param name="s">A string containing a json response to convert.</param>
        /// <returns>A <see cref="Event"/> equivalent to the json response contained in s.</returns>
        public static Event Parse(string s)
        {
            using (var stringReader = new StringReader(s))
            using (var jsonReader = new JsonTextReader(stringReader))
            {
                return new Event(jsonReader);
            }
        }

        /// <summary>
        /// Converts the string representation of json response to its <see cref="Event"/> equivalent. A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="s">A string containing a json response to convert.</param>
        /// <param name="result">When this method returns, contains <see cref="Event"/> equivalent of the number contained in s, if the conversion succeeded, or null if the conversion failed. The conversion fails if the s parameter is null or String.Empty, is not of the correct format, or doesn't represent a json response. This parameter is passed uninitialized.</param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string s, out Event result)
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

        public string DisplayHtml { get; internal set; }

        public int? BeatmapId { get; internal set; }

        public int? BeatmapSetId { get; internal set; }

        public DateTime? Date { get; internal set; }

        /// <summary>
        /// How "epic" this event is (between 1 and 32).
        /// </summary>
        public int? Epificator { get; internal set; }

        #endregion
    }
}
