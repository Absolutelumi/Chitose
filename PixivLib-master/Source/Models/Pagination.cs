using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixivLib.Models
{
    public class Pagination
    {
        [JsonProperty("previous")]
        public int? Previous { get; set; }

        [JsonProperty("next")]
        public int? Next { get; set; }

        [JsonProperty("current")]
        public int Current { get; set; }

        [JsonProperty("per_page")]
        public int PerPage { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("pages")]
        public int Pages { get; set; }
    }
}
