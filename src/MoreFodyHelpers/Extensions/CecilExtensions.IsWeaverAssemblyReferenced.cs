namespace MoreFodyHelpers.Extensions;

public static partial class CecilExtensions
{
    private static readonly ConcurrentDictionary<TypeReference, bool> _usageCache = new();

    public static bool IsAssemblyReferenced(this ModuleDefinition module, MethodDefinition method, string assemblyName)
    {
        if (method.IsAssemblyReferenced(module, assemblyName))
            return true;

        if (!method.HasBody)
            return false;

        if (method.Body.HasVariables && method.Body.Variables.Any(i => i.VariableType.IsAssemblyReferenced(module, assemblyName)))
            return true;

        foreach (var instruction in method.Body.Instructions)
        {
            switch (instruction.Operand)
            {
                case MethodReference methodRef when methodRef.IsAssemblyReferenced(module, assemblyName):
                case TypeReference typeRef when typeRef.IsAssemblyReferenced(module, assemblyName):
                case FieldReference fieldRef when fieldRef.IsAssemblyReferenced(module, assemblyName):
                case CallSite callSite when callSite.IsAssemblyReferenced(module, assemblyName):
                    return true;
            }
        }
        return false;
    }

    public static bool IsAssemblyReferenced(this TypeReference? type, ModuleDefinition module, string assemblyName)
    {
        return type != null && _usageCache.GetOrAdd(type, k => DoCheck(k, module, assemblyName));

        static bool DoCheck(TypeReference typeRef, ModuleDefinition module, string assemblyName)
        {
            return typeRef switch
            {
                GenericInstanceType t => t.ElementType.IsAssemblyReferenced(module, assemblyName)
                                         || t.GenericParameters.Any(i => i.IsAssemblyReferenced(module, assemblyName))
                                         || t.GenericArguments.Any(i => i.IsAssemblyReferenced(module, assemblyName)),
                GenericParameter t => t.HasConstraints && t.Constraints.Any(c => c.IsAssemblyReferenced(module, assemblyName))
                                      || t.HasCustomAttributes && t.CustomAttributes.Any(i => i.IsAssemblyReferenced(module, assemblyName)),
                IModifierType t => t.ElementType.IsAssemblyReferenced(module, assemblyName) || t.ModifierType.IsAssemblyReferenced(module, assemblyName),
                FunctionPointerType t => ((IMethodSignature)t).IsAssemblyReferenced(module, assemblyName),
                _ => typeRef.Scope is { MetadataScopeType: MetadataScopeType.AssemblyNameReference } scope && scope.Name == assemblyName,
            };
        }
    }

    public static bool IsAssemblyReferenced(this IMethodSignature? method, ModuleDefinition module, string assemblyName)
    {
        if (method == null)
            return false;

        if (method.ReturnType.IsAssemblyReferenced(module, assemblyName) || method.HasParameters && method.Parameters.Any(i => i.IsAssemblyReferenced(module, assemblyName)))
            return true;

        if (method is IGenericInstance { HasGenericArguments: true } genericInstance && genericInstance.GenericArguments.Any(i => i.IsAssemblyReferenced(module, assemblyName)))
            return true;

        if (method is IGenericParameterProvider { HasGenericParameters: true } generic && generic.GenericParameters.Any(i => i.IsAssemblyReferenced(module, assemblyName)))
            return true;

        if (method is MethodReference methodRef)
        {
            if (methodRef is MethodDefinition methodDef)
            {
                if (methodDef.HasCustomAttributes && methodDef.CustomAttributes.Any(i => i.IsAssemblyReferenced(module, assemblyName)))
                    return true;
            }
            else
            {
                if (methodRef.DeclaringType.IsAssemblyReferenced(module, assemblyName))
                    return true;
            }
        }

        return false;
    }

    public static bool IsAssemblyReferenced(this FieldReference? fieldRef, ModuleDefinition module, string assemblyName)
    {
        if (fieldRef == null)
            return false;

        if (fieldRef.FieldType.IsAssemblyReferenced(module, assemblyName))
            return true;

        if (fieldRef is FieldDefinition fieldDef)
        {
            if (fieldDef.HasCustomAttributes && fieldDef.CustomAttributes.Any(i => i.IsAssemblyReferenced(module, assemblyName)))
                return true;
        }
        else
        {
            if (fieldRef.DeclaringType.IsAssemblyReferenced(module, assemblyName))
                return true;
        }

        return false;
    }

    public static bool IsAssemblyReferenced(this ParameterDefinition? paramDef, ModuleDefinition module, string assemblyName)
    {
        if (paramDef == null)
            return false;

        if (paramDef.ParameterType.IsAssemblyReferenced(module, assemblyName))
            return true;

        if (paramDef.HasCustomAttributes && paramDef.CustomAttributes.Any(i => i.IsAssemblyReferenced(module, assemblyName)))
            return true;

        return false;
    }

    public static bool IsAssemblyReferenced(this CustomAttribute? attr, ModuleDefinition module, string assemblyName)
    {
        if (attr == null)
            return false;

        if (attr.AttributeType.IsAssemblyReferenced(module, assemblyName))
            return true;

        if (attr.HasConstructorArguments && attr.ConstructorArguments.Any(i => i.Value is TypeReference typeRef && typeRef.IsAssemblyReferenced(module, assemblyName)))
            return true;

        if (attr.HasProperties && attr.Properties.Any(i => i.Argument.Value is TypeReference typeRef && typeRef.IsAssemblyReferenced(module, assemblyName)))
            return true;

        return false;
    }

    public static bool IsAssemblyReferenced(this GenericParameterConstraint? constraint, ModuleDefinition module, string assemblyName)
    {
        if (constraint == null)
            return false;

        if (constraint.ConstraintType.IsAssemblyReferenced(module, assemblyName))
            return true;

        if (constraint.HasCustomAttributes && constraint.CustomAttributes.Any(i => i.IsAssemblyReferenced(module, assemblyName)))
            return true;

        return false;
    }
}