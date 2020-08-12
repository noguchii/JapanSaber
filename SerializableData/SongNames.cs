using Newtonsoft.Json;

namespace JapanSaber.SerializableData
{
    /// <summary>
    /// JapanSaberのプレイリストのアイテム
    /// </summary>
    [JsonObject]
    public class SongNames
    {
        [JsonProperty("hash", Required = Required.Always)]
        public string Hash;
        [JsonProperty(Required = Required.Always)]
        public string MapKey;
        [JsonProperty(Required = Required.Always)]
        public string Title;
        [JsonProperty(Required = Required.AllowNull)]
        public string Subtitle;
        [JsonProperty(Required = Required.AllowNull)]
        public string Author;
        [JsonProperty(Required = Required.AllowNull)]
        public string Product;
        [JsonProperty(Required = Required.AllowNull)]
        public string Mapper;
        [JsonProperty(Required = Required.AllowNull)]
        public string Yomi;
    }
}