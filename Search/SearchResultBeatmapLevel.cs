using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using JapanSaber.Modification;
using JapanSaber.Objects;
using BS_Utils.Utilities;
using System.IO;
using System;
using System.Net;
using System.IO.Compression;
using System.Linq;
using Imazen.WebP;
using System.Drawing.Imaging;
using System.Reflection;
using System.Collections;
using System.Drawing;
using System.Collections.Generic;
using JapanSaber.SerializableData;

namespace JapanSaber.Search
{
    public class SearchResultBeatmapLevel : IBeatmapLevel
    {
        private static readonly string CoverUrlBase = "https://japansaber.noguchii.net/covers/";
        private static readonly string CoverFolder = Path.Combine(Environment.CurrentDirectory, "UserData", Plugin.Name, "Covers");
        
        private static readonly string BeatserverDownloadUrl = "https://beatsaver.com/api/download/key/";
        private static readonly string CustomBeatmapsFolder = Path.Combine(Environment.CurrentDirectory, "Beat Saber_Data", "CustomLevels");
        
        private static readonly string DefaultCoverResourceName = "JapanSaber.Resources.NoCover.png";
        private static readonly string DownladableLevelPrefixId = "_Downloadable_Level";

        private static StandardLevelDetailViewController _DetailsController;

        private readonly SearchResultBeatmapLevelData Source;

        public SearchResultBeatmapLevel(SearchResultBeatmapLevelData source)
        {
            this.Source = source;
            this.Source.Init(this);

            levelID = $"{DownladableLevelPrefixId}_{Hash}";
            // kCustomLevelPrefixId を使うと所持/購入済みの扱いを受ける
            // Annotated...Collectionで購入済みの数が表示されない
            // 未購入（未入手）の文字色（グレー）がなくなる
            // levelID = $"{CustomLevelLoader.kCustomLevelPrefixId}_{Hash}";

            beatmapLevelData = new BeatmapLevelData(source.AudioClip, source.DifficultyBeatmapSets);
        }

        public string MapKey => Source.MapKey;
        public string Hash => Source.Hash.ToUpper();
        public string levelID { get; set; }
        public string songName { get; set; }
        public string songSubName { get; set; }
        public string songAuthorName { get; set; }
        public string levelAuthorName { get; set; }
        public float beatsPerMinute => Source.Bpm;
        public float songTimeOffset => 0;
        public float shuffle => 0;
        public float shufflePeriod => 0;
        public float previewStartTime => 0;
        public float previewDuration => 0;
        public float songDuration => Source.Length / 60f;

        public bool IsDownloadingCover { get; protected set; } = false;
        public bool IsDownloadingBeatmap { get; protected set; } = false;
        public bool IsDownloadedBeatmap { get; protected set; } = false;

        private static StandardLevelDetailViewController DetailsController
        {
            get
            {
                if (_DetailsController == null)
                {
                    _DetailsController = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>().FirstOrDefault();
                }
                return _DetailsController;
            }
        }

        public static EnvironmentInfoSO SheredEnvironmentInfoSO  = EnvironmentInfoSO.CreateInstance<EnvironmentInfoSO>();

        public EnvironmentInfoSO environmentInfo => SheredEnvironmentInfoSO;

        public EnvironmentInfoSO allDirectionsEnvironmentInfo => SheredEnvironmentInfoSO;

        public PreviewDifficultyBeatmapSet[] previewDifficultyBeatmapSets => Source.PreviewDifficultyBeatmapSets;

        public IBeatmapLevelData beatmapLevelData { get; protected set; }

        public void ApplyDisplayFormat(Modification.DisplayFormat type)
        {
            songName = SongNamesModifier.ToFormattedTitle(Source, type);
            songSubName = SongNamesModifier.ToFormattedSubtitle(Source, type);
            songAuthorName = SongNamesModifier.ToFormattedAuthor(Source, type);
            levelAuthorName = SongNamesModifier.ToFormattedMapper(Source, type);
        }

        public async Task<Texture2D> GetCoverImageTexture2DAsync(CancellationToken cancellationToken)
        {
            // Logger.Debug($"GetCoverImageTextre2DAsync() : {Source.Title}");
            try
            {

                var cachePath = Path.Combine(CoverFolder, $"{Source.Hash}.jpg");
                // キャッシュ利用
                if (File.Exists(cachePath))
                {
                    return await Utils.GetTextureFromFileAsync(cachePath, cancellationToken);
                }

                if (!IsDownloadingCover)
                {
                    IsDownloadingCover = true;

                    var context = new DownloadContext($"{CoverUrlBase}{Source.Hash}.webp")
                    {
                        OnSuccess = (status, data) =>
                        {
                            // WebPはUnityで使えないので変換する
                            var decoder = new SimpleDecoder();
                            var bitmap = decoder.DecodeFromBytes(data, data.Length);

                            // キャッシュ作成
                            bitmap.Save(cachePath, ImageFormat.Jpeg);

                            /*
                            // Texture2Dを作成
                            using (var stream = new MemoryStream())
                            {
                                bitmap.Save(stream, bitmap.RawFormat);
                                var texture = new Texture2D(2, 2);
                                texture.LoadImage(stream.ToArray());
                            }
                            */
                        },
                        OnError = (_, __) => IsDownloadingCover = false,
                        OnCancelling = () => IsDownloadingCover = false,
                    };

                    WebRequestClient.Instance.EnqueueRequest(context);
                }

                return await Utils.GetTextureFromResourceAsync(DefaultCoverResourceName, cancellationToken);
            }
            catch(Exception ex)
            {
                Logger.Debug("in GetCoverImageTexture2DAsync");
                Logger.Debug(ex);

                return null;
            }
        }

