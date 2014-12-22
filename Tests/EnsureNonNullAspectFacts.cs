using System;
using System.Security;
using JetBrains.Annotations;
using NullGuard.PostSharp;
using Xunit;

namespace Tests
{
    public class RewritingConstructors
    {
        [Fact]
        public void RequiresNonNullArgument()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new SampleClass(null, ""));
            Assert.Equal("nonNullArg", exception.ParamName);
        }

        [Fact]
        public void RequiresNonNullOutArgument()
        {
            string someArg;
            Assert.Throws<InvalidOperationException>(() => new SampleClass(out someArg));
        }

        [Fact]
        public void AllowsNullWhenAttributeApplied()
        {
            new SampleClass("", null);
        }
    }

    public class RewritingMethods
    {
        [Fact]
        public void RequiresNonNullArgument()
        {
            var sample = new SampleClass();
            var exception = Assert.Throws<ArgumentNullException>(() => sample.SomeMethod(null, ""));
            Assert.Equal("nonNullArg", exception.ParamName);
        }

        [Fact]
        public void AllowsNullWhenAttributeApplied()
        {
            var sample = new SampleClass();
            sample.SomeMethod("", null);
        }

        [Fact]
        public void RequiresNonNullMethodReturnValue()
        {
            var sample = new SampleClass();
            Assert.Throws<InvalidOperationException>(() => sample.MethodWithReturnValue(returnNull: true));
        }

        [Fact]
        public void AllowsNullReturnValueWhenAttributeApplied()
        {
            var sample = new SampleClass();
            sample.MethodAllowsNullReturnValue();
        }

        [Fact]
        public void RequiresNonNullOutValue()
        {
            var sample = new SampleClass();
            string value;
            Assert.Throws<InvalidOperationException>(() => sample.MethodWithOutValue(out value));
        }

        [Fact]
        public void RequiresNonNullOutValueNotEnforcedUponException()
        {
            var sample = new SampleClass();
            string value;
            Assert.Throws<ContextMarshalException>(() => sample.MethodWithOutValueAndException(out value));
        }

        [Fact]
        public void DoesNotRequireNonNullForNonPublicMethod()
        {
            var sample = new SampleClass();
            sample.PublicWrapperOfPrivateMethod();
        }

        [Fact]
        public void RequiresNonNullForNonPublicMethodWhenAttributeSpecifiesNonPublic()
        {
            var sample = new ClassWithPrivateMethod();
            Assert.Throws<ArgumentNullException>(() => sample.PublicWrapperOfPrivateMethod());
        }


    }

    public class RewritingProperties
    {
        [Fact]
        public void PropertySetterRequiresNonNullArgument()
        {
            var sample = new SampleClass();
            var exception = Assert.Throws<ArgumentNullException>(() => {sample.NonNullProperty = null;});
            Assert.Equal("value", exception.ParamName);
        }

        [Fact]
        public void PropertyGetterRequiresNonNullReturnValue()
        {
            var sample = new SampleClass();
            Assert.Throws<InvalidOperationException>(() => Console.WriteLine(sample.NonNullProperty));
        }

        [Fact]
        public void PropertyAllowsNullGetButNotSet()
        {
            var sample = new SampleClass();
            Assert.Null(sample.PropertyAllowsNullGetButDoesNotAllowNullSet);
            Assert.Throws<ArgumentNullException>(() => {sample.NonNullProperty = null;});
        }

        [Fact]
        public void PropertyAllowsNullSetButNotGet()
        {
            var sample = new SampleClass {PropertyAllowsNullSetButDoesNotAllowNullGet = null};
            Assert.Throws<InvalidOperationException>(() =>
                Console.Write(sample.PropertyAllowsNullSetButDoesNotAllowNullGet));
        }

        [Fact]
        public void PropertySetterRequiresAllowsNullArgumentForNullableType()
        {
            new SampleClass {NonNullNullableProperty = null};
        }

        [Fact]
        public void DoesNotRequireNullSetterWhenPropertiesNotSpecifiedByAttribute()
        {
            new ClassWithPrivateMethod {SomeProperty = null};
        }
    }

    [EnsureNonNullAspect]
    public class SampleClass
    {
        public SampleClass()
        {
        }

        // Why would anyone place an out parameter on a ctor?! I don't know, but I'll support your idiocy.
        public SampleClass(out string nonNullOutArg)
        {
            nonNullOutArg = null;
        }

        public SampleClass(string nonNullArg, [CanBeNull] string nullArg)
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

        public string PropertyAllowsNullGetButDoesNotAllowNullSet { [return: CanBeNull] get; set; }

        public string PropertyAllowsNullSetButDoesNotAllowNullGet { get; [param: CanBeNull] set; }

        public int? NonNullNullableProperty { get; set; }

        public string MethodWithReturnValue(bool returnNull)
        {
            return returnNull ? null : "";
        }

        [return: CanBeNull]
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

    [EnsureNonNullAspect(ValidationFlags.NonPublic | ValidationFlags.Methods | ValidationFlags.Arguments)]
    public class ClassWithPrivateMethod
    {
        public void PublicWrapperOfPrivateMethod()
        {
            SomePrivateMethod(null);
        }

        void SomePrivateMethod(string x)
        {
        }

        public string SomeProperty { get; set; }
    }
}
