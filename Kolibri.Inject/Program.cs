using System;
using System.IO;
using Kolibri.Lib;
using Newtonsoft.Json;

namespace Kolibri.Inject
{
    class Program
    {
        static void Main(string[] args)
        {
            var configFile = new FileInfo("config.json");
            if (configFile.Exists)
            {
                var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configFile.FullName));
                var injector = new ModInjecter(config.GameDirectory);
                injector.Inject();
            }
            else
            {
                var injector = new ModInjecter(Directory.GetCurrentDirectory());
                injector.Inject();
            }
            Console.WriteLine("Done. Press any key to exit...");
            Console.ReadLine();
        }
    }
}
