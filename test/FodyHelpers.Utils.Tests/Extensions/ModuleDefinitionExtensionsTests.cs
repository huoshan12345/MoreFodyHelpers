using System.Reflection;

#if NET6_0_OR_GREATER
using System.Runtime.Loader;
#endif

namespace FodyHelpers.Tests.Extensions;

public class ModuleDefinitionExtensionsTests
{
    private static readonly string _root = AppContext.BaseDirectory;
    private static readonly string _dllFileName = typeof(ModuleDefinitionExtensionsTests).Assembly.ManifestModule.Name;
    private static readonly string _dllFileFullName = Path.Combine(_root, _dllFileName);
    private static readonly DirectoryInfo _tempDir = new(Path.Combine(_root, "temp"));
    private static readonly string _tempDllFileFullName = Path.Combine(_tempDir.FullName, _dllFileName);

    [Fact]
    public void GetOrAddIgnoresAccessChecksToAttribute_Test()
    {
        _tempDir.Create();
        File.Copy(_dllFileFullName, _tempDllFileFullName, true);

        const string typeName = "System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute";

#if NET6_0_OR_GREATER
        {
            var assembly = ReadAssembly(_tempDllFileFullName);
            var attr = assembly.GetType(typeName);
            Assert.Null(attr);
        }

        var bytes = File.ReadAllBytes(_tempDllFileFullName);
        using (var ms = new MemoryStream(bytes))
        using (var module = ModuleDefinition.ReadModule(ms))
        {
            module.GetOrAddIgnoresAccessChecksToAttribute();
            module.Write(_tempDllFileFullName);
        }

        {
            var assembly = ReadAssembly(_tempDllFileFullName);
            var attr = assembly.GetType(typeName);
            Assert.NotNull(attr);
        }
#endif
    }

    private static Assembly ReadAssembly(string path)
    {
#if NET6_0_OR_GREATER
        var context = new AssemblyLoadContext(null, true);
        using var fs = File.Open(path, FileMode.Open);
        var assembly = context.LoadFromStream(fs);
        context.Unload();
        return assembly;
#else
    throw new NotImplementedException();
#endif
    }
}

