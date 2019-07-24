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
            var gameDir = new DirectoryInfo(Path.Combine(ModAssemblyFile.Directory.Parent.Parent.GetDirectories("*_Data")[0].FullName, "managed"));
            _modAssembly = Assembly.LoadFrom(ModAssemblyFile.FullName);
            var refAssemblies = _modAssembly.GetReferencedAssemblies().Where(a => a.Name != "mscorlib" && a.Name != "Kolibri.Lib" && a.Name != "Mono.Cecil.Inject");
            foreach (var referencedAssembly in refAssemblies)
            {
                var path = new FileInfo(Path.Combine(gameDir.FullName, referencedAssembly.Name + ".dll"));
                if (path.Exists)
                {
                    Assembly.LoadFrom(path.FullName);
                    continue;
                    
                }
                else
                {
                    var modRefDep = ModDependencyDirectory.GetFiles(path.Name)[0];
                    if (modRefDep != null)
                    {
                        Assembly.LoadFrom(modRefDep.FullName);
                        continue;
                    }
                }
                throw new FileNotFoundException($"Unable to find {referencedAssembly.Name}. Add it to dependencies folder");
            }

            var definedTypes = _modAssembly.GetExportedTypes();

            var methodsToInject = definedTypes
                .SelectMany(t => t.GetMethods())
                .Where(info => info.DeclaringType.Assembly.FullName == _modAssembly.FullName)
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
                        InjectFlags = attribute.InjectFlags,
                        InjectionAssembly = attribute.InjectionAssembly
                    });
                }
            }
        }
    }
}
