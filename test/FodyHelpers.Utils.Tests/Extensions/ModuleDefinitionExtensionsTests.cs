extern alias FodyHelpers;

using System.Reflection;
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
    private static readonly string _tempDllFileFullName = Path.Combine(_tempDir.FullName, _dllFileName.Replace(".dll", ".new.dll"));
    private const string TypeName = "System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute";

    [Fact]
    public void GetOrAddIgnoresAccessChecksToAttribute_Test()
    {
        _tempDir.Create();
        File.Copy(_dllFileFullName, _tempDllFileFullName, true);

        {
            var assembly = ReadAssembly(_tempDllFileFullName);
            var attr = assembly.GetType(TypeName);
            Assert.Null(attr);
        }

        var bytes = File.ReadAllBytes(_tempDllFileFullName);
        using (var resolver = new TestAssemblyResolver())
        using (var ms = new MemoryStream(bytes))
        using (var module = ModuleDefinition.ReadModule(ms, new ReaderParameters
        {
            AssemblyResolver = resolver, // 将增加好目标目录的对象作为参数给AssemblyResolver
            ReadSymbols = false,
            ReadingMode = ReadingMode.Immediate,
        }))
        {
            module.AddIgnoresAccessCheck();
            module.Write(_tempDllFileFullName);
        }
        {
            var assembly = ReadAssembly(_tempDllFileFullName);
            var types = assembly.GetTypes().OrderBy(m => m.Name).ToArray();
            var attr = assembly.GetType(TypeName);
            Assert.NotNull(attr);
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
        var file = new FileInfo(path);
        var info = new AppDomainSetup
        {
            ApplicationBase = _root,
            ApplicationName = Path.GetFileNameWithoutExtension(file.Name),
            LoaderOptimization = LoaderOptimization.SingleDomain
        };
        var domain = AppDomain.CreateDomain(Guid.NewGuid().ToString(), null, info);
        var assemblyName = new AssemblyName
        {
            CodeBase = path
        };
        var assembly = domain.Load(assemblyName);
        AppDomain.Unload(domain);
        return assembly;
#endif
    }
}

