using MoreFodyHelpers.Support;

namespace MoreFodyHelpers.Extensions;

public static class MethodReferenceExtensions
{
    public static bool IsEqualTo(this MethodReference a, MethodReference b)
    {
        return MethodReferenceEqualityComparer.Instance.Equals(a, b);
    }

    public static GenericInstanceMethod MakeGenericMethod(this MethodReference method, IEnumerable<TypeReference> args)
    {
        if (!method.HasGenericParameters)
            throw new WeavingException($"Not a generic method: {method.FullName}");

        var arguments = args.ToArray();

        if (arguments.Length == 0)
            throw new WeavingException("No generic arguments supplied");

        if (method.GenericParameters.Count != arguments.Length)
            throw new ArgumentException($"Incorrect number of generic arguments supplied for method {method.FullName} - expected {method.GenericParameters.Count}, but got {arguments.Length}");

        var instance = new GenericInstanceMethod(method);
        foreach (var argument in arguments)
            instance.GenericArguments.Add(argument);

        return instance;
    }
}