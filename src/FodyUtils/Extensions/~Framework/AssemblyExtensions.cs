using System.Reflection;

namespace FodyUtils.Extensions;

public static class AssemblyExtensions
{
    public static Type GetRequiredType(this Assembly assembly, string name, bool ignoreCase = false)
    {
        return assembly.GetType(name, true, ignoreCase) ?? throw new InvalidOperationException($"Cannot find type '{name}' in assembly '{assembly.FullName}'");
    }
}