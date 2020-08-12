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
using UnityEngine.EventSystems;

/// <summary>
/// @see https://github.com/pardeike/Harmony/wiki
/// </summary>
namespace JapanSaber.HarmonyPatches
{
    [HarmonyPatch()]
    public static class AnnotatedBeatmapLevelCollectionTableCell_OnPointerClickPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(AnnotatedBeatmapLevelCollectionTableCell).GetMethod("OnPointerClick",
                BindingFlags.Public | BindingFlags.Instance);
        }

        public static void Prefix(AnnotatedBeatmapLevelCollectionTableCell __instance)
        {
            // Logger.Debug("AnnotatedBeatmapLevelCollectionTableCell.OnPointerClick");
            try
            {
                // なぜかLevelListTableCellも呼ばれるのでオブジェクト名ではじく
                if (__instance.name.IndexOf("AnnotatedBeatmapLevelCollectionTableCell") < 0) return;

                var collection = __instance.GetPrivateField<IAnnotatedBeatmapLevelCollection>("_annotatedBeatmapLevelCollection");
                if (collection is IAsyncDataSource c)
                {
                    // 選択されたコレクションをビューに反映
                    var controller = Resources.FindObjectsOfTypeAll<LevelCollectionViewController>().First();
                    controller.SetData(collection.beatmapLevelCollection, collection.collectionName, collection.coverImage, false, null);

                    // 選択されてない状態のとき、クリックされたらなぜか一回目は選択されず
                    // 二回目だと選択される状態になっているので一回目で強制的に選択する
                    if (!__instance.selected)
                    {
                        __instance.selected = true;
                    }

                    controller.SelectLevel(collection.beatmapLevelCollection.beatmapLevels.FirstOrDefault());
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex);
            }
        }
    }

    [HarmonyPatch()]
    public static class AnnotatedBeatmapLevelCollectionTableCell_OnPointerEnterPatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(AnnotatedBeatmapLevelCollectionTableCell).GetMethod("OnPointerEnter",
                BindingFlags.Public | BindingFlags.Instance);
        }

        public static void Prefix(AnnotatedBeatmapLevelCollectionTableCell __instance)
        {
            // Logger.Debug("AnnotatedBeatmapLevelCollectionTableCell.OnPointerEnter");
            
            try
            {
                // なぜかLevelListTableCellも呼ばれるのでオブジェクト名ではじく
                if (__instance.name.IndexOf("AnnotatedBeatmapLevelCollectionTableCell") < 0) return;

                var collection = __instance.GetPrivateField<IAnnotatedBeatmapLevelCollection>("_annotatedBeatmapLevelCollection");
                if (collection is IAsyncDataSource c)
                {
                    // データは非同期に取ってくるので、読み込みが終わっていたらビューを更新
                    if (!c.IsRefreshedVisualAfterInitialLoad && c.IsInitialLoaded)
                    {
                        Logger.Debug(__instance.name);
                        var parent = __instance.GetComponentInParent<AnnotatedBeatmapLevelCollectionsTableView>();
                        var model = parent.GetPrivateField<AdditionalContentModel>("_additionalContentModel");
                        __instance.RefreshAvailabilityAsync(model);
                        c.IsRefreshedVisualAfterInitialLoad = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex);
            }
        }
    }
}