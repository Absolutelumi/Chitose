using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixivLib.Models
{
    public class EmojiResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("response")]
        public Emoji[] Emojis { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }

    public class Emoji
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("image_urls")]
        public EmojiImageUrls ImageURLs { get; set; }
    }

    public class EmojiImageUrls
    {
        [JsonProperty("px_56x56")]
        public string SmallURL { get; set; }

        [JsonProperty("px_128x128")]
        public string LargeURL { get; set; }
    }
}
