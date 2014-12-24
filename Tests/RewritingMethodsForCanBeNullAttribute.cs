using System;
using Xunit;

namespace Tests
{
    public class RewritingMethodsForCanBeNullAttribute
    {
        [Fact]
        public void RequiresNonNullArgument()
        {
            var sample = new SampleClassForReSharper();
            var exception = Assert.Throws<ArgumentNullException>(() => sample.SomeMethodForReSharper(null, ""));
            Assert.Equal("nonNullArg", exception.ParamName);
        }

        [Fact]
        public void AllowsNullWhenAttributeApplied()
        {
            var sample = new SampleClassForReSharper();
            sample.SomeMethodForReSharper("", null);
        }

        [Fact]
        public void RequiresNonNullMethodReturnValue()
        {
            var sample = new SampleClassForReSharper();
            Assert.Throws<InvalidOperationException>(() => sample.MethodWithReturnValue(returnNull: true));
        }

        [Fact]
        public void AllowsNullReturnValueWhenAttributeApplied()
        {
            var sample = new SampleClassForReSharper();
            sample.MethodAllowsNullReturnValue();
        }

        [Fact]
        public void RequiresNonNullOutValue()
        {
            var sample = new SampleClassForReSharper();
            string value;
            Assert.Throws<InvalidOperationException>(() => sample.MethodWithOutValue(out value));
        }

        [Fact]
        public void RequiresNonNullOutValueNotEnforcedUponException()
        {
            var sample = new SampleClassForReSharper();
            string value;
            Assert.Throws<ContextMarshalException>(() => sample.MethodWithOutValueAndException(out value));
        }

        [Fact]
        public void DoesNotRequireNonNullForNonPublicMethod()
        {
            var sample = new SampleClassForReSharper();
            sample.PublicWrapperOfPrivateMethod();
        }

        [Fact]
        public void RequiresNonNullForNonPublicMethodWhenAttributeSpecifiesNonPublic()
        {
            var sample = new ClassWithPrivateMethod();
            Assert.Throws<ArgumentNullException>(() => sample.PublicWrapperOfPrivateMethod());
        }


    }
}