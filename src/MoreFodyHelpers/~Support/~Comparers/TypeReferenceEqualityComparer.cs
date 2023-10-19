namespace MoreFodyHelpers;

public class TypeReferenceEqualityComparer : IEqualityComparer<TypeReference>
{
    private static readonly Type _typeReferenceEqualityComparer = typeof(TypeReference).Assembly.GetRequiredType("Mono.Cecil.TypeReferenceEqualityComparer");
    public static readonly IEqualityComparer<TypeReference> Instance = _typeReferenceEqualityComparer.New<IEqualityComparer<TypeReference>>();

    public bool Equals(TypeReference x, TypeReference y)
    {
        return Instance.Equals(x, y);
    }

    public int GetHashCode(TypeReference obj)
    {
        return Instance.GetHashCode(obj);
    }
}