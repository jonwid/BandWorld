using System;
using System.Collections.Generic;
using System.Reflection;

namespace JTRazorPortable
{
    public static class ObjectAccess
    {
        public static MethodInfo GetRuntimeMethod(this Type type, string name)
        {
            var methods = type.GetMethods();
            foreach (var method in methods)
                if (method.Name == name)
                    return method;
            return null;
        }

        public static PropertyInfo GetRuntimeProperty(this Type type, string name)
        {
            var properties = type.GetProperties();
            foreach (var property in properties)
                if (property.Name == name)
                    return property;
            return null;
        }
    }
}
