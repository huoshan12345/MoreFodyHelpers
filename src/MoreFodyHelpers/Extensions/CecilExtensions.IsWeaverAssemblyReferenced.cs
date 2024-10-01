namespace MoreFodyHelpers.Extensions;

public static partial class CecilExtensions
{
    public static bool IsTypeUsage([NotNullWhen(true)] this TypeReference? type, ModuleWeavingContext context, string assemblyName)
    {
        if (type == null)
            return false;

        if (context.LibUsageTypeCache.TryGetValue(type, out var result))
            return result;

        context.LibUsageTypeCache[type] = false;
        result = DoCheck(type, context, assemblyName);
        context.LibUsageTypeCache[type] = result;
        return result;

        static bool DoCheck(TypeReference typeRef, ModuleWeavingContext ctx, string assemblyName)
        {
            switch (typeRef)
            {
                case GenericInstanceType t:
                    return t.ElementType.IsTypeUsage(ctx, assemblyName)
                           || t.GenericParameters.Any(i => i.IsTypeUsage(ctx, assemblyName))
                           || t.GenericArguments.Any(i => i.IsTypeUsage(ctx, assemblyName));

                case GenericParameter t:
                    return t.HasConstraints && t.Constraints.Any(c => c.IsTypeUsage(ctx, assemblyName))
                           || t.HasCustomAttributes && t.CustomAttributes.Any(i => i.IsTypeUsage(ctx, assemblyName));

                case IModifierType t:
                    return t.ElementType.IsTypeUsage(ctx, assemblyName)
                           || t.ModifierType.IsTypeUsage(ctx, assemblyName);

                case FunctionPointerType t:
                    return ((IMethodSignature)t).IsTypeUsage(ctx, assemblyName);

                default:
                    return typeRef.Scope is { MetadataScopeType: MetadataScopeType.AssemblyNameReference } scope && scope.Name == assemblyName;
            }
        }
    }

    public static bool IsTypeUsageDeep([NotNullWhen(true)] this TypeDefinition? typeDef, ModuleWeavingContext context, string assemblyName)
    {
        if (typeDef == null)
            return false;

        return typeDef.IsTypeUsage(context, assemblyName)
               || typeDef.BaseType.IsTypeUsage(context, assemblyName)
               || typeDef.HasInterfaces && typeDef.Interfaces.Any(i => i.IsTypeUsage(context, assemblyName))
               || typeDef.HasGenericParameters && typeDef.GenericParameters.Any(i => i.IsTypeUsage(context, assemblyName))
               || typeDef.HasCustomAttributes && typeDef.CustomAttributes.Any(i => i.IsTypeUsage(context, assemblyName))
               || typeDef.HasMethods && typeDef.Methods.Any(i => i.IsTypeUsage(context, assemblyName))
               || typeDef.HasFields && typeDef.Fields.Any(i => i.IsTypeUsage(context, assemblyName))
               || typeDef.HasProperties && typeDef.Properties.Any(i => i.IsTypeUsage(context, assemblyName))
               || typeDef.HasEvents && typeDef.Events.Any(i => i.IsTypeUsage(context, assemblyName));
    }

    public static bool IsTypeUsage([NotNullWhen(true)] this IMethodSignature? method, ModuleWeavingContext context, string assemblyName)
    {
        if (method == null)
            return false;

        if (method.ReturnType.IsTypeUsage(context, assemblyName) || method.HasParameters && method.Parameters.Any(i => i.IsTypeUsage(context, assemblyName)))
            return true;

        if (method is IGenericInstance { HasGenericArguments: true } genericInstance && genericInstance.GenericArguments.Any(i => i.IsTypeUsage(context, assemblyName)))
            return true;

        if (method is IGenericParameterProvider { HasGenericParameters: true } generic && generic.GenericParameters.Any(i => i.IsTypeUsage(context, assemblyName)))
            return true;

        if (method is MethodReference methodRef)
        {
            if (methodRef is MethodDefinition methodDef)
            {
                if (methodDef.HasCustomAttributes && methodDef.CustomAttributes.Any(i => i.IsTypeUsage(context, assemblyName)))
                    return true;
            }
            else
            {
                if (methodRef.DeclaringType.IsTypeUsage(context, assemblyName))
                    return true;
            }
        }

        return false;
    }

