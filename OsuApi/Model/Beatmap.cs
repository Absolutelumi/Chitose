using System;

namespace OsuApi.Model
{
    public class Beatmap
    {
        public double ApproachRate { get => diff_approach; }
        public DateTime ApprovalDate { get => DateTime.Parse(approved_date); }
        public string Artist { get => artist; }
        public string BeatmapId { get => beatmap_id; }
        public string Beatmapper { get => creator; }
        public string BeatmapSetId { get => beatmapset_id; }
        public double Bpm { get => bpm; }
        public double CircleSize { get => diff_size; }
        public string Difficulty { get => version; }
        public int DrainLength { get => hit_length; }
        public Genre Genre { get => (Genre)genre_id; }
        public double HealthDrain { get => diff_drain; }
        public Language Language { get => (Language)language_id; }
        public DateTime LastUpdate { get => DateTime.Parse(last_update); }
        public int MaxCombo { get => max_combo; }
        public string Md5Hash { get => file_md5; }
        public Mode Mode { get => (Mode)mode; }
        public int NumberOfFavorites { get => favorite_count; }
        public int NumberOfPasses { get => passcount; }
        public int NumberOfPlays { get => playcount; }
        public double OverallDifficulty { get => diff_overall; }
        public string Source { get => source; }
        public double Stars { get => difficultyrating; }
        public Status Status { get => (Status)approved; }
        public string[] Tags { get => tags.Split(' '); }
        public string Title { get => title; }
        public int TotalLength { get => total_length; }

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