using Newtonsoft.Json;

namespace JapanSaber.SerializableData
{
    [JsonObject]
    public class Playlist
    {
        [JsonProperty("songs")]
        public SongNames[] Songs;
        [JsonProperty("playlistTitle")]
        public string PlaylistTitle;
        [JsonProperty("playlistAuthor")]
        public string PlaylistAuthor;
        [JsonProperty("image")]
        public string Image;
    }
}
