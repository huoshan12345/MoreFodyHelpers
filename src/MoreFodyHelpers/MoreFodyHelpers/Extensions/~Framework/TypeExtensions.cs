using System.Reflection;
using static System.Reflection.BindingFlags;

namespace MoreFodyHelpers.Extensions;

public static class TypeExtensions
{
    public static T New<T>(this Type type, params object[] args)
    {
        return (T)Activator.CreateInstance(type, args);
    }

    private const BindingFlags Flags = Public | NonPublic | Instance | Static;

    public static PropertyInfo GetRequiredProperty(this Type type, string name)
    {
        return type.GetProperty(name, Flags)
               ?? throw new InvalidOperationException($"Cannot find property '{name}' in type '{type.FullName}'");
    }

    public static FieldInfo GetRequiredField(this Type type, string name)
    {
        return type.GetField(name, Flags)
               ?? throw new InvalidOperationException($"Cannot find field '{name}' in type '{type.FullName}'");
    }
}