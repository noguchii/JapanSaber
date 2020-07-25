using System;
using TMPro;
using HMUI;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine.UI;

namespace JapanSaber.Configuration.UI
{
    public class SettingsController : ViewController
    {
        public string ResourceName => "JapanSaber.Configuration.UI.BSML.Settings.bsml";

        /*
        TitleSubtitleAuthorMapper = 0,
        TitleAuthorProductMapper,
        TitleSubtitleProductMapper,
        TitleProductAuthorMapper,
        TitleSubtitleAuthorProduct,
        */

        public Tuple<string, string, string, string>[] ViewTypes => new [] {
            Tuple.Create("タイトル", " サブタイトル", "アーティスト", " [マッパー]"),
            Tuple.Create("タイトル", " アーティスト", "原作", " [マッパー]"),
            Tuple.Create("タイトル", " サブタイトル", "原作", " [マッパー]"),
            Tuple.Create("タイトル", " 原作", "アーティスト", " [マッパー]"),
            Tuple.Create("タイトル", " サブタイトル", "アーティスト", " [原作]"),
        };
        protected void Awake()
        {
            IsAutoModify = Config.Instance.IsAutoModification;
            SelectedViewTypeIndex = Config.Instance.ViewType;
        }

        [UIAction("#post-parse")]
        public void OnParsed()
        {
            IsAutoModify = Config.Instance.IsAutoModification;
            SelectedViewTypeIndex = Config.Instance.ViewType;

            UpdatePreview();
        }

        protected override void DidActivate(bool firstActivation, ActivationType type)
        {
            base.DidActivate(firstActivation, type);

            if (firstActivation)
            {
                try
                {
                    var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    var content = Utilities.GetResourceContent(assembly, ResourceName);
                    BSMLParser.instance.Parse(content, gameObject, this);
                }
                catch (Exception ex)
                {
                    Logger.IPALogger.Critical($"BSML Parse error:\n{ex.ToString()}");
                }
            }
        }
        public void OnChanged()
        {
            Logger.Debug($"OnChanged {IsAutoModify}, {SelectedViewTypeIndex}");

            Config.Instance.IsAutoModification = IsAutoModify;
            Config.Instance.ViewType = SelectedViewTypeIndex;
            Config.Instance.OnChanged();

            ResetManualButton();
        }

        #region preview component
        public int SelectedViewTypeIndex { get; set; } = 0;

        [UIComponent("preview-title")]
        public TextMeshProUGUI PreviewTitle;
        [UIComponent("preview-subtitle")]
        public TextMeshProUGUI PreviewSubtitle;
        [UIComponent("preview-author")]
        public TextMeshProUGUI PreviewAuthor;
        [UIComponent("preview-mapper")]
        public TextMeshProUGUI PreviewMapper;

        public void UpdatePreview()
        {
            var index = SelectedViewTypeIndex;
            PreviewTitle?.SetText(ViewTypes[index].Item1);
            PreviewSubtitle?.SetText(ViewTypes[index].Item2);
            PreviewAuthor?.SetText(ViewTypes[index].Item3);
            PreviewMapper?.SetText(ViewTypes[index].Item4);
        }

        [UIAction("OnLeft")]
        public void OnLeft()
        {
            if (SelectedViewTypeIndex > 0)
            {
                SelectedViewTypeIndex--;
                Logger.Debug($"OnLeft to {SelectedViewTypeIndex}");
                UpdatePreview();

                OnChanged();
            }
        }

        [UIAction("OnRight")]
        public void OnRight()
        {
            if (SelectedViewTypeIndex < ViewTypes.Length - 1)
            {
                SelectedViewTypeIndex++;
                Logger.Debug($"OnRight to {SelectedViewTypeIndex}");
                UpdatePreview();

                OnChanged();
            }
        }
        #endregion

        #region IsAutoModify component

        [UIValue("IsAutoModify")]
        public bool IsAutoModify { get; set; }
        [UIAction("OnChangedIsAutoModify")]
        public void OnChangedIsAutoModify(bool value)
        {
            Logger.Debug("OnChangedIsAutoModify");
            IsAutoModify = value;

            OnChanged();
        }

        #endregion

        #region ManualButton region

        private bool IsExecuting = false;
        [UIComponent("ManualButton")]
        public Button ManualButton;
        
        public void ResetManualButton()
        {
            ManualButton.SetButtonText("手動実行");
            IsExecuting = false;
        }

        [UIAction("OnClickManual")]
        public void OnClickManual()
        {
            Logger.Debug("OnClick Manual");

            if (IsExecuting) return;
            IsExecuting = true;
            try
            {
                var modifer = Plugin.Instance.Modifer;
                modifer.Type = Config.Instance.GetViewType();
                modifer.ModifySongs(true);
                ManualButton.SetButtonText("完了");
            }
            catch (Exception ex)
            {
                Logger.Debug(ex);
            }
        }

        #endregion
    }
}
