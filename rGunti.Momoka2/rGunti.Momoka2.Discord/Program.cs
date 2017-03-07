using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rGunti.Momoka2.Discord {
    class Program {
        static Logger log = LogManager.GetCurrentClassLogger();

        static void Main(string[] args) {
            log.Info("Hallo Welt, ich bin Momoka2 Discord Bot!");

            Console.ReadLine();
        }
    }
}
