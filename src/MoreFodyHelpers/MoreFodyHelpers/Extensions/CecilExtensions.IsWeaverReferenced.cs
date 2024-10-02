namespace MoreFodyHelpers.Extensions;

public static partial class CecilExtensions
{
    public static bool IsWeaverReferenced([NotNullWhen(true)] this TypeReference? type, ModuleWeavingContext context)
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
                    return t.ElementType.IsWeaverReferenced(ctx)
                           || t.GenericParameters.Any(i => i.IsWeaverReferenced(ctx))
                           || t.GenericArguments.Any(i => i.IsWeaverReferenced(ctx));

                case GenericParameter t:
                    return t.HasConstraints && t.Constraints.Any(c => c.IsWeaverReferenced(ctx))
                           || t.HasCustomAttributes && t.CustomAttributes.Any(i => i.IsWeaverReferenced(ctx));

                case IModifierType t:
                    return t.ElementType.IsWeaverReferenced(ctx)
                           || t.ModifierType.IsWeaverReferenced(ctx);

                case FunctionPointerType t:
                    return ((IMethodSignature)t).IsWeaverReferenced(ctx);

                default:
                    return typeRef.Scope is { MetadataScopeType: MetadataScopeType.AssemblyNameReference } scope && scope.Name == ctx.WeaverAssemblyName;
            }
        }
    }

    public static bool IsWeaverReferencedDeep([NotNullWhen(true)] this TypeDefinition? typeDef, ModuleWeavingContext context)
    {
        if (typeDef == null)
            return false;

        return typeDef.IsWeaverReferenced(context)
               || typeDef.BaseTypes().Any(m => m.IsWeaverReferenced(context))
               || typeDef.HasInterfaces && typeDef.Interfaces.Any(i => i.IsWeaverReferenced(context))
               || typeDef.HasGenericParameters && typeDef.GenericParameters.Any(i => i.IsWeaverReferenced(context))
               || typeDef.HasCustomAttributes && typeDef.CustomAttributes.Any(i => i.IsWeaverReferenced(context))
               || typeDef.HasMethods && typeDef.Methods.Any(i => i.IsWeaverReferenced(context))
               || typeDef.HasFields && typeDef.Fields.Any(i => i.IsWeaverReferenced(context))
               || typeDef.HasProperties && typeDef.Properties.Any(i => i.IsWeaverReferenced(context))
               || typeDef.HasEvents && typeDef.Events.Any(i => i.IsWeaverReferenced(context));
    }

    public static bool IsWeaverReferenced([NotNullWhen(true)] this IMethodSignature? method, ModuleWeavingContext context)
    {
        if (method == null)
            return false;

        if (method.ReturnType.IsWeaverReferenced(context) || method.HasParameters && method.Parameters.Any(i => i.IsWeaverReferenced(context)))
            return true;

        if (method is IGenericInstance { HasGenericArguments: true } genericInstance && genericInstance.GenericArguments.Any(i => i.IsWeaverReferenced(context)))
            return true;

        if (method is IGenericParameterProvider { HasGenericParameters: true } generic && generic.GenericParameters.Any(i => i.IsWeaverReferenced(context)))
            return true;

        if (method is MethodReference methodRef)
        {
            if (methodRef is MethodDefinition methodDef)
            {
                if (methodDef.HasCustomAttributes && methodDef.CustomAttributes.Any(i => i.IsWeaverReferenced(context)))
                    return true;

                if (IsReferenced(methodDef, context))
                    return true;
            }
            else
            {
                if (methodRef.DeclaringType.IsWeaverReferenced(context))
                    return true;
            }
        }

        return false;

        static bool IsReferenced(MethodDefinition method, ModuleWeavingContext context)
        {
            if (!method.HasBody)
                return false;

            if (method.Body.HasVariables && method.Body.Variables.Any(i => i.VariableType.IsWeaverReferenced(context)))
                return true;

            foreach (var instruction in method.Body.Instructions)
            {
                switch (instruction.Operand)
                {
                    case MethodReference methodRef when methodRef.IsWeaverReferenced(context):
                    case TypeReference typeRef when typeRef.IsWeaverReferenced(context):
                    case FieldReference fieldRef when fieldRef.IsWeaverReferenced(context):
                    case CallSite callSite when callSite.IsWeaverReferenced(context):
                        return true;
                }
            }
            return false;
        }
    }

    public static bool IsWeaverReferenced([NotNullWhen(true)] this FieldReference? fieldRef, ModuleWeavingContext context)
    {
        if (fieldRef == null)
            return false;

        if (fieldRef.FieldType.IsWeaverReferenced(context))
            return true;

        if (fieldRef is FieldDefinition fieldDef)
        {
            if (fieldDef.HasCustomAttributes && fieldDef.CustomAttributes.Any(i => i.IsWeaverReferenced(context)))
                return true;
        }
        else
        {
            if (fieldRef.DeclaringType.IsWeaverReferenced(context))
                return true;
        }

        return false;
    }

    public static bool IsWeaverReferenced([NotNullWhen(true)] this PropertyReference? propRef, ModuleWeavingContext context)
    {
        if (propRef == null)
            return false;

        if (propRef.PropertyType.IsWeaverReferenced(context))
            return true;

        if (propRef is PropertyDefinition propDef)
        {
            if (propDef.HasCustomAttributes && propDef.CustomAttributes.Any(i => i.IsWeaverReferenced(context)))
                return true;
        }
        else
        {
            if (propRef.DeclaringType.IsWeaverReferenced(context))
                return true;
        }

        return false;
    }

    public static bool IsWeaverReferenced([NotNullWhen(true)] this EventReference? eventRef, ModuleWeavingContext context)
    {
        if (eventRef == null)
            return false;

        if (eventRef.EventType.IsWeaverReferenced(context))
            return true;

        if (eventRef is EventDefinition eventDef)
        {
            if (eventDef.HasCustomAttributes && eventDef.CustomAttributes.Any(i => i.IsWeaverReferenced(context)))
                return true;
        }
        else
        {
            if (eventRef.DeclaringType.IsWeaverReferenced(context))
                return true;
        }

        return false;
    }

    public static bool IsWeaverReferenced([NotNullWhen(true)] this ParameterDefinition? paramDef, ModuleWeavingContext context)
    {
        if (paramDef == null)
            return false;

        if (paramDef.ParameterType.IsWeaverReferenced(context))
            return true;

        if (paramDef.HasCustomAttributes && paramDef.CustomAttributes.Any(i => i.IsWeaverReferenced(context)))
            return true;

        return false;
    }

    public static bool IsWeaverReferenced([NotNullWhen(true)] this CustomAttribute? attr, ModuleWeavingContext context)
    {
        if (attr == null)
            return false;

        if (attr.AttributeType.IsWeaverReferenced(context))
            return true;

        if (attr.HasConstructorArguments && attr.ConstructorArguments.Any(i => i.Value is TypeReference typeRef && typeRef.IsWeaverReferenced(context)))
            return true;

        if (attr.HasProperties && attr.Properties.Any(i => i.Argument.Value is TypeReference typeRef && typeRef.IsWeaverReferenced(context)))
            return true;

        return false;
    }

    public static bool IsWeaverReferenced([NotNullWhen(true)] this InterfaceImplementation? interfaceImpl, ModuleWeavingContext context)
    {
        if (interfaceImpl == null)
            return false;

        return interfaceImpl.InterfaceType.IsWeaverReferenced(context)
               || interfaceImpl.HasCustomAttributes && interfaceImpl.CustomAttributes.Any(i => i.IsWeaverReferenced(context));
    }

    public static bool IsWeaverReferenced([NotNullWhen(true)] this GenericParameterConstraint? constraint, ModuleWeavingContext context)
    {
        if (constraint == null)
            return false;

        if (constraint.ConstraintType.IsWeaverReferenced(context))
            return true;

        if (constraint.HasCustomAttributes && constraint.CustomAttributes.Any(i => i.IsWeaverReferenced(context)))
            return true;

        return false;
    }
}