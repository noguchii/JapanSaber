using System;
using TMPro;
using UnityEngine;

namespace JapanSaber.Modification
{
    public static class FontsModifier
    {
        public static void ClearDefectiveFonts()
        {
            try
            {
                Logger.Debug("ClearMultiByteFonts");
                // ===========================================
                // NECESSARY CODE
                // instance property load TMP_Settings resource in get accessor
                // Can't load TMP_FontAsset resources until load TMP_Settings
                if (TMP_Settings.instance == null) return;
                // ===========================================

                Logger.Debug($"Default font: {TMP_Settings.defaultFontAsset?.name}");

                // 中国語フォントが使われているため、日本語の漢字が中途半端に置き換わっている
                // 全て削除したらWindows標準フォント（游ゴシック）が使われているはず？
                // ➡　途中で明朝とか混ざりだした、勝手に自動生成している？

                // SourceHanSansCN-Bold-SDF-Common-1(2k)
                // SourceHanSansCN-Bold-SDF-Common-2(2k)
                // SourceHanSansCN-Bold-SDF-Uncommon(2k)
              
                foreach (var f in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
                {
                    if (f.name == "Teko-Medium SDF No Glow")
                    {
                        f.fallbackFontAssetTable.Clear();

                        // 以下コードは動かず
                        // リフレクションでTMP_Settings.m_defaultFontAssetに設定してみたら
                        // 動いてるようだが、???表示される、未対応文字あり？
                        /*
                        var font = Font.CreateDynamicFontFromOSFont("Yu Gosick", 24);
                        var fontAssets = TMP_FontAsset.CreateFontAsset(font, 24, 5, 
                            GlyphRenderMode.SDFAA, 4096, 4096, AtlasPopulationMode.Dynamic);
                        f.fallbackFontAssetTable.Add(font);
                        */

                        continue;
                    }

                    Logger.Debug(f.name);
                    f.ClearFontAssetData(true);
                }
            }
            catch (Exception ex)
            {
                Logger.IPALogger.Error(ex);
            }
        }
    }
}
