using System.Collections.Generic;
using IPA;
using JapanSaber.Configuration;

namespace JapanSaber
{

    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public Modification.Modifer Modifer;
        internal static Plugin Instance { get; private set; }
        public static string Name => "JapanSaber";
       
        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
      
        public void Init(IPA.Logging.Logger logger)
        {
            Instance = this;

            Logger.IPALogger = logger;

            Logger.IPALogger.Info("JapanSaber initialized");
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Logger.Debug("OnApplicationStart");


            Modifer = new Modification.Modifer();

            SongCore.Loader.SongsLoadedEvent += Loader_SongsLoadedEvent;
            Configuration.UI.JapanSaberMenuUI.Create();
        }

        private void Loader_SongsLoadedEvent(SongCore.Loader loader,
            Dictionary<string, CustomPreviewBeatmapLevel> songs)
        {
            Logger.IPALogger?.Info($"SongCore loaded {songs.Count} songs");

            if (!Config.Instance.IsAutoModification) return;

            Modifer.ModifySongs(false);
        }


        [OnExit]
        public void OnApplicationQuit()
        {
            Logger.Debug("OnApplicationQuit");

            SongCore.Loader.SongsLoadedEvent -= Loader_SongsLoadedEvent;
        }
    }
}
