namespace MoreFodyHelpers.Extensions;

partial class CecilExtensions
{
    public static GraphNode<int> BuildGraph(this IList<Instruction> instructions)
    {
        // Failed to execute weaver due to a failure to load ValueTuple.
        // This is a known issue with in dotnet(https://github.com/dotnet/runtime/issues/27533).
        // The recommended workaround is to avoid using ValueTuple inside a weaver.
        var dic = instructions.Select((m, i) => Tuple.Create(m, i)).ToDictionary(x => x.Item1, x => x.Item2);
        var root = new GraphNode<int>(0);
        var queue = new Queue<GraphNode<int>>();
        queue.Enqueue(root);

        while (queue.Count != 0)
        {
            var node = queue.Dequeue();
            var index = node.Value;
            var instruction = instructions[index];
            switch (instruction.OpCode.FlowControl)
            {
                case FlowControl.Cond_Branch:
                {
                    AddBranchOperand();
                    AddNext();
                    break;
                }
                case FlowControl.Branch:
                {
                    AddBranchOperand();
                    break;
                }
                case FlowControl.Return:
                case FlowControl.Throw:
                    break;

                case FlowControl.Break:
                case FlowControl.Meta:
                case FlowControl.Next:
                case FlowControl.Phi:
                case FlowControl.Call when instruction.OpCode != OpCodes.Jmp:
                default:
                {
                    AddNext();
                    break;
                }
            }

            void AddBranchOperand()
            {
                var ins = (Instruction)instruction!.Operand;
                var i = dic![ins];
                var child = GraphNode.Create(i);
                node!.Children.Add(child);
                queue!.Enqueue(child);
            }

            void AddNext()
            {
                var nextIndex = index + 1;
                if (nextIndex >= instructions.Count)
                    return;

                var child = GraphNode.Create(nextIndex);
                node!.Children.Add(child);
                queue!.Enqueue(child);
            }

        }

        return root;
    }

    public static Instruction[] GetArgumentPushInstructions(this Instruction instruction, IList<Instruction> instructions, GraphNode<int> graph)
    {
        if (instruction.OpCode.FlowControl != FlowControl.Call)
            throw new InstructionWeavingException(instruction, "Expected a call instruction");

        var method = (IMethodSignature)instruction.Operand;
        var argCount = GetArgCount(instruction.OpCode, method);

        if (argCount == 0)
            return Array.Empty<Instruction>();

        var stack = new Stack<Instruction>();
        Dfs(instruction, instructions, graph, ref stack);

        if (stack.Count < argCount)
        {
            throw new InstructionWeavingException(instruction, $"The stack count {stack.Count} is less than the expected argument count {argCount} for {instruction}");
        }

        return stack.Take(argCount).Reverse().ToArray();
    }

    private static bool Dfs(in Instruction instruction, in IList<Instruction> instructions, GraphNode<int> node, ref Stack<Instruction> stack)
    {
        var index = node.Value;
        if (instruction == instructions[index])
        {
            return true;
        }

        var cur = instructions[index];
        var popCount = GetPopCount(cur);
        var pushCount = GetPushCount(cur);

        if (stack.Count < popCount)
        {
            throw new InstructionWeavingException(cur, $"Could not pop {popCount} values from stack whose count is {stack.Count} due to {cur}");
        }

        if (pushCount > 2)
        {
            throw new InstructionWeavingException(cur, $"Unknown instruction {cur} that pops {popCount} values from stack");
        }

        for (var i = 0; i < popCount; i++)
        {
            stack.Pop();
        }

        for (var i = 0; i < pushCount; i++)
        {
            stack.Push(cur);
        }

        if (node.Children.Count == 1)
        {
            return Dfs(instruction, instructions, node.Children[0], ref stack);
        }

        if (node.Children.Count > 1)
        {
            foreach (var child in node.Children)
            {
                var preservedStack = stack.Clone();
                if (!Dfs(instruction, instructions, child, ref preservedStack))
                    continue;

                stack = preservedStack;
                return true;
            }
        }

        return false;
    }
}