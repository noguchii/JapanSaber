using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace JapanSaber.Configuration
{
    public static class SongsStore
    {
        private static string CachedPlaylistPath =>
            Path.Combine(Environment.CurrentDirectory, $"UserData/{Plugin.Name}-Playlists.cache");
        private static string StoredSongsPath =>
            Path.Combine(Environment.CurrentDirectory, $"UserData/{Plugin.Name}-Songs.json");
        private static string PlaylistDirecotry =>
            Path.Combine(Environment.CurrentDirectory, $"Playlists/");
        private static string PlaylistExtension => "*.jsaber.json";
        
        public static Dictionary<string, SongInfos> Load(bool isForce = false)
        {
            Dictionary<string, SongInfos> result = null;
            List<string> cachedPlaylists = null;

            try
            {
                if (File.Exists(StoredSongsPath))
                {
                    // キャッシュを読み込み
                    var songs = JsonConvert.DeserializeObject<SongInfos[]>(File.ReadAllText(StoredSongsPath));
                    result = songs.ToDictionary(s => s.hash);
                }
            }
            catch (Exception ex)
            {
                Logger.IPALogger.Error($"Faild to load store\n{ex.Message}");
            }

            if (result == null)
            {
                Logger.Debug("SongsStore use default");
                result = new Dictionary<string, SongInfos>();
            }

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
                    if (playlist.songs == null)
                    {
                        Logger.IPALogger.Notice($"Invalid Playlist:\n{p}");
                        continue;
                    }

                    foreach (var s in playlist.songs)
                    {
                        // 不備がありそうなデータは使わない（プレイリスト編集ソフトで後から追加された曲とか）
                        if (string.IsNullOrWhiteSpace(s.hash) || string.IsNullOrWhiteSpace(s.Title))
                        {
                            Logger.IPALogger.Notice($"Invalid Infos: {s.hash} {s.MapKey} {s.Title}");
                            continue;
                        }

                        result[s.hash] = s;
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
            }

            try
            {
                File.WriteAllLines(CachedPlaylistPath, cachedPlaylists.Distinct());
                File.WriteAllText(StoredSongsPath, JsonConvert.SerializeObject(result.Values.ToArray()));
            }
            catch { }

            Logger.IPALogger.Info($"Has {result.Count} infos");

            return result;
        }
    }
}
