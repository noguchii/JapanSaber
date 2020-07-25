using System;
using System.Diagnostics;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;

namespace JapanSaber.Configuration.UI
{
    public class AboutController : ViewController
    {
        public  string ResourceName => "JapanSaber.Configuration.UI.BSML.About.bsml";

        protected override void DidActivate(bool firstActivation, ActivationType activationType)
        {
            base.DidActivate(firstActivation, activationType);

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
                    Logger.IPALogger.Critical($"BSML Parse Error\n{ex.ToString()}");
                }
            }
        }

        [UIAction("OnOpen")]
        public void OnOpen()
        {
            Process.Start("https://japansaber.noguchii.net");
        }
    }
}