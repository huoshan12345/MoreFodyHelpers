namespace MoreFodyHelpers.Processing;

public static class WeaverILProcessorExtensions
{
    public static Instruction Create(this WeaverILProcessor il, OpCode opCode, int value)
    {
        return il.Create(opCode, (i, o) => i.Create(o, value));
    }

    public static Instruction Create(this WeaverILProcessor il, OpCode opCode, string value)
    {
        return il.Create(opCode, (i, o) => i.Create(o, value));
    }

    public static Instruction CreateLdarg(this WeaverILProcessor il, int index)
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
