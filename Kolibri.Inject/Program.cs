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
            try
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            
            Console.WriteLine("Done. Press any key to exit...");
            Console.ReadLine();
        }
    }
}
