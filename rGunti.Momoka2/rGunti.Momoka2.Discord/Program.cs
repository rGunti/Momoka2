using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rGunti.Momoka2.Discord {
    class Program {
        static Logger log = LogManager.GetCurrentClassLogger();

        static void Main(string[] args) {
            log.Info(" === PROGRAM START ===");
            log.Info("Hallo Welt, ich bin Momoka2 Discord Bot!");

            string botKey = ReadBotTokenFile();
            log.Trace($"Bot Key read, is {botKey}");

            log.Info(" === PROGRAM END ===");
            Console.ReadLine();
        }

        static string ReadBotTokenFile() {
            return File.ReadAllText(Properties.Settings.Default.DiscordBotTokenPath);
        }
    }
}
