using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JapanSaber.SerializableData
{
    /// <summary>
    /// JapanSaberの検索結果
    /// </summary>
    [JsonObject]
    public class SearchResult
    {
        [JsonProperty]
        public int Version;
        [JsonProperty]
        public int Count;
        [JsonProperty]
        public SearchResultBeatmapLevelData[] Result;
    }
}
