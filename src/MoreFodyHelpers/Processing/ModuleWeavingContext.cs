namespace MoreFodyHelpers.Processing;

public class ModuleWeavingContext : IDisposable
{
    public ModuleDefinition Module { get; }

    public bool IsDebugBuild { get; }
    public string? ProjectDirectory { get; }

    internal Dictionary<TypeReference, bool> LibUsageTypeCache { get; } = new();
    internal InjectedAssemblyResolver InjectedAssemblyResolver { get; }

    public ModuleWeavingContext(ModuleDefinition module, string? projectDirectory)
    {
        Module = module;

        IsDebugBuild = Module.IsDebugBuild();
        ProjectDirectory = projectDirectory;

        InjectedAssemblyResolver = new InjectedAssemblyResolver(this);
    }

    public void Dispose()
    {
        InjectedAssemblyResolver.Dispose();
    }
}
