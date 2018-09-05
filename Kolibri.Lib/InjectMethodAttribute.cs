using System;

namespace Kolibri.Lib
{
    public class InjectMethodAttribute : Attribute
    {
        public InjectMethodAttribute(Type injectionType, string injectionMethod)
        {
            InjectionLocation = injectionType;
            InjectionMethod = injectionMethod;
        }

        public Type InjectionLocation { get; private set; }
        public string InjectionMethod { get; private set; }
    }
}
