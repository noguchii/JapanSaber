using JapanSaber.Configuration;
using JapanSaber.Modification;
using JapanSaber.Objects;
using JapanSaber.SerializableData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace JapanSaber.Search
{
    /// <summary>
    /// プレイリストやレベルパックのようにメインパネルの下に表示されるリスト
    /// </summary>
    public class SearchLevelCollection : ScriptableObject, IAnnotatedBeatmapLevelCollection,
        IAsyncDataSource, IHaveDownladableLevel
    {
        private static readonly string API = "https://japansaber.noguchii.net/api/search-songs";

        public IBeatmapLevelCollection beatmapLevelCollection { get; protected set; }
            = new SimpleBeatmapLevelCollection();

        public string collectionName { get; set; }
        public Sprite coverImage { get; set; }
        public Dictionary<string, string> Query { get; set; }
        public int Page { get; set; } = 1;
        public string OrderBy { get; set; }
        public bool IsInitialLoaded { get; protected set; } = false;
        public bool IsRefreshedVisualAfterInitialLoad { get; set; } = false;
        public bool IsInitialLoading { get; protected set; } = false;
        public int ResultCount { get; protected set; } = 0;

        public string CreateGetURL()
        {
            try
            {
                var data = "";
                var query = Query;
                if (!string.IsNullOrWhiteSpace(OrderBy))
                {
                    query["OrderBy"] = OrderBy;
                }

                foreach (var item in query)
                {
                    if (item.Key == "page")
                    {
                        continue;
                    }
                    data += $"{item.Key}={UnityWebRequest.EscapeURL(item.Value)}&";
                }

                if (JSConfig.Instance.IsExceptLowRating && !query.ContainsKey("rating"))
                {
                    data += "rating=50,100&";
                }

                data += $"page={Page}&data_format=beatsaber";

                return $"{API}?{data}";
            }
            catch (Exception ex)
            {
                Logger.IPALogger.Error(ex);
            }

            return null;
        }

        public void OnSuccess(long statusCode, byte[] response)
        {
            Logger.Debug($"OnSuccess() : {CreateGetURL()}");

            if (statusCode == 200)
            {
                try
                {
                    var beatmapLevels = new List<IPreviewBeatmapLevel>();
                    var searchResult = JsonConvert.DeserializeObject<SearchResult>(Encoding.UTF8.GetString(response));
                    // Countはレスポンスされた数ではなく、検索結果の総数
                    ResultCount = searchResult.Count;

                    foreach (var songData in searchResult.Result)
                    {
                        // 未ダウンロードのレベルは独自データを使用してプレビューできるようにする
                        var songCoreBeatmap = SongCore.Loader.GetLevelByHash(songData.Hash);
                        if (songCoreBeatmap == null)
                        {
                            var level = new SearchResultBeatmapLevel(songData);
                            level.ApplyDisplayFormat(JSConfig.Instance.GetViewType());
                            beatmapLevels.Add(level);
                        }
                        else
                        {
                            // データ追加、更新
                            SongsStore.Instance.Store[songData.Hash] = songData;
                            beatmapLevels.Add(songCoreBeatmap);
                        }
                    }

                    ((SimpleBeatmapLevelCollection)beatmapLevelCollection).beatmapLevels = beatmapLevels.ToArray();
                }
                catch  (Exception ex)
                {
                    Logger.IPALogger.Critical("Failed to parse response from JapanSaber");
                    Logger.IPALogger.Critical(ex);
                }
            }

            IsInitialLoading = false;
            IsInitialLoaded = true;
        }
        
        public void OnError(long statusCode, string error)
        {
            IsInitialLoading = false;
            IsInitialLoaded = true;
        }

        public void GetDataAsync()
        {
            Logger.Debug($"GetDataFromWeb() : {CreateGetURL()}");

            if (!IsInitialLoaded)
            {
                IsInitialLoading = true;
            }

            var context = new DownloadContext(CreateGetURL())
            {
                OnSuccess = OnSuccess,
                OnError = OnError,
            };

            WebRequestClient.Instance.EnqueueRequest(context);
        }
    }
}
