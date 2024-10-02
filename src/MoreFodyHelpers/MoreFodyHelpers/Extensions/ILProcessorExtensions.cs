namespace MoreFodyHelpers.Extensions;

public static class ILProcessorExtensions
{
    public static Instruction InsertAfter(this ILProcessor il, Instruction target, IEnumerable<Instruction> instructions)
    {
        var p = target;
        foreach (var instruction in instructions)
        {
            il.InsertAfter(p, instruction);
            p = instruction;
        }
        return p;
    }
}