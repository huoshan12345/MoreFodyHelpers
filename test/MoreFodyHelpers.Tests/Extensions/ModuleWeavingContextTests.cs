using MoreFodyHelpers.Model;

namespace MoreFodyHelpers.Tests.Extensions;

public class ModuleWeavingContextTests
{
    [Fact]
    public void Import_RuntimeMethodHandle_Test()
    {
        var path = typeof(ModuleWeavingContextTests).Assembly.Location;
        using var module = ModuleDefinition.ReadModule(path);
        using var context = new ModuleWeavingContext(module, Path.GetDirectoryName(path));
        var intPtr = module.ImportReference(typeof(IntPtr));
        var runtimeMethodHandle = module.ImportReference(typeof(RuntimeMethodHandle));
        var method = MethodRefBuilder.MethodByNameAndSignature(context, runtimeMethodHandle, nameof(RuntimeMethodHandle.GetFunctionPointer), 0, intPtr, []).Build();
        Assert.Equal("System.IntPtr System.RuntimeMethodHandle::GetFunctionPointer()", method.FullName);
    }
}