using NullGuard.PostSharp;

namespace Tests
{
    public abstract class SampleBaseClass
    {
        public int BaseArgumentLength { get; private set; }

        [EnsureNonNullAspect(ValidationFlags.All)]
        protected SampleBaseClass(string notNull)
        {
            BaseArgumentLength = notNull.Length;
        }
    }

    public class SampleDerivedClass : SampleBaseClass
    {
        [EnsureNonNullAspect]
        public SampleDerivedClass(string notNull)
            : base (notNull)
        {
        }
    }
}