        public async Task<AudioClip> GetPreviewAudioClipAsync(CancellationToken cancellationToken)
        {
            return await Task.Factory.StartNew<AudioClip>(() =>
            {
                return Source.AudioClip;
            }, cancellationToken);
        }

        #region Download beatmap

        public virtual void DownloadBeatmap()
        {
            if (Plugin.Instance.CurrentContinuousDownloadCount >= Plugin.Instance.MaxContinuousDownloadCount) return;
            Plugin.Instance.CurrentContinuousDownloadCount++;

            if (IsDownloadingBeatmap || Plugin.Instance.IsDownloadingBeatmap) return;
            IsDownloadingBeatmap = true;
            Plugin.Instance.IsDownloadingBeatmap = true;

            Logger.IPALogger.Info($"DownloadBeatmap() : {Source.MapKey} {Source.Title}");

            var context = new DownloadContext($"{BeatserverDownloadUrl}{Source.MapKey}")
            {
                OnProgress = this.OnProgressDownloadingBeatmap,
                OnSuccess = this.OnSuccessDownloadBeatmap,
                OnError = this.OnErrorDownloadBeatmap,
                OnCancelling = this.OnCancellingDownloadBeatmap,
            };
            WebRequestClient.Instance.EnqueueRequest(context);
        }

        protected virtual void OnProgressDownloadingBeatmap(float progress)
        {
          //  Logger.Debug($"OnProgressDownloadingBeatmap() : {progress}");

            IsDownloadingBeatmap = true;
            Plugin.Instance.IsDownloadingBeatmap = true;
            
            if (Plugin.Instance.LastSelectedPreviewLevel?.levelID == levelID)
            {
                DetailsController?.ShowContent(StandardLevelDetailViewController.ContentType.OwnedAndDownloading, "", progress, "Downloading");
            }
        }

        protected virtual void OnSuccessDownloadBeatmap(long statusCode, byte[] response)
        {
            Logger.IPALogger.Info($"OnSuccessDownloadBeatmap() : {Source.MapKey} {Source.Title}");

            IsDownloadingBeatmap = false;
            try
            {
                var folderName = $"{Source.MapKey} {Source.Title}";
                foreach (var c in Path.GetInvalidFileNameChars())
                {
                    folderName = folderName.Replace(c.ToString(), "");
                }
                var downlaodFolder = Path.Combine(CustomBeatmapsFolder, folderName);
                Directory.CreateDirectory(downlaodFolder);

                // ZIPを解凍
                using (var archiveStream = new MemoryStream(response))
                using (var zip = new ZipArchive(archiveStream, ZipArchiveMode.Read))
                {
                    foreach (var entry in zip.Entries)
                    {
                        var path = Path.Combine(downlaodFolder, entry.FullName);
                        using (var entryStream = new FileStream(path, FileMode.Create))
                        {
                            entry.Open().CopyTo(entryStream);
                            entryStream.Flush();
                        }
                    }
                }
              
                SongCore.Loader.Instance.RefreshSongs(false);
                Plugin.Instance.IsDownloadingBeatmap = false;
                IsDownloadedBeatmap = true;

                Plugin.Instance.DownloadButton?.SetActive(false);

                SongsStore.Instance.Add((SongNames)Source);
                SongsStore.Instance.Save();
            }
            catch (Exception ex)
            {
                Logger.IPALogger.Error($"in OnSuccessDownloadBeatmap() : {Source.MapKey} {Source.Title}");
                Logger.IPALogger.Error(ex);
            }
        }

        protected virtual void OnErrorDownloadBeatmap(long statusCode, string error)
        {
            Logger.Debug($"OnErrorDownloadBeatmap({statusCode}) : {Source.Title}");

            Plugin.Instance.IsDownloadingBeatmap = false;
            IsDownloadingBeatmap = false;
            Plugin.Instance.CurrentContinuousDownloadCount--;
        }

        protected virtual void OnCancellingDownloadBeatmap()
        {
            Logger.Debug($"OnCancellingDownloadBeatmap() : {Source.Title}");

            Plugin.Instance.IsDownloadingBeatmap = false;
            IsDownloadingBeatmap = false;
            Plugin.Instance.CurrentContinuousDownloadCount--;
        }

        #endregion
    }
}
