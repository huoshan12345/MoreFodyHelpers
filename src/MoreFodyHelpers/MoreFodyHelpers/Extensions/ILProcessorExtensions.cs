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

    public static Instruction CreateLdarg(this ILProcessor il, int index)
    {
        return index switch
        {
            0 => il.Create(OpCodes.Ldarg_0),
            1 => il.Create(OpCodes.Ldarg_1),
            2 => il.Create(OpCodes.Ldarg_2),
            3 => il.Create(OpCodes.Ldarg_3),
            _ => il.Create(OpCodes.Ldarg, index),
        };
    }
}