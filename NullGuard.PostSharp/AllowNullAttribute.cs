using System;

namespace NullGuard.PostSharp
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Property)]
    public class AllowNullAttribute : Attribute
    {
    }
}
