using MoreFodyHelpers.Support;

namespace MoreFodyHelpers.Extensions;

public static class TypeReferenceExtensions
{
    /// <summary>
    /// Get name of type with generic parameters without namespace.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string SimpleName(this TypeReference type)
    {
        if (type.IsGenericInstance == false)
            return type.Name;

        var name = type.Name;
        var index = name.IndexOf('`');
        return index == -1 ? name : name.Substring(0, index);
    }

    public static bool IsEqualTo(this TypeReference a, TypeReference b)
    {
        return TypeReferenceEqualityComparer.Instance.Equals(a, b);
    }
    public static TypeRefBuilder ToTypeRefBuilder(this TypeReference type, ModuleWeavingContext context)
    {
        return TypeRefBuilder.FromTypeReference(context, type);
    }

    public static IEnumerable<TypeReference> BaseTypes(this TypeReference type)
    {
        var p = type;
        while (true)
        {
            var b = p.Resolve().BaseType;
            if (b == null)
                yield break;

            yield return b;
            p = b;
        }
    }
}