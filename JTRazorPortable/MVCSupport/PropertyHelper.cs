using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JTRazorPortable
{
    internal class PropertyHelper
    {
        private static Dictionary<Type, PropertyHelper[]> _reflectionCache = new Dictionary<Type, PropertyHelper[]>();

        private Func<object, object> _valueGetter;

        public PropertyHelper(PropertyInfo property)
        {
            Name = property.Name;
            _valueGetter = MakeFastPropertyGetter(property);
        }

        public static Action<TDeclaringType, object> MakeFastPropertySetter<TDeclaringType>(PropertyInfo propertyInfo)
            where TDeclaringType : class
        {
            MethodInfo setMethod = propertyInfo.GetSetMethod();

            // Instance methods in the CLR can be turned into static methods where the first parameter
            // is open over "this". This parameter is always passed by reference, so we have a code
            // path for value types and a code path for reference types.
            Type typeInput = propertyInfo.PropertyType;
            Type typeValue = setMethod.GetParameters()[0].ParameterType;

            Delegate callPropertySetterDelegate;

            // Create a delegate TValue -> "TDeclaringType.Property"
            // Core version
            //var propertySetterAsAction = setMethod.GetGenericMethodDefinition().CreateDelegate(typeof(Action<,>).MakeGenericType(typeInput, typeValue));
            var propertySetterAsAction = Delegate.CreateDelegate(typeof(Action<,>).MakeGenericType(typeInput, typeValue), setMethod.GetGenericMethodDefinition());

            var callPropertySetterClosedGenericMethod = _callPropertySetterOpenGenericMethod.MakeGenericMethod(typeInput, typeValue);

            // Core version
            //callPropertySetterDelegate = callPropertySetterClosedGenericMethod.GetGenericMethodDefinition().CreateDelegate(typeof(Action<TDeclaringType, object>), propertySetterAsAction);
            callPropertySetterDelegate = Delegate.CreateDelegate(typeof(Action<TDeclaringType, object>), propertySetterAsAction, callPropertySetterClosedGenericMethod.GetGenericMethodDefinition());

            return (Action<TDeclaringType, object>)callPropertySetterDelegate;
        }

        public virtual string Name { get; protected set; }

        public object GetValue(object instance)
        {
            return _valueGetter(instance);
        }

        /// <summary>
        /// Creates and caches fast property helpers that expose getters for every public get property on the underlying type.
        /// </summary>
        /// <param name="instance">the instance to extract property accessors for.</param>
        /// <returns>a cached array of all public property getters from the underlying type of this instance.</returns>
        public static PropertyHelper[] GetProperties(object instance)
        {
            return GetProperties(instance, CreateInstance, _reflectionCache);
        }

        public static Func<object, object> MakeFastPropertyGetter(PropertyInfo propertyInfo)
        {
            MethodInfo getMethod = propertyInfo.GetGetMethod();

            // Instance methods in the CLR can be turned into static methods where the first parameter
            // is open over "this". This parameter is always passed by reference, so we have a code
            // path for value types and a code path for reference types.
            Type typeInput = propertyInfo.PropertyType;
            Type typeOutput = getMethod.ReturnType;

            Delegate callPropertyGetterDelegate;
            //if (typeInput.GetTypeInfo().IsValueType)
            if (typeInput.IsValueType)
            {
                // Create a delegate (ref TDeclaringType) -> TValue
                //Delegate propertyGetterAsFunc = getMethod.CreateDelegate(typeof(ByRefFunc<,>).MakeGenericType(typeInput, typeOutput));
                Delegate propertyGetterAsFunc = Delegate.CreateDelegate(typeof(ByRefFunc<,>).MakeGenericType(typeInput, typeOutput), getMethod);

                MethodInfo callPropertyGetterClosedGenericMethod = _callPropertyGetterByReferenceOpenGenericMethod.MakeGenericMethod(typeInput, typeOutput);

                //callPropertyGetterDelegate = callPropertyGetterClosedGenericMethod.CreateDelegate(typeof(Func<object, object>), propertyGetterAsFunc);
                callPropertyGetterDelegate = Delegate.CreateDelegate(typeof(Func<object, object>), propertyGetterAsFunc, callPropertyGetterClosedGenericMethod);
            }
            else
            {
                // Create a delegate TDeclaringType -> TValue
                //Delegate propertyGetterAsFunc = getMethod.CreateDelegate(typeof(Func<,>).MakeGenericType(typeInput, typeOutput));
                Delegate propertyGetterAsFunc = Delegate.CreateDelegate(typeof(Func<,>).MakeGenericType(typeInput, typeOutput), getMethod);

                MethodInfo callPropertyGetterClosedGenericMethod = _callPropertyGetterOpenGenericMethod.MakeGenericMethod(typeInput, typeOutput);

                //callPropertyGetterDelegate = callPropertyGetterClosedGenericMethod.CreateDelegate(typeof(Func<object, object>), propertyGetterAsFunc);
                callPropertyGetterDelegate = Delegate.CreateDelegate(typeof(Func<object, object>), propertyGetterAsFunc, callPropertyGetterClosedGenericMethod);
            }

            return (Func<object, object>)callPropertyGetterDelegate;
        }

        private static PropertyHelper CreateInstance(PropertyInfo property)
        {
            return new PropertyHelper(property);
        }

        // Implementation of the fast getter.
        private delegate TValue ByRefFunc<TDeclaringType, TValue>(ref TDeclaringType arg);

        private static readonly MethodInfo _callPropertyGetterOpenGenericMethod = GetPublicInstanceMethod(typeof(PropertyHelper), "CallPropertyGetter");
        private static readonly MethodInfo _callPropertyGetterByReferenceOpenGenericMethod = GetPublicInstanceMethod(typeof(PropertyHelper), "CallPropertyGetterByReference");

        private static object CallPropertyGetter<TDeclaringType, TValue>(Func<TDeclaringType, TValue> getter, object @this)
        {
            return getter((TDeclaringType)@this);
        }

        private static object CallPropertyGetterByReference<TDeclaringType, TValue>(ByRefFunc<TDeclaringType, TValue> getter, object @this)
        {
            TDeclaringType unboxed = (TDeclaringType)@this;
            return getter(ref unboxed);
        }

        // Implementation of the fast setter.
        private static readonly MethodInfo _callPropertySetterOpenGenericMethod = GetPublicInstanceMethod(typeof(PropertyHelper), "CallPropertySetter");

        private static void CallPropertySetter<TDeclaringType, TValue>(Action<TDeclaringType, TValue> setter, object @this, object value)
        {
            setter((TDeclaringType)@this, (TValue)value);
        }

        protected static PropertyHelper[] GetProperties(object instance,
                                                        Func<PropertyInfo, PropertyHelper> createPropertyHelper,
                                                        Dictionary<Type, PropertyHelper[]> cache)
        {
            // Using an array rather than IEnumerable, as this will be called on the hot path numerous times.
            PropertyHelper[] helpers;

            Type type = instance.GetType();

            if (!cache.TryGetValue(type, out helpers))
            {
                // We avoid loading indexed properties using the where statement.
                // Indexed properties are not useful (or valid) for grabbing properties off an anonymous object.
                IEnumerable<PropertyInfo> properties = GetPublicInstanceProperties(type)
                                                           .Where(prop => prop.GetIndexParameters().Length == 0 &&
                                                                          prop.GetGetMethod() != null);

                var newHelpers = new List<PropertyHelper>();

                foreach (PropertyInfo property in properties)
                {
                    PropertyHelper propertyHelper = createPropertyHelper(property);

                    newHelpers.Add(propertyHelper);
                }

                helpers = newHelpers.ToArray();
                if (!cache.ContainsKey(type))
                    cache.Add(type, helpers);
            }

            return helpers;
        }

        public static MethodInfo GetPublicInstanceMethod(Type type, string name)
        {
            var methods = type.GetMethods();
            foreach (MethodInfo methodInfo in methods)
            {
                if (methodInfo.IsStatic && methodInfo.IsPublic && (methodInfo.Name == name))
                    return methodInfo;
            }
            return null;
        }

        public static IEnumerable<PropertyInfo> GetPublicInstanceProperties(Type type)
        {
            List<PropertyInfo> propertyInfos = new List<PropertyInfo>();
            var properties = type.GetProperties();
            foreach (PropertyInfo propertyInfo in properties)
            {
                MethodInfo getMethod = propertyInfo.GetGetMethod();
                if (!getMethod.IsStatic && getMethod.IsPublic)
                    propertyInfos.Add(propertyInfo);
            }
            return propertyInfos;
        }
    }
}
