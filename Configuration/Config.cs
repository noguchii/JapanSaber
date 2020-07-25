
using System;
using System.IO;
using UnityEngine;
using JapanSaber.Modification;
using Newtonsoft.Json;

namespace JapanSaber.Configuration
{
    [Serializable]
    public class Config
    {
        private static string DefaultConfigPath =>
            Path.Combine(Environment.CurrentDirectory, "UserData", $"{Plugin.Name}.json");
        [NonSerialized]
        private static Config _Instance;
        internal static Config Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = Load(DefaultConfigPath);
                }
                return _Instance;
            }
        }

        public Config() { }

        [SerializeField]
        public int ViewType = 0;
        [SerializeField]
        public bool IsAutoModification = true;

        [NonSerialized]
        private string ConfigPath;

        public ModificationFormat GetViewType()
        {
            return (ModificationFormat)ViewType;
        }

        public void OnChanged()
        {
            try
            {
                var json = JsonConvert.SerializeObject(this);
                File.WriteAllText(ConfigPath, json);

                Logger.Debug($"Config OnChanged, {ConfigPath} {json}");
            }
            catch (Exception ex)
            {
                Logger.Debug(ex);
            }
        }

        private static Config Load(string path)
        {
            try
            {
                var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
                config.ConfigPath = path;
                Logger.Debug("Coinfig loaded form file");
                return config;
            }
            catch (Exception ex)
            {
                var config = new Config() {
                    ConfigPath = path
                };
                Logger.Debug("Config use default");
                Logger.Debug(ex);
                return config;
            }
        }
    }
}
