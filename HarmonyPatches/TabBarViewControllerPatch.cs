using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BS_Utils.Utilities;
using JapanSaber.Search;
using UnityEngine;
using HMUI;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

using JapanSaber.Objects;
using System.Collections;
using JapanSaber.SerializableData;

/// <summary>
/// TabViewのセットアップ時に割り込むパッチ
/// 想定されてないものを追加してるせいかAnnotatedCollectionの選択時の
/// イベントが発火されないので、別途UIイベント発生時に割り込んで処理している。
/// AnnotatedBeatmapLevelCollectionTableCellPatch.cs
/// 
/// @see https://github.com/pardeike/Harmony/wiki
/// </summary>
namespace JapanSaber.HarmonyPatches
{
    [HarmonyPatch(typeof(TabBarViewController), "Setup", new Type[] {
            typeof(TabBarViewController.TabBarItem[])
        })]
    public static class TabBarViewController_SetupPatch
    {
        public static string DefaultCollectionPath =>
            Path.Combine(Environment.CurrentDirectory, "UserData", $"{Plugin.Name}-collection.json");
        public static string DefaultCollectionResourceName => "JapanSaber.Resources.DefaultCollectionData.json";
        public static string DefaultCoverResourceName => "JapanSaber.Resources.DefaultThumbnail.png";

        public static List<IAnnotatedBeatmapLevelCollection> AnnotatedBeatmapLevelCollections;

        public static bool Prepare()
        {
            if (AnnotatedBeatmapLevelCollections != null) return true;

            Logger.Debug("TabBarViewControllerPatch.Prepare()");
            string json;
            try
            {
                if (File.Exists(DefaultCollectionPath))
                {
                    json = File.ReadAllText(DefaultCollectionPath);

                    Logger.Debug(json);
                }
                else
                {
                    json = Encoding.UTF8.GetString(UIUtilities.GetResource(
                        Assembly.GetExecutingAssembly(),
                        DefaultCollectionResourceName));

                    Logger.Debug(json);
                }

                var collectionData = JsonConvert.DeserializeObject<JapanSaberCollectionData[]>(json.Trim());
                var list = new List<IAnnotatedBeatmapLevelCollection>();

                foreach (var data in collectionData)
                {
                    if (data.Type.ToLower() == "search")
                    {
                        list.Add(new SearchLevelCollection()
                        {
                            collectionName = data.Name,
                            Query = data.Query ?? new Dictionary<string, string>(),
                            coverImage = LoadSprite(data.Cover),
                        });
                    }
                    else if (data.Type.ToLower() == "recommend")
                    {
                        list.Add(new RecommendLevelCollection()
                        {
                            collectionName = data.Name,
                            coverImage = LoadSprite(data.Cover),
                        });
                    }
                }
                AnnotatedBeatmapLevelCollections = list;
            }
            catch (Exception ex)
            {
                Logger.IPALogger.Critical(ex);
            }

            return true;
        }

        public static void Prefix(ref TabBarViewController.TabBarItem[] items)
        {
            items = items.AddToArray(new TabBarViewController.TabBarItem("Japan Saber", () =>
            {
                Logger.Debug("Selected MyAnnotatedCollection");
                try
                {
                    var annotated = Resources.FindObjectsOfTypeAll<AnnotatedBeatmapLevelCollectionsViewController>().First();
                    annotated.SetData(AnnotatedBeatmapLevelCollections.ToArray(), 0, true);

                    foreach (var collection in AnnotatedBeatmapLevelCollections)
                    {
                        if (collection is IAsyncDataSource source)
                        {
                            if (!source.IsInitialLoaded && !source.IsInitialLoading)
                            {
                                source.GetDataAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.IPALogger.Critical(ex);
                }
            }));
        }
        private static Sprite LoadSprite(string name)
        {
            Sprite sprite = null;
            try
            {
                sprite = UIUtilities.LoadSpriteFromResources(name);
            }
            catch { }

            try
            {
                if (sprite == null)
                {
                    sprite = UIUtilities.LoadSpriteFromFile(name);
                }
            }
            catch { }

            try
            {
                if (sprite == null)
                {
                    sprite = UIUtilities.LoadSpriteFromResources(DefaultCoverResourceName);
                }
            }
            catch { }
            return sprite;
        }
    }
}