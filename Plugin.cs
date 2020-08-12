using System.Linq;
using System.Collections.Generic;
using IPA;
using JapanSaber.Configuration;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.TextCore.LowLevel;
using JapanSaber.Modification;
using BS_Utils.Utilities;
using UnityEngine.SceneManagement;
using HarmonyLib;
using System.Reflection;
using UnityEngine.UI;
using System.Threading;
using System.Collections;
using JapanSaber.Search;
using UnityEngine.Rendering;

namespace JapanSaber
{

    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public const string HarmonyId = "net.noguchii.japansaber.JapanSaber";
        public static string Name => "JapanSaber";
        // ビルド番号を省いたバージョン
        public static string Version { get; private set; } 
            = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

        public static Plugin Instance;

        public int MaxContinuousDownloadCount = 3;
        public int CurrentContinuousDownloadCount = 0;
        public bool IsDownloadingBeatmap = false;

        public GameObject DownloadButton;

        private Harmony harmony;
        
        public IPreviewBeatmapLevel LastSelectedPreviewLevel { get; private set; }
       
        [Init]
        public void Init(IPA.Logging.Logger logger)
        { 
            Instance = this;
            Logger.IPALogger = logger;
            Logger.IPALogger.Info("JapanSaber initialized");

            harmony = new Harmony(HarmonyId);
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Logger.Debug("OnApplicationStart");

            ApplyHarmonyPatches();

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            SongCore.Loader.SongsLoadedEvent += Loader_SongsLoadedEvent;
            BSEvents.levelSelected += BSEvents_levelSelected;
            BSEvents.lateMenuSceneLoadedFresh += BSEvents_lateMenuSceneLoadedFresh;

            ConfigMenuUI.Create();
            FontsModifier.ClearDefectiveFonts();
        }

        private void BSEvents_lateMenuSceneLoadedFresh(ScenesTransitionSetupDataSO obj)
        {
            var standardLevelDetailViewController = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>().First();
            standardLevelDetailViewController.didPresentContentEvent += StandardLevelDetailViewController_didPresentContentEvent;
            standardLevelDetailViewController.didChangeDifficultyBeatmapEvent += StandardLevelDetailViewController_didChangeDifficultyBeatmapEvent;
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            Logger.Debug($"Scene : {arg0.name}");
        }

        private void StandardLevelDetailViewController_didChangeDifficultyBeatmapEvent(
            StandardLevelDetailViewController controller, IDifficultyBeatmap difficultyBeatmap)
        {
            try
            {
                if (difficultyBeatmap.level is Search.SearchResultBeatmapLevel)
                {
                    Logger.Debug("didChangeDifficultyBeatmapEvent()");

                    var _standardLevelDetailView = controller.GetPrivateField<StandardLevelDetailView>("_standardLevelDetailView");
                    var _levelParamsPanel = _standardLevelDetailView.GetPrivateField<LevelParamsPanel>("_levelParamsPanel");

                    _levelParamsPanel.bpm = Mathf.Ceil(difficultyBeatmap.level.beatsPerMinute);
                    _levelParamsPanel.duration = difficultyBeatmap.level.songDuration * 60;
                    _levelParamsPanel.notesPerSecond = difficultyBeatmap.beatmapData.notesCount / (difficultyBeatmap.level.songDuration * 60);
                    _levelParamsPanel.notesCount = difficultyBeatmap.beatmapData.notesCount;
                    _levelParamsPanel.obstaclesCount = difficultyBeatmap.beatmapData.obstaclesCount;
                    _levelParamsPanel.bombsCount = difficultyBeatmap.beatmapData.bombsCount;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex);
            }
        }

