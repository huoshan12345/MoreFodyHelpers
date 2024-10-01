namespace MoreFodyHelpers.Extensions;

public static partial class CecilExtensions
{
    public static bool IsTypeUsage([NotNullWhen(true)] this TypeReference? type, ModuleWeavingContext context)
    {
        if (type == null)
            return false;

        if (context.LibUsageTypeCache.TryGetValue(type, out var result))
            return result;

        context.LibUsageTypeCache[type] = false;
        result = DoCheck(type, context);
        context.LibUsageTypeCache[type] = result;
        return result;

        static bool DoCheck(TypeReference typeRef, ModuleWeavingContext ctx)
        {
            switch (typeRef)
            {
                case GenericInstanceType t:
                    return t.ElementType.IsTypeUsage(ctx)
                           || t.GenericParameters.Any(i => i.IsTypeUsage(ctx))
                           || t.GenericArguments.Any(i => i.IsTypeUsage(ctx));

                case GenericParameter t:
                    return t.HasConstraints && t.Constraints.Any(c => c.IsTypeUsage(ctx))
                           || t.HasCustomAttributes && t.CustomAttributes.Any(i => i.IsTypeUsage(ctx));

                case IModifierType t:
                    return t.ElementType.IsTypeUsage(ctx)
                           || t.ModifierType.IsTypeUsage(ctx);

                case FunctionPointerType t:
                    return ((IMethodSignature)t).IsTypeUsage(ctx);

                default:
                    return typeRef.Scope is { MetadataScopeType: MetadataScopeType.AssemblyNameReference, Name: "InlineIL" };
            }
        }
    }

    public static bool IsTypeUsageDeep([NotNullWhen(true)] this TypeDefinition? typeDef, ModuleWeavingContext context)
    {
        if (typeDef == null)
            return false;

        return typeDef.IsTypeUsage(context)
               || typeDef.BaseType.IsTypeUsage(context)
               || typeDef.HasInterfaces && typeDef.Interfaces.Any(i => i.IsTypeUsage(context))
               || typeDef.HasGenericParameters && typeDef.GenericParameters.Any(i => i.IsTypeUsage(context))
               || typeDef.HasCustomAttributes && typeDef.CustomAttributes.Any(i => i.IsTypeUsage(context))
               || typeDef.HasMethods && typeDef.Methods.Any(i => i.IsTypeUsage(context))
               || typeDef.HasFields && typeDef.Fields.Any(i => i.IsTypeUsage(context))
               || typeDef.HasProperties && typeDef.Properties.Any(i => i.IsTypeUsage(context))
               || typeDef.HasEvents && typeDef.Events.Any(i => i.IsTypeUsage(context));
    }

    public static bool IsTypeUsage([NotNullWhen(true)] this IMethodSignature? method, ModuleWeavingContext context)
    {
        if (method == null)
            return false;

        if (method.ReturnType.IsTypeUsage(context) || method.HasParameters && method.Parameters.Any(i => i.IsTypeUsage(context)))
            return true;

        if (method is IGenericInstance { HasGenericArguments: true } genericInstance && genericInstance.GenericArguments.Any(i => i.IsTypeUsage(context)))
            return true;

        if (method is IGenericParameterProvider { HasGenericParameters: true } generic && generic.GenericParameters.Any(i => i.IsTypeUsage(context)))
            return true;

        if (method is MethodReference methodRef)
        {
            if (methodRef is MethodDefinition methodDef)
            {
                if (methodDef.HasCustomAttributes && methodDef.CustomAttributes.Any(i => i.IsTypeUsage(context)))
                    return true;
            }
            else
            {
                if (methodRef.DeclaringType.IsTypeUsage(context))
                    return true;
            }
        }

        return false;
    }

    public static bool IsTypeUsage([NotNullWhen(true)] this FieldReference? fieldRef, ModuleWeavingContext context)
    {
        if (fieldRef == null)
            return false;

        if (fieldRef.FieldType.IsTypeUsage(context))
            return true;

        if (fieldRef is FieldDefinition fieldDef)
        {
            if (fieldDef.HasCustomAttributes && fieldDef.CustomAttributes.Any(i => i.IsTypeUsage(context)))
                return true;
        }
        else
        {
            if (fieldRef.DeclaringType.IsTypeUsage(context))
                return true;
        }

        return false;
    }

    public static bool IsTypeUsage([NotNullWhen(true)] this PropertyReference? propRef, ModuleWeavingContext context)
    {
        if (propRef == null)
            return false;

        if (propRef.PropertyType.IsTypeUsage(context))
            return true;

        if (propRef is PropertyDefinition propDef)
        {
            if (propDef.HasCustomAttributes && propDef.CustomAttributes.Any(i => i.IsTypeUsage(context)))
                return true;
        }
        else
        {
            if (propRef.DeclaringType.IsTypeUsage(context))
                return true;
        }

        return false;
    }

    public static bool IsTypeUsage([NotNullWhen(true)] this EventReference? eventRef, ModuleWeavingContext context)
    {
        if (eventRef == null)
            return false;

        if (eventRef.EventType.IsTypeUsage(context))
            return true;

        if (eventRef is EventDefinition eventDef)
        {
            if (eventDef.HasCustomAttributes && eventDef.CustomAttributes.Any(i => i.IsTypeUsage(context)))
                return true;
        }
        else
        {
            if (eventRef.DeclaringType.IsTypeUsage(context))
                return true;
        }

        return false;
    }

    public static bool IsTypeUsage([NotNullWhen(true)] this ParameterDefinition? paramDef, ModuleWeavingContext context)
    {
        if (paramDef == null)
            return false;

        if (paramDef.ParameterType.IsTypeUsage(context))
            return true;

        if (paramDef.HasCustomAttributes && paramDef.CustomAttributes.Any(i => i.IsTypeUsage(context)))
            return true;

        return false;
    }

    public static bool IsTypeUsage([NotNullWhen(true)] this CustomAttribute? attr, ModuleWeavingContext context)
    {
        if (attr == null)
            return false;

        if (attr.AttributeType.IsTypeUsage(context))
            return true;

        if (attr.HasConstructorArguments && attr.ConstructorArguments.Any(i => i.Value is TypeReference typeRef && typeRef.IsTypeUsage(context)))
            return true;

        if (attr.HasProperties && attr.Properties.Any(i => i.Argument.Value is TypeReference typeRef && typeRef.IsTypeUsage(context)))
            return true;

        return false;
    }

    public static bool IsTypeUsage([NotNullWhen(true)] this InterfaceImplementation? interfaceImpl, ModuleWeavingContext context)
    {
        if (interfaceImpl == null)
            return false;

        return interfaceImpl.InterfaceType.IsTypeUsage(context)
               || interfaceImpl.HasCustomAttributes && interfaceImpl.CustomAttributes.Any(i => i.IsTypeUsage(context));
    }

    public static bool IsTypeUsage([NotNullWhen(true)] this GenericParameterConstraint? constraint, ModuleWeavingContext context)
    {
        if (constraint == null)
            return false;

        if (constraint.ConstraintType.IsTypeUsage(context))
            return true;

        if (constraint.HasCustomAttributes && constraint.CustomAttributes.Any(i => i.IsTypeUsage(context)))
            return true;

        return false;
    }
}