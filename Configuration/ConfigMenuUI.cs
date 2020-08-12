using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;

namespace JapanSaber.Configuration
{
    public static class ConfigMenuUI
    {
        private static ConfigFlowCoordinator flowCoordinator;

        public static void Create()
        {
            var button = new MenuButton("JapanSaber", "", OnClick);
            MenuButtons.instance.RegisterButton(button);
        }

        private static void OnClick()
        {
            Logger.Debug("ConfigButton Clicked");

            if (flowCoordinator == null)
            {
                flowCoordinator = BeatSaberUI.CreateFlowCoordinator<ConfigFlowCoordinator>();
            }

            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(flowCoordinator, null, false, false);
        }
    }
}