    public static bool IsTypeUsage([NotNullWhen(true)] this FieldReference? fieldRef, ModuleWeavingContext context, string assemblyName)
    {
        if (fieldRef == null)
            return false;

        if (fieldRef.FieldType.IsTypeUsage(context, assemblyName))
            return true;

        if (fieldRef is FieldDefinition fieldDef)
        {
            if (fieldDef.HasCustomAttributes && fieldDef.CustomAttributes.Any(i => i.IsTypeUsage(context, assemblyName)))
                return true;
        }
        else
        {
            if (fieldRef.DeclaringType.IsTypeUsage(context, assemblyName))
                return true;
        }

        return false;
    }

    public static bool IsTypeUsage([NotNullWhen(true)] this PropertyReference? propRef, ModuleWeavingContext context, string assemblyName)
    {
        if (propRef == null)
            return false;

        if (propRef.PropertyType.IsTypeUsage(context, assemblyName))
            return true;

        if (propRef is PropertyDefinition propDef)
        {
            if (propDef.HasCustomAttributes && propDef.CustomAttributes.Any(i => i.IsTypeUsage(context, assemblyName)))
                return true;
        }
        else
        {
            if (propRef.DeclaringType.IsTypeUsage(context, assemblyName))
                return true;
        }

        return false;
    }

    public static bool IsTypeUsage([NotNullWhen(true)] this EventReference? eventRef, ModuleWeavingContext context, string assemblyName)
    {
        if (eventRef == null)
            return false;

        if (eventRef.EventType.IsTypeUsage(context, assemblyName))
            return true;

        if (eventRef is EventDefinition eventDef)
        {
            if (eventDef.HasCustomAttributes && eventDef.CustomAttributes.Any(i => i.IsTypeUsage(context, assemblyName)))
                return true;
        }
        else
        {
            if (eventRef.DeclaringType.IsTypeUsage(context, assemblyName))
                return true;
        }

        return false;
    }

    public static bool IsTypeUsage([NotNullWhen(true)] this ParameterDefinition? paramDef, ModuleWeavingContext context, string assemblyName)
    {
        if (paramDef == null)
            return false;

        if (paramDef.ParameterType.IsTypeUsage(context, assemblyName))
            return true;

        if (paramDef.HasCustomAttributes && paramDef.CustomAttributes.Any(i => i.IsTypeUsage(context, assemblyName)))
            return true;

        return false;
    }

    public static bool IsTypeUsage([NotNullWhen(true)] this CustomAttribute? attr, ModuleWeavingContext context, string assemblyName)
    {
        if (attr == null)
            return false;

        if (attr.AttributeType.IsTypeUsage(context, assemblyName))
            return true;

        if (attr.HasConstructorArguments && attr.ConstructorArguments.Any(i => i.Value is TypeReference typeRef && typeRef.IsTypeUsage(context, assemblyName)))
            return true;

        if (attr.HasProperties && attr.Properties.Any(i => i.Argument.Value is TypeReference typeRef && typeRef.IsTypeUsage(context, assemblyName)))
            return true;

        return false;
    }

    public static bool IsTypeUsage([NotNullWhen(true)] this InterfaceImplementation? interfaceImpl, ModuleWeavingContext context, string assemblyName)
    {
        if (interfaceImpl == null)
            return false;

        return interfaceImpl.InterfaceType.IsTypeUsage(context, assemblyName)
               || interfaceImpl.HasCustomAttributes && interfaceImpl.CustomAttributes.Any(i => i.IsTypeUsage(context, assemblyName));
    }

    public static bool IsTypeUsage([NotNullWhen(true)] this GenericParameterConstraint? constraint, ModuleWeavingContext context, string assemblyName)
    {
        if (constraint == null)
            return false;

        if (constraint.ConstraintType.IsTypeUsage(context, assemblyName))
            return true;

        if (constraint.HasCustomAttributes && constraint.CustomAttributes.Any(i => i.IsTypeUsage(context, assemblyName)))
            return true;

        return false;
    }
}