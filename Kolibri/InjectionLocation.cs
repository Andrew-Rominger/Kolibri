using System;

namespace Kolibri
{
    public class InjectionLocation
    {
        public InjectionLocation(string injectionType, string injectionMethod)
        {
            if (string.IsNullOrEmpty(injectionType))
                throw new ArgumentNullException(nameof(injectionType));
            if (string.IsNullOrEmpty(injectionMethod))
                throw new ArgumentNullException(nameof(injectionMethod));

            InjectionType = injectionType;
            InjectionMethod = injectionMethod;
        }

        public string InjectionType { get; set; }
        public string InjectionMethod { get; set; }
    }
}
