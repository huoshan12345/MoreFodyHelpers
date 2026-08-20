namespace MoreFodyHelpers.Tests.Processing;

public class ModuleWeavingContextTests
{
    [Fact]
    public void Import_RuntimeMethodHandle_Test()
    {
        using var context = CreateContext(nameof(Import_RuntimeMethodHandle_Test));
        var intPtr = context.ImportReference<nint>();
        var runtimeMethodHandle = context.ImportReference<RuntimeMethodHandle>();
        var method = MethodRefBuilder.MethodByNameAndSignature(context, runtimeMethodHandle, nameof(RuntimeMethodHandle.GetFunctionPointer), 0, intPtr, []).Build();
        Assert.Equal("System.IntPtr System.RuntimeMethodHandle::GetFunctionPointer()", method.FullName);
    }

    [Fact]
    public void ImportReference_Test()
    {
        using var context = CreateContext(nameof(ImportReference_Test));
        var type = context.ImportReference<RuntimeMethodHandle>();
        var typeDef = type.Resolve();
        Assert.NotNull(typeDef);
    }

    private static ModuleWeavingContext CreateContext(string name)
    {
        var module = ModuleDefinition.CreateModule(name, ModuleKind.Dll);
        var context = new ModuleWeavingContext(module, "", null);
        return context;
    }
}