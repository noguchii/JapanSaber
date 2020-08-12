
using System;
using System.Linq;
using System.IO;
using UnityEngine;
using JapanSaber.Modification;
using Newtonsoft.Json;

namespace JapanSaber.Configuration
{
    [JsonObject]
    public class JSConfig
    {
        private static string DefaultConfigPath =>
            Path.Combine(Environment.CurrentDirectory, "UserData", $"{Plugin.Name}.json");

        [JsonIgnore]
        private static JSConfig _Instance;
        internal static JSConfig Instance
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
        [JsonIgnore]
        private string ConfigPath;

        public bool IsExceptLowRating { get; set; } = false;
        public int ViewType { get; set; } = 0;
        public bool IsAutoModification { get; set; } = true;

        public DisplayFormat GetViewType()
        {
            return (DisplayFormat)ViewType;
        }

        public void Save()
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

        private static JSConfig Load(string path)
        {
            try
            {
                var config = JsonConvert.DeserializeObject<JSConfig>(File.ReadAllText(path));
                config.ConfigPath = path;
                Logger.Debug("Coinfig loaded form file");
                return config;
            }
            catch (Exception ex)
            {
                var config = new JSConfig() {
                    ConfigPath = path
                };
                Logger.Debug("Config use default");
                Logger.Debug(ex);
                return config;
            }
        }
    }
}
