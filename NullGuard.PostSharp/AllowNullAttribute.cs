using System;

namespace NullGuard.PostSharp
{
    [Obsolete("Use CanBeNullAttribute instead in order to improve ReSharper's static analysis.")]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Property)]
    public class AllowNullAttribute : Attribute
    {
    }
}