using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JTRazorPortable
{
    internal static class TypeHelper
    {
        /// <summary>
        /// Given an object of anonymous type, add each property as a key and associated with its value to a dictionary.
        ///
        /// This helper will cache accessors and types, and is intended when the anonymous object is accessed multiple
        /// times throughout the lifetime of the web application.
        /// </summary>
        public static RouteValueDictionary ObjectToDictionary(object value)
        {
            RouteValueDictionary dictionary = new RouteValueDictionary();

            if (value != null)
            {
                foreach (PropertyInfo property in value.GetType().GetProperties())
                {
                    object val = property.GetGetMethod().Invoke(value, null);
                    dictionary.Add(property.Name, val);
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Given an object of anonymous type, add each property as a key and associated with its value to a dictionary.
        ///
        /// This helper will not cache accessors and types, and is intended when the anonymous object is accessed once
        /// or very few times throughout the lifetime of the web application.
        /// </summary>
        public static RouteValueDictionary ObjectToDictionaryUncached(object value)
        {
            return ObjectToDictionary(value);
        }

        /// <summary>
        /// Given an object of anonymous type, add each property as a key and associated with its value to the given dictionary.
        /// </summary>
        public static void AddAnonymousObjectToDictionary(IDictionary<string, object> dictionary, object value)
        {
            var values = ObjectToDictionary(value);
            foreach (var item in values)
            {
                dictionary.Add(item);
            }
        }

        /// <remarks>This code is copied from http://www.liensberger.it/web/blog/?p=191 </remarks>
        public static bool IsAnonymousType(Type type)
        {
            /*
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            // TODO: The only way to detect anonymous types right now.
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                   && type.IsGenericType && type.Name.Contains("AnonymousType")
                   && (type.Name.StartsWith("<>", StringComparison.OrdinalIgnoreCase) || type.Name.StartsWith("VB$", StringComparison.OrdinalIgnoreCase))
                   && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
            */
            return false;
        }

        public static RouteValueDictionary AnonymousObjectToHtmlAttributes(object htmlAttributes)
        {
            RouteValueDictionary result = new RouteValueDictionary();

            if (htmlAttributes != null)
            {
                foreach (PropertyInfo property in htmlAttributes.GetType().GetProperties())
                {
                    object val = property.GetGetMethod().Invoke(htmlAttributes, null);
                    result.Add(property.Name, val);
                }
            }

            return result;
        }
    }
}
