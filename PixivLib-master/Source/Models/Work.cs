using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixivLib.Models
{
    public class WorksResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("response")]
        public Work[] Results { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("pagination")]
        public Pagination Pagination { get; set; }
    }
    
    public class Work
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("caption")]
        public string Caption { get; set; }

        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("tools")]
        public string[] Tools { get; set; }

        [JsonProperty("image_urls")]
        public ImageURLs ImageURLs { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("stats")]
        public WorkStats Stats { get; set; }

        [JsonProperty("publicity")]
        public int Publicity { get; set; }

        [JsonProperty("age_limit")]
        public string AgeLimit { get; set; }

        [JsonProperty("created_time")]
        public string CreatedTime { get; set; }

        [JsonProperty("reuploaded_time")]
        public string ReuploadedTime { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("is_manga")]
        public bool IsManga { get; set; }

        [JsonProperty("is_liked")]
        public bool IsLiked { get; set; }

        [JsonProperty("favorite_id")]
        public int FavoriteID { get; set; }

        [JsonProperty("page_count")]
        public int PageCount { get; set; }

        [JsonProperty("book_style")]
        public string BookStyle { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("metadata")]
        public object Metadata { get; set; }

        [JsonProperty("content_type")]
        public object ContentType { get; set; }
    }

    public class WorkStats
    {
        [JsonProperty("scored_count")]
        public int ScoreCount { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("views_count")]
        public int ViewCount { get; set; }

        [JsonProperty("favorited_count")]
        public FavoriteCount FavoriteCount { get; set; }

        [JsonProperty("commented_count")]
        public object CommentCount { get; set; }
    }

    public class FavoriteCount
    {
        [JsonProperty("_public")]
        public int? Public { get; set; }

        [JsonProperty("_private")]
        public int? Private { get; set; }
    }

    public class ImageURLs
    {
        [JsonProperty("px_128x128")]
        public string ThumbURL { get; set; }

        [JsonProperty("px_480mw")]
        public string PhoneURL { get; set; }

        [JsonProperty("large")]
        public string OriginalURL { get; set; }
    }
}
