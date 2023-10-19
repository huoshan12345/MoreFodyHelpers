using MoreFodyHelpers.Processing;

namespace MoreFodyHelpers;

using static AssemblyNames;

public class TypeReferenceEqualityComparerByName : IEqualityComparer<TypeReference>
{
    private static readonly string[] RuntimeNames = { MsCoreLib, Runtime, CoreLib, Netstandard };
    
    public static IEqualityComparer<TypeReference> Instance { get; } = new TypeReferenceEqualityComparerByName(false);
    public static IEqualityComparer<TypeReference> IgnoreRuntimeInstance { get; } = new TypeReferenceEqualityComparerByName(true);

    private readonly bool _ignoreRuntime;

    public TypeReferenceEqualityComparerByName(bool ignoreRuntime)
    {
        _ignoreRuntime = ignoreRuntime;
    }

    public bool Equals(TypeReference? x, TypeReference? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x == null || y == null) return false;

        return x.FullName == y.FullName
               && GetAssemblyName(x.Scope) == GetAssemblyName(y.Scope);
    }

    public int GetHashCode(TypeReference? obj)
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + obj?.FullName?.GetHashCode() ?? 0;
            hash = hash * 31 + GetAssemblyName(obj?.Scope)?.GetHashCode() ?? 0;
            return hash;
        }
    }

    private string? GetAssemblyName(IMetadataScope? scope)
    {
        if (scope == null)
            return null;

        var name = scope.Name;

        return RuntimeNames.Contains(name) && _ignoreRuntime
            ? Runtime
            : name;
    }
}