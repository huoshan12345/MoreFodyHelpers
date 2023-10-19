namespace MoreFodyHelpers;

public class MethodReferenceEqualityComparer : IEqualityComparer<MethodReference>
{
    private static readonly Type _typeReferenceEqualityComparer = typeof(MethodReference).Assembly.GetRequiredType("Mono.Cecil.MethodReferenceComparer");
    public static readonly IEqualityComparer<MethodReference> Instance = _typeReferenceEqualityComparer.New<IEqualityComparer<MethodReference>>();

    public bool Equals(MethodReference x, MethodReference y)
    {
        return Instance.Equals(x, y);
    }

    public int GetHashCode(MethodReference obj)
    {
        return Instance.GetHashCode(obj);
    }
}