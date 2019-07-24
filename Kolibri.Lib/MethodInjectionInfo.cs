using Mono.Cecil.Inject;

namespace Kolibri.Lib
{
    public class MethodInjectionInfo
    {
        public enum MethodInjectionLocation
        {
            Top,
            Bottom
        }
        public InjectionLocation TargetMethod { get; set; }
        public InjectionLocation SourceMethod { get; set; }
        public MethodInjectionLocation InjectionLocation { get; set; }
        public InjectFlags InjectFlags { get; set; }
        public string InjectionAssembly { get; set; }
        
    }
}
