using Discord;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rGunti.Momoka2.Discord.Permissions {
    public class ServerPermissionManager {
        private const string STORAGE_PATH = "permissions";

        private static Dictionary<ulong, ServerPermissionManager> managers = new Dictionary<ulong, ServerPermissionManager>();
        public static ServerPermissionManager GetPermissionManager(Server server) {
            if (!managers.ContainsKey(server.Id)) {
                managers.Add(server.Id, new ServerPermissionManager(server));
            }
            return managers[server.Id];
        }

        private Logger log = LogManager.GetCurrentClassLogger();
        private ulong serverID;
        private Dictionary<ulong, bool> permissionDictionary;
        private string contentFilePath;

        private ServerPermissionManager(Server server) {
            serverID = server.Id;
            permissionDictionary = new Dictionary<ulong, bool>();
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
            log.Trace($"{this.GetType()} initialized for Server {server.Name} ({server.Id})");
        }

        private void LoadFile() {
            log.Info($"Loading Permission File {contentFilePath}...");
            permissionDictionary = JsonConvert.DeserializeObject<Dictionary<ulong, bool>>(File.ReadAllText(contentFilePath)) ?? new Dictionary<ulong, bool>();
        }

        private void SaveFile() {
            log.Info($"Saving Permission File {contentFilePath}...");
            File.WriteAllText(contentFilePath, JsonConvert.SerializeObject(permissionDictionary, Formatting.Indented));
        }

        public Dictionary<ulong, bool> Dictionary { get { return permissionDictionary; } }

        public void SetHasPermission(ulong roleID, bool canUseCommands) {
            permissionDictionary[roleID] = canUseCommands;
            SaveFile();
        }

        public bool HasPermission(ulong roleID) {
            if (permissionDictionary.ContainsKey(roleID)) {
                return permissionDictionary[roleID];
            } else {
                return false;
            }
        }

        public bool HasPermission(Server server, User user) {
            // Owner always has permission
            if (server.Owner.Id == user.Id) { return true; }
            else {
                foreach (Role role in user.Roles) {
                    if (HasPermission(role.Id)) return true;
                }
                return false;
            }
        }
    }
}
