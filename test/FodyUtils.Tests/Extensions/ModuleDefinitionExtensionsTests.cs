extern alias FodyHelpers;

using System.Configuration.Assemblies;
using System.Reflection;
using System.Security.Policy;
using FodyHelpers::Fody;

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
    private static readonly string _newDllFileFullName = Path.Combine(_tempDir.FullName, _dllFileName.Replace(".dll", ".new.dll"));
    private static readonly string _newAssemblyName = Path.GetFileNameWithoutExtension(_newDllFileFullName);
    private const string TypeName = "System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute";

    [Fact]
    public void GetOrAddIgnoresAccessChecksToAttribute_Test()
    {
        if (_tempDir.Exists)
        {
            _tempDir.Delete(true);
        }
        _tempDir.Create();
        _tempDir.Refresh();

        File.Copy(_dllFileFullName, _newDllFileFullName, true);

        {
            var assembly = ReadAssembly(_newDllFileFullName);
            var attr = assembly.GetType(TypeName);
            Assert.Null(attr);
        }

        using (var module = ReadModule(_newDllFileFullName))
        {
            module.Name = _newAssemblyName;
            module.Assembly.Name = new AssemblyNameDefinition(_newAssemblyName, module.Assembly.Name.Version);
            module.AddIgnoresAccessCheck();
            module.Write(_newDllFileFullName);
        }

        using (var module = ReadModule(_newDllFileFullName))
        {
            Assert.NotNull(module.GetType(TypeName));
        }

        {
            var assembly = ReadAssembly(_newDllFileFullName);
            var attr = assembly.GetType(TypeName);
            Assert.NotNull(attr);
            var obj = attr.New<Attribute>("test");
            Assert.NotNull(obj);
            Assert.Equal("test", obj.GetType().GetProperty("AssemblyName")?.GetValue(obj));

            var assemblyAttr = assembly.CustomAttributes.FirstOrDefault(m => m.AttributeType == attr);
            Assert.NotNull(assemblyAttr);
            Assert.Equal(_newAssemblyName, assemblyAttr.ConstructorArguments?.First().Value);
        }
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
        var bytes = File.ReadAllBytes(path);
        return Assembly.Load(bytes);
#endif
    }

    private static ModuleDefinition ReadModule(string path)
    {
        var bytes = File.ReadAllBytes(path);
        using var resolver = new TestAssemblyResolver();
        var ms = new MemoryStream(bytes);
        var module = ModuleDefinition.ReadModule(ms, new ReaderParameters
        {
            AssemblyResolver = resolver, // 将增加好目标目录的对象作为参数给AssemblyResolver
            ReadSymbols = false,
            ReadingMode = ReadingMode.Immediate,
        });
        return module;
    }
}