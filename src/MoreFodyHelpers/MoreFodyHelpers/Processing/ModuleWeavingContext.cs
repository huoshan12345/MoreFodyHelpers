namespace MoreFodyHelpers.Processing;

public class ModuleWeavingContext : IDisposable
{
    public ModuleDefinition Module { get; }

    public bool IsDebugBuild { get; }
    public string? ProjectDirectory { get; }
    public string? WeaverAssemblyName { get; }

    internal Dictionary<TypeReference, bool> LibUsageTypeCache { get; } = [];
    internal InjectedAssemblyResolver InjectedAssemblyResolver { get; }

    public ModuleWeavingContext(ModuleDefinition module, string? weaverAssemblyName, string? projectDirectory)
    {
        Module = module;

        IsDebugBuild = Module.IsDebugBuild();
        ProjectDirectory = projectDirectory;
        WeaverAssemblyName = weaverAssemblyName;

        InjectedAssemblyResolver = new InjectedAssemblyResolver(this);
    }

    public void Dispose()
    {
        InjectedAssemblyResolver.Dispose();
    }
}
