using JapanSaber.Objects;
using JapanSaber.SerializableData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JapanSaber.Modification
{
    public static class SongNamesModifier
    {
        public static int ModifySongs(IEnumerable<CustomPreviewBeatmapLevel> previewBeatmapLevels,
            DisplayFormat type)
        {
            Logger.Debug("ModifySongs");

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
                    return counter;
                }

                foreach (var song in previewBeatmapLevels)
                {
                    var hash = song.levelID.Split('_')[2].ToLower();
                    if (!SongsStore.Instance.Store.TryGetValue(hash, out var infos)) continue;

                    ModifiyTitle(_songName, song, infos, type);
                    ModifiySubtitle(_songSubName, song, infos, type);
                    ModifiyAuthor(_songAuthorName, song, infos, type);
                    ModifiyMapper(_levelAuthorName, song, infos, type);

                    counter++;
                }
            }
            catch (Exception ex)
            {
                Logger.IPALogger.Error(ex);
            }
            finally
            {
            }

            return counter;
        }

        public static int ModifySongCoreStore(DisplayFormat type, bool isForce = false)
        {
            Logger.Debug("ModifySongCoreStore");

            try
            {
                SongsStore.Instance.LoadPlaylist(isForce);
            }
            catch (Exception ex)
            {
                Logger.Debug(ex);
            }

            if (SongCore.Loader.AreSongsLoading) return 0;

            return ModifySongs(SongCore.Loader.CustomLevels.Values, type);
        }

        public static void ModifiyTitle(FieldInfo field, object level, SongNames info, DisplayFormat type)
        {
            field.SetValue(level, ToFormattedTitle(info, type));
        }
        public static void ModifiySubtitle(FieldInfo field, object level, SongNames info, DisplayFormat type)
        {
            field.SetValue(level, ToFormattedSubtitle(info, type));
        }
        public static void ModifiyAuthor(FieldInfo field, object level, SongNames info, DisplayFormat type)
        {
            field.SetValue(level, ToFormattedAuthor(info, type));
        }
        public static void ModifiyMapper(FieldInfo field, object level, SongNames info, DisplayFormat type)
        {
            field.SetValue(level, ToFormattedMapper(info, type));
        }

        public static string ToFormattedTitle(SongNames infos, DisplayFormat type)
        {
            switch (type)
            {
                case DisplayFormat.Title_Author_Product_Mapper:
                case DisplayFormat.Title_Product_Author_Mapper:
                case DisplayFormat.Title_Subtitle_Author_Mapper:
                case DisplayFormat.Title_Subtitle_Author_Product:
                case DisplayFormat.Title_Subtitle_Product_Mapper:
                default:
                    return infos.Title ?? "";
            };
        }
        public static string ToFormattedSubtitle(SongNames info, DisplayFormat type)
        {
            switch (type)
            {
                case DisplayFormat.Title_Author_Product_Mapper:
                    return info.Author ?? "";
                case DisplayFormat.Title_Product_Author_Mapper:
                    return info.Product ?? "";
                case DisplayFormat.Title_Subtitle_Author_Mapper:
                case DisplayFormat.Title_Subtitle_Author_Product:
                case DisplayFormat.Title_Subtitle_Product_Mapper:
                default:
                    return info.Subtitle ?? "";
            };
        }
        public static string ToFormattedAuthor(SongNames info, DisplayFormat type)
        {
            switch (type)
            {
                case DisplayFormat.Title_Author_Product_Mapper:
                case DisplayFormat.Title_Subtitle_Product_Mapper:
                    return info.Product ?? "";
                case DisplayFormat.Title_Product_Author_Mapper:
                case DisplayFormat.Title_Subtitle_Author_Mapper:
                case DisplayFormat.Title_Subtitle_Author_Product:
                default:
                    return info.Author ?? "";
            };
        }
        public static string ToFormattedMapper(SongNames info, DisplayFormat type)
        {
            switch (type)
            {
                case DisplayFormat.Title_Subtitle_Author_Product:
                    return info.Product ?? "";
                case DisplayFormat.Title_Author_Product_Mapper:
                case DisplayFormat.Title_Product_Author_Mapper:
                case DisplayFormat.Title_Subtitle_Author_Mapper:
                case DisplayFormat.Title_Subtitle_Product_Mapper:
                default:
                    return info.Mapper;
            };
        }
    }
}
