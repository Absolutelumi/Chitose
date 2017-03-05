using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixivLib.Models
{
    public class RankingResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("response")]
        public RankingData[] Results { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("pagination")]
        public Pagination Pagination { get; set; }
    }

    public class RankingData
    {
        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("works")]
        public RankedWork[] Works { get; set; }
    }

    public class RankedWork
    {
        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("previous_rank")]
        public int PreviousRank { get; set; }

        [JsonProperty("work")]
        public Work Work { get; set; }
    }
}
