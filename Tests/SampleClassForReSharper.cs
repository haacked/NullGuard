using System;
using JetBrains.Annotations;
using NullGuard.PostSharp;

namespace Tests
{
    [EnsureNonNullAspect]
    public class SampleClassForReSharper
    {
        public SampleClassForReSharper()
        {
        }

        // Why would anyone place an out parameter on a ctor?! I don't know, but I'll support your idiocy.
        public SampleClassForReSharper(out string nonNullOutArg)
        {
            nonNullOutArg = null;
        }

        public SampleClassForReSharper(string nonNullArg, [CanBeNull] string nullArg)
        {
            Console.WriteLine(nonNullArg + " " + nullArg);
        }

        public void SomeMethod(string nonNullArg, [CanBeNull] string nullArg)
        {
            Console.WriteLine(nonNullArg);
        }

        public string NonNullProperty { get; set; }

        [CanBeNull]
        public string NullProperty { get; set; }

        public string PropertyAllowsNullGetButDoesNotAllowNullSet { [return: AllowNull] get; set; }

        public string PropertyAllowsNullSetButDoesNotAllowNullGet { get; [param: AllowNull] set; }

        public int? NonNullNullableProperty { get; set; }

        public string MethodWithReturnValue(bool returnNull)
        {
            return returnNull ? null : "";
        }

        // nothing to be supported by ReSharper here
        [return: AllowNull]
        public string MethodAllowsNullReturnValue()
        {
            return null;
        }

        public void MethodWithOutValue(out string nonNullOutArg)
        {
            nonNullOutArg = null;
        }

        public void MethodWithOutValueAndException(out string nonNullOutArg)
        {
            throw new ContextMarshalException();
        }

        public void PublicWrapperOfPrivateMethod()
        {
            SomePrivateMethod(null);
        }

        void SomePrivateMethod(string x)
        {
        }
    }
}