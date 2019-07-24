using System;
using Mono.Cecil.Inject;

namespace Kolibri.Lib
{
    public class InjectMethodAttribute : Attribute
    {
        public InjectMethodAttribute()
        {
        }

        public InjectMethodAttribute(Type injectionType, string injectionMethod, MethodInjectionInfo.MethodInjectionLocation injectionLocation = MethodInjectionInfo.MethodInjectionLocation.Top, InjectFlags injectFlags = InjectFlags.None, string injectionAssembly = null)
        {
            InjectionLocation = injectionType;
            InjectionMethod = injectionMethod;
            MethodInjectionLocation = injectionLocation;
            InjectFlags = injectFlags;
            InjectionAssembly = injectionAssembly;
        }

        public MethodInjectionInfo.MethodInjectionLocation MethodInjectionLocation { get; private set; }
        public InjectFlags InjectFlags { get; private set; }
        public Type InjectionLocation { get; private set; }
        public string InjectionMethod { get; private set; }
        public string InjectionAssembly { get; private set; }
    }
}