        private void StandardLevelDetailViewController_didPresentContentEvent(
            StandardLevelDetailViewController controller, StandardLevelDetailViewController.ContentType contentType)
        {
            try
            {
                if (LastSelectedPreviewLevel is Search.SearchResultBeatmapLevel level &&
                    !level.IsDownloadingBeatmap &&
                    !level.IsDownloadedBeatmap &&
                    contentType != StandardLevelDetailViewController.ContentType.OwnedAndDownloading)
                {
                    Logger.Debug("didPresentContentEvent()");

                    var _standardLevelBuyView = controller.GetPrivateField<StandardLevelBuyView>("_standardLevelBuyView");
                    var _standardLevelDetailView = controller.GetPrivateField<StandardLevelDetailView>("_standardLevelDetailView");
                    var _playerDataModel = controller.GetPrivateField<PlayerDataModel>("_playerDataModel");
                    var _showPlayerStats = controller.GetPrivateField<bool>("_showPlayerStats");

                    _standardLevelDetailView.SetContent(level, 
                        _playerDataModel.playerData.lastSelectedBeatmapDifficulty,
                        _playerDataModel.playerData.lastSelectedBeatmapCharacteristic,
                        _playerDataModel.playerData,
                        _showPlayerStats);

                    StandardLevelDetailViewController_didChangeDifficultyBeatmapEvent(controller, controller.selectedDifficultyBeatmap);

                    // loading, error, downloading
                    controller.GetPrivateField<LoadingControl>("_loadingControl").gameObject.SetActive(false); 
                    controller.GetPrivateField<StandardLevelBuyView>("_standardLevelBuyView").gameObject.SetActive(false);
                    _standardLevelDetailView.gameObject.SetActive(true);

                    // didPresentContentEventが発生して無限ループになるので ShowContent() は使わない
                    // _standardLevelDetailView.ShowContent(StandardLevelDetailViewController.ContentType.OwnedAndReady);

                    _standardLevelDetailView.playButton.gameObject.SetActive(false);
                    _standardLevelDetailView.practiceButton.gameObject.SetActive(false);

                    if (DownloadButton == null)
                    {
                        DownloadButton = GameObject.Instantiate(_standardLevelDetailView.playButton.gameObject, 
                            _standardLevelDetailView.playButton.transform.parent);
                        var button = DownloadButton.GetComponent<Button>();
                        button.onClick = new Button.ButtonClickedEvent();
                        button.onClick.AddListener(new UnityEngine.Events.UnityAction(OnClickDownloadButton));
                        DownloadButton.SetActive(true); // GetComponentInChildren でひかっけるのに必要？
                        DownloadButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Download");
                        DownloadButton.name = "DownloadButton";
                        Logger.PrintHierarchy(DownloadButton.transform.parent.parent);
                    }
                    else if (level.IsDownloadedBeatmap)
                    {
                        DownloadButton.SetActive(false);
                    }
                    else
                    {
                        DownloadButton.SetActive(true); 
                        // GetComponentInChildren でひかっけるのに必要
                       //  DownloadButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Download");
                    }
                }
                else
                {
                    DownloadButton?.SetActive(false);
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex);
            }
        }

        private void OnClickDownloadButton()
        {
            Logger.Debug($"OnClickDownloadButton() : {LastSelectedPreviewLevel.songName}");

            if (LastSelectedPreviewLevel is Search.SearchResultBeatmapLevel level)
            {
                level.DownloadBeatmap();
            }
        }

        private void BSEvents_levelSelected(LevelCollectionViewController controller, IPreviewBeatmapLevel selectedLevel)
        {
            LastSelectedPreviewLevel = selectedLevel;
        }

        private void Loader_SongsLoadedEvent(SongCore.Loader loader,
            Dictionary<string, CustomPreviewBeatmapLevel> songs)
        {
            Logger.IPALogger?.Info($"SongCore loaded {songs.Count} songs");

            // 日本語化修正
            if (JSConfig.Instance.IsAutoModification)
            {
                var count = SongNamesModifier.ModifySongCoreStore(JSConfig.Instance.GetViewType(), false);

                Logger.IPALogger?.Info($"Modified {count} songs");
            }

            UpdateDownloadedBeatmapLevel();
        }

        private void UpdateDownloadedBeatmapLevel()
        {
            // 更新のあったレベルを入れ替える
            foreach (var collection in HarmonyPatches.TabBarViewController_SetupPatch.AnnotatedBeatmapLevelCollections)
            {
                if (collection is IHaveDownladableLevel)
                {
                    var beatmapLevelCollection = collection.beatmapLevelCollection.beatmapLevels;
                    for (var i = 0; i < beatmapLevelCollection.Length; i++)
                    {
                        if (beatmapLevelCollection[i] is Search.SearchResultBeatmapLevel undownloadedLevel)
                        {
                            var songCoreBeatmapLevel = SongCore.Loader.GetLevelByHash(undownloadedLevel.Hash);
                            if (songCoreBeatmapLevel != null)
                            {
                                Logger.Debug($"updated {songCoreBeatmapLevel.songName}");
                                beatmapLevelCollection[i] = songCoreBeatmapLevel;
                            }
                        }
                    }
                }
            }

            // ダウンロード完了まで待機していたとき、ビューを更新
            if (LastSelectedPreviewLevel is SearchResultBeatmapLevel level)
            {
                var songCoreBeatmapLevel = SongCore.Loader.GetLevelByHash(level.Hash);
                if (songCoreBeatmapLevel != null)
                {
                    LastSelectedPreviewLevel = songCoreBeatmapLevel;
                    var standardLevelDetailViewController = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>().First();
                    standardLevelDetailViewController.SetData(songCoreBeatmapLevel, true, false, false);
                }
            }
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            Logger.Debug("OnApplicationQuit"); 

            SongCore.Loader.SongsLoadedEvent -= Loader_SongsLoadedEvent;
            BSEvents.levelSelected -= BSEvents_levelSelected;
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }

        public void ApplyHarmonyPatches()
        {
            Logger.Debug("ApplyHarmonyPatches");
            try
            {
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                Logger.IPALogger.Critical("Error applying Harmony patches: " + ex.Message);
                Logger.Debug(ex);
            }
        }
    }
}
