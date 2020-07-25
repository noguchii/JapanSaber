using System;
using HMUI;
using BeatSaberMarkupLanguage;

namespace JapanSaber.Configuration.UI
{
    public class JapanSaberFlowCoordinator : FlowCoordinator
    {
        public SettingsController Settings;
        public AboutController About;

        protected void Awake()
        {
            Logger.Debug("Coodinator Awake");

            if (!Settings)
            {
                Settings = BeatSaberUI.CreateViewController<SettingsController>();
            }
            if (!About)
            {
                About = BeatSaberUI.CreateViewController<AboutController>();
            }

            title = "Japan Saber";
        }

        protected override void DidActivate(bool firstActivation, ActivationType activationType)
        {
            try
            {
                if (firstActivation)
                {
                    showBackButton = true;
                }
                this.ProvideInitialViewControllers(About, Settings);
            }
            catch (Exception ex)
            {
                Logger.IPALogger.Error(ex.ToString());
            }
        }

        protected override void BackButtonWasPressed(ViewController controller)
        {
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this, null, false);
            Config.Instance.OnChanged();
        }
    }
}
