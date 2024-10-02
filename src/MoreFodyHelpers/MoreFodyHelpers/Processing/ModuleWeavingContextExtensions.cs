namespace MoreFodyHelpers.Processing;

public static class ModuleWeavingContextExtensions
{
    public static TypeReference ImportReference(this ModuleWeavingContext context, Type type)
    {
        var name = type.Assembly.GetName().Name;
        if (name == AssemblyNames.SystemPrivateCoreLib)
        {
            // NOTE: TypeReference.Resolve() may return null sometimes, especially in release mode.
            // Use this assembly redirection as a workaround.
            name = AssemblyNames.SystemRuntime;
        }
        return TypeRefBuilder.FromAssemblyNameAndTypeName(context, name, type.FullName ?? type.Name).Build();
    }

    public static TypeReference ImportReference<T>(this ModuleWeavingContext context)
    {
        return context.ImportReference(typeof(T));
    }
}
