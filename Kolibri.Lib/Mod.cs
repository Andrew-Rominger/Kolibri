using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Kolibri.Lib
{
    public class Mod
    {
        public FileInfo ModAssemblyFile { get; set; }
        public DirectoryInfo ModDependencyDirectory { get; set; }
        private Assembly _modAssembly;
        public List<MethodInjectionInfo> ModMethodInjections { get; } = new List<MethodInjectionInfo>();
        public Mod(FileInfo modAssemblyFile)
        {
            ModAssemblyFile = modAssemblyFile;
            ModDependencyDirectory = ModAssemblyFile.Directory.GetDirectories("Dependencies").SingleOrDefault();
            LoadMod();
        }

        private void LoadMod()
        {
            _modAssembly = Assembly.LoadFrom(ModAssemblyFile.FullName);
            var methodsToInject = _modAssembly.GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes(typeof(InjectMethodAttribute), false).Any())
                .ToList();
            foreach (var method in methodsToInject)
            {
                var injecetAttr = (InjectMethodAttribute[]) method.GetCustomAttributes(typeof(InjectMethodAttribute), false);
                foreach (var attribute in injecetAttr)
                {
                    ModMethodInjections.Add(new MethodInjectionInfo()
                    {
                        SourceMethod = new InjectionLocation(method.DeclaringType.FullName, method.Name),
                        TargetMethod = new InjectionLocation(attribute.InjectionLocation.FullName, attribute.InjectionMethod),
                        InjectionLocation = attribute.MethodInjectionLocation,
                        InjectFlags = attribute.InjectFlags
                    });
                }
            }
        }
    }
}
