using System;
using System.Reflection;

namespace NullGuard.PostSharp
{
    public static class ReflectionExtensions
    {
        static bool IsCustomAttributeDefined<T>(this ICustomAttributeProvider value) where T : Attribute
        {
            return value.IsDefined(typeof(T), false);
        }

        public static bool AllowsNull(this ICustomAttributeProvider value)
        {
            return value.IsCustomAttributeDefined<AllowNullAttribute>();
        }

        public static bool MayNotBeNull(this ParameterInfo arg)
        {
            return !arg.AllowsNull() && !arg.IsOptional && !arg.ParameterType.IsValueType && !arg.IsOut;
        }
    }
}
