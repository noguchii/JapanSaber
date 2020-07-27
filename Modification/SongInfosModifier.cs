using System;
using System.Collections.Generic;
using System.Reflection;
using JapanSaber.Configuration;

namespace JapanSaber.Modification
{
    public class SongInfosModifier
    {
        Dictionary<string, SongInfos> SongsDictionary;

        public ModificationFormat Type { get; set; }


        public void ModifySongs(bool isForce = false)
        {
            Logger.Debug("ModifySongs");

            try
            {
                SongsDictionary = SongsStore.Load(isForce);
            }
            catch (Exception ex)
            {
                SongsDictionary = new Dictionary<string, SongInfos>();
                Logger.Debug(ex);
            }

            if (SongsDictionary == null) return;
            if (SongCore.Loader.AreSongsLoading) return;

            var counter = 0;
            try
            {
                var beatmapType = typeof(CustomPreviewBeatmapLevel);
                var _songName = beatmapType.GetField("_songName", BindingFlags.NonPublic | BindingFlags.Instance);
                var _songSubName = beatmapType.GetField("_songSubName", BindingFlags.NonPublic | BindingFlags.Instance);
                var _songAuthorName = beatmapType.GetField("_songAuthorName", BindingFlags.NonPublic | BindingFlags.Instance);
                var _levelAuthorName = beatmapType.GetField("_levelAuthorName", BindingFlags.NonPublic | BindingFlags.Instance);

                if (_songName == null || _songSubName == null || _songAuthorName == null || _levelAuthorName == null)
                {
                    Logger.IPALogger.Critical("Failed to reflect fields");
                    return;
                }

                var songCoreDictionary = SongCore.Loader.CustomLevels;

                Logger.Debug($"SongCore loaded {songCoreDictionary.Count} songs");

                foreach (var song in songCoreDictionary.Values)
                {
                    var hash = song.levelID.Split('_')[2].ToLower();
                    if (!SongsDictionary.TryGetValue(hash, out var infos)) continue;

                    ModifiyTitle(_songName, song, infos);
                    ModifiySubtitle(_songSubName, song, infos);
                    ModifiyAuthor(_songAuthorName, song, infos);
                    ModifiyMapper(_levelAuthorName, song, infos);

                    counter++;
                }
            }
            catch (Exception ex)
            {
                Logger.IPALogger.Error(ex);
            }
            finally
            {
                SongsDictionary = null;
                Logger.IPALogger.Info($"Modified {counter} Songs");
            }
        }

        public void ModifiyTitle(FieldInfo field, CustomPreviewBeatmapLevel level, SongInfos infos)
        {
            switch (Type)
            {
                case ModificationFormat.Title_Author_Product_Mapper:
                case ModificationFormat.Title_Product_Author_Mapper:
                case ModificationFormat.Title_Subtitle_Author_Mapper:
                case ModificationFormat.Title_Subtitle_Author_Product:
                case ModificationFormat.Title_Subtitle_Product_Mapper:
                default:
                    field.SetValue(level, infos.Title ?? "");
                    return;
            };
        }
        public void ModifiySubtitle(FieldInfo field, CustomPreviewBeatmapLevel level, SongInfos info)
        {
            switch (Type)
            {
                case ModificationFormat.Title_Author_Product_Mapper:
                    field.SetValue(level, info.Author ?? "");
                    return;
                case ModificationFormat.Title_Product_Author_Mapper:
                    field.SetValue(level, info.Product ?? "");
                    return;
                case ModificationFormat.Title_Subtitle_Author_Mapper:
                case ModificationFormat.Title_Subtitle_Author_Product:
                case ModificationFormat.Title_Subtitle_Product_Mapper:
                    field.SetValue(level, info.Subtitle ?? "");
                    return;
                default:
                    field.SetValue(level, info.Subtitle ?? "");
                    return;
            };
        }
        public void ModifiyAuthor(FieldInfo field, CustomPreviewBeatmapLevel level, SongInfos info)
        {
            switch (Type)
            {
                case ModificationFormat.Title_Author_Product_Mapper:
                case ModificationFormat.Title_Subtitle_Product_Mapper:
                    field.SetValue(level, info.Product ?? "");
                    return;
                case ModificationFormat.Title_Product_Author_Mapper:
                case ModificationFormat.Title_Subtitle_Author_Mapper:
                case ModificationFormat.Title_Subtitle_Author_Product:
                default:
                    field.SetValue(level, info.Author ?? "");
                    return;
            };
        }
        public void ModifiyMapper(FieldInfo field, CustomPreviewBeatmapLevel level, SongInfos info)
        {
            switch (Type)
            {
                case ModificationFormat.Title_Subtitle_Author_Product:
                    field.SetValue(level, info.Product ?? "");
                    return;
                case ModificationFormat.Title_Author_Product_Mapper:
                case ModificationFormat.Title_Product_Author_Mapper:
                case ModificationFormat.Title_Subtitle_Author_Mapper:
                case ModificationFormat.Title_Subtitle_Product_Mapper:
                    return;
                default:
                    return;
            };
        }
    }
}
