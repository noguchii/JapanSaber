using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JapanSaber.Objects;
using JapanSaber.SerializableData;
using Newtonsoft.Json;

namespace JapanSaber.Modification
{
    public class SongsStore
    {
        private static readonly string CachedPlaylistPath =
            Path.Combine(Environment.CurrentDirectory, $"UserData/{Plugin.Name}-Playlists.cache");
        private static readonly string StoredSongsPath =
            Path.Combine(Environment.CurrentDirectory, $"UserData/{Plugin.Name}-Songs.json");
        private static readonly string PlaylistDirecotry =
            Path.Combine(Environment.CurrentDirectory, $"Playlists/");
        private static readonly string PlaylistExtension = "*.jsaber.json";

        private static SongsStore _Instance;
        public static SongsStore Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new SongsStore();
                    _Instance.Load();
                }
                return _Instance;
            }
        }

        public Dictionary<string, SongNames> Store { get; private set; }

        public bool IsLoaded { get; private set; }

        public bool Save()
        {
            if (!IsLoaded)
            {
                throw new InvalidOperationException("Need to call after called Load()");
            }

            try
            {
                File.WriteAllText(StoredSongsPath, JsonConvert.SerializeObject(Store.Values.ToArray()));
            }
            catch
            {
                return false;
            }

            return true;
        }

        public void LoadPlaylist(bool isForce = true)
        {
            List<string> cachedPlaylists = null;

            try
            {
                if (File.Exists(CachedPlaylistPath))
                {
                    // キャッシュ済みプレイリストを取得する
                    cachedPlaylists = File.ReadAllLines(CachedPlaylistPath).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.IPALogger.Error($"Faild to load playlits cache. {ex.Message}");
            }
            if (cachedPlaylists == null)
            {
                cachedPlaylists = new List<string>();
            }

            var playlists = Directory.GetFiles(PlaylistDirecotry, PlaylistExtension);
            foreach (var p in playlists)
            {
                try
                {
                    var isCached = cachedPlaylists.Contains(p);
                    // 既にキャッシュ済みかどうか調べる
                    if (isForce == false && isCached) continue;

                    var playlist = JsonConvert.DeserializeObject<Playlist>(File.ReadAllText(p));
                    if (playlist.Songs == null)
                    {
                        Logger.IPALogger.Error($"Invalid Playlist:\n{p}");
                        continue;
                    }

                    foreach (var s in playlist.Songs)
                    {
                        // 不備がありそうなデータは使わない（プレイリスト編集ソフトで後から追加された曲とか）
                        if (string.IsNullOrWhiteSpace(s.Hash) || string.IsNullOrWhiteSpace(s.MapKey))
                        {
                            Logger.IPALogger.Notice($"Invalid Infos: {s.Hash} {s.MapKey} {s.MapKey}");
                            continue;
                        }

                        Store[s.Hash] = s;
                    }

                    if (!isCached)
                    {
                        cachedPlaylists.Add(p);
                    }
                }
                catch (Exception ex)
                {
                    Logger.IPALogger.Error(ex);
                }

                File.AppendAllLines(CachedPlaylistPath, cachedPlaylists);
            }
        }

        public void Load()
        {
            if (IsLoaded)
            {
                throw new InvalidOperationException("Already loaded");
            }

            Dictionary<string, SongNames> result = null;
            try
            {
                if (File.Exists(StoredSongsPath))
                {
                    // キャッシュを読み込み
                    var songs = JsonConvert.DeserializeObject<SongNames[]>(File.ReadAllText(StoredSongsPath));
                    result = songs.ToDictionary(s => s.Hash);
                }
            }
            catch (Exception ex)
            {
                Logger.IPALogger.Error($"Faild to load store\n{ex.Message}");
            }
            
            LoadPlaylist(false);

            if (result == null)
            {
                Logger.Debug("SongsStore use default");
                result = new Dictionary<string, SongNames>();
            }

            Logger.IPALogger.Info($"Has {result.Count} infos");

            IsLoaded = true;
            Store = result;
        }

        public bool Add(SongNames song)
        {
            if (string.IsNullOrWhiteSpace(song.Hash) || string.IsNullOrWhiteSpace(song.Title))
            {
                Logger.IPALogger.Notice($"Invalid Infos: {song.Hash} {song.MapKey} {song.Title}");
                return false;
            }

            Store[song.Hash] = song;
            return true;
        }

        public int AddRange(IEnumerable<SongNames> songs)
        {
            return AddRange(songs.ToArray());
        }

        public int AddRange(params SongNames[] songs)
        {
            if (!IsLoaded)
            {
                throw new InvalidOperationException("Still not load store");
            }

            var count = 0;
            foreach (var s in songs)
            {
                if (Add(s))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
