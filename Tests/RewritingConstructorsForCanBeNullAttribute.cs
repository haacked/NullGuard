using System;
using Xunit;

namespace Tests
{
    public class RewritingConstructorsForCanBeNullAttribute
    {
        [Fact]
        public void RequiresNonNullArgument()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new SampleClassForReSharper(null, ""));
            Assert.Equal("nonNullArg", exception.ParamName);
        }

        [Fact]
        public void RequiresNonNullOutArgument()
        {
            string someArg;
            Assert.Throws<InvalidOperationException>(() => new SampleClassForReSharper(out someArg));
        }

        [Fact]
        public void AllowsNullWhenAttributeApplied()
        {
            new SampleClassForReSharper("", null);
        }
    }
}