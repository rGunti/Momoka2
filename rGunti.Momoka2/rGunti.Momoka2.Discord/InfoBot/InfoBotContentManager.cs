using Discord;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rGunti.Momoka2.Discord.InfoBot {
    public class InfoBotContentManager {
        private const string STORAGE_PATH = "infoBot";

        private Logger log = LogManager.GetCurrentClassLogger();
        private Dictionary<string, string> content;
        private string contentFilePath;

        public InfoBotContentManager(Server server) {
            content = new Dictionary<string, string>();
            contentFilePath = Path.Combine(STORAGE_PATH, $"{server.Id}.json");
            
            if (!Directory.Exists(STORAGE_PATH)) {
                log.Warn($"Creating Info Bot directory at {STORAGE_PATH}...");
                Directory.CreateDirectory(STORAGE_PATH);
            }
            if (!File.Exists(contentFilePath)) {
                log.Warn($"Creating Info Bot file at {contentFilePath}...");
                //File.Create(contentFilePath);
                SaveFile();
            }

            LoadFile();
            log.Trace($"{typeof(InfoBotContentManager)} initialized for Server {server.Name} ({server.Id})");
        }

        private void LoadFile() {
            log.Info($"Loading Info Bot File {contentFilePath}...");
            content = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(contentFilePath)) ?? new Dictionary<string, string>();
        }

        private void SaveFile() {
            log.Info($"Saving Info Bot File {contentFilePath}...");
            File.WriteAllText(contentFilePath, JsonConvert.SerializeObject(content, Formatting.Indented));
        }

        public void SetContent(string key, string value) {
            content[key.ToLower()] = value;
            SaveFile();
        }

        public string GetContent(string key) {
            if (content.ContainsKey(key)) {
                return content[key];
            } else {
                return null;
            }
        }
    }
}
