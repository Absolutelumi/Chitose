using System;

namespace OsuApi.Model
{
    public class Beatmap
    {
        public double ApproachRate { get { return diff_approach; } }
        public DateTime ApprovalDate { get { return DateTime.Parse(approved_date); } }
        public string Artist { get { return artist; } }
        public string BeatmapId { get { return beatmap_id; } }
        public string Beatmapper { get { return creator; } }
        public string BeatmapSetId { get { return beatmapset_id; } }
        public double Bpm { get { return bpm; } }
        public double CircleSize { get { return diff_size; } }
        public string Difficulty { get { return version; } }
        public int DrainLength { get { return hit_length; } }
        public Genre Genre { get { return (Genre)genre_id; } }
        public double HealthDrain { get { return diff_drain; } }
        public Language Language { get { return (Language)language_id; } }
        public DateTime LastUpdate { get { return DateTime.Parse(last_update); } }
        public int MaxCombo { get { return max_combo; } }
        public string Md5Hash { get { return file_md5; } }
        public Mode Mode { get { return (Mode)mode; } }
        public int NumberOfFavorites { get { return favorite_count; } }
        public int NumberOfPasses { get { return passcount; } }
        public int NumberOfPlays { get { return playcount; } }
        public double OverallDifficulty { get { return diff_overall; } }
        public string Source { get { return source; } }
        public double Stars { get { return difficultyrating; } }
        public Status Status { get { return (Status)approved; } }
        public string[] Tags { get { return tags.Split(' '); } }
        public string Title { get { return title; } }
        public int TotalLength { get { return total_length; } }

        #region Json Fields

        public int approved;
        public string approved_date;
        public string artist;
        public string beatmap_id;
        public string beatmapset_id;
        public double bpm;
        public string creator;
        public double diff_approach;
        public double diff_drain;
        public double diff_overall;
        public double diff_size;
        public double difficultyrating;
        public int favorite_count;
        public string file_md5;
        public int genre_id;
        public int hit_length;
        public int language_id;
        public string last_update;
        public int max_combo;
        public int mode;
        public int passcount;
        public int playcount;
        public string source;
        public string tags;
        public string title;
        public int total_length;
        public string version;

        #endregion Json Fields
    }
}