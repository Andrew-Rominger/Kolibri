using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AudioMod
{
    public static class VerySneaky
    {
        public static T GetField<T>(this object obj, string fieldName)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            if(string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException(nameof(fieldName));
           return (T) obj.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(obj);
        }
    }
}
