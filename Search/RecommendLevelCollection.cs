using JapanSaber.Configuration;
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
    public class RecommendLevelCollection : ScriptableObject, IAnnotatedBeatmapLevelCollection,
        IAsyncDataSource, IHaveDownladableLevel
    {
        private static readonly string API = "https://japansaber.noguchii.net/api/recommend";

        public IBeatmapLevelCollection beatmapLevelCollection { get; protected set; }
            = new SimpleBeatmapLevelCollection();
        public string collectionName { get; set; }
        public Sprite coverImage { get; set; }
        public bool IsInitialLoaded { get; protected set; } = false;
        public bool IsRefreshedVisualAfterInitialLoad { get; set; } = false;
        public bool IsInitialLoading { get; protected set; } = false;

        public void OnSuccess(long statusCode, byte[] response)
        {
            if (statusCode == 200)
            {
                var beatmapLevels = new List<IPreviewBeatmapLevel>();

                var searchResult = JsonConvert.DeserializeObject<SearchResult>(Encoding.UTF8.GetString(response));
                foreach (var source in searchResult.Result)
                {
                    var songCoreBeatmap = SongCore.Loader.GetLevelByHash(source.Hash);
                    if (songCoreBeatmap == null)
                    {
                        beatmapLevels.Add(new SearchResultBeatmapLevel(source));
                    }
                    else
                    {
                        beatmapLevels.Add(songCoreBeatmap);
                    }
                }

                ((SimpleBeatmapLevelCollection)beatmapLevelCollection).beatmapLevels = beatmapLevels.ToArray();
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
            Logger.Debug("GetDataFromWeb()");

            if (!IsInitialLoaded)
            {
                IsInitialLoading = true;
            }

            var context = new DownloadContext(API)
            {
                OnSuccess = OnSuccess,
                OnError = OnError,
            };

            WebRequestClient.Instance.EnqueueRequest(context);
        }
    }
}
