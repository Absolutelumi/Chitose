namespace OsuApi.Model
{
    public class User
    {
        public string UserID { get { return user_id; } }
        public string Username { get { return username; } }
        public int Count300 { get { return count300; } }
        public int Count100 { get { return count100; } }
        public int Count50 { get { return count50; } }
        public int Playcount { get { return playcount; } }
        public long RankedScore { get { return ranked_score; } }
        public long TotalScore { get { return total_score; } }
        public int Rank { get { return pp_rank; } }
        public double Level { get { return level; } }
        public double PPRaw { get { return pp_raw; } }
        public double Accuracy { get { return accuracy; } }
        public int SSCount { get { return count_rank_ss; } }
        public int SCount { get { return count_rank_s; } }
        public int ACount { get { return count_rank_a; } }
        public string Country { get { return country; } }
        public int CountryRank { get { return pp_country_rank; } }

        #region Json Fields

        public string user_id;
        public string username;
        public int count300;
        public int count100;
        public int count50;
        public int playcount;
        public long ranked_score;
        public long total_score;
        public int pp_rank;
        public double level;
        public double pp_raw;
        public double accuracy;
        public int count_rank_ss;
        public int count_rank_s;
        public int count_rank_a;
        public string country;
        public int pp_country_rank; 

        #endregion
    }
}
