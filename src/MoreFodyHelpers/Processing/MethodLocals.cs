namespace MoreFodyHelpers.Processing;

public class MethodLocals
{
    private readonly Dictionary<string, VariableDefinition> _localsByName = new();
    private readonly List<VariableDefinition> _localsByIndex = new();
    public MethodDefinition Method { get; }

    public MethodLocals(MethodDefinition method, IEnumerable<LocalVarBuilder>? locals = null)
    {
        Method = method ?? throw new ArgumentNullException(nameof(method));

        foreach (var local in locals.EmptyIfNull())
        {
            AddLocalVar(local);
        }
    }

    public VariableDefinition AddLocalVar(LocalVarBuilder local)
    {
        var localVar = local.Build();
        Method.Body.Variables.Add(localVar);
        var name = local.Name ?? $"_LocalVar_{_localsByIndex.Count - 1}";

        Method.DebugInformation.Scope?.Variables.Add(new VariableDebugInformation(localVar, name));

        if (_localsByName.ContainsKey(name))
            throw new WeavingException($"Local {local.Name} is already defined");

        _localsByName.Add(name, localVar);
        _localsByIndex.Add(localVar);

        return localVar;
    }

    public VariableDefinition? TryGetByName(string name)
        => _localsByName.GetValueOrDefault(name);

    public void MapMacroInstruction(Instruction instruction)
    {
        switch (instruction.OpCode.Code)
        {
            case Code.Ldloc_0:
                MapIndex(OpCodes.Ldloc, 0);
                break;
            case Code.Ldloc_1:
                MapIndex(OpCodes.Ldloc, 1);
                break;
            case Code.Ldloc_2:
                MapIndex(OpCodes.Ldloc, 2);
                break;
            case Code.Ldloc_3:
                MapIndex(OpCodes.Ldloc, 3);
                break;
            case Code.Stloc_0:
                MapIndex(OpCodes.Stloc, 0);
                break;
            case Code.Stloc_1:
                MapIndex(OpCodes.Stloc, 1);
                break;
            case Code.Stloc_2:
                MapIndex(OpCodes.Stloc, 2);
                break;
            case Code.Stloc_3:
                MapIndex(OpCodes.Stloc, 3);
                break;
        }

        void MapIndex(OpCode opCode, int index)
        {
            var local = GetLocalByIndex(index);
            instruction.OpCode = opCode;
            instruction.Operand = local;
        }
    }

    public bool MapIndexInstruction(ref OpCode opCode, int index, [MaybeNullWhen(false)] out VariableDefinition result)
    {
        switch (opCode.Code)
        {
            case Code.Ldloc:
            case Code.Ldloc_S:
            case Code.Ldloca:
            case Code.Ldloca_S:
            case Code.Stloc:
            case Code.Stloc_S:
            {
                result = GetLocalByIndex(index);
                return true;
            }

            default:
                result = null;
                return false;
        }
    }

    private VariableDefinition GetLocalByIndex(int index)
    {
        if (index < 0 || index >= _localsByIndex.Count)
            throw new WeavingException($"Local index {index} is out of range");

        return _localsByIndex[index];
    }
}
