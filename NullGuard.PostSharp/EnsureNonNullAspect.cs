using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using PostSharp.Aspects;

namespace NullGuard.PostSharp
{
    [Serializable]
    public class EnsureNonNullAspect : OnMethodBoundaryAspect
    {
        public EnsureNonNullAspect() : this(ValidationFlags.AllPublic)
        {
        }

        public EnsureNonNullAspect(ValidationFlags validationFlags)
        {
            ValidationFlags = validationFlags;
        }

        public ValidationFlags ValidationFlags { get; set; }

        public override void OnEntry(MethodExecutionArgs args)
        {
            if (!ValidationFlags.HasFlag(ValidationFlags.Arguments)) return;

            var method = MethodInformation.GetMethodInformation(args);
            if (method == null
                || !method.HasArguments
                || (!ValidationFlags.HasFlag(ValidationFlags.NonPublic) && !method.IsPublic)
                || (!ValidationFlags.HasFlag(ValidationFlags.Properties) && method.IsProperty)
                || (!ValidationFlags.HasFlag(ValidationFlags.Methods) && !method.IsProperty)
                // TODO: What about events?
                )
                return;

            var invalidArgument = (from arg in args.Method.GetParameters()
                where arg.MayNotBeNull() && args.Arguments[arg.Position] == null
                select arg).FirstOrDefault();

            if (invalidArgument == null) return;

            if (method.IsProperty)
            {
                throw new ArgumentNullException(invalidArgument.Name,
                    String.Format(CultureInfo.InvariantCulture,
                        "Cannot set the value of property '{0}' to null.",
                        method.Name));
            }
            throw new ArgumentNullException(invalidArgument.Name);
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            var method = MethodInformation.GetMethodInformation(args);
            if (method == null
                || (!ValidationFlags.HasFlag(ValidationFlags.NonPublic) && !method.IsPublic)
                || (!ValidationFlags.HasFlag(ValidationFlags.Properties) && method.IsProperty)
                || (!ValidationFlags.HasFlag(ValidationFlags.Methods) && !method.IsProperty)
                // TODO: Deal with events later?
                )
                return;

            if (ValidationFlags.HasFlag(ValidationFlags.OutValues))
            {
                foreach (var arg in args.Method.GetParameters().Where(p => p.IsOut))
                {
                    if (arg.ParameterType.IsValueType) continue;
                    if (args.Arguments[arg.Position] == null)
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                            "Out parameter '{0}' is null.", arg.Name));
                }
            }

            if (!ValidationFlags.HasFlag(ValidationFlags.ReturnValues)
                || method.ReturnType == null
                || method.ReturnType.IsValueType
                || method.ReturnParameter.AllowsNull()) return;

            if (method.ReturnValue != null) return;
            if (method.IsProperty)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    "Return value of property '{0}' is null.",
                    method.Name));
            }
            throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                "Return value of method '{0}' is null.",
                method.Name));
        }

        class MethodInformation
        {
            MethodInformation(MethodExecutionArgs args, ConstructorInfo constructor) : this(constructor, args)
            {
                IsConstructor = true;
                Name = constructor.Name;
                HasArguments = args.Arguments.Any();
                ReturnValue = args.ReturnValue;
            }

            MethodInformation(MethodExecutionArgs args, MethodInfo method) : this(method, args)
            {
                IsConstructor = false;
                Name = method.Name;
                if (method.IsSpecialName &&
                    (Name.StartsWith("set_", StringComparison.Ordinal) ||
                        Name.StartsWith("get_", StringComparison.Ordinal)))
                {
                    Name = Name.Substring(4);
                    IsProperty = true;
                }
                ReturnType = method.ReturnType;
                ReturnParameter = method.ReturnParameter;
            }

            MethodInformation(MethodBase method, MethodExecutionArgs args)
            {
                IsPublic = method.IsPublic;
                HasArguments = args.Arguments.Any();
                ReturnValue = args.ReturnValue;
            }

            public static MethodInformation GetMethodInformation(MethodExecutionArgs args)
            {
                var ctor = args.Method as ConstructorInfo;
                if (ctor != null) return new MethodInformation(args, ctor);
                var method = args.Method as MethodInfo;
                return method == null ? null : new MethodInformation(args, method);
            }

            public string Name { get; private set; }

            public bool IsProperty { get; private set; }

            public bool IsPublic { get; private set; }

            public bool IsConstructor { get; private set; }

            public bool HasArguments { get; private set; }

            public Type ReturnType { get; private set; }

            public object ReturnValue { get; private set; }

            public ParameterInfo ReturnParameter { get; private set; }
        }
    }
}
