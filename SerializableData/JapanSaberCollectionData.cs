using System.Collections.Generic;
using Newtonsoft.Json;

namespace JapanSaber.SerializableData
{
    /// <summary>
    /// AnnotatedBeatmapLevelsCollection のデータ
    /// </summary>
    [JsonObject]
    public class JapanSaberCollectionData
    {
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("type")]
        public string Type;
        [JsonProperty("query")]
        public Dictionary<string, string> Query;
        [JsonProperty("cover")]
        public string Cover;
    }
}
