namespace MoreFodyHelpers.Extensions;

public static class PropertyDefinitionExtensions
{
    public static MethodReference BuildSetter(this PropertyDefinition property, ModuleWeavingContext context)
    {
        if (property.SetMethod == null)
            throw new WeavingException($"Property '{property.Name}' in type {property.DeclaringType.FullName} has no setter");

        return new MethodRefBuilder(context, property.DeclaringType, property.SetMethod).Build();
    }

    public static MethodReference BuildGetter(this PropertyDefinition property, ModuleWeavingContext context)
    {
        if (property.GetMethod == null)
            throw new WeavingException($"Property '{property.Name}' in type {property.DeclaringType.FullName} has no getter");

        return new MethodRefBuilder(context, property.DeclaringType, property.GetMethod).Build();
    }
}