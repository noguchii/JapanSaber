using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BS_Utils;
using BS_Utils.Utilities;
using TMPro;
using UnityEngine;

namespace JapanSaber.HarmonyPatches
{
    /*
    [HarmonyPatch(typeof(LevelListTableCell), "SetDataFromLevelAsync",
        new[] { typeof(IPreviewBeatmapLevel), typeof(bool) })]
    public static class LevelListTableCell_SetDataFromLevelAsyncPatch
    {
        public static void Postfix(LevelListTableCell __instance, IPreviewBeatmapLevel level, bool isFavorite)
        {
            if (level is Search.SearchResultBeatmapLevel)
            {
                var _songNameText = __instance.GetPrivateField<TextMeshProUGUI>("_songNameText");
                var _authorText = __instance.GetPrivateField<TextMeshProUGUI>("_authorText");

                if (__instance.selected)
                {
                    _songNameText.color = __instance.highlighted ? __instance.GetPrivateField<Color>("_selectedHighlightElementsColor") : Color.black;
                    _authorText.color = _songNameText.color;
                }
                else
                {
                    _songNameText.color = __instance.highlighted ? new Color(1f, 1f, 1f, 0.75f) : new Color(1f, 1f, 1f, 0.25f);
                    _authorText.color = __instance.highlighted ? new Color(1f, 1f, 1f, 0.25f) : new Color(1f, 1f, 1f, 0.1f);
                }
            }
        }
    }*/
}