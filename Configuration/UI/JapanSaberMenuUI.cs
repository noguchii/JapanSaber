using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;

namespace JapanSaber.Configuration.UI
{
    public static class JapanSaberMenuUI
    {
        private static JapanSaberFlowCoordinator flowCoordinator;

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
                flowCoordinator = BeatSaberUI.CreateFlowCoordinator<JapanSaberFlowCoordinator>();
            }

            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(flowCoordinator, null, false, false);
        }
    }
}
