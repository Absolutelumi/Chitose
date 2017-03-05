using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixivLib.Models
{
    public class AppSettingsResponse
    {
        [JsonProperty("server_status")]
        public string ServerStatus { get; set; }

        [JsonProperty("response")]
        public AppSettings Settings { get; set; }
    }

    public class AppSettings
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("google_play_url")]
        public string GooglePlayURL { get; set; }
    }
}
