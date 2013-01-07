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
        int[] inputArgumentsToValidate;
        int[] outputArgumentsToValidate;
        string[] parameterNames;
        bool validateReturnValue;
        string memberName;
        bool isProperty;
        
        public EnsureNonNullAspect() : this(ValidationFlags.AllPublic)
        {
        }

        public EnsureNonNullAspect(ValidationFlags validationFlags)
        {
            ValidationFlags = validationFlags;
        }

        public ValidationFlags ValidationFlags { get; set; }


        public override bool CompileTimeValidate(MethodBase method)
        {
            // This method executes as build time. It should return 'true' if the aspect is actually needed.
            // It sets some aspect fields which will be serialized into the assembly, and deserialized at runtime,
            // so we don't need reflection at runtime.

            MethodInformation methodInformation = MethodInformation.GetMethodInformation(method);
            ParameterInfo[] parameters = method.GetParameters();

            // Check that the aspect applies on the current method.
            if (!ValidationFlags.HasFlag(ValidationFlags.NonPublic) && !methodInformation.IsPublic) return false;
            if (!ValidationFlags.HasFlag(ValidationFlags.Properties) && methodInformation.IsProperty) return false;
            if (!ValidationFlags.HasFlag(ValidationFlags.Methods) && !methodInformation.IsProperty) return false;

            // Store pieces information needed at runtime.
            this.parameterNames = parameters.Select(p => p.Name).ToArray();
            this.memberName = methodInformation.Name;
            this.isProperty = methodInformation.IsProperty;

            ParameterInfo[] argumentsToValidate = parameters.Where(p => p.MayNotBeNull()).ToArray();
      
            // Build the list of input arguments that need to be validated.
            if (ValidationFlags.HasFlag(ValidationFlags.Arguments))
            {
                this.inputArgumentsToValidate = argumentsToValidate.Where(p => !p.IsOut).Select(p => p.Position).ToArray();
            }
            else
            {
                this.inputArgumentsToValidate = new int[0];
            }

            // Build the list of output arguments that need to be validated.
            if (ValidationFlags.HasFlag(ValidationFlags.OutValues))
            {
                this.outputArgumentsToValidate = argumentsToValidate.Where(p => p.ParameterType.IsByRef).Select(p => p.Position).ToArray();
            }
            else
            {
                this.outputArgumentsToValidate = new int[0];
            }

            // Determine whether the return value should be validated.
            if (!methodInformation.IsConstructor)
            {
                this.validateReturnValue = ValidationFlags.HasFlag(ValidationFlags.ReturnValues) && methodInformation.ReturnParameter.MayNotBeNull();
            }

            // Finally, determine if the aspect is useful on the aspect.
            bool validationRequired = this.validateReturnValue || this.inputArgumentsToValidate.Length > 0 || this.outputArgumentsToValidate.Length > 0;

            return validationRequired;
        }

   
        public override void OnEntry(MethodExecutionArgs args)
        {
            // Validate input arguments. No reflection is used and no memory is allocated by the aspect itself.

            foreach (int argumentPosition in inputArgumentsToValidate)
            {
                if (args.Arguments[argumentPosition] == null)
                {
                    string parameterName = this.parameterNames[argumentPosition];

                    if (this.isProperty)
                    {
                        
                        throw new ArgumentNullException(parameterName,
                            String.Format(CultureInfo.InvariantCulture,
                                "Cannot set the value of property '{0}' to null.",
                                this.memberName));
                    }
                    else
                    {
                        throw new ArgumentNullException(parameterName);
                    }
                }
            }

     

        }

        public override void OnSuccess(MethodExecutionArgs args)
        {

            // Validate output arguments. 

            foreach (int argumentPosition in outputArgumentsToValidate)
            {
                if (args.Arguments[argumentPosition] == null)
                {
                    string parameterName = this.parameterNames[argumentPosition];

                      throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                            "Out parameter '{0}' is null.", parameterName));
                }
                
            }

            // Validate the return value.

            if (this.validateReturnValue && args.ReturnValue == null )
            {
                if (this.isProperty)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                        "Return value of property '{0}' is null.",
                        this.memberName));
                }

                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    "Return value of method '{0}' is null.", this.memberName));
            }

      
           
        }

        class MethodInformation
        {
            MethodInformation(ConstructorInfo constructor) : this((MethodBase) constructor)
            {
                IsConstructor = true;
                Name = constructor.Name;
                
            }

            MethodInformation(MethodInfo method) : this((MethodBase) method)
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
                ReturnParameter = method.ReturnParameter;
            }

            MethodInformation(MethodBase method)
            {
                IsPublic = method.IsPublic;
       
            }

            public static MethodInformation GetMethodInformation(MethodBase  methodBase)
            {
                var ctor = methodBase as ConstructorInfo;
                if (ctor != null) return new MethodInformation(ctor);
                var method = methodBase as MethodInfo;
                return method == null ? null : new MethodInformation(method);
            }

            public string Name { get; private set; }

            public bool IsProperty { get; private set; }

            public bool IsPublic { get; private set; }

            public bool IsConstructor { get; private set; }

            
             public ParameterInfo ReturnParameter { get; private set; }
        }
    }
}
