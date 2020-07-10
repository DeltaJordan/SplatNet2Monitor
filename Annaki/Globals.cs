using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace Annaki
{
    public static class Globals
    {
        /// <summary>
        /// Returns the root directory of the application.
        /// </summary>
        public static readonly string AppPath = Directory.GetParent(new Uri(Assembly.GetEntryAssembly()?.CodeBase).LocalPath).FullName;

        public static string ConfigPath = Path.Combine(AppPath, "config.json");

        /// <summary>
        /// Gets or sets the bots settings.
        /// </summary>
        public static Settings BotSettings
        {
            get
            {
                if (botSettings != null) return botSettings;

                string settingsLocation = Path.Combine(AppPath, "Data", "settings.json");
                string jsonFile = File.ReadAllText(settingsLocation);
                botSettings = JsonConvert.DeserializeObject<Settings>(jsonFile);

                return botSettings;
            }
            set => botSettings = value;
        }

        private static Settings botSettings;
    }
}
